module ExcessDeathsChart.Relative

open Browser
open Highcharts
open Fable.Core.JsInterop

open Types

let colors = {|
    ExcessDeaths = "#ff3333"
    CovidDeaths = "#a483c7"
|}

let YEAR = 2020

let renderChartOptions (data : MonthlyDeathsData) (statsData : StatsData) =

    let baselineStartYear, baselineEndYear = 2015, 2019

    let deceasedBaseline =
        data
        |> List.filter (fun dp -> dp.year >= baselineStartYear && dp.year <= baselineEndYear)
        |> List.groupBy (fun dp -> dp.month)
        |> List.map (fun (month, dps) ->
            (month, (List.sumBy (fun (dp : Data.MonthlyDeaths.DataPoint) -> float dp.deceased) dps) / float (baselineEndYear - baselineStartYear + 1)) )

    let deceasedBaselineMap =
        deceasedBaseline
        |> FSharp.Collections.Map

    let deceasedCurrentYear =
        data
        |> List.filter (fun dp -> dp.year = YEAR)
        |> List.map (fun dp -> (dp.month, dp.deceased))

    let deceasedCurrentYearRelativeToBaseline =
        deceasedCurrentYear
        |> List.map (fun (month, deceased) ->
            match deceasedBaselineMap.TryFind(month) with
            | None -> None
            | Some baseline ->
                Some (month, (float deceased - baseline) / baseline * 100.) )
        |> List.choose id

    let deceasedCovidCurrentYear =
        let data =
            statsData
            // Filter the data to the current year
            |> List.filter (fun dp -> dp.Date.Year = YEAR)
            // Select only the non-empty deceased data points
            |> List.map (fun dp ->
                match dp.StatePerTreatment.Deceased with
                | None -> None
                | Some deceased -> Some (dp.Date.Month, deceased) )
            |> List.choose id
            |> List.groupBy (fun (month, deceased) -> month)
            |> List.map (fun (month, deceased) ->
                let deceasedSum =
                    deceased
                    |> List.map (fun (month, deceased) -> deceased)
                    |> List.sum
                (month, deceasedSum) )
            |> List.sort

        // Add 0 to the month before the first month to smooth the area
        match data with
        | [ ] -> data
        | _ ->
            let (firstMonth, _) = data.Head
            if firstMonth = 1 then
                data
            else
                List.append [firstMonth - 1, 0] data

    let deceasedCovidCurrentYearPercent =
        deceasedCovidCurrentYear
        |> List.map (fun (month, deceasedCovid) ->
            match deceasedBaselineMap.TryFind(month) with
            | None -> None
            | Some deceasedTotal ->
                Some (month, float deceasedCovid / float deceasedTotal * 100.) )
        |> List.choose id

    let series =
        [|
            {| ``type`` = "line"
               name = (I18N.t "charts.excessDeaths.excess.excessDeaths")
               marker = {| enabled = false |} |> pojo
               color = colors.ExcessDeaths
               data =
                   deceasedCurrentYearRelativeToBaseline
                   |> List.map (fun (month, percent) ->
                       {| x = month
                          y = percent
                          name = Utils.monthNameOfIndex month
                       |} |> pojo)
                   |> List.toArray
            |} |> pojo
            {| ``type`` = "area"
               name = (I18N.t "charts.excessDeaths.excess.covidDeaths")
               marker = {| enabled = false |} |> pojo
               color = colors.CovidDeaths
               lineWidth = 0
               data =
                   deceasedCovidCurrentYearPercent
                   |> List.map (fun (month, percent) ->
                       {| x = month
                          y = percent
                          name = Utils.monthNameOfIndex month
                       |} |> pojo)
                   |> List.toArray
            |} |> pojo
        |]

    {| baseOptions with
        yAxis = {| title = {| text = None |} ; opposite = true ; labels = {| formatter = fun (x) -> x?value + " %" |} |> pojo |}
        tooltip = {| formatter = fun () -> sprintf "%s<br>%.2f %%" (Utils.monthNameOfIndex jsThis?x) jsThis?y |} |> pojo
        series = series |} |> pojo
