[<RequireQualifiedAccess>]
module SpreadChart

open System
open Elmish
open Fable.Core
open Feliz
open Feliz.ElmishComponents
open Browser

open Types
open Highcharts

// if set to true:
// - SpreadChart will show exponential growth pages
let showExpGrowthFeatures = true

let chartText = I18N.chartText "spread"

type Scale =
    | Absolute
    | Percentage
    | DoublingRate
  with
    static member all = [ Absolute; Percentage; DoublingRate ]
    static member getName = function
        | Absolute      -> chartText "absolute"
        | Percentage    -> chartText "percentage"
        | DoublingRate  -> chartText "doublingRate"

type Page =
    | Chart of Scale
    | Explainer
  with
    static member all = (Scale.all |> List.map Chart) @ [ Explainer ]
    static member getName = function
        | Chart scale   -> Scale.getName scale
        | Explainer     -> chartText "explainer"

type State = {
    Page: Page
    Data: StatsData
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangePage of Page
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        Page = Chart Absolute
        Data = data
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangePage page ->
        { state with Page = page }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let maxOption a b =
    match a, b with
    | None, None -> None
    | Some x, None -> Some x
    | None, Some y -> Some y
    | Some x, Some y -> Some (max x y)

let inline yAxisBase() =
     {|
        ``type`` = "linear"
        opposite = true
        reversed = false
        title = {| text = null |}
        showFirstLabel = not showExpGrowthFeatures // need to hide negative label for addContainmentMeasuresFlags
        tickInterval = None
        gridZIndex = -1
        max = None
        plotLines = [| {| value = 0; color = "black" |} |]
        crosshair = true
    |}

let inline legend title =
    {|
        enabled = showExpGrowthFeatures
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
                legendTitle = chartText "absoluteTitle"
                seriesLabel = chartText "absoluteLabel"
                yAxis = yAxisBase ()
                dataKey = fun dp ->
                    (dp.Date |> jsTime12h),
                    dp.Cases.ConfirmedToday
                    |> Option.map float
                    |> Option.defaultValue nan
            }
        | Percentage ->
            {
                legendTitle = chartText "relativeTitle"
                seriesLabel = chartText "relativeLabel"
                yAxis = {| yAxisBase () with ``type``="logarithmic" |}
                dataKey = fun dp ->
                    let todayCases = dp.Cases.ConfirmedToday |> Utils.zeroToNone
                    let activeCases = dp.Cases.Active |> Utils.zeroToNone
                    let value =
                        (todayCases, activeCases)
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
                legendTitle = chartText "doublingRateTitle"
                seriesLabel = chartText "doublingRateLabel"
                yAxis =
                    {| yAxisBase () with
                        ``type``="logarithmic"
                        reversed=true
                        plotLines=[|
                            //pojo {| value=40.0; label={| text=chartText "averageSouthKorea"; align="right"; y= 12; x= -300 |}; color="#408040"; width=3; dashStyle="longdashdot" |} // rotation=270; align="right"; x=12 |} |}
                            pojo {| value= 1.0; label={| text=chartText "oneDay"  |}; color="#aaa"; dashStyle="ShortDash" |}
                            pojo {| value= 7.0; label={| text=chartText "oneWeek" |}; color="#888"; dashStyle="ShortDash" |}
                            pojo {| value=30.0; label={| text=chartText "oneMonth"|}; color="#888"; dashStyle="ShortDash" |}
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
                    state.Data
                    |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                    |> Seq.map chartCfg.dataKey
                    |> Seq.toArray
            |}
        if showExpGrowthFeatures then
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
    // calculate the current doubling time

    // get the array of active cases by days
    let activeByDays =
        data
        |> List.map(fun dp -> (dp.Date, dp.Cases.Active))

    let currentDoublingTime = activeByDays |> Utils.findDoublingTime

    let multiplicationForDaysFromNow daysFromNow =
        match currentDoublingTime with
        | Some doublingTime -> Math.Pow(2., daysFromNow / doublingTime) |> Some
        | None -> None

    let curActive, curHospitalized =
        data
        |> List.rev
        |> List.choose (fun dp ->
            match dp.Cases.Active, dp.StatePerTreatment.InHospital with
            | Some p, Some h -> Some (p, h)
            | _, _ -> None)
        |> List.take 1
        |> List.head
        |> fun (p, h) -> (p,h)

    let box (title: string) weekFromNow =
        let multiplication =
            weekFromNow * 7
            |> float
            |> multiplicationForDaysFromNow

        match multiplication with
        | Some multiplication ->
            Html.div [
                prop.className "box"
                prop.children [
                    Html.h3 title
                    Html.p [
                        match weekFromNow with
                        | 0 -> ""
                        | _ ->
                            let timesAsMany = multiplication - 1.
                            let timesAsManyRounded =
                                timesAsMany
                                |> Utils.formatTo1DecimalWithTrailingZero
                            let timesAsManyText = chartText "timesAsMany"
                            sprintf "%s%s" timesAsManyRounded timesAsManyText
                        |> Html.span
                    ]

                    let activeCasesProjection =
                        (float curActive * multiplication)
                        |> Math.Round
                        |> int

                    let hospitalizedCasesProjection =
                        (float curHospitalized * multiplication)
                        |> Math.Round
                        |> int

                    Html.div [ Html.h4 (string activeCasesProjection)
                               Html.p (chartText "activeCases") ]
                    Html.div [ Html.h4 (string hospitalizedCasesProjection)
                               Html.p (chartText "hospitalized") ]
                ]
            ]
        | None -> Html.div []

    Html.div [
        prop.className "exponential-explainer"
        prop.style [ (Interop.mkStyle "width" "100%"); style.position.absolute ]
        prop.children [
            let explanationTextFormat = chartText "ifExpGrowth"
            let explanationText =
                String.Format(explanationTextFormat, currentDoublingTime)
            yield Html.h1 explanationText
            yield Html.div [
                prop.className "container"
                prop.children [
                    yield!
                        [ chartText "today", 0
                          chartText "inOneWeek", 1
                          chartText "inTwoWeeks", 2
                          chartText "inThreeWeeks", 3
                          chartText "inFourWeeks", 4 ]
                        |> List.map (fun (title, weekFromNow) ->
                            box title weekFromNow)
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
            |> chartFromWindow
        ]
    ]

let renderScaleSelectors state dispatch =

    let renderScaleSelector (page: Page) dispatch =
        let isActive = state.Page = page
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
        prop.children [
            Html.div [
                prop.style [
                    style.height 480
                    (Interop.mkStyle "width" "100%")
                    style.position.relative
                ]
                prop.children [
                    match state.Page with
                    | Chart scale ->
                        yield renderChartContainer scale state dispatch
                    | Explainer ->
                        yield renderChartContainer DoublingRate state dispatch
                        yield renderExplainer state.Data
                ]
            ]

            if showExpGrowthFeatures then
                renderScaleSelectors state dispatch
        ]
    ]

let spreadChart (props : {| data : StatsData |}) =
    React.elmishComponent("SpreadChart", init props.data, update, render)
