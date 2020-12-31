module DataAnalysis.AgeGroupsTimeline

open System
open System.Collections.Generic
open DataAnalysis.DatedTypes
open Types
open Highcharts

type CasesByAgeGroupsForDay = AgeGroupsList

type CasesByAgeGroupsTimeline = DatedArray<AgeGroupsList>

/// Calculates the difference of total cases by age groups between two
/// consecutive days which represents new cases by age groups.
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

/// Converts total cases by age groups timeline into daily new cases by
/// age groups timeline.
let calculateDailyCasesByAgeTimeline
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

    // converts AgeGroupsList array to an array of options, with None as
    //  the first value in the array. We need this so we can apply pairwise
    //  function below.
    let totalCasesTimelineArray: AgeGroupsList option[] =
        Array.append
            [| None |]
            (totalCasesByAgeGroups.Data |> Array.map Some)

    // converts the array of total AgeGroupsList values into diffs, which
    //  represent daily new cases
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

let listAgeGroups (timeline: CasesByAgeGroupsTimeline): AgeGroupKey list  =
    timeline.Data.[0]
    |> List.map (fun group -> group.GroupKey)
    |> List.sortBy (fun groupKey -> groupKey.AgeFrom)

type CasesInAgeGroupForDay = int
type CasesInAgeGroupTimeline = DatedArray<CasesInAgeGroupForDay>
type CasesInAgeGroupSeries = {
    AgeGroupKey: AgeGroupKey
    Timeline: CasesInAgeGroupTimeline
}

type ValueCalculationFormula = Daily | Active | Total

/// Calculates the timeline for an age group specified by its index.
/// The source data is the timeline of all age groups.
/// The metric/value of the timeline is defined by the calculation formula.
let extractTimelineForAgeGroup
    ageGroupKey
    (calculationFormula: ValueCalculationFormula)
    (totalValuesAllAgeGroupsTimeline: CasesByAgeGroupsTimeline)
    (dailyValuesAllAgeGroupsTimeline: CasesByAgeGroupsTimeline)
    : CasesInAgeGroupTimeline =

    let extractDataForSingleAgeGroup allAgeGroupsTimeline =
        allAgeGroupsTimeline
        |> mapDatedArrayItems (fun dayGroupsData ->
                    let dataForGroup =
                        dayGroupsData
                        |> List.find(fun group -> group.GroupKey = ageGroupKey)
                    dataForGroup.All
                    |> Utils.optionToInt
                )
    match calculationFormula with
    | Daily -> extractDataForSingleAgeGroup dailyValuesAllAgeGroupsTimeline
    | Active ->
        extractDataForSingleAgeGroup dailyValuesAllAgeGroupsTimeline
        |> mapDatedArray (Statistics.calculateWindowedSumInt 14)
    | Total -> extractDataForSingleAgeGroup totalValuesAllAgeGroupsTimeline

let getAgeGroupTimelineAllSeriesData
        (statsData: StatsData)
        (valueCalculationFormula: ValueCalculationFormula)
        (pointAgeGroupListSelector: StatsDataPoint -> AgeGroupsList) =
    // extract just a list of date + AgeGroupsList tuples
    let totalValuesByAgeGroupsList =
        statsData
        |> List.map (fun point -> (point.Date,
                                   point |> pointAgeGroupListSelector))

    // convert to DatedArray that has just a start date
    //  and an array of AgeGroupsList values
    let totalValuesByAgeGroupsTimeline: CasesByAgeGroupsTimeline =
        mapDateTuplesListToArray totalValuesByAgeGroupsList

    // converts total new cases to daily cases
    let dailyValuesByAgeGroupsTimeline =
        calculateDailyCasesByAgeTimeline totalValuesByAgeGroupsTimeline

    // get keys of all age groups
    let allGroupsKeys = listAgeGroups dailyValuesByAgeGroupsTimeline

    let mapPoint
        (startDate: DateTime)
        (daysFromStartDate: int)
        (cases: CasesInAgeGroupForDay) =
        let date = startDate |> Days.add daysFromStartDate

        pojo {|
             x = date |> jsTime12h :> obj
             y = cases
             date = I18N.tOptions "days.longerDate" {| date = date |}
        |}

    let mapAllPoints (groupTimeline: CasesInAgeGroupTimeline) =
        let startDate = groupTimeline.StartDate
        let timelineArray = groupTimeline.Data

        timelineArray
        |> Array.mapi (mapPoint startDate)

    let renderAgeGroupSeriesMaybe index ageGroupKey =
        let ageGroupTimeline =
            extractTimelineForAgeGroup ageGroupKey valueCalculationFormula
                totalValuesByAgeGroupsTimeline dailyValuesByAgeGroupsTimeline

        // if the timeline has just zero values, we will skip it so it doesn't
        //  show up in the chart
        let hasAnyNonZeroValues =
            ageGroupTimeline.Data |> Array.exists (fun value -> value > 0)

        if hasAnyNonZeroValues then
            let points = ageGroupTimeline |> mapAllPoints
            let color = AgeGroup.colorOfAgeGroup index

            pojo {|
                 ``type`` = "column"
                 visible = true
                 name = ageGroupKey.Label
                 color = color
                 data = points
                 animation = false
            |} |> Some
        else
            None

    allGroupsKeys
    |> List.mapi renderAgeGroupSeriesMaybe
    // skip series which do not have any non-0 data
    |> List.choose id
    |> List.toArray
