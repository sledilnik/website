
[<RequireQualifiedAccess>]
module RegionsChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Types
open Recharts

let dict =
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
      "t",  "Tujina" ]
    |> Map.ofList

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
      "#024a66"
      "#003f5c" ]

let excludedRegions = ["regija"]

let dictOfKey key =
    dict
    |> Map.tryFind key
    |> Option.defaultValue key

type Metric =
    { Key : string
      Color : string
      Visible : bool }

type State =
    { Data : RegionsData
      Regions : Region list
      Metrics : Metric list }

type Msg =
    | ToggleRegionVisible of string

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
        |> List.filter (fun region -> not (List.contains region.Name excludedRegions))
        |> List.mapi2 (fun i color region ->
            let config = dictOfKey region.Name
            { Key = region.Name
              Color = color
              Visible = i <= 2 } ) colors

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

    let renderRegion (metric : Metric) (dataKey : RegionsDataPoint -> int) =
        Recharts.line [
            line.name (dictOfKey metric.Key)
            line.monotone
            line.isAnimationActive false
            line.stroke metric.Color
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let metricsToRender =
        state.Metrics
        |> List.filter (fun metric -> metric.Visible)

    let children =
        seq {
            yield Recharts.xAxis [ xAxis.dataKey (fun point -> formatDate point.Date) ]
            yield Recharts.yAxis [ yAxis.label {| value = "Število pozitivnih testov" ; angle = -90 ; position = "insideLeft" |} ]
            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            for metricToRender in metricsToRender do
                yield renderRegion metricToRender
                    (fun (point : RegionsDataPoint) ->
                        let region =
                            point.Regions
                            |> List.find (fun reg -> reg.Name = metricToRender.Key)
                        region.Municipalities
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
        prop.text (dictOfKey metric.Key) ]

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
