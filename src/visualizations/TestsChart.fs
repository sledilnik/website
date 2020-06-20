[<RequireQualifiedAccess>]
module TestsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types
open Highcharts

type DisplayType =
    | Total
    | Regular
    | NsApr20
with
    static member all = [ Regular; NsApr20; Total; ]
    static member getName = function
        | Total -> I18N.t "charts.tests.allTesting"
        | Regular -> I18N.t "charts.tests.regularTesting"
        | NsApr20 -> I18N.t "charts.tests.nationalStudyTesting"

type State = {
    data: StatsData
    displayType: DisplayType
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        data = data
        displayType = Regular
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType dt ->
        { state with displayType = dt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions (state : State) dispatch =
    let className = "tests-chart"
    let scaleType = ScaleType.Linear

    let positiveTests (dp: StatsDataPoint) =
        match state.displayType with
        | Total     -> dp.Tests.Positive.Today.Value
        | Regular   -> dp.Tests.Regular.Positive.Today.Value
        | NsApr20   -> dp.Tests.NsApr20.Positive.Today.Value
    let negativeTests (dp: StatsDataPoint) =
        match state.displayType with
        | Total     -> dp.Tests.Performed.Today.Value - dp.Tests.Positive.Today.Value
        | Regular   -> dp.Tests.Regular.Performed.Today.Value - dp.Tests.Regular.Positive.Today.Value
        | NsApr20   -> dp.Tests.NsApr20.Performed.Today.Value - dp.Tests.NsApr20.Positive.Today.Value
    let percentPositive (dp: StatsDataPoint) =
        let positive = positiveTests dp
        let performed = positiveTests dp + negativeTests dp
        Math.Round(float positive / float performed * float 100.0, 2)

    let allYAxis = [|
        {|
            index = 0
            title = {| text = null |}
            labels = pojo {| format = "{value}"; align = "center"; x = -15; reserveSpace = false; |}
            opposite = true
            visible = true
            max = None
        |}
        {|
            index = 1
            title = {| text = null |}
            labels = pojo {| format = "{value}%"; align = "center"; x = 10; reserveSpace = false; |}
            opposite = false
            visible = true
            max = Some 9
        |}
    |]

    let allSeries = [
        yield pojo
            {|
                name = I18N.t "charts.tests.negativeTests"
                ``type`` = "column"
                color = "#19aebd"
                yAxis = 0
                data = state.data |> Seq.filter (fun dp -> dp.Tests.Positive.Today.IsSome )
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, negativeTests dp)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.tests.positiveTests"
                ``type`` = "column"
                color = "#d5c768"
                yAxis = 0
                data = state.data |> Seq.filter (fun dp -> dp.Tests.Positive.Today.IsSome )
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, positiveTests dp)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.tests.shareOfPositive"
                ``type`` = "line"
                color = "#665191"
                yAxis = 1
                data = state.data |> Seq.filter (fun dp -> dp.Tests.Positive.Today.IsSome )
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, percentPositive dp)) |> Seq.toArray
            |}
    ]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        Highcharts.basicChartOptions
            scaleType className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with
        yAxis = allYAxis
        series = List.toArray allSeries
        plotOptions = pojo
            {|
                series = {| stacking = "normal"; crisp = false; borderWidth = 0; pointPadding = 0; groupPadding = 0 |}
            |}

        legend = pojo {| enabled = true ; layout = "horizontal" |}

        responsive = pojo
            {|
                rules =
                    [| {|
                        condition = {| maxWidth = 768 |}
                        chartOptions =
                            {|
                                yAxis = [|
                                    {| labels = {| enabled = false |} |}
                                    {| labels = {| enabled = false |} |}
                                |]
                            |}
                    |} |]
            |}
    |}

let renderChartContainer (state : State) dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> Highcharts.chartFromWindow
        ]
    ]

let renderSelector state (dt: DisplayType) dispatch =
    Html.div [
        let isActive = state.displayType = dt
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        prop.className [ true, "btn btn-sm metric-selector"; isActive, "metric-selector--selected" ]
        prop.text (DisplayType.getName dt) ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.all
            |> List.map (fun dt -> renderSelector state dt dispatch) ) ]

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state dispatch
        renderDisplaySelectors state dispatch
    ]

let testsChart (props : {| data : StatsData |}) =
    React.elmishComponent("TestsChart", init props.data, update, render)
