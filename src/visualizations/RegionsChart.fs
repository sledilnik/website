
[<RequireQualifiedAccess>]
module RegionsChart

open Elmish

open Feliz
open Feliz.ElmishComponents
open Feliz.Recharts

open Types

let dictionary =
    [ "ce", "Celje"
      "kk", "Krško"
      "kp", "Koper"
      "kr", "Kranj"
      "lj", "Ljubljana"
      "mb", "Maribor"
      "ms", "Murska Sobota"
      "ng", "Nova Gorica"
      "nm", "Novo Mesto"
      "po", "Postojna"
      "sg", "Slovenj Gradec"
      "za", "Zagorje"
      "t", "Tujina" ]
    |> Map.ofList

let excludedRegions = ["regija"]

let dictOrKey key =
    dictionary
    |> Map.tryFind key
    |> Option.defaultValue key

type Metric =
    { Key : string
      Color : string
      Visible : bool
      Label : string }

type State =
    { Data : RegionsData
      Regions : Region list
      Metrics : Metric list }

type Msg =
    | ToggleRegionVisible of string

let init (data : RegionsData) : State * Cmd<Msg> =
    let lastDataPoint = List.last data
    let regions = lastDataPoint.Regions
    let metrics =
        regions
        |> List.filter (fun region -> not (List.contains region.Name excludedRegions))
        |> List.map (fun region ->
            { Key = region.Name
              Color = "hotpink"
              Visible = false
              Label = dictOrKey region.Name } )

    { Data = data ; Regions = regions ; Metrics = metrics }, Cmd.none

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

let formatDate (date : System.DateTime) =
    sprintf "%d.%d." date.Date.Day date.Date.Month

let renderChart (state : State) =

    let renderLineLabel (input: ILabelProperties) =
        Html.text [
            prop.x(input.x)
            prop.y(input.y)
            prop.fill color.black
            prop.textAnchor.middle
            prop.dy(-10)
            prop.fontSize 10
            prop.text input.value
        ]

    let renderRegion (region : Region) (dataKey : RegionsDataPoint -> int) =
        Recharts.line [
            line.name (dictOrKey region.Name)
            line.monotone
            line.stroke "#666"
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let regionsToRender =
        state.Regions
        |> List.filter (fun region ->
            state.Metrics
            |> List.exists (fun metric ->
                metric.Visible = true && region.Name = metric.Key) )

    let children =
        seq {
            yield Recharts.xAxis [ xAxis.dataKey (fun point -> formatDate point.Date) ]
            yield Recharts.yAxis [ ]
            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            for regionToRender in regionsToRender do
                yield renderRegion regionToRender
                    (fun (point : RegionsDataPoint) ->
                        let region =
                            point.Regions
                            |> List.find (fun reg -> reg.Name = regionToRender.Name)
                        region.Cities
                        |> List.map (fun city -> city.PositiveTests)
                        |> List.choose id
                        |> List.sum
                )
        }

    Recharts.lineChart [
        lineChart.data state.Data
        lineChart.children (Seq.toList children)
    ]

let renderChartContainer state =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 500
        responsiveContainer.chart (renderChart state)
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
        prop.text metric.Label ]

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
        renderChartContainer state
        renderMetricsSelectors state.Metrics dispatch
    ]

type Props = {
    data : RegionsData
}

let regionsChart (props : Props) =
    React.elmishComponent("RegionsChart", init props.data, update, render)
