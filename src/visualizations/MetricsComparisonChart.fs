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
    { ScaleType : ScaleType
      Data : StatsData
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
    | ScaleTypeChanged of ScaleType

let init data : State * Cmd<Msg> =
    let state =
        { ScaleType = Linear
          Data = data
          Metrics =
            { Tests =              { Color = "#ffa600" ; Visible = false ; Label = "Testiranja" }
              TotalTests =         { Color = "#bda535" ; Visible = false ; Label = "Testiranja - skupaj" }
              PositiveTests =      { Color = "#7aa469" ; Visible = false ; Label = "Pozitivni testi" }
              TotalPositiveTests = { Color = "#38a39e" ; Visible = true  ; Label = "Pozitivni testi - skupaj" }
              Hospitalized =       { Color = "#1494ab" ; Visible = true  ; Label = "Hospitalizirani" }
              HospitalizedIcu =    { Color = "#0d7891" ; Visible = false ; Label = "Intenzivna nega" }
              Deaths =             { Color = "#075b76" ; Visible = false ; Label = "Umrli" }
              TotalDeaths =        { Color = "#003f5c" ; Visible = false ; Label = "Umrli - skupaj" } } }
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
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none

let renderChart scaleType (data : StatsData) (metrics : Metrics) =

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

    let renderMetric (metric : Metric) (dataKey : StatsDataPoint -> int option) =
        Recharts.line [
            line.name metric.Label
            line.monotone
            line.isAnimationActive false
            line.stroke metric.Color
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let children =
        seq {
            yield Recharts.xAxis [ xAxis.dataKey (fun point -> Utils.formatChartAxixDate point.Date) ]

            let yAxisPropsDefaut = [ yAxis.label {| value = "Število testiranj / Število oseb" ; angle = -90 ; position = "insideLeft" |} ]
            match scaleType with
            | Log ->
                yield Recharts.yAxis (yAxisPropsDefaut @ [yAxis.scale ScaleType.Log ; yAxis.domain (domain.auto, domain.auto) ])
            | _ ->
                yield Recharts.yAxis yAxisPropsDefaut

            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            if metrics.Tests.Visible then
                yield renderMetric metrics.Tests
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.Tests)

            if metrics.TotalTests.Visible then
                yield renderMetric metrics.TotalTests
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.TotalTests)

            if metrics.PositiveTests.Visible then
                yield renderMetric metrics.PositiveTests
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.PositiveTests)

            if metrics.TotalPositiveTests.Visible then
                yield renderMetric metrics.TotalPositiveTests
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.TotalPositiveTests)

            if metrics.Hospitalized.Visible then
                yield renderMetric metrics.Hospitalized
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.Hospitalized)

            if metrics.HospitalizedIcu.Visible then
                yield renderMetric metrics.HospitalizedIcu
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.HospitalizedIcu)

            if metrics.Deaths.Visible then
                yield renderMetric metrics.Deaths
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.Deaths)

            if metrics.TotalDeaths.Visible then
                yield renderMetric metrics.TotalDeaths
                    (fun (point : StatsDataPoint) -> Utils.zeroToNone point.TotalDeaths)
        }

    Recharts.lineChart [
        lineChart.data data
        lineChart.children (Seq.toList children)
    ]

let renderChartContainer scaleType data metrics =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 500
        responsiveContainer.chart (renderChart scaleType data metrics)
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
        Utils.renderScaleSelector state.ScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state.ScaleType state.Data state.Metrics
        renderMetricsSelectors state.Metrics dispatch
    ]

type Props = {
    data : StatsData
}

let metricsComparisonChart (props : Props) =
    React.elmishComponent("MetricsComparisonChart", init props.data, update, render)
