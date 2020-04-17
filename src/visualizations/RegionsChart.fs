[<RequireQualifiedAccess>]
module RegionsChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Highcharts
open Types
open System
open Browser
open Types

let colors =
    [ "#ffa600"
      "#dba51d"
      "#afa53f"
      "#777c29"
      "#70a471"
      "#457844"
      "#f95d6a"
      "#d45087"
      "#a05195"
      "#665191"
      "#10829a"
      "#024a66" ]

let excludedRegions = Set.ofList ["t"]

let getRegionName key =
    match Utils.Dictionaries.regions.TryFind key with
    | None -> key
    | Some region -> region.Name

type Metric =
    { Key : string
      Color : string
      Visible : bool }

type State =
    { ScaleType : ScaleType
      Data : RegionsData
      Regions : Region list
      Metrics : Metric list }

type Msg =
    | ToggleRegionVisible of string
    | ScaleTypeChanged of ScaleType

let regionTotal (region : Region) : int =
    region.Municipalities
    |> List.map (fun city -> city.PositiveTests)
    |> List.choose id
    |> List.sum

let init (data : RegionsData) : State * Cmd<Msg> =
    let lastDataPoint = List.last data
    let regions =
        lastDataPoint.Regions
        |> List.sortByDescending (fun region -> regionTotal region)
    let metrics =
        regions
        |> List.filter (fun region -> not (Set.contains region.Name excludedRegions))
        |> List.mapi2 (fun i color region ->
            let config = getRegionName region.Name
            { Key = region.Name
              Color = color
              Visible = i <= 2 } ) colors

    { ScaleType = Linear ; Data = data ; Regions = regions ; Metrics = metrics }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleRegionVisible regionKey ->
        let newMetrics =
            state.Metrics
            |> List.map (fun m ->
                if m.Key = regionKey
                then { m with Visible = not m.Visible }
                else m)
        { state with Metrics = newMetrics }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none

let myLoadEvent(name: String) =
    let ret(event: Event) =
        let evt = document.createEvent("event")
        evt.initEvent("chartLoaded", true, true);
        document.dispatchEvent(evt)
    ret

let renderChartOptions (state : State) =

    let metricsToRender =
        state.Metrics
        |> List.filter (fun metric -> metric.Visible)

    let renderRegion metricToRender (point : RegionsDataPoint) =
        let ts = point.Date |> jsTime12h
        let region =
            point.Regions
            |> List.find (fun reg -> reg.Name = metricToRender.Key)
        let count =
            region.Municipalities
            |> Seq.sumBy (fun city -> city.PositiveTests |> Option.defaultValue 0)
        ts,count

    let allSeries =
        metricsToRender
        |> List.map (fun metric ->
            let renderPoint = renderRegion metric
            {|
                visible = metric.Visible
                color = metric.Color
                name = getRegionName metric.Key
                data = state.Data |> Seq.map renderPoint |> Array.ofSeq
                //yAxis = 0 // axis index
                //showInLegend = true
                //fillOpacity = 0
            |}
            |> pojo
        )
        |> List.toArray

    let baseOptions = Highcharts.basicChartOptions state.ScaleType "covid19-regions"
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = "spline"
                zoomType = "x"
                styledMode = false // <- set this to 'true' for CSS styling
                events = pojo {| load = myLoadEvent("covid19-regions") |}
            |}
        series = allSeries
        yAxis = baseOptions.yAxis |> Array.map (fun yAxis -> {| yAxis with min = None |})
    |}

let renderChartContainer state =
    Html.div [
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state
            |> Highcharts.chart
        ]
    ]

let renderMetricSelector (metric : Metric) dispatch =
    let style =
        if metric.Visible
        then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleRegionVisible metric.Key |> dispatch)
        prop.className [ true, "btn  btn-sm metric-selector"; metric.Visible, "metric-selector--selected" ]
        prop.style style
        prop.text (getRegionName metric.Key) ]

let renderMetricsSelectors metrics dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            metrics
            |> List.map (fun metric ->
                renderMetricSelector metric dispatch
            ) ) ]

let render (state : State) dispatch =
    Html.div [
        Utils.renderScaleSelector state.ScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state
        renderMetricsSelectors state.Metrics dispatch
    ]

let regionsChart (props : {| data : RegionsData |}) =
    React.elmishComponent("RegionsChart", init props.data, update, render)
