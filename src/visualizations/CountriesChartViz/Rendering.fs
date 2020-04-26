module CountriesChartViz.Rendering

open CountriesChartViz.Analysis
open CountriesChartViz.Synthesis
open System
open Browser
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

open Highcharts
open Types

type Msg =
    | DataRequested
    | DataLoaded of Data.OurWorldInData.Data
    | ChangeCountriesSelection of CountriesSelection

[<Literal>]
let DaysOfMovingAverage = 5

let init data : ChartState * Cmd<Msg> =
    let state = {
        OurWorldInData = NotAsked
        Data = parseCountriesCsv DaysOfMovingAverage
        DisplayedCountries = Scandinavia
    }
    state, Cmd.ofMsg DataRequested

let update (msg: Msg) (state: ChartState) : ChartState * Cmd<Msg> =
    match msg with
    | ChangeCountriesSelection countries ->
        { state with DisplayedCountries=countries }, Cmd.none
    | DataRequested ->
        let countries = ["Slovenia" ; "Italy" ; "Norway"]
        { state with OurWorldInData = Loading },
        Cmd.OfAsync.result (Data.OurWorldInData.load countries DataLoaded)
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

        { state with OurWorldInData = remoteData }, Cmd.none

let renderChartCode (state: ChartState) (chartData: ChartData) =
    let myLoadEvent(name: String) =
        let ret(event: Event) =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let allSeries =
        chartData
        |> Array.map (fun countrySeries ->
             pojo
                {|
                visible = true
                color = countrySeries.Color
                name = countrySeries.CountryAbbr
                data =
                    countrySeries.Data
                    |> Array.map (fun (day, value) ->
                        ((day |> jsTime12h), value))
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
                ``type`` = "spline"
                zoomType = "x"
                events = {| load = myLoadEvent("countries") |}
            |}
        title = pojo {| text = None |}
        series = allSeries
        xAxis = baseOptions.xAxis |> Array.map (fun ax ->
            {| ax with
                plotBands = [||]
                plotLines = [||]
            |})
        plotOptions = pojo
            {|
                series = pojo {| stacking = ""; |}
            |}
        legend = pojo {| legend with enabled = true |}
        tooltip = pojo {| formatter = fun () -> legendFormatter jsThis |}
    |}


let renderChartContainer state chartData =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartCode state chartData
            |> chart
        ]
    ]

let render state dispatch =
    let chartData = prepareChartData state

    Html.div [
        renderChartContainer state chartData
//        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)

        Html.div [
            prop.className "disclaimer"
        ]
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent
        ("CountriesChartViz/CountriesChart", init props.data, update, render)
