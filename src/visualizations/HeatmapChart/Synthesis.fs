module HeatmapChart.Synthesis

open Types
open HeatmapChart.Analysis
open System.Collections.Generic
open System.Text
open Fable.Core
open JsInterop

type CasesInAgeGroupForDay = int
type CasesInAgeGroupTimeline = DatedArray<CasesInAgeGroupForDay>
type CasesInAgeGroupSeries = {
    AgeGroupKey: AgeGroupKey
    Timeline: CasesInAgeGroupTimeline
}

type AllCasesInAgeGroupSeries = IDictionary<AgeGroupKey, CasesInAgeGroupSeries>


type DisplayMetricsType = NewCases | ActiveCases
type DisplayMetrics = {
    Id: string
    MetricsType: DisplayMetricsType
}

let availableDisplayMetrics = [|
    { Id = "absolute"; MetricsType = NewCases}
    { Id = "relative"; MetricsType = NewCases}
|]

let listAgeGroups (timeline: CasesByAgeGroupsTimeline): AgeGroupKey list  =
    timeline.Data.[0]
    |> List.map (fun group -> group.GroupKey)
    |> List.sortBy (fun groupKey -> groupKey.AgeFrom)

let extractTimelineForAgeGroup
    ageGroupKey
    (metricsType: DisplayMetricsType)
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
    match metricsType with
    | NewCases -> newCasesTimeline
    | ActiveCases ->
        newCasesTimeline
        |> mapDatedArray (Statistics.calculateWindowedSumInt 14)

let tooltipFormatter jsThis =
    //TODO: IMPLEMENT THIS
    "a"

    // match points with
    // | [| |] -> ""
    // | _ -> 
    //     let name = points?name
    //     let date = points?date
    //     let cases = points?cases


    //     sprintf date