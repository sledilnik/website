module DataAnalysis.AgeGroupsTimeline

open System
open System.Collections.Generic
open DataAnalysis.DatedTypes
open Types
open Highcharts

type CasesByAgeGroupsForDay = AgeGroupsList

type CasesByAgeGroupsTimeline = DatedArray<AgeGroupsList>

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

type CasesMetricsType = NewCases | ActiveCases

let extractTimelineForAgeGroup
    ageGroupKey
    (casesMetricsType: CasesMetricsType)
    (casesTimeline: CasesByAgeGroupsTimeline)
    : CasesInAgeGroupTimeline =

    let newCasesTimeline =
        casesTimeline
        |> mapDatedArrayItems (fun dayGroupsData ->
                    let dataForGroup =
                        dayGroupsData
                        |> List.find(fun group -> group.GroupKey = ageGroupKey)
                    dataForGroup.All
                    |> Utils.optionToInt
                )
    match casesMetricsType with
    | NewCases -> newCasesTimeline
    | ActiveCases ->
        newCasesTimeline
        |> mapDatedArray (Statistics.calculateWindowedSumInt 14)

let getAgeGroupTimelineAllSeriesData
        (statsData: StatsData)
        (casesMetricsType: CasesMetricsType) =
    let totalCasesByAgeGroupsList =
        statsData
        |> List.map (fun point -> (point.Date, point.StatePerAgeToDate))

    let totalCasesByAgeGroups =
        mapDateTuplesListToArray totalCasesByAgeGroupsList

    // calculate complete merged timeline
    let timeline = calculateCasesByAgeTimeline totalCasesByAgeGroups

    // get keys of all age groups
    let allGroupsKeys = listAgeGroups timeline

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
        |> Array.mapi (fun i cases -> mapPoint startDate i cases)

    allGroupsKeys
    |> List.mapi (fun index ageGroupKey ->
        let points =
            timeline
            |> extractTimelineForAgeGroup ageGroupKey casesMetricsType
            |> mapAllPoints

        pojo {|
             visible = true
             name = ageGroupKey.Label
             color = AgeGroup.ColorOfAgeGroup index
             data = points
        |}
    )
    |> List.toArray
