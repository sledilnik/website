module AgeGroupsTimelineViz.Analysis

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
        | Some value1, Some value2 -> value2 - value1 |> Some
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
