[<RequireQualifiedAccess>]
module InfectionsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Browser

open Highcharts
open Types

type Metric =
    | HospitalStaff
    | RestHomeStaff
    | RestHomeOccupant
    | OtherPeople

type MetricCfg = {
    Metric : Metric
    Color : string
    Label : string
    Line : Highcharts.DashStyle
}

type Metrics = MetricCfg list

module Metrics  =
    let all = [
        { Metric=OtherPeople;       Color="#d5c768"; Line=Solid; Label="Ostale osebe" }
        { Metric=HospitalStaff;     Color="#73ccd5"; Line=Solid; Label="Zaposleni v zdravstvu" }
        { Metric=RestHomeStaff;     Color="#20b16d"; Line=Solid; Label="Zaposleni v domovih za starejše občane" }
        { Metric=RestHomeOccupant;  Color="#bf5747"; Line=Solid; Label="Oskrbovanci domov za starejše občane" }
    ]
    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type Stacking =
    | Normal
    | Percent

type DisplayType = {
    Label: string
    IsRelative: bool
    Stacking: Stacking
    ShowLegend: bool
}

let availableDisplayTypes: DisplayType[] = [|
    { Label = "Skupaj"; IsRelative = false; 
        Stacking = Normal; ShowLegend = true }
    { Label = "Po dnevih"; IsRelative = true; 
        Stacking = Normal; ShowLegend = true }
    { Label = "Relativno"; IsRelative = true; 
        Stacking = Percent; ShowLegend = false }
    { Label = "Skupaj relativno"; IsRelative = false; 
        Stacking = Percent; ShowLegend = false }
|]

type State = {
    DisplayType : DisplayType
    Data : StatsData
}

type Msg =
    | ChangeDisplayType of DisplayType

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        DisplayType = availableDisplayTypes.[0]
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType rt ->
        { state with DisplayType=rt }, Cmd.none

let renderChartOptions displayType (data : StatsData) =

    let maxOption a b =
        match a, b with
        | None, None -> None
        | Some x, None -> Some x
        | None, Some y -> Some y
        | Some x, Some y -> Some (max x y)

    let xAxisPoint (dp: StatsDataPoint) = dp.Date

    let metricDataGenerator mc : (StatsDataPoint -> int option) =
        match mc.Metric with
        | HospitalStaff -> fun pt -> pt.HospitalEmployeePositiveTestsToDate |> Utils.zeroToNone
        | RestHomeStaff -> fun pt -> pt.RestHomeEmployeePositiveTestsToDate |> Utils.zeroToNone
        | RestHomeOccupant -> fun pt -> pt.RestHomeOccupantPositiveTestsToDate |> Utils.zeroToNone
        | OtherPeople -> fun pt -> pt.UnclassifiedPositiveTestsToDate |> Utils.zeroToNone

    let makeRelative (data: (JsTimestamp*int option)[]) =
        let mutable last = 0
        Array.init data.Length (fun i ->
            match data.[i] with
            | ts, None -> ts, None
            | ts, Some current ->
                let result = current - last
                last <- current
                ts, Some result
        )

    let allSeries = [
        for metric in Metrics.all do
            let pointData = metricDataGenerator metric
            yield pojo
                {|
                    visible = true
                    color = metric.Color
                    name = metric.Label
                    dashStyle = metric.Line |> DashStyle.toString
                    data =
                        data
                        |> Seq.map (fun dp -> ((xAxisPoint dp |> jsTime12h), pointData dp))
                        |> Seq.skipWhile (fun (ts,value) -> value.IsNone)
                        |> Seq.toArray
                        |> if displayType.IsRelative then makeRelative else id
                |}
    ]

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

    let myLoadEvent(name: String) = 
        let ret(event: Event) =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let baseOptions = basicChartOptions Linear "covid19-metrics-comparison"
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = "column" // "spline"
                zoomType = "x"
                events = {| load = myLoadEvent("infections") |}
            |}
        title = pojo {| text = None |}
        series = List.toArray allSeries
        xAxis = baseOptions.xAxis |> Array.map (fun ax ->
            {| ax with
                plotBands = shadedWeekendPlotBands
                plotLines = [||]
            |})
        plotOptions = pojo
            {|
                series = pojo 
                    {| 
                        stacking = 
                            match displayType.Stacking with
                            | Normal -> "normal"
                            | Percent -> "percent"
                    |}                        
            |}
        legend = pojo {| legend with enabled = displayType.ShowLegend |}
    |}

let renderChartContainer data metrics =
    Html.div [
        prop.style [ style.height 480 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions data metrics
            |> Highcharts.chart
        ]
    ]

let renderDisplaySelectors activeDisplayType dispatch =
    let renderSelector (displayType : DisplayType) =
        let active = displayType = activeDisplayType
        Html.div [
            prop.text displayType.Label
            prop.className [
                true, "btn btn-sm metric-selector"
                active, "metric-selector--selected selected" ]
            if not active then prop.onClick (fun _ -> dispatch displayType)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        availableDisplayTypes 
        |> Array.map renderSelector
        |> prop.children
    ]


let render state dispatch =
    Html.div [
        renderChartContainer state.DisplayType state.Data
        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)
        Html.div [
            prop.className "disclaimer"
            prop.children [
                Html.span "Prirast okuženih zdravstvenih delavcev ne pomeni, da so bili odkriti točno na ta dan; lahko so bili pozitivni že prej in se je samo podatek o njihovem statusu pridobil naknadno. Postavka Zaposleni v DSO vključuje zdravstvene delavce, sodelavce in zunanjo pomoč (študentje zdravstvenih smeri), zato so dnevni podatki o zdravstvenih delavcih (modri stolpci) ustrezno zmanjšani na račun zaposlenih v DSO. To pomeni, da je število zdravstvenih delavcev zelo konzervativna ocena."
            ]
        ]
    ]

let infectionsChart (props : {| data : StatsData |}) =
    React.elmishComponent("InfectionsChart", init props.data, update, render)
