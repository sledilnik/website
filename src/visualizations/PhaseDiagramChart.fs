[<RequireQualifiedAccess>]
module PhaseDiagramChart

open Browser
open Fable.Core.JsInterop
open Elmish
open Feliz
open Feliz.UseElmish

open Highcharts
open Types

let i18n = I18N.tt "charts.phaseDiagram"

type DiagramKind =
    | TotalVsWeek
    | WeekVsWeekBefore

    with

    member this.Name =
        match this with
        | TotalVsWeek -> i18n "totalVsWeek.name"
        | WeekVsWeekBefore -> i18n "weekVsWeekBefore.name"

    member this.Color =
        match this with
        | TotalVsWeek -> "#dba51d"
        | WeekVsWeekBefore -> "#dba51d"

    static member All = [TotalVsWeek ; WeekVsWeekBefore]

type State = {
    StatsData : StatsData
    DiagramKind : DiagramKind
}

type Msg =
    | DiagramKindSelected of DiagramKind

let init statsData : State * Cmd<Msg> =
    let cmd = Cmd.none
    let state = {
        StatsData = statsData
        DiagramKind = TotalVsWeek
    }
    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DiagramKindSelected diagramKind ->
        { state with DiagramKind = diagramKind }, Cmd.none

let totalVsWeekData statsData =
    let windowSize = 7

    statsData
    |> List.map (fun (dp : StatsDataPoint) ->
        {| Date = dp.Date
           ConfirmedToday = dp.Cases.ConfirmedToday
           ConfirmedToDate = dp.Cases.ConfirmedToDate
        |} )
    |> List.windowed windowSize
    |> List.map (fun window ->
        let last =
            window
            |> List.toArray
            |> Array.last

        match last.ConfirmedToDate with
        | None -> None
        | Some confirmedToDate ->
            let confirmedInWindow =
                window
                |> List.map (fun dp -> dp.ConfirmedToday)
                |> List.choose id
                |> List.sum
            {| Date = last.Date
               ConfirmedInWindow = confirmedInWindow
               ConfirmedToDate = confirmedToDate
            |} |> Some)
    |> List.choose id
    |> List.map (fun dp ->
        pojo {| date = dp.Date
                x = dp.ConfirmedToDate
                y = dp.ConfirmedInWindow |} )
    |> List.toArray

let weekVsWeekBeforeData statsData =
    let windowSize = 7

    statsData
    |> List.map (fun (dp : StatsDataPoint) ->
        {| Date = dp.Date
           ConfirmedToday = dp.Cases.ConfirmedToday
        |} )
    |> List.windowed (windowSize * 2)
    |> List.map (fun doubleWindow ->
        let firstWindow, secondWindow = List.splitAt windowSize doubleWindow
        pojo {| date = (secondWindow |> List.toArray |> Array.last).Date
                x = firstWindow |> List.map (fun dp -> dp.ConfirmedToday |> Option.defaultValue 0) |> List.sum
                y = secondWindow |> List.map (fun dp -> dp.ConfirmedToday |> Option.defaultValue 0) |> List.sum
             |})
    |> List.toArray

let chartOptions state dispatch =
    let data, xAxisTitle, yAxisTitle =
        match state.DiagramKind with
        | TotalVsWeek ->
            (totalVsWeekData state.StatsData, i18n "totalVsWeek.xAxisTitle", i18n "totalVsWeek.yAxisTitle")
        | WeekVsWeekBefore ->
            (weekVsWeekBeforeData state.StatsData, i18n "weekVsWeekBefore.xAxisTitle", i18n "weekVsWeekBefore.yAxisTitle")

    {|
        title = None

        chart = pojo
            {| ``type`` = "scatter" ; animation = false ; zoomType = "xy" |}

        legend = {| |}

        xAxis = pojo
            {| ``type`` = "logarithmic"
               title = pojo {| text = xAxisTitle |}
            |}

        yAxis = pojo
            {| ``type`` = "logarithmic"
               title = pojo {| text = yAxisTitle |}
            |}

        tooltip =  pojo
            {| formatter = fun () ->
                    let date = I18N.tOptions "days.longerDate" {| date = jsThis?point?date |}
                    sprintf "<b>%s</b><br>%s: %d<br>%s: %d"
                        date
                        (xAxisTitle) jsThis?x
                        (yAxisTitle) jsThis?y
            |}

        plotOptions = pojo
            {| series = pojo
                {| lineWidth = 1
                   marker = pojo {| enabled = true ; symbol = "circle" ; radius = 2 |}
                   states = pojo {| hover = pojo {| lineWidth = 1 |} |}
                |}
            |}

        series = [|
            {| data = totalVsWeekData state.StatsData
               visible = state.DiagramKind = TotalVsWeek
               color = TotalVsWeek.Color |}
            {| data = weekVsWeekBeforeData state.StatsData
               visible = state.DiagramKind = WeekVsWeekBefore
               color = WeekVsWeekBefore.Color |}
        |]
    |}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            chartOptions state dispatch
            |> Highcharts.chart
        ]
    ]

let renderDiagramKindSelectors (currentDiagramKind : DiagramKind) dispatch =
    let renderDiagramKindSelector diagramKind =
        Html.div [
            prop.onClick (fun _ -> dispatch (DiagramKindSelected diagramKind))
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (diagramKind = currentDiagramKind, "selected") ]
            prop.text diagramKind.Name
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (DiagramKind.All |> List.map renderDiagramKindSelector)
    ]

let renderChart state dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderDiagramKindSelectors
                state.DiagramKind dispatch
            // Utils.renderScaleSelector
            //     state.ScaleType (ScaleTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch
    ]

let chart =
    React.functionComponent(fun (props : {| data : StatsData |}) ->
        let state, dispatch = React.useElmish(init props.data, update, [| |])
        renderChart state dispatch
    )
