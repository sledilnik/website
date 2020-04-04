[<RequireQualifiedAccess>]
module InfectionsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

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
        { Metric=OtherPeople;       Color="#d5c768"; Line=Solid; Label="Ostali državljani" }
        { Metric=HospitalStaff;     Color="#19aebd"; Line=Solid; Label="Zaposleni v zdravstvu" }
        { Metric=RestHomeStaff;     Color="#73ccd5"; Line=Solid; Label="Osebje domov za ostarele" }
        { Metric=RestHomeOccupant;  Color="#bda506"; Line=Solid; Label="Varovanci domov za ostarele" }
    ]
    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type DisplayType =
    | Daily
    | Cummulative
    | Relative


type State = {
    ScaleType : ScaleType
    DisplayType : DisplayType
    Data : StatsData
}

type Msg =
    | ChangeDisplayType of DisplayType
    | ScaleTypeChanged of ScaleType

let init data : State * Cmd<Msg> =
    let state = {
        ScaleType = Linear
        Data = data
        DisplayType = Cummulative
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType rt ->
        { state with DisplayType=rt }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none

let renderChartOptions (scaleType: ScaleType) (displayType) (data : StatsData) =

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
        | OtherPeople -> fun pt ->
            let sum =
                (pt.PositiveTestsToDate |> Option.defaultValue 0)
                - (pt.HospitalEmployeePositiveTestsToDate |> Option.defaultValue 0)
                - (pt.RestHomeEmployeePositiveTestsToDate |> Option.defaultValue 0)
                - (pt.RestHomeOccupantPositiveTestsToDate |> Option.defaultValue 0)
            if sum <= 0 then None else Some sum

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
                    //color = metric.Color
                    name = metric.Label
                    //className = metric.Class
                    dashStyle = metric.Line |> DashStyle.toString
                    data =
                        data
                        |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime, pointData dp))
                        |> Seq.skipWhile (fun (ts,value) -> value.IsNone)
                        |> Seq.toArray
                        |> if displayType<>Cummulative then makeRelative else id
                    //yAxis = 0 // axis index
                    //showInLegend = true
                    //fillOpacity = 0
                |}
    ]

    let baseOptions = basicChartOptions scaleType "covid19-metrics-comparison"
    {| baseOptions with
        chart = pojo
            {|
                //height = "100%"
                //``type`` = "spline"
                ``type`` = "area"
                zoomType = "x"
                //styledMode = false // <- set this to 'true' for CSS styling
            |}
        title = pojo {| text = None |}

        series = List.toArray allSeries
        yAxis =
            let showFirstLabel = scaleType <> Linear
            baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel |})
        plotOptions = pojo
            {|
                series = pojo {| stacking = if displayType=Relative then "percent" else "normal" |}
                area = pojo
                    {|
                        dataLabels = pojo {| enabled = true |}
                        //enableMouseTracking = false
                        //seriesStacking = true
                    |}
            |}
        legend = pojo {| reversed = true |}
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


let renderDisplaySelectors scaleType dispatch =
    let renderSelector (scaleType : DisplayType) (currentScaleType : DisplayType) (label : string) =
        let active = scaleType = currentScaleType
        Html.div [
            prop.text label
            prop.className [
                true, "btn btn-sm metric-selector"
                active, "metric-selector--selected selected" ]
            if not active then prop.onClick (fun _ -> dispatch scaleType)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]


    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            //Html.text "Skala na Y osi: "
            renderSelector Cummulative scaleType "Skupaj"
            renderSelector Daily scaleType "Po dnevih"
            renderSelector Relative scaleType "Relativno"
        ]
    ]


let render state dispatch =
    Html.div [
        //Utils.renderScaleSelector state.ScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state.ScaleType state.DisplayType state.Data
        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)
    ]

let infectionsChart (props : {| data : StatsData |}) =
    React.elmishComponent("InfectionsChart", init props.data, update, render)
