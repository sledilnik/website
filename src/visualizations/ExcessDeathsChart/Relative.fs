module ExcessDeathsChart.Relative

open Browser
open Highcharts
open Fable.Core.JsInterop
open Fable.DateFunctions

open Types

let colors = {|
    ExcessDeaths = "#ff3333"
    CovidDeaths = "#a483c7"
    ConfidenceInterval = "#a0a0a0"
|}

let baselineStartYear, baselineEndYear = 2015, 2019

let renderChartOptions (statsData : StatsData) (data : WeeklyDeathsData)  =

    let baselineData =
        data
        |> List.filter (fun dp -> dp.Year >= baselineStartYear && dp.Year <= baselineEndYear)

    let deceasedBaseline, deceasedminMax =
        baselineData
        |> List.groupBy (fun dp -> dp.Week)
        |> List.map (fun (week, dps) ->
            let baseline =
                let sum = (List.sumBy (fun (dp : WeeklyDeaths) -> float dp.Deceased) dps)
                let countWeeks =
                    baselineData
                    |> List.filter (fun dataDp -> dataDp.Week = week)
                    |> List.length
                float sum / float countWeeks
            let min = (List.minBy (fun (dp : WeeklyDeaths) -> float dp.Deceased) dps).Deceased
            let max = (List.maxBy (fun (dp : WeeklyDeaths) -> float dp.Deceased) dps).Deceased
            (week, baseline), ((week, (float min / float baseline - 1.) * 100.), (week, (float max / float baseline - 1.) * 100.)))
        |> List.unzip

    let deceasedBaselineMap = deceasedBaseline |> FSharp.Collections.Map
    let deceasedMin, deceasedMax = List.unzip deceasedminMax
    let deceasedMinMap, deceasedMaxMap = deceasedMin |> FSharp.Collections.Map, deceasedMax |> FSharp.Collections.Map

    let deceasedFromStartYear =
        data
        |> List.filter (fun dp -> dp.Year >= START_YEAR)

    let deceasedFromStartYearRelativeToBaseline =
        deceasedFromStartYear
        |> List.map (fun dp ->
            match deceasedBaselineMap.TryFind(dp.Week) with
            | None -> None
            | Some baseline ->
                Some (dp, System.Math.Round((float dp.Deceased - baseline) / baseline * 100.)) )
        |> List.choose id

    let deceasedCovidFromStartYear =
        statsData
        // Filter the data to the current year
        |> List.filter (fun dp -> dp.Date.Year >= START_YEAR)

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

        // Sum the data by week
        |> List.map (fun ((year, week), dps) ->
            { Year = year
              Week = week
              WeekStartDate = dps |> List.map (fun dp -> dp.date) |> List.head
              WeekEndDate = dps |> List.map (fun dp -> dp.date) |> List.last
              Deceased = dps |> List.sumBy (fun dp -> dp.deceased) } )

    let deceasedCovidFromStartYearPercent =
        deceasedCovidFromStartYear
        |> List.map (fun dp ->
            match deceasedBaselineMap.TryFind(dp.Week) with
            | None -> None
            | Some deceasedTotal ->
                Some (dp, System.Math.Round(float dp.Deceased / float deceasedTotal * 100., 1)) )
        |> List.choose id

    let deceasedMinPercent =
        deceasedFromStartYearRelativeToBaseline |> List.map (fun (dp, _) -> dp.Week, dp.WeekStartDate)
        |> List.map (fun (week, weekStartDate) ->
            match deceasedMinMap.TryFind(week) with
            | None -> None
            | Some percent -> Some (weekStartDate, percent) )
        |> List.choose id

    let deceasedMaxPercent =
        deceasedFromStartYearRelativeToBaseline |> List.map (fun (dp, _) -> dp.Week, dp.WeekStartDate)
        |> List.map (fun (week, weekStartDate) ->
            match deceasedMaxMap.TryFind(week) with
            | None -> None
            | Some percent -> Some (weekStartDate, percent) )
        |> List.choose id

    let series =
        [|
            {| ``type`` = "line"
               animation = false
               marker = {| enabled = false |} |> pojo
               lineWidth = 1
               color = colors.ConfidenceInterval
               dashStyle = "Dash"
               enableMouseTracking = false
               showInLegend = false
               data =
                   deceasedMinPercent
                   |> List.map (fun (weekStartDate, percent) ->
                        {| x = weekStartDate
                           y = percent
                        |} |> pojo)
                   |> List.toArray
            |} |> pojo

            {| ``type`` = "line"
               animation = false
               marker = {| enabled = false |} |> pojo
               lineWidth = 1
               color = colors.ConfidenceInterval
               dashStyle = "Dash"
               enableMouseTracking = false
               showInLegend = false
               data =
                   deceasedMaxPercent
                   |> List.map (fun (weekStartDate, percent) ->
                        {| x = weekStartDate
                           y = percent
                        |} |> pojo)
                   |> List.toArray
            |} |> pojo

            {| ``type`` = "line"
               name = (I18N.t "charts.excessDeaths.excess.excessDeaths")
               animation = false
               marker = {| enabled = false |} |> pojo
               color = colors.ExcessDeaths
               data =
                   deceasedFromStartYearRelativeToBaseline
                   |> List.map (fun (dp, percent) ->
                        {| x = dp.WeekStartDate
                           y = percent
                           name = (I18N.tOptions "charts.excessDeaths" {| dateFrom = dp.WeekStartDate ; dateTo = dp.WeekEndDate |})?weekDateWithYear
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
                   deceasedCovidFromStartYearPercent
                   |> List.map (fun (dp, percent) ->
                        {| x = dp.WeekStartDate
                           y = System.Math.Round(percent, 1)
                           name = (I18N.tOptions "charts.excessDeaths" {| dateFrom = dp.WeekStartDate ; dateTo = dp.WeekEndDate |})?weekDateWithYear
                        |} |> pojo)
                   |> List.toArray
            |} |> pojo
        |]

    {| title = ""
       xAxis = {| ``type`` = "datetime" |}
       yAxis = {| title = {| text = None |} ; opposite = true ; labels = {| formatter = fun (x) -> x?value + " %" |} |> pojo |}
       tooltip = {| formatter = fun () -> sprintf "%s: <b>%.1f %%</b>" jsThis?key jsThis?y |} |> pojo
       responsive = ChartOptions.responsive
       series = series
       credits =
        {| enabled = true
           text = sprintf "%s: %s, %s"
                (I18N.t "charts.common.dataSource")
                (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
                (I18N.tOptions ("charts.common.dsMZ") {| context = localStorage.getItem ("contextCountry") |})
           href = "https://www.stat.si/StatWeb/Field/Index/17/95" |} |> pojo
    |} |> pojo
