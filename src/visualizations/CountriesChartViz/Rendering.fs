﻿module CountriesChartViz.Rendering

open CountriesChartViz.Synthesis
open System
open Browser
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

open Analysis
open Highcharts
open Types
open I18N

let countriesDisplaySets = [|
    { Label = "groupNeighbouring"
      CountriesCodes = [| "AUT"; "CZE"; "DEU"; "HRV"; "HUN"; "ITA"; "SVK" |]
    }
    { Label = "groupCriticalEU"
      CountriesCodes = [| "BEL"; "ESP"; "FRA"; "GBR"; "ITA"; "SWE" |]
    }
    { Label = "groupCriticalWorld"
      CountriesCodes = [| "BRA"; "ECU"; "ITA"; "RUS"; "SWE"; "USA" |]
    }
    { Label = "groupNordic"
      CountriesCodes = [| "DNK"; "FIN"; "ISL"; "NOR"; "SWE" |]
    }
    { Label = "groupExYu"
      CountriesCodes = [| "BIH"; "HRV"; "MKD"; "MNE"; "OWID_KOS"; "SRB" |]
    }
    { Label = "groupEastAsiaOceania"
      CountriesCodes = [| "AUS"; "CHN"; "JPN"; "KOR"; "NZL"; "SGP"; "TWN" |]
    }
    { Label = "groupLatinAmerica"
      CountriesCodes = [| "ARG"; "BRA"; "CHL"; "COL"; "ECU"; "MEX"; "PER" |]
    }
|]

type Msg =
    | DataRequested
    | DataLoaded of Data.OurWorldInData.OurWorldInDataRemoteData
    | CountriesSelectionChanged of CountriesDisplaySet
    | ScaleTypeChanged of ScaleType

[<Literal>]
let DaysOfMovingAverage = 7

let init (config: CountriesChartConfig):
    ChartState * Cmd<Msg> =
    let state = {
        OwidDataState = NotLoaded
        DisplayedCountriesSet = countriesDisplaySets.[0]
        MetricToDisplay = config.MetricToDisplay
        ScaleType = Linear
        ChartTextsGroup = config.ChartTextsGroup
    }
    state, Cmd.ofMsg DataRequested

let update (msg: Msg) (state: ChartState) : ChartState * Cmd<Msg> =
    let getCountriesCodes selectedSet =
        "SVN" ::
        (selectedSet.CountriesCodes |> Array.toList)

    match msg with
    | CountriesSelectionChanged selectedSet ->
        let countriesCodes = getCountriesCodes selectedSet

        let newOwidDataState =
            match state.OwidDataState with
            | NotLoaded -> PreviousAndLoadingNew Loading
            | PreviousAndLoadingNew oldOwidData ->
                PreviousAndLoadingNew oldOwidData
            | Current oldOwidData -> PreviousAndLoadingNew oldOwidData

        { state with
            OwidDataState = newOwidDataState
            DisplayedCountriesSet = selectedSet
        },
        Cmd.OfAsync.result
            (Data.OurWorldInData.loadCountryComparison
                 countriesCodes DataLoaded)
    | DataRequested ->
        let countriesCodes = getCountriesCodes state.DisplayedCountriesSet

        let newOwidDataState =
            match state.OwidDataState with
            | NotLoaded -> PreviousAndLoadingNew Loading
            | PreviousAndLoadingNew oldOwidData ->
                PreviousAndLoadingNew oldOwidData
            | Current oldOwidData -> PreviousAndLoadingNew oldOwidData

        { state with OwidDataState = newOwidDataState },
        Cmd.OfAsync.result
            (Data.OurWorldInData.loadCountryComparison
                 countriesCodes DataLoaded)
    | DataLoaded remoteData ->
        { state with OwidDataState = Current remoteData }, Cmd.none
    | ScaleTypeChanged newScaleType ->
        { state with ScaleType = newScaleType }, Cmd.none

let renderChartCode (state: ChartState) (chartData: ChartData) =
    let myLoadEvent _ =
        let ret _ =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let allSeries =
        chartData.Series
        |> Array.map (fun countrySeries ->
             pojo
                {|
                visible = true
                color = countrySeries.Color
                name = countrySeries.CountryName
                data =
                    countrySeries.Entries
                    |> Array.mapi (fun i entry ->
                        pojo {|
                             x = entry.Date |> jsTime12h :> obj
                             y = entry.Value
                             date = tOptions "days.longerDate"
                                        {| date = entry.Date |}
                             dataLabels =
                                  if i = countrySeries.Entries.Length-1 then
                                    pojo {|
                                            enabled = true
                                            format = countrySeries.CountryName
                                            align = "left"
                                            verticalAlign = "middle"
                                            x = 0
                                            y = 0
                                     |}
                                  else pojo {||}
                        |}
                        )
                marker = pojo {| enabled = false |}
                |}
            )


    let legend =
        pojo {|
                enabled = true
                title = pojo {| text = chartData.LegendTitle |}
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                layout = "vertical"
                floating = true
                padding = 15
                x = 0
                y = 0
                backgroundColor = "rgba(255,255,255,0.9)"
        |}

    let baseOptions =
        basicChartOptions state.ScaleType "covid19-metrics-comparison"
            0 (fun _ -> (fun _ -> true))
    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` = "spline"
                zoomType = "x"
                events = {| load = myLoadEvent("countries") |}
            |}
        title = pojo {| text = None |}
        series = allSeries
        xAxis =
            pojo {|
                   ``type`` = "datetime"
                   allowDecimals = false
                   title = pojo {| text = chartData.XAxisTitle |}
                   dateTimeLabelFormats = pojo
                    {|
                        week = t "charts.common.shortDateFormat"
                        day = t "charts.common.shortDateFormat"
                    |}
            |}
        yAxis =
            pojo {|
                   ``type`` =
                       match state.ScaleType with
                       | Linear -> "linear"
                       | Logarithmic -> "logarithmic"
                   min =
                       match state.ScaleType with
                       | Linear -> 0
                       | Logarithmic -> 1
//                   max =
//                       match state.MetricToDisplay with
//                       // double of the red condition
//                       | ActiveCasesPer1M -> Some 800
//                       | _ -> None
                   opposite = true
                   crosshair = true
                   title =
                       pojo {|
                            align = "middle"
                            text = chartData.YAxisTitle
                        |}
                   plotLines =
                       match state.MetricToDisplay with
                       | ActiveCasesPer1M -> [|
                           {| value=400.0
                              label={|
                                       text=t "charts.countriesActiveCasesPer1M.red"
                                       align="left"
                                       verticalAlign="bottom"
                                        |}
                              color="red"
                              width=1
                              dashStyle="longdashdot"
                              zIndex=9
                            |}
                        |]
                       | _ -> [| |]
                   plotBands =
                       match state.MetricToDisplay with
                       | ActiveCasesPer1M -> [|
                           {| from=400.0; ``to``=100000.0
                              color="#FEF8F7"
                            |}
                        |]
                       | _ -> [| |]
            |}
        plotOptions = pojo
            {|
                series = pojo {| stacking = "" |}
            |}
        legend = legend
        tooltip = pojo {|
                          formatter = fun () ->
                              tooltipFormatter chartData jsThis
                          shared = true
                          useHTML = true
                        |}
        rangeSelector = pojo {| enabled = false |}
        credits = pojo
            {|
                enabled = true
                text =
                    sprintf "%s: %s"
                        (t "charts.common.dataSource")
                        (t "charts.common.dsOWD")
                href = "https://ourworldindata.org/coronavirus"
            |}
    |}

let renderChartContainer state chartData =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [ renderChartCode state chartData |> chart ]
    ]

let renderCountriesSetsSelectors
    (activeSet: CountriesDisplaySet)
    dispatch =
    let renderCountriesSetSelector (setToRender: CountriesDisplaySet) =
        let active = setToRender = activeSet
        Html.div [
            prop.text (t ("country-groups." + setToRender.Label))
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected") ]
            if not active then prop.onClick (fun _ -> dispatch setToRender)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        countriesDisplaySets
        |> Array.map renderCountriesSetSelector
        |> prop.children
    ]

let render (state: ChartState) dispatch =
    let chartData =
        state |> prepareChartData DaysOfMovingAverage

    let topControls = [
            Html.div [ prop.className "chart-display-property-selector" ]
            Utils.renderScaleSelector
                state.ScaleType (ScaleTypeChanged >> dispatch)
    ]

    match chartData with
    | Some chartData ->
        Html.div [
            Utils.renderChartTopControls topControls
            renderChartContainer state chartData
            renderCountriesSetsSelectors
                state.DisplayedCountriesSet
                (CountriesSelectionChanged >> dispatch)

            Html.div [
                prop.className "disclaimer"
            ]
        ]
    | None -> Html.none

let renderChart (config: CountriesChartConfig) =
    React.elmishComponent
        ("CountriesChartViz/CountriesChart", init config, update, render)
