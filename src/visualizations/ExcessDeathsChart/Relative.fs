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
        |> List.filter (fun dp -> dp.Year = CURRENT_YEAR)

    let deceasedCurrentYearRelativeToBaseline =
        deceasedCurrentYear
        |> List.map (fun dp ->
            match deceasedBaselineMap.TryFind(dp.Week) with
            | None -> None
            | Some baseline ->
                Some (dp, System.Math.Round((float dp.Deceased - baseline) / baseline * 100.)) )
        |> List.choose id

    let deceasedCovidCurrentYear =
        statsData
        // Filter the data to the current year
        |> List.filter (fun dp -> dp.Date.Year = CURRENT_YEAR)
        // Select only the non-empty deceased data points
        |> List.map (fun dp ->
            match dp.StatePerTreatment.Deceased with
            | None -> None
            | Some deceased -> Some (dp.Date, deceased) )
        |> List.choose id
        |> List.map (fun (date, deceased) ->
            {| year = Utils.getISOWeekYear(date) ; week = date.GetISOWeek() ; date = date ; deceased = deceased |} )
        |> List.groupBy (fun dp -> (dp.year, dp.week))

        // Trim the last week if it is incomplete
        |> List.rev
        |> List.mapi (fun i x -> i, x)
        |> List.filter (fun (i, ((year, week), dps)) -> i <> 0 || i = 0 && dps.Length = 7)
        |> List.map (fun (i, x) -> x)
        |> List.rev

        |> List.map (fun ((year, week), dps) ->
            { Year = year
              Week = week
              WeekStartDate = dps |> List.map (fun dp -> dp.date) |> List.head
              WeekEndDate = dps |> List.map (fun dp -> dp.date) |> List.last
              Deceased = dps |> List.sumBy (fun dp -> dp.deceased) } )

    let deceasedCovidCurrentYearPercent =
        deceasedCovidCurrentYear
        |> List.map (fun dp ->
            match deceasedBaselineMap.TryFind(dp.Week) with
            | None -> None
            | Some deceasedTotal ->
                Some (dp, System.Math.Round(float dp.Deceased / float deceasedTotal * 100., 1)) )
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
                   |> List.map (fun (dp, percent) ->
                        {| x = dp.WeekStartDate
                           y = percent
                           name = (I18N.tOptions "charts.excessDeaths" {| dateFrom = dp.WeekStartDate ; dateTo = dp.WeekEndDate |})?weekDate
                        |} |> pojo)
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
                   |> List.map (fun (dp, percent) ->
                        {| x = dp.WeekStartDate
                           y = System.Math.Round(percent, 1)
                           name = (I18N.tOptions "charts.excessDeaths" {| dateFrom = dp.WeekStartDate ; dateTo = dp.WeekEndDate |})?weekDate
                        |} |> pojo)
                   |> List.toArray
            |} |> pojo
        |]

    {| title = ""
       xAxis = {| ``type`` = "datetime" ; dateTimeLabelFormats = {| day = "%e. %b" |} |> pojo |}
       yAxis = {| title = {| text = None |} ; opposite = true ; labels = {| formatter = fun (x) -> x?value + " %" |} |> pojo |}
       tooltip = {| formatter = fun () -> sprintf "%s: <b>%.1f %%</b>" jsThis?key jsThis?y |} |> pojo
       series = series
       credits =
        {| enabled = true
           text = sprintf "%s: %s, %s"
                (I18N.t "charts.common.dataSource")
                (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
                (I18N.tOptions ("charts.common.dsMZ") {| context = localStorage.getItem ("contextCountry") |})
           href = "https://www.stat.si/StatWeb/Field/Index/17/95" |} |> pojo
    |} |> pojo
