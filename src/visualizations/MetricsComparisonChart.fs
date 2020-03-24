[<RequireQualifiedAccess>]
module MetricsComparisonChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Types
open Recharts

type Metric =
    { Color : string
      Visible : bool
      Label : string }

type Metrics =
    { Tests : Metric
      TotalTests : Metric
      PositiveTests : Metric
      TotalPositiveTests : Metric
      Hospitalized : Metric
      HospitalizedIcu : Metric
      Deaths : Metric
      TotalDeaths : Metric }

type State =
    { Data : StatsData
      Metrics : Metrics }

type MetricMsg =
    | Tests
    | TotalTests
    | PositiveTests
    | TotalPositiveTests
    | Hospitalized
    | HospitalizedIcu
    | Deaths
    | TotalDeaths

type Msg =
    | ToggleMetricVisible of MetricMsg

let init data : State * Cmd<Msg> =
    let state =
        { Data = data
          Metrics =
            { Tests =              { Color = "#ffa600" ; Visible = false ; Label = "Testiranja" }
              TotalTests =         { Color = "#bda535" ; Visible = false ; Label = "Testiranja skupaj" }
              PositiveTests =      { Color = "#7aa469" ; Visible = false ; Label = "Pozitivni testi" }
              TotalPositiveTests = { Color = "#38a39e" ; Visible = true  ; Label = "Pozitivni testi skupaj" }
              Hospitalized =       { Color = "#1494ab" ; Visible = true  ; Label = "Hospitalizirani" }
              HospitalizedIcu =    { Color = "#0d7891" ; Visible = false ; Label = "Intenzivna nega" }
              Deaths =             { Color = "#075b76" ; Visible = false ; Label = "Umrli" }
              TotalDeaths =        { Color = "#003f5c" ; Visible = false ; Label = "Umrli skupaj" } } }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleMetricVisible metric ->
        let newMetrics =
            match metric with
            | Tests -> { state.Metrics with Tests = { state.Metrics.Tests with Visible = not state.Metrics.Tests.Visible } }
            | TotalTests -> { state.Metrics with TotalTests = { state.Metrics.TotalTests with Visible = not state.Metrics.TotalTests.Visible } }
            | PositiveTests -> { state.Metrics with PositiveTests = { state.Metrics.PositiveTests with Visible = not state.Metrics.PositiveTests.Visible } }
            | TotalPositiveTests -> { state.Metrics with TotalPositiveTests = { state.Metrics.TotalPositiveTests with Visible = not state.Metrics.TotalPositiveTests.Visible } }
            | Hospitalized -> { state.Metrics with Hospitalized = { state.Metrics.Hospitalized with Visible = not state.Metrics.Hospitalized.Visible } }
            | HospitalizedIcu -> { state.Metrics with HospitalizedIcu = { state.Metrics.HospitalizedIcu with Visible = not state.Metrics.HospitalizedIcu.Visible } }
            | Deaths -> { state.Metrics with Deaths = { state.Metrics.Deaths with Visible = not state.Metrics.Deaths.Visible } }
            | TotalDeaths -> { state.Metrics with TotalDeaths = { state.Metrics.TotalDeaths with Visible = not state.Metrics.TotalDeaths.Visible } }
        { state with Metrics = newMetrics }, Cmd.none

let formatDate (date : System.DateTime) =
    sprintf "%d.%d." date.Date.Day date.Date.Month

let renderChart (data : StatsData) (metrics : Metrics) =

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

    let renderMetric (metric : Metric) (dataKey : StatsDataPoint -> int) =
        Recharts.line [
            line.name metric.Label
            line.monotone
            line.animationDuration 1000
            line.stroke metric.Color
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let children =
        seq {
            yield Recharts.xAxis [ xAxis.dataKey (fun point -> formatDate point.Date) ]
            yield Recharts.yAxis [ yAxis.label {| value = "Število testov / Število oseb" ; angle = -90 ; position = "insideLeft" |} ]
            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            if metrics.Tests.Visible then
                yield renderMetric metrics.Tests
                    (fun (point : StatsDataPoint) -> point.Tests |> Option.defaultValue 0)

            if metrics.TotalTests.Visible then
                yield renderMetric metrics.TotalTests
                    (fun (point : StatsDataPoint) -> point.TotalTests |> Option.defaultValue 0)

            if metrics.PositiveTests.Visible then
                yield renderMetric metrics.PositiveTests
                    (fun (point : StatsDataPoint) -> point.PositiveTests |> Option.defaultValue 0)

            if metrics.TotalPositiveTests.Visible then
                yield renderMetric metrics.TotalPositiveTests
                    (fun (point : StatsDataPoint) -> point.TotalPositiveTests |> Option.defaultValue 0)

            if metrics.Hospitalized.Visible then
                yield renderMetric metrics.Hospitalized
                    (fun (point : StatsDataPoint) -> point.Hospitalized |> Option.defaultValue 0)

            if metrics.HospitalizedIcu.Visible then
                yield renderMetric metrics.HospitalizedIcu
                    (fun (point : StatsDataPoint) -> point.HospitalizedIcu |> Option.defaultValue 0)

            if metrics.Deaths.Visible then
                yield renderMetric metrics.Deaths
                    (fun (point : StatsDataPoint) -> point.Deaths |> Option.defaultValue 0)

            if metrics.TotalDeaths.Visible then
                yield renderMetric metrics.TotalDeaths
                    (fun (point : StatsDataPoint) -> point.TotalDeaths |> Option.defaultValue 0)
        }

    Recharts.lineChart [
        lineChart.data data
        lineChart.children (Seq.toList children)
    ]

let renderChartContainer data metrics =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 500
        responsiveContainer.chart (renderChart data metrics)
    ]

let renderMetricSelector (metric : Metric) metricMsg dispatch =
    let style =
        if metric.Visible
        then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleMetricVisible metricMsg |> dispatch)
        prop.className [ true, "btn  btn-sm metric-selector"; metric.Visible, "metric-selector--selected" ]
        prop.style style
        prop.text metric.Label ]

let renderMetricsSelectors metrics dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            renderMetricSelector metrics.Tests Tests dispatch
            renderMetricSelector metrics.TotalTests TotalTests dispatch
            renderMetricSelector metrics.PositiveTests PositiveTests dispatch
            renderMetricSelector metrics.TotalPositiveTests TotalPositiveTests dispatch
            renderMetricSelector metrics.Hospitalized Hospitalized dispatch
            renderMetricSelector metrics.HospitalizedIcu HospitalizedIcu dispatch
            renderMetricSelector metrics.Deaths Deaths dispatch
            renderMetricSelector metrics.TotalDeaths TotalDeaths dispatch ] ]

let render state dispatch =
    Html.div [
        renderChartContainer state.Data state.Metrics
        renderMetricsSelectors state.Metrics dispatch
    ]

type Props = {
    data : StatsData
}

let metricsComparisonChart (props : Props) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
