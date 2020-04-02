[<RequireQualifiedAccess>]
module Map

open Elmish

open Feliz
open Feliz.ElmishComponents

open GoogleCharts

open Types

let colors =
    [ "#ffa600"
      "#dba51d"
      "#afa53f"
      "#70a471"
      "#159ab0"
      "#128ea5"
      "#10829a"
      "#0d768f"
      "#0a6b85"
      "#085f7a"
      "#055470"
      "#024a66" ]
    //   "#003f5c" ]

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


let renderChartContainer state =
    Html.div [
        prop.style [ style.height 450 ]
        prop.className "highcharts-wrapper"
        prop.children [
            Chart [
                Props.chartType ChartType.GeoChart
                Props.width "100%"
                Props.height 450
                Props.data [
                    ("Country", "Label", "Value")
                    ("SI-006", "Bovec", 0)
                    ("SI-061", "Ljubljana", 100)
                    ("SI-013", "Cerknica", 40)
                ]
                Props.options [
                    Options.Region "SI"
                    Options.Resolution Resolution.Provinces
                    Options.Legend None
                ]
            ]
        ]
    ]

let render (state : State) dispatch =
    Html.div [
        renderChartContainer state
    ]

let mapChart (props : {| data : RegionsData |}) =
    React.elmishComponent("MapChart", init props.data, update, render)
