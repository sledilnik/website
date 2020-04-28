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

// source: https://unstats.un.org/unsd/tradekb/knowledgebase/country-code
let countriesDisplaySets = [|
    { Label = "okolica (brez Italije)"
      CountriesCodes = [|
          "AUT"; "CZE"; "DEU"; "HRV"; "HUN"; "SWZ"
      |]
    }
    { Label = "kritične države (EU)"
      CountriesCodes = [| "BEL"; "ESP"; "GBR"; "ITA"; "SWE" |]
    }
    { Label = "Nordijske države"
      CountriesCodes = [| "DNK"; "FIN"; "ISL"; "NOR"; "SWE" |]
    }
    { Label = "Ex-Jugoslavija"
      CountriesCodes = [| "BIH"; "HRV"; "MKD"; "MNE"; "RKS"; "SRB" |]
    }
|]

type Msg =
    | DataRequested
    | DataLoaded of Data.OurWorldInData.OurWorldInDataRemoteData
    | ChangeCountriesSelection of CountriesDisplaySet

[<Literal>]
let DaysOfMovingAverage = 5

let init: ChartState * Cmd<Msg> =
    let state = {
        OwidDataState = NotLoaded
        DisplayedCountriesSet = countriesDisplaySets.[0]
    }
    state, Cmd.ofMsg DataRequested

let update (msg: Msg) (state: ChartState) : ChartState * Cmd<Msg> =
    let getCountriesCodes selectedSet =
        "SVN" ::
        (selectedSet.CountriesCodes |> Array.toList)

    match msg with
    | ChangeCountriesSelection selectedSet ->
        let countriesCodes = getCountriesCodes selectedSet

        let newOwidDataState =
            match state.OwidDataState with
            | NotLoaded -> PreviousAndLoadingNew Loading
            | PreviousAndLoadingNew oldOwidData ->
                PreviousAndLoadingNew oldOwidData
            | Current oldOwidData -> PreviousAndLoadingNew oldOwidData

        {
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

        match remoteData with
        | NotAsked ->
            printfn "Not asked"
        | Loading ->
            printfn "Loading"
        | Failure error ->
            printfn "Error: %s" error
        | Success data ->
            printfn "Success %A" data

        { state with OwidDataState = Current remoteData }, Cmd.none

let renderChartCode (chartData: ChartData) =
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
                    countrySeries.Data
                    |> Array.map (fun ((dayIndex, _), value) ->
                        (dayIndex, value))
                marker = pojo {| enabled = false |}
                |}
            )
        // we need to reverse the array, for some reason
        |> Array.rev

    let legend =
        {|
            enabled = true
            title = ""
            align = "left"
            verticalAlign = "top"
            borderColor = "#ddd"
            borderWidth = 1
            //labelFormatter = string //fun series -> series.name
            layout = "vertical"
            floating = true
            x = 20
            y = 30
            backgroundColor = "rgba(255,255,255,0.5)"
            reversed = true
        |}

    let baseOptions = basicChartOptions Linear "covid19-metrics-comparison"
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
                   ``type`` = "int"
                   allowDecimals = false
                   title = pojo {| text = chartData.XAxisTitle |}
            |}
        yAxis =
            pojo {|
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
        legend = pojo {| legend with enabled = true |}
        tooltip = pojo {| formatter = fun () -> legendFormatter jsThis |}
    |}


let renderChartContainer chartData =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [ renderChartCode chartData |> chart ]
    ]

let renderCountriesSetsSelectors (activeSet: CountriesDisplaySet) dispatch =
    let renderCountriesSetSelector (setToRender: CountriesDisplaySet) =
        let active = setToRender = activeSet
        Html.div [
            prop.text setToRender.Label
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

let render state dispatch =
    let chartData =
        state
        |> prepareChartData FirstDeath DaysOfMovingAverage

    match chartData with
    | Some chartData ->
        Html.div [
            renderChartContainer chartData
            renderCountriesSetsSelectors
                state.DisplayedCountriesSet
                (ChangeCountriesSelection >> dispatch)

            Html.div [
                prop.className "disclaimer"
            ]
        ]
    | None -> Html.div []

let renderChart() =
    React.elmishComponent
        ("CountriesChartViz/CountriesChart", init, update, render)
