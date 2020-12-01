module ExcessDeathsChart.Relative

open Browser
open Highcharts
open Fable.Core.JsInterop
open Fable.DateFunctions

open Types

let colors = {|
    ExcessDeaths = "#ff3333"
    CovidDeaths = "#a483c7"
|}

let YEAR = 2020

let renderChartOptions (data : WeeklyDeathsData) (statsData : StatsData) =

    let baselineStartYear, baselineEndYear = 2015, 2019

    let deceasedBaseline =
        data
        |> List.filter (fun dp -> dp.Year >= baselineStartYear && dp.Year <= baselineEndYear)
        |> List.groupBy (fun dp -> dp.Week)
        |> List.map (fun (week, dps) ->
            (week, (List.sumBy (fun (dp : WeeklyDeaths) -> float dp.Deceased) dps) / float (baselineEndYear - baselineStartYear + 1)) )

    let deceasedBaselineMap =
        deceasedBaseline
        |> FSharp.Collections.Map

    let deceasedCurrentYear =
        data
        |> List.filter (fun dp -> dp.Year = YEAR)
        |> List.map (fun dp -> (dp.Week, dp.Deceased))

    let deceasedCurrentYearRelativeToBaseline =
        deceasedCurrentYear
        |> List.map (fun (week, deceased) ->
            match deceasedBaselineMap.TryFind(week) with
            | None -> None
            | Some baseline ->
                Some (week, (float deceased - baseline) / baseline * 100.) )
        |> List.choose id

    let deceasedCovidCurrentYear =
        statsData
        // Filter the data to the current year
        |> List.filter (fun dp -> dp.Date.Year = YEAR)
        // Select only the non-empty deceased data points
        |> List.map (fun dp ->
            match dp.StatePerTreatment.Deceased with
            | None -> None
            | Some deceased -> Some (dp.Date, deceased) )
        |> List.choose id
        |> List.map (fun (date, deceased) ->
                (Utils.getISOWeekYear(date), date.GetISOWeek(), deceased) )
        |> List.groupBy (fun (year, week, _) -> (year, week))
        |> List.map (fun ((year, week), dps) ->
            { Year = year
              Week = week
              Deceased = dps |> List.sumBy (fun (_, _, deceased) -> deceased) } )

    let deceasedCovidCurrentYearPercent =
        deceasedCovidCurrentYear
        |> List.map (fun dp ->
            match deceasedBaselineMap.TryFind(dp.Week) with
            | None -> None
            | Some deceasedTotal ->
                Some (dp.Week, float dp.Deceased / float deceasedTotal * 100.) )
        |> List.choose id

    let series =
        [|
            {| ``type`` = "line"
               name = (I18N.t "charts.excessDeaths.excess.excessDeaths")
               animation = false
               marker = {| enabled = false |} |> pojo
               color = colors.ExcessDeaths
               data =
                   deceasedCurrentYearRelativeToBaseline
                   |> List.map (fun (week, percent) ->
                       {| x = week
                          y = System.Math.Round(percent, 1) |} |> pojo)
                   |> List.toArray
            |} |> pojo
            {| ``type`` = "area"
               name = (I18N.t "charts.excessDeaths.excess.covidDeaths")
               animation = false
               marker = {| enabled = false |} |> pojo
               color = colors.CovidDeaths
               lineWidth = 0
               data =
                   deceasedCovidCurrentYearPercent
                   |> List.map (fun (week, percent) ->
                       {| x = week
                          y = System.Math.Round(percent, 1) |} |> pojo)
                   |> List.toArray
            |} |> pojo
        |]

    {| baseOptions with
        yAxis = {| title = {| text = None |} ; opposite = true ; labels = {| formatter = fun (x) -> x?value + " %" |} |> pojo |}
        tooltip = {| formatter = fun () -> sprintf "<b>%s %d</b>: %.1f %%" (I18N.t "week") jsThis?x jsThis?y |} |> pojo
        series = series |} |> pojo
