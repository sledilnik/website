module AgeGroupsTimelineViz.Synthesis

open DataAnalysis.AgeGroupsTimeline
open DataVisualization.ChartingTypes
open System.Text
open Fable.Core
open JsInterop


type DisplayMetrics = {
    Id: string
    ValueCalculation: ValueCalculationFormula
    ChartType: ChartType
} with
    static member All = [|
        { Id = "newCases"; ValueCalculation = Daily
          ChartType = StackedBarNormal }
        { Id = "newCasesRelative"; ValueCalculation = Daily
          ChartType = StackedBarPercent }
        { Id = "activeCases"; ValueCalculation = Active
          ChartType = StackedBarNormal }
        { Id = "activeCasesRelative"; ValueCalculation = Active
          ChartType = StackedBarPercent }
    |]
    static member Default = DisplayMetrics.All.[0]

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
                    | 0 -> ()
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
                            |> Utils.percentWith1DecimalFormatter

                        s.Append "<tr>" |> ignore
                        let ageGroupTooltip =
                            System.String.Format
                                (format,
                                 ageGroupColor,
                                 ageGroupLabel,
                                 I18N.NumberFormat.formatNumber(dataValue),
                                 percentage)
                        s.Append ageGroupTooltip |> ignore
                        s.Append "</tr>" |> ignore
                )

        s.Append "</table>" |> ignore
        s.ToString()
