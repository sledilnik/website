module AgeGroupsTimelineViz.Analysis

open System
open System.Collections.Generic
open Types


type CasesByAgeGroupsForDay = {
    Date: System.DateTime
    Cases: AgeGroupsList
}

type CasesByAgeGroupsTimeline = CasesByAgeGroupsForDay list

let calcCasesByAgeForDay
    (prevDay: AgeGroupsList option)
    (currentDay: AgeGroupsList): AgeGroupsList =

    let calcIntOptionDiff value1 value2 =
        match value1, value2 with
        | Some value1, Some value2 ->
            // sometimes NIJZ's data is corrected and total cases can decrease,
            // so we need to ignore this for the purposes of our visualization
            max (value2 - value1) 0 |> Some
        | Some _, None -> None
        | None, Some value2 -> Some value2
        | None, None -> None

    let calcAgeGroupDiff
        (prevDay: IDictionary<AgeGroupKey, AgeGroup>) ageGroup =
        match prevDay.TryGetValue ageGroup.GroupKey with
        | true, prevDayGroup ->
            { GroupKey = ageGroup.GroupKey
              Male = calcIntOptionDiff prevDayGroup.Male ageGroup.Male
              Female = calcIntOptionDiff prevDayGroup.Female ageGroup.Female
              All = calcIntOptionDiff prevDayGroup.All ageGroup.All
            }
        | false, _ -> ageGroup

    match prevDay with
    | None -> currentDay
    | Some prevDay ->
        // create a dictionary of prevDay age groups
        let prevDayGroups =
            prevDay
            |> List.map (fun ageGroup -> (ageGroup.GroupKey, ageGroup))
            |> dict

        currentDay
        // for each age group of currentDay find the group in prevDay
        // and calculate the difference
        |> List.map (fun ageGroup -> calcAgeGroupDiff prevDayGroups ageGroup)

let calculateCasesByAgeTimeline
    (totalCasesByAgeGroups: (DateTime * AgeGroupsList) list)
    : CasesByAgeGroupsTimeline =

    // returns True if there is at least one new infection for any
    // age group for the given day
    let thereAreSomeCases (forDay: CasesByAgeGroupsForDay) =
        forDay.Cases
        |> List.exists
               (fun group ->
                    match group.All with
                    | Some cases -> cases > 0
                    | None -> false
               )

    None :: (totalCasesByAgeGroups |> List.map Some)
    |> List.pairwise
    |> List.map
           (fun (prevDayRecord, currentDayRecord) ->
                match prevDayRecord, currentDayRecord with
                | Some (_, prevDay), Some (date, currentDay) ->
                    { Date = date
                      Cases =
                          calcCasesByAgeForDay (Some prevDay) currentDay }
                | None, Some (date, currentDay) ->
                    { Date = date
                      Cases = calcCasesByAgeForDay None currentDay }
                | _ -> invalidOp "BUG: this should never happen"
            )
    // skip initial days without any cases
    |> List.skipWhile(fun x -> not (thereAreSomeCases x))
    // now skip trailing days without any cases
    |> List.rev
    |> List.skipWhile(fun x -> not (thereAreSomeCases x))
    |> List.rev
