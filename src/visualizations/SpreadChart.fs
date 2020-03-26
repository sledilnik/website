[<RequireQualifiedAccess>]
module SpreadChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Types
open Recharts
open System

type Scale =
    | Absolute
    | Percentage
    | DoublingRate
  with
    static member all = [ Absolute; Percentage; DoublingRate ]
    static member getName = function
        | Absolute -> "Absoluten dnevni prirast"
        | Percentage -> "Relativen dnevni prirast"
        | DoublingRate -> "Število dni do podvojitve"

type State = {
    scale: Scale
    data: StatsData
}

type Msg =
    | ScaleTypeChanged of Scale

let init data : State * Cmd<Msg> =
    let state = {
        scale = Absolute
        data = data
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ScaleTypeChanged scaleType ->
        { state with scale = scaleType }, Cmd.none

let maxOption a b =
    match a, b with
    | None, None -> None
    | Some x, None -> Some x
    | None, Some y -> Some y
    | Some x, Some y -> Some (max x y)

let roundDecimals (nDecimals:int) (f: float) = Math.Round(f,nDecimals)

type ChartCfg = {
    seriesLabel: string
    yAxisName: string
    yAxisProps: IReactProperty list
    dataKey: StatsDataPoint -> float
  } with
    static member fromScale = function
        | Absolute ->
            {
                seriesLabel = "Dnevni prirast potrjeno okuženih"
                yAxisName = "Število novo-okuženih oseb"
                yAxisProps = [ yAxis.domain (domain.auto, domain.auto) ]
                dataKey = fun dp -> maxOption dp.PositiveTests dp.TestsAt14.Positive |> Option.map float |> Option.defaultValue nan
            }
        | Percentage ->
            {
                seriesLabel = "Dnevni prirast potrjeno okuženih v %"
                yAxisName = "Dnevni prirast okuženih v %"
                yAxisProps = [ yAxis.domain (domain.auto, domain.auto) ]
                dataKey = fun dp ->
                    let daily = maxOption dp.PositiveTests dp.TestsAt14.Positive |> Utils.zeroToNone
                    let total = maxOption dp.TotalPositiveTests dp.TestsAt14.PositiveToDate |> Utils.zeroToNone
                    (daily, total)
                    ||> Option.map2 (fun daily total ->
                        let yesterday = total-daily
                        if yesterday < 2 then nan
                        else (float total / float yesterday - 1.0) * 100.0 |> roundDecimals 1
                    )
                    |> Option.defaultValue nan
            }

        | DoublingRate ->
            {
                seriesLabel = "Doba podvojitve števila okuženih v dnevih"
                yAxisName = "Čas podvojitve števila okuženih v dnevih"
                yAxisProps = [
                    yAxis.domain (domain.auto, domain.auto)
                    yAxis.padding (16,0,16,0)
                    yAxis.tickCount 10
                    yAxis.reversed;
                    yAxis.scale ScaleType.Log;
                ]
                dataKey = fun dp ->
                    let daily = maxOption dp.PositiveTests dp.TestsAt14.Positive |> Utils.zeroToNone
                    let total = maxOption dp.TotalPositiveTests dp.TestsAt14.PositiveToDate |> Utils.zeroToNone
                    (daily, total)
                    ||> Option.map2 (fun daily total ->
                        let yesterday = total-daily
                        let v =
                            if yesterday < 2 then nan
                            else
                                let rate = float total / float yesterday
                                let days = Math.Log 2.0 / Math.Log rate
                                days |> roundDecimals 1

                        printfn "val: %f" v
                        v
                    )
                    |> Option.defaultValue nan
            }

        // yAxis.scale ScaleType.Log ; yAxis.domain (domain.auto, domain.auto); yAxis.padding (16,0,0,0)


let renderChart scaleType (data : StatsData) =

    let chartCfg = ChartCfg.fromScale scaleType

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

    let renderMetric chartCfg =
        Recharts.line [
            line.name chartCfg.seriesLabel
            line.monotone
            line.isAnimationActive false
            line.stroke "#804040"
            line.label renderLineLabel
            line.dataKey chartCfg.dataKey
        ]

    let children =
        seq {
            yield Recharts.xAxis [ xAxis.dataKey (fun point -> Utils.formatChartAxixDate point.Date); xAxis.padding (0,10,0,0) ]

            yield Recharts.yAxis <| yAxis.label {| value = chartCfg.yAxisName ; angle = -90 ; position = "insideLeft" |} :: chartCfg.yAxisProps

            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            yield renderMetric chartCfg
        }

    Recharts.lineChart [
        lineChart.data data
        lineChart.children (Seq.toList children)
    ]

let renderChartContainer scaleType data =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 500
        responsiveContainer.chart (renderChart scaleType data)
    ]


let renderScaleSelectors state dispatch =

    let renderScaleSelector (scale: Scale) dispatch =
        let isActive = state.scale = scale
        let style =
            if isActive
            then [ style.backgroundColor "#808080" ]
            else [ ]
        Html.div [
            prop.onClick (fun _ -> ScaleTypeChanged scale |> dispatch)
            prop.className [ true, "btn  btn-sm metric-selector"; isActive, "metric-selector--selected" ]
            prop.style style
            prop.text (scale |> Scale.getName) ]

    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            for scale in Scale.all do
                yield renderScaleSelector scale dispatch
        ]
    ]

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state.scale state.data
        renderScaleSelectors state dispatch
    ]

type Props = {
    data : StatsData
}

let spreadChart (props : Props) =
    React.elmishComponent("SpreadChart", init props.data, update, render)
