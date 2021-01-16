module WeeklyDemographicsViz.Analysis

open System
open System.Collections.Generic
open Types


type DateTuple<'T> = DateTime * 'T
type DateTupleList<'T> = DateTuple<'T> list
type DatedArray<'T> = {
    StartDate: DateTime
    Data: 'T[]
}

type CasesByAgeGroupsForDay = AgeGroupsList

type CasesByAgeGroupsTimeline = DatedArray<AgeGroupsList>


let mapDateTuplesListToArray (dateTupleList: DateTupleList<'T>)
    : DatedArray<'T> =
    // find the first and the last dates in the list
    let (firstDate, _) = dateTupleList |> List.minBy(fun (date, _) -> date.Date)
    let (lastDate, _) = dateTupleList |> List.maxBy(fun (date, _) -> date.Date)

    // from that, we can know the size of the resulting array
    let arraySize = (Days.between firstDate lastDate) + 1

    // create an empty array
    let array: 'T[] = Array.zeroCreate arraySize

    // now fill the array with the values from the original list
    dateTupleList
    |> List.iter(fun (date, itemData) ->
        let daysSince = Days.between firstDate date
        array.[daysSince] <- itemData
        )

    { StartDate = firstDate; Data = array }

let trimDatedArray trimPredicate (datedArray: DatedArray<'T>) =
    let trimmedLeading =
        datedArray.Data
        |> Array.skipWhile trimPredicate

    // we need to increment the start date by the count of trimmed leading days
    let daysToIncrement =
        (datedArray.Data |> Array.length) - (trimmedLeading |> Array.length)

    let newStartDate = datedArray.StartDate |> Days.add daysToIncrement

    let trimmed =
        trimmedLeading
        |> Array.rev
        |> Array.skipWhile trimPredicate
        |> Array.rev

    { StartDate = newStartDate; Data = trimmed }

let mapDatedArrayItems mapping datedArray =
    let startDate = datedArray.StartDate
    let originalArray = datedArray.Data
    { StartDate = startDate; Data = originalArray |> Array.map mapping }

let mapDatedArray mapping datedArray =
    let startDate = datedArray.StartDate
    let originalArray = datedArray.Data
    { StartDate = startDate; Data = originalArray |> mapping }


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
    (totalCasesByAgeGroups: CasesByAgeGroupsTimeline)
    : CasesByAgeGroupsTimeline =

    // returns False if there is at least one new infection for any
    // age group for the given day
    let thereAreNoCases (forDay: CasesByAgeGroupsForDay) =
        forDay
        |> List.exists
               (fun group ->
                    match group.All with
                    | Some cases -> cases > 0
                    | None -> false
               )
        |> not

    let totalCasesTimelineArray: AgeGroupsList option[] =
        Array.append
            [| None |]
            (totalCasesByAgeGroups.Data |> Array.map Some)

    let newCasesTimelineArray =
        totalCasesTimelineArray
        |> Array.pairwise
        |> Array.map
               (fun (prevDayRecord, currentDayRecord) ->
                    match prevDayRecord, currentDayRecord with
                    | Some prevDay, Some currentDay ->
                        calcCasesByAgeForDay (Some prevDay) currentDay
                    | None, Some currentDay ->
                        calcCasesByAgeForDay None currentDay
                    | _ -> invalidOp "BUG: this should never happen"
                )

    { StartDate = totalCasesByAgeGroups.StartDate
      Data = newCasesTimelineArray
    }
    // skip initial and trailing days without any cases
    |> trimDatedArray thereAreNoCases
