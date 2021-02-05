[<RequireQualifiedAccess>]
module SpreadChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types
open Highcharts

type Scale =
    | Absolute
    | Percentage
    | DoublingRate
  with
    static member All = [ Absolute; Percentage; DoublingRate ]
    static member GetName = function
        | Absolute      -> I18N.t "charts.spread.absolute"
        | Percentage    -> I18N.t "charts.spread.percentage"
        | DoublingRate  -> I18N.t "charts.spread.doublingRate"

type Page =
    | Chart of Scale
    | Explainer
  with
    static member All = (Scale.All |> List.map Chart) @ [ Explainer ]
    static member Default = Chart Absolute
    static member GetName = function
        | Chart scale   -> Scale.GetName scale
        | Explainer     -> I18N.t "charts.spread.explainer"

type State = {
    page: Page
    data: StatsData
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangePage of Page
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        page = Page.Default
        data = data
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangePage page ->
        { state with page = page }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let maxOption a b =
    match a, b with
    | None, None -> None
    | Some x, None -> Some x
    | None, Some y -> Some y
    | Some x, Some y -> Some (max x y)

let inline yAxisBase () =
     {|
        ``type`` = "linear"
        opposite = true
        reversed = false
        title = {| text = null |}
        showFirstLabel = not Highcharts.showExpGrowthFeatures // need to hide negative label for addContainmentMeasuresFlags
        tickInterval = None
        gridZIndex = -1
        max = None
        plotLines = [| {| value = 0; color = "black" |} |]
        crosshair = true
    |}

let inline legend title =
    {|
        enabled = Highcharts.showExpGrowthFeatures
        title = {| text=title |}
        align = "left"
        verticalAlign = "middle"
        borderColor = "#aaa"
        borderWidth = 1
        layout = "vertical"
        floating = true
        x = 8
        y = 60
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
                legendTitle = I18N.t "charts.spread.absoluteTitle"
                seriesLabel = I18N.t "charts.spread.absoluteLabel"
                yAxis = yAxisBase ()
                dataKey = fun dp -> (dp.Date |> jsTime12h), dp.Cases.ConfirmedToday |> Option.map float |> Option.defaultValue nan
            }
        | Percentage ->
            {
                legendTitle = I18N.t "charts.spread.relativeTitle"
                seriesLabel = I18N.t "charts.spread.relativeLabel"
                yAxis = {| yAxisBase () with ``type``="logarithmic" |}
                dataKey = fun dp ->
                    let daily = dp.Cases.ConfirmedToday |> Utils.zeroToNone
                    let total = dp.Cases.Active |> Utils.zeroToNone
                    let value =
                        (daily, total)
                        ||> Option.map2 (fun daily total ->
                            let yesterday = total-daily
                            if yesterday < 2 then nan
                            else
                                (float total / float yesterday - 1.0) * 100.0
                                |> Utils.roundDecimals 1
                        )
                        |> Option.defaultValue nan
                    dp.Date |> jsTime12h, value
            }

        | DoublingRate ->
            {
                legendTitle = I18N.t "charts.spread.doublingRateTitle"
                seriesLabel = I18N.t "charts.spread.doublingRateLabel"
                yAxis =
                    {| yAxisBase () with
                        ``type``="logarithmic"
                        reversed=true
                        plotLines=[|
                            //pojo {| value=40.0; label={| text=I18N.t "charts.spread.averageSouthKorea"; align="right"; y= 12; x= -300 |}; color="#408040"; width=3; dashStyle="longdashdot" |} // rotation=270; align="right"; x=12 |} |}
                            pojo {| value= 1.0; label={| text=I18N.t "charts.spread.oneDay"  |}; color="#aaa"; dashStyle="ShortDash" |}
                            pojo {| value= 7.0; label={| text=I18N.t "charts.spread.oneWeek" |}; color="#888"; dashStyle="ShortDash" |}
                            pojo {| value=30.0; label={| text=I18N.t "charts.spread.oneMonth"|}; color="#888"; dashStyle="ShortDash" |}
                        |]
                        max = Some 100
                    |}

                dataKey = fun dp ->
                    let daily = dp.Cases.ConfirmedToday |> Utils.zeroToNone
                    let total = dp.Cases.Active |> Utils.zeroToNone
                    let value =
                        (daily, total)
                        ||> Option.map2 (fun daily total ->
                            let yesterday = total-daily
                            let v =
                                if yesterday < 2 then nan
                                else
                                    let rate = float total / float yesterday
                                    let days = Math.Log 2.0 / Math.Log rate
                                    days |> Utils.roundDecimals 1
                            v
                        )
                        |> Option.defaultValue nan
                    dp.Date |> jsTime12h, value
            }


let renderChartOptions scaleType state dispatch =

    let chartCfg = ChartCfg.fromScale scaleType
    let startDate = DateTime(2020,3,4)
    let mutable startTime = startDate |> jsTime

    let allSeries = [|
        yield pojo
            {|
                id = "data"
                color = "#bda506"
                name = chartCfg.seriesLabel
                dataLabels = pojo {| enabled = true |}
                data =
                    state.data
                    |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                    |> Seq.map chartCfg.dataKey
                    |> Seq.toArray
            |}
        if Highcharts.showExpGrowthFeatures then
            yield addContainmentMeasuresFlags startTime None |> pojo
    |]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    // return highcharts options
    {| basicChartOptions Linear "covid19-spread"
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
        with
        series = allSeries
        yAxis=chartCfg.yAxis
        legend=legend chartCfg.legendTitle
    |}

let renderExplainer (data: StatsData) =
    let curPositive, curHospitalized =
        data
        |> List.rev
        |> Seq.choose (fun dp ->
            match dp.Cases.Active, dp.StatePerTreatment.InHospital with
            | Some p, Some h -> Some (p, h)
            | _, _ -> None)
        |> Seq.take 1
        |> Seq.toList |> List.head
        |> fun (p, h) -> (p,h)

    let box (title: string) times positive hospitalized =
        Html.div [
            prop.className "box"
            prop.children [
                Html.h3 title
                Html.p [
                    match times with
                    | 0 -> Html.span ""
                    | 1 -> Html.span (sprintf "%d%s" (1<<<times) (I18N.t "charts.spread.timesAsMany"))
                    | _ -> Html.span (sprintf "%d%s" (1<<<times) (I18N.t "charts.spread.timesAsMany"))
                ]
                Html.div [ Html.h4 (string positive); Html.p (I18N.t "charts.spread.activeCases") ]
                Html.div [ Html.h4 (string hospitalized); Html.p (I18N.t "charts.spread.hospitalized") ]
            ]
        ]

    Html.div [
        prop.className "exponential-explainer"
        prop.style [ (Interop.mkStyle "width" "100%"); style.position.absolute ]
        prop.children [
            yield Html.h1 (I18N.t "charts.spread.ifExpGrowth")
            yield Html.div [
                prop.className "container"
                prop.children [
                    yield!
                        [ I18N.t "charts.spread.today", 0
                          I18N.t "charts.spread.inOneWeek", 1
                          I18N.t "charts.spread.inTwoWeeks", 2
                          I18N.t "charts.spread.inThreeWeeks", 3
                          I18N.t "charts.spread.inFourWeeks", 4 ]
                        |> List.map (fun (title, doublings) ->
                            box title doublings (curPositive <<< doublings) (curHospitalized <<< doublings)
                        )
                ]
            ]
        ]
    ]

let renderChartContainer scaleType data dispatch =
    Html.div [
        prop.style [ style.height 480; (Interop.mkStyle "width" "100%"); style.position.absolute  ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions scaleType data dispatch
            |> Highcharts.chartFromWindow
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
            Utils.classes
                [(true, "btn  btn-sm metric-selector")
                 (isActive, "metric-selector--selected")]
            prop.style style
            prop.text (page |> Page.GetName) ]

    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            for page in Page.All do
                yield renderScaleSelector page dispatch
        ]
    ]

let render (state: State) dispatch =
    Html.div [
        prop.children [
            Html.div [
                prop.style [ style.height 480; (Interop.mkStyle "width" "100%"); style.position.relative ]
                prop.children [
                    match state.page with
                    | Chart scale ->
                        yield renderChartContainer scale state dispatch
                    | Explainer ->
                        yield renderChartContainer DoublingRate state dispatch
                        yield renderExplainer state.data
                ]
            ]
            if Highcharts.showExpGrowthFeatures then
                renderScaleSelectors state dispatch
        ]
    ]

let spreadChart (props : {| data : StatsData |}) =
    React.elmishComponent("SpreadChart", init props.data, update, render)
