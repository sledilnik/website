module CountriesChartViz.Rendering

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

let countriesDisplaySets = [|
    { Label = "groupNeighbouringWoItaly"
      CountriesCodes = [| "AUT"; "CZE"; "DEU"; "HRV"; "HUN"; "SVK" |]
    }
    { Label = "groupCriticalEU"
      CountriesCodes = [| "BEL"; "ESP"; "FRA"; "GBR"; "ITA"; "SWE" |]
    }
    { Label = "groupCriticalWorld"
      CountriesCodes = [| "CHN"; "ESP"; "IRN"; "ITA"; "SWE"; "USA" |]
    }
    { Label = "groupNordic"
      CountriesCodes = [| "DNK"; "FIN"; "ISL"; "NOR"; "SWE" |]
    }
    { Label = "groupExYu"
      CountriesCodes = [| "BIH"; "HRV"; "SVN"; "MNE"; "RKS"; "SRB" |]
    }
    { Label = "groupEastAsiaOceania"
      CountriesCodes = [| "AUS"; "CHN"; "JPN"; "KOR"; "NZL"; "SGP"; "TWN" |]
    }
|]

type Msg =
    | DataRequested
    | DataLoaded of Data.OurWorldInData.OurWorldInDataRemoteData
    | CountriesSelectionChanged of CountriesDisplaySet
    | XAxisTypeChanged of XAxisType
    | ScaleTypeChanged of ScaleType

[<Literal>]
let DaysOfMovingAverage = 5

let init: ChartState * Cmd<Msg> =
    let state = {
        OwidDataState = NotLoaded
        DisplayedCountriesSet = countriesDisplaySets.[0]
        XAxisType = ByDate// DaysSinceFirstDeath
        ScaleType = Linear
    }
    state, Cmd.ofMsg DataRequested

let update (msg: Msg) (state: ChartState) : ChartState * Cmd<Msg> =
    let getCountriesCodes selectedSet =
        "MKD" ::
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
        Cmd.OfAsync.result (Data.OurWorldInData.load countriesCodes DataLoaded)
    | DataRequested ->
        let countriesCodes = getCountriesCodes state.DisplayedCountriesSet

        let newOwidDataState =
            match state.OwidDataState with
            | NotLoaded -> PreviousAndLoadingNew Loading
            | PreviousAndLoadingNew oldOwidData ->
                PreviousAndLoadingNew oldOwidData
            | Current oldOwidData -> PreviousAndLoadingNew oldOwidData

        { state with OwidDataState = newOwidDataState },
        Cmd.OfAsync.result (Data.OurWorldInData.load countriesCodes DataLoaded)
    | DataLoaded remoteData ->
        { state with OwidDataState = Current remoteData }, Cmd.none
    | XAxisTypeChanged newXAxisType ->
        { state with XAxisType = newXAxisType }, Cmd.none
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
                             x =
                                 match state.XAxisType with
                                 | ByDate -> entry.Date |> jsTime12h :> obj
                                 | DaysSinceFirstDeath -> i :> obj
                                 | DaysSinceOneDeathPerMillion -> i :> obj
                             y = entry.TotalDeathsPerMillion
                             date = I18N.tOptions "days.longerDate" {| date = entry.Date |}
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
                title = pojo {| text = chartData.DataDescription |}
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                layout = "vertical"
                floating = true
                padding = 15
                x = 20
                y = 30
                backgroundColor = "rgba(255,255,255,0.5)"
        |}

    let baseOptions =
        basicChartOptions state.ScaleType "covid19-metrics-comparison"
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
                   ``type`` =
                        match state.XAxisType with
                        | ByDate -> "datetime"
                        | DaysSinceFirstDeath -> "int"
                        | DaysSinceOneDeathPerMillion -> "int"
                   allowDecimals = false
                   title = pojo {| text = chartData.XAxisTitle |}
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
                   opposite = true
                   title =
                       pojo {|
                            align = "middle"
                            text = chartData.YAxisTitle
                        |}
            |}
        plotOptions = pojo
            {|
                series = pojo {| stacking = "" |}
            |}
        legend = legend
        tooltip = pojo {|
                          formatter = fun () ->
                              tooltipFormatter state chartData jsThis
                          shared = true
                          useHTML = true
                        |}

        credits = pojo
            {|
                enabled = true
                text = I18N.t "charts.countries.dataSourceOWID"
                href = "https://ourworldindata.org"
            |}
    |}


let renderChartContainer state chartData =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [ renderChartCode state chartData |> chart ]
    ]

let renderCountriesSetsSelectors (activeSet: CountriesDisplaySet) dispatch =
    let renderCountriesSetSelector (setToRender: CountriesDisplaySet) =
        let active = setToRender = activeSet
        Html.div [
            prop.text (I18N.tt "charts.countries" setToRender.Label)
            prop.className [
                true, "btn btn-sm metric-selector"
                active, "metric-selector--selected selected" ]
            if not active then prop.onClick (fun _ -> dispatch setToRender)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        countriesDisplaySets
        |> Array.map renderCountriesSetSelector
        |> prop.children
    ]

let renderXAxisSelectors (activeXAxisType: XAxisType) dispatch =
    let renderXAxisSelector (axisSelector: XAxisType) =
        let active = axisSelector = activeXAxisType

        let defaultProps =
            [
                match axisSelector with
                | ByDate -> I18N.t "charts.countries.chronologically"
                | DaysSinceFirstDeath -> I18N.t "charts.countries.sinceFirstDeath"
                | DaysSinceOneDeathPerMillion -> I18N.t "charts.countries.sinceOneDeathPerMillion"
                |> prop.text

                prop.className [
                    true, "chart-display-property-selector__item"
                    active, "selected" ]
            ]

        if active then Html.div defaultProps
        else
            Html.div
                ((prop.onClick (fun _ -> dispatch axisSelector))
                    :: defaultProps)

    let xAxisTypesSelectors =
        [ ByDate; DaysSinceFirstDeath; DaysSinceOneDeathPerMillion]
        |> List.map renderXAxisSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children ((Html.text (I18N.t "charts.common.xAxis")) :: xAxisTypesSelectors)
    ]

let render state dispatch =
    let xAxisType = state.XAxisType

    let chartData =
        state |> prepareChartData xAxisType DaysOfMovingAverage

    let topControls = [
            renderXAxisSelectors state.XAxisType (XAxisTypeChanged >> dispatch)
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

let renderChart() =
    React.elmishComponent
        ("CountriesChartViz/CountriesChart", init, update, render)
