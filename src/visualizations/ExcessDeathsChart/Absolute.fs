module ExcessDeathsChart.Absolute

open Fable.Core.JsInterop
open Highcharts

open Types
open Browser

let colors = {|
    CurrentYear = "#a483c7"
    BaselineYear = "#d5d5d5"
|}

let renderChartOptions (data : WeeklyDeathsData) =
    let series =
        data
        |> List.groupBy (fun dp -> dp.Year)
        |> List.map (fun (year, data) ->
            let seriesData =
                data
                |> List.map (fun dp ->
                {| x = dp.Week
                   y = dp.Deceased
                   name = sprintf "%s %d" (I18N.t "week") dp.Week |} |> pojo)
                |> List.toArray
            {| ``type`` = "line"
               name = year
               animation = false
               showInLegend = year = System.DateTime.Now.Year
               data = seriesData
               color = if year = System.DateTime.Now.Year then colors.CurrentYear else colors.BaselineYear
               lineWidth = if year = System.DateTime.Now.Year then 2 else 1
               marker = {| enabled = true ; symbol = "circle" ; radius = 2 |} |> pojo
            |} |> pojo)
        |> List.toArray

    {| baseOptions with
        yAxis = {| min = 0 ; title = {| text = None |} ; opposite = true |}
        tooltip = {| formatter = fun () -> sprintf "<b>%s, %s %d</b>: %d umrlih" jsThis?series?name ((I18N.t "week").ToLower()) jsThis?x jsThis?y |} |> pojo
        series = series |} |> pojo
