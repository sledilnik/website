module AgeGroupsTimelineViz.Synthesis

open Types
open AgeGroupsTimelineViz.Analysis
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

type ChartType =
    | StackedBarNormal
    | StackedBarPercent

type DisplayMetricsType = NewCases | ActiveCases
type DisplayMetrics = {
    Id: string
    MetricsType: DisplayMetricsType
    ChartType: ChartType
}

let availableDisplayMetrics = [|
    { Id = "newCases"; MetricsType = NewCases
      ChartType = StackedBarNormal }
    { Id = "newCasesRelative"; MetricsType = NewCases
      ChartType = StackedBarPercent }
    { Id = "activeCases"; MetricsType = ActiveCases
      ChartType = StackedBarNormal }
    { Id = "activeCasesRelative"; MetricsType = ActiveCases
      ChartType = StackedBarPercent }
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
    let points: obj[] = jsThis?points

    match points with
    | [||] -> ""
    | _ ->
        // points.[0].point.y

        let totalCases =
            points
            |> Array.sumBy(fun point -> float point?point?y)

        let s = StringBuilder()

        let date = points.[0]?point?date
        s.AppendFormat ("<b>{0}</b><br/>", date.ToString()) |> ignore

        s.Append "<table>" |> ignore

        points
        |> Array.iter
               (fun ageGroup ->
                    let ageGroupLabel = ageGroup?series?name
                    let ageGroupColor = ageGroup?series?color
                    let dataPoint = ageGroup?point

                    let dataValue: int = dataPoint?y

                    match dataValue with
                    | 0 -> ignore()
                    | _ ->
                        let format =
                            "<td style='color: {0}'>●</td>"+
                            "<td style='text-align: center; padding-left: 6px'>{1}:</td>"+
                            "<td style='text-align: right; padding-left: 6px'>"+
                            "<b>{2}</b></td>" +
                            "<td style='text-align: right; padding-left: 10px'>" +
                            "{3}</td>"

                        let percentage =
                            (float dataValue) * 100. / totalCases
                            |> Utils.percentageValuesWith1DecimalTrailingZeroLabelFormatter

                        s.Append "<tr>" |> ignore
                        let ageGroupTooltip =
                            System.String.Format
                                (format,
                                 ageGroupColor,
                                 ageGroupLabel,
                                 dataValue,
                                 percentage)
                        s.Append ageGroupTooltip |> ignore
                        s.Append "</tr>" |> ignore
                )

        s.Append "</table>" |> ignore
        s.ToString()
