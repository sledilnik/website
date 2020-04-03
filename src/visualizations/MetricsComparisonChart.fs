[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Highcharts
open Types

type Metric =
    | PerformedTests
    | PerformedTestsToDate
    | ConfirmedCasesToday
    | ConfirmedCasesToDate
    | InHospital
    | InHospitalToDate
    | InICU
    | OutOfHospital
    | OutOfHospitalToDate
    | RecoveredToDate
    | Deceased
    | DeceasedToDate

type MetricCfg = {
    Metric: Metric
    Color : string
    Visible : bool
    Label : string
    Class: string
    DashStyle: DashStyle
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric = PerformedTests;       Color = "#19aebd" ; Visible = false ; Label = "Testiranja (na dan)"; Class="cs-tests"; DashStyle= Solid }
        { Metric = PerformedTestsToDate; Color = "#73ccd5" ; Visible = false ; Label = "Testiranja (skupaj)"; Class="cs-testsToDate"; DashStyle= Dash }
        { Metric = ConfirmedCasesToday;  Color = "#bda506" ; Visible = true  ; Label = "Potrjeno okuženi (na dan)"; Class="cs-positiveTests"; DashStyle= Solid }
        { Metric = ConfirmedCasesToDate; Color = "#d5c768" ; Visible = false ; Label = "Potrjeno okuženi (skupaj)"; Class="cs-positiveTestsToDate"; DashStyle= Dash }
        { Metric = InHospital;           Color = "#be7A2a" ; Visible = true  ; Label = "Hospitalizirani (trenutno)"; Class="cs-inHospital"; DashStyle= Solid }
        { Metric = InHospitalToDate;     Color = "#de9a5a" ; Visible = false ; Label = "Hospitalizirani (skupaj)"; Class="cs-inHospitalToDate"; DashStyle= Dash }
        { Metric = InICU;                Color = "#bf5747" ; Visible = true  ; Label = "V intenzivni enoti (trenutno)"; Class="cs-inHospitalICU"; DashStyle= Solid }
        { Metric = OutOfHospital;        Color = "#20b16d" ; Visible = false ; Label = "Odpuščeni iz bolnišnice (na dan)"; Class="cs-outOfHospital"; DashStyle= Solid }
        { Metric = OutOfHospitalToDate;  Color = "#57c491" ; Visible = false ; Label = "Odpuščeni iz bolnišnice (skupaj)"; Class="cs-outOfHospitalToDate"; DashStyle= Dash }
        { Metric = RecoveredToDate;      Color = "#8cd4b2" ; Visible = true  ; Label = "Ozdraveli (skupaj)"; Class="cs-recoveredToDate"; DashStyle=Dash }
        { Metric = Deceased;             Color = "#000000" ; Visible = false ; Label = "Umrli (na dan)"; Class="cs-deceased"; DashStyle=Solid }
        { Metric = DeceasedToDate;       Color = "#666666" ; Visible = true  ; Label = "Umrli (skupaj)"; Class="cs-deceasedToDate"; DashStyle= Dash }
    ]
    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type State =
    { ScaleType : ScaleType
      Data : StatsData
      Metrics : Metrics }

type Msg =
    | ToggleMetricVisible of Metric
    | ScaleTypeChanged of ScaleType

let init data : State * Cmd<Msg> =
    let state = {
        ScaleType = Linear
        Data = data
        Metrics = Metrics.initial
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleMetricVisible metric ->
        { state with
            Metrics = state.Metrics |> Metrics.update (fun mc -> { mc with Visible = not mc.Visible}) metric
        }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none

let renderChartOptions (scaleType: ScaleType) (data : StatsData) (metrics : Metrics) =

    let maxOption a b =
        match a, b with
        | None, None -> None
        | Some x, None -> Some x
        | None, Some y -> Some y
        | Some x, Some y -> Some (max x y)

    let xAxisPoint (dp: StatsDataPoint) = dp.Date

    let metricDataGenerator mc =
        fun point ->
            match mc.Metric with
            | PerformedTests -> point.PerformedTests |> Utils.zeroToNone
            | PerformedTestsToDate -> point.PerformedTestsToDate |> Utils.zeroToNone
            | ConfirmedCasesToday -> point.Cases.ConfirmedToday |> Utils.zeroToNone
            | ConfirmedCasesToDate -> point.Cases.ConfirmedToDate |> Utils.zeroToNone
            | InHospital -> point.StatePerTreatment.InHospital |> Utils.zeroToNone
            | InHospitalToDate -> point.StatePerTreatment.InHospitalToDate |> Utils.zeroToNone
            | InICU -> point.StatePerTreatment.InICU |> Utils.zeroToNone
            | OutOfHospital -> point.StatePerTreatment.OutOfHospital |> Utils.zeroToNone
            | OutOfHospitalToDate -> point.StatePerTreatment.OutOfHospitalToDate |> Utils.zeroToNone
            | RecoveredToDate -> point.StatePerTreatment.RecoveredToDate |> Utils.zeroToNone
            | Deceased -> point.StatePerTreatment.Deceased |> Utils.zeroToNone
            | DeceasedToDate -> point.StatePerTreatment.DeceasedToDate |> Utils.zeroToNone

    let renderFlags startTime =
        let events = [|
        // day, mo, title,       tooltip text
            4,  3, "1. primer", "Prvi potrjen primer:<br/>turist iz Maroka"
            6,  3, "DSO",       "Prepoved obiskov v domovih starejših občanov,<br/>potrjena okužba zdravnika v Metliki"
            8,  3, "Točke",     "16 vstopnih točk za testiranje"
            10, 3, "Meje",      "Zapora nekaterih mejnih prehodov z Italijo,<br/>poostren nadzor za osebna vozila"
            13, 3, "Vlada",     "Sprejeta nova vlada"
            14, 3, "Prevozi",   "Ukinitev javnih prevozov"
            16, 3, "Šole",      "Zaprtje šol, restavracij"
            20, 3, "Zbiranje",  "Prepoved zbiranja na javnih mestih"
            29, 3, "Trg.",      "Trgovine za upokojence do 10. ure"
            30, 3, "Občine",    "Prepoved gibanja izven meja občin"
        |]
        {|
            ``type`` = "flags"
            shape = "flag"
            showInLegend = false
            color = "#444"
            data =
                events |> Array.choose (fun (d,m,title,text) ->
                    let ts = DateTime(2020,m,d) |> jsTime
                    if ts >= startTime then Some {| x=ts; title=title; text=text |}
                    else None
                )
        |}


    let allSeries = [
        let mutable startTime = DateTime.Today |> jsTime
        for metric in metrics do
            let pointData = metricDataGenerator metric
            yield pojo
                {|
                    visible = metric.Visible
                    color = metric.Color
                    name = metric.Label
                    className = metric.Class
                    DashStyle = metric.DashStyle
                    data =
                        data
                        |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime, pointData dp))
                        |> Seq.skipWhile (fun (ts,value) ->
                            if metric.Visible && value.IsSome then
                                startTime <- min startTime ts
                            value.IsNone)
                        |> Seq.toArray
                    //yAxis = 0 // axis index
                    //showInLegend = true
                    //fillOpacity = 0
                |}
        if scaleType = Linear then
            yield renderFlags startTime |> pojo
    ]

    let baseOptions = basicChartOptions scaleType "covid19-metrics-comparison"
    {| baseOptions with
        series = List.toArray allSeries
        yAxis =
            let showFirstLabel = scaleType <> Linear
            baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel |})
    |}

let renderChartContainer scaleType data metrics =
    Html.div [
        prop.style [ style.height 480 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions scaleType data metrics
            |> Highcharts.chart
        ]
    ]

let renderMetricSelector (metric : MetricCfg) dispatch =
    let style =
        if metric.Visible
        then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleMetricVisible metric.Metric |> dispatch)
        prop.className [ true, "btn  btn-sm metric-selector"; metric.Visible, "metric-selector--selected" ]
        prop.style style
        prop.text metric.Label ]

let renderMetricsSelectors metrics dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            for mc in metrics do
                yield renderMetricSelector mc dispatch
        ]
    ]

let render state dispatch =
    Html.div [
        Utils.renderScaleSelector state.ScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state.ScaleType state.Data state.Metrics
        renderMetricsSelectors state.Metrics dispatch
    ]

let metricsComparisonChart (props : {| data : StatsData |}) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
