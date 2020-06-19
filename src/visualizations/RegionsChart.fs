[<RequireQualifiedAccess>]
module RegionsChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Highcharts
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
    | ScaleTypeChanged of ScaleType

let regionTotal (region : Region) : int =
    region.Municipalities
    |> List.map (fun city -> city.ConfirmedToDate)
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
            { Key = region.Name
              Color = color
              Visible = i <= 2 } ) colors

    { ScaleType = Linear ; Data = data ; Regions = regions ; Metrics = metrics }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none

let renderChartOptions (state : State) =

    let renderRegion metricToRender (point : RegionsDataPoint) =
        let ts = point.Date |> jsTime12h
        let region =
            point.Regions
            |> List.find (fun reg -> reg.Name = metricToRender.Key)
        let count =
            region.Municipalities
            |> Seq.sumBy (fun city -> city.ConfirmedToDate |> Option.defaultValue 0)
        ts,count

    let allSeries =
        state.Metrics
        |> List.map (fun metric ->
            let renderPoint = renderRegion metric
            {|
                visible = true
                color = metric.Color
                name = I18N.tt "region" metric.Key
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
                ``type`` = "line"
                zoomType = "x"
                styledMode = false // <- set this to 'true' for CSS styling
            |}
        series = allSeries
        yAxis = baseOptions.yAxis |> Array.map (fun yAxis -> {| yAxis with min = None |})
        legend = pojo {| enabled = true ; layout = "horizontal" |}
    |}

let renderChartContainer state =
    Html.div [
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state
            |> Highcharts.chartFromWindow
        ]
    ]

let render (state : State) dispatch =
    Html.div [
        renderChartContainer state
    ]

let regionsChart (props : {| data : RegionsData |}) =
    React.elmishComponent("RegionsChart", init props.data, update, render)
