module MonthlyDeathsChart.Absolute

open Feliz
open Browser
open Fable.Core.JsInterop

open Highcharts

open Types

let renderChartOptions (data : MonthlyDeathsData) (state : State) dispatch =

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
            {| name = year
               data = seriesData
               color = if year = System.DateTime.Now.Year then colors.CurrentYear else colors.BaselineYear
               lineWidth = if year = System.DateTime.Now.Year then 2 else 1
               marker = {| enabled = true ; symbol = "circle" ; radius = 2 |} |> pojo
            |} |> pojo)
        |> List.toArray

    {| title = None
       legend = {| enabled = false |}
       xAxis = {| labels = {| formatter = fun x -> Utils.monthNameOfIndex x?value |} |> pojo |}
       yAxis = {| min =0 ; title = {| text = null |} |}
       series = series
       credits = {| enabled = true
                    text = sprintf "%s: %s, %s"
                        (I18N.t "charts.common.dataSource")
                        (I18N.tOptions ("charts.common.dsSURS") {| context = localStorage.getItem ("contextCountry") |})
                        (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
                    href = "https://www.stat.si/StatWeb/Field/Index/17/95" |} |> pojo
    |} |> pojo

let renderChart (data : MonthlyDeathsData) (state : State) dispatch =
    Html.div [ prop.style [ style.height 450 ]
               prop.className "highcharts-wrapper"
               prop.children [
                   renderChartOptions data state dispatch |> Highcharts.chart ] ]
