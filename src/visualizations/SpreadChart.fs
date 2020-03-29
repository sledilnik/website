[<RequireQualifiedAccess>]
module SpreadChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Types
open Highcharts

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

let inline yAxisBase () =
     {|
        //index = 0
        ``type`` = "linear"
        //min = if scaleType=Linear then None else Some 1.0
        //floor = if scaleType=Linear then None else Some 1.0
        opposite = true // right side
        reversed = false
        title = {| text = null |} // "oseb" |}
        //showFirstLabel = false
        tickInterval = None
        gridZIndex = -1
        max = None
        plotLines = [||]
    |}

let inline legend title =
    {|
        enabled = Some true
        title = {| text=title |}
        align = "left"
        verticalAlign = "middle"
        borderColor = "#ddd"
        borderWidth = 1
        //labelFormatter = string //fun series -> series.name
        layout = "vertical"
        floating = true
        x = 20
        y = 30
        backgroundColor = "#FFF"
    |}
    |> pojo

type ChartCfg = {
    seriesLabel: string
    legendTitle: string
    yAxis: obj
    dataKey: StatsDataPoint -> float*float
  } with
    static member fromScale = function
        | Absolute ->
            {
                legendTitle = "Dnevni prirast okuženih"
                seriesLabel = "Pozitivni testi dnevno"
                yAxis = yAxisBase ()
                dataKey = fun dp -> (dp.Date |> jsTime), maxOption dp.PositiveTests dp.TestsAt14.Positive |> Option.map float |> Option.defaultValue nan
            }
        | Percentage ->
            {
                legendTitle = "Dnevni prirast okuženih"
                seriesLabel = "Relativna rast v %"
                yAxis = {| yAxisBase () with ``type``="logarithmic" |}
                dataKey = fun dp ->
                    let daily = maxOption dp.PositiveTests dp.TestsAt14.Positive |> Utils.zeroToNone
                    let total = maxOption dp.TotalPositiveTests dp.TestsAt14.PositiveToDate |> Utils.zeroToNone
                    let value =
                        (daily, total)
                        ||> Option.map2 (fun daily total ->
                            let yesterday = total-daily
                            if yesterday < 2 then nan
                            else (float total / float yesterday - 1.0) * 100.0 |> roundDecimals 1
                        )
                        |> Option.defaultValue nan
                    dp.Date |> jsTime, value
            }

        | DoublingRate ->
            {
                legendTitle = "Doba podvojitve števila okuženih"
                seriesLabel = "V koliko dneh se število okuženih podvoji"
                yAxis =
                    {| yAxisBase () with
                        ``type``="logarithmic"
                        reversed=true
                        plotLines=[|
                            {| value=40.0; label={| text="Povprečje Južne Koreje v preteklih dneh"; align="right"; y= -8; x= -10 |}; color="#408040"; width=3; dashStyle="longdashdot" |} // rotation=270; align="right"; x=12 |} |}
                        |]
                        max = Some 50
                    |}

                dataKey = fun dp ->
                    let daily = maxOption dp.PositiveTests dp.TestsAt14.Positive |> Utils.zeroToNone
                    let total = maxOption dp.TotalPositiveTests dp.TestsAt14.PositiveToDate |> Utils.zeroToNone
                    let value =
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
                    dp.Date |> jsTime, value
            }

        // yAxis.scale ScaleType.Log ; yAxis.domain (domain.auto, domain.auto); yAxis.padding (16,0,0,0)


let renderChartOptions scaleType (data : StatsData) =

    let chartCfg = ChartCfg.fromScale scaleType

    let allSeries =
        {|
            //visible = true
            color = "#be7a2a"
            name = chartCfg.seriesLabel
            data = data |> Seq.map chartCfg.dataKey |> Seq.toArray
            //yAxis = 0 // axis index
            //showInLegend = true
            //fillOpacity = 0
        |}
        |> pojo

    // return highcharts options
    {| basicChartOptions Linear with series = [| allSeries |]; yAxis=chartCfg.yAxis; legend=legend chartCfg.legendTitle |}


let renderChartContainer scaleType data =
    Html.div [
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions scaleType data
            |> Highcharts.chart
        ]
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
