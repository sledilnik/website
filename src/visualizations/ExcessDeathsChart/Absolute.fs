module ExcessDeathsChart.Absolute

open Highcharts

open Types

let colors = {|
    CurrentYear = "#a483c7"
    BaselineYear = "#d5d5d5"
|}

let renderChartOptions (data : MonthlyDeathsData) =
    let series =
        data
        |> List.groupBy (fun dp -> dp.year)
        |> List.map (fun (year, data) ->
            let seriesData =
                data
                |> List.map (fun dp ->
                {| x = dp.month
                   y = dp.deceased
                   name = Utils.monthNameOfIndex dp.month |} |> pojo)
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
        series = series |} |> pojo
