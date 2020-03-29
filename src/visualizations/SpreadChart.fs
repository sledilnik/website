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
        | DoublingRate -> "Eksponentna rast v dnevih" // "Število dni do podvojitve"

type Page =
    | Chart of Scale
    | Explainer
  with
    static member all = (Scale.all |> List.map Chart) @ [ Explainer ]
    static member getName = function
        | Chart scale -> Scale.getName scale
        | Explainer -> "Kaj pomeni eksponenta rast"

type State = {
    page: Page
    data: StatsData
}

type Msg =
    | ChangePage of Page

let init data : State * Cmd<Msg> =
    let state = {
        page = Chart Absolute
        data = data
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangePage page ->
        { state with page = page }, Cmd.none

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
        borderColor = "#aaa"
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
                seriesLabel = "Relativen prirast v %"
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
                legendTitle = "Hitrost eksponentne rasti [v dnevih]"
                seriesLabel = "V koliko dneh se število okuženih podvoji"
                yAxis =
                    {| yAxisBase () with
                        ``type``="logarithmic"
                        reversed=true
                        plotLines=[|
                            pojo {| value=40.0; label={| text="⬑ Povprečje Južne Koreje v preteklih dneh"; align="right"; y= 12; x= -10 |}; color="#408040"; width=3; dashStyle="longdashdot" |} // rotation=270; align="right"; x=12 |} |}
                            pojo {| value= 1.0; label={| text="En dan"   |}; color="#aaa"; dashStyle="ShortDash" |}
                            pojo {| value= 7.0; label={| text="En teden" |}; color="#888"; dashStyle="ShortDash" |}
                            pojo {| value=30.0; label={| text="En mesec" |}; color="#888"; dashStyle="ShortDash" |}
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

let renderExplainer (data: StatsData) =
    let curPositive, curHospitalzed, doublingRate =
        data
        |> List.rev
        |> Seq.choose (fun dp ->
            match dp.TotalPositiveTests, dp.Hospitalized with
            | Some p, Some h -> Some (p, h)
            | _, _ -> None)
        |> Seq.take 1
        |> Seq.toList |> List.head
        |> fun (p, h) -> (p,h,7.0)

    let box (title: string) times positive hospitalized =
        Html.div [
            prop.className "box"
            prop.children [
                Html.h2 title
                Html.span (if times<2 then "" else sprintf "%d-krat toliko" times)
                Html.div [ Html.h4 (string positive); Html.p [ Html.text "potrjeno"; Html.br []; Html.text "okuženih"  ]]
                Html.div [ Html.h4 (string hospitalized); Html.p "hospitaliziranih" ]
            ]
        ]

    Html.div [
        prop.className "exponential-explainer"
        prop.style [ style.height 450 ]
        prop.children [
            yield Html.h1 "Ob nezmanjšani eksponentni rasti s podavajanjem na 7 dni lahko pričakujemo"
            yield Html.div [
                prop.className "container"
                prop.children [
                    yield!
                        ["Danes",0; "Čez en teden",1; "Čez dva tedna",2; "Čez tri tedne",3; "Čez štiri tedne",4;]
                        |> List.map (fun (title, doublings) ->
                            box title (1<<<doublings) (curPositive <<< doublings) (curHospitalzed <<< doublings)
                        )
                ]
            ]
        ]
    ]


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

    let renderScaleSelector (page: Page) dispatch =
        let isActive = state.page = page
        let style =
            if isActive
            then [ style.backgroundColor "#808080" ]
            else [ ]
        Html.div [
            prop.onClick (fun _ -> ChangePage page |> dispatch)
            prop.className [ true, "btn  btn-sm metric-selector"; isActive, "metric-selector--selected" ]
            prop.style style
            prop.text (page |> Page.getName) ]

    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            for page in Page.all do
                yield renderScaleSelector page dispatch
        ]
    ]

let render (state: State) dispatch =
    Html.div [
        match state.page with
        | Chart scale -> renderChartContainer scale state.data
        | Explainer -> renderExplainer state.data
        renderScaleSelectors state dispatch
    ]

type Props = {
    data : StatsData
}

let spreadChart (props : Props) =
    React.elmishComponent("SpreadChart", init props.data, update, render)
