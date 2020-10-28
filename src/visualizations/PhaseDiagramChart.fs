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

type Metric =
    | Cases
    | Deceased

    with

    member this.Name =
        match this with
        | Cases -> i18n "cases"
        | Deceased -> i18n "deceased"

let (|CasesMetric|DeceasedMetric|UnknownMetric|) str =
    if str = Metric.Cases.ToString()
    then CasesMetric
    elif str = Metric.Deceased.ToString()
    then DeceasedMetric
    else UnknownMetric

type State = {
    StatsData : StatsData
    DiagramKind : DiagramKind
    Metric : Metric
}

type Msg =
    | DiagramKindSelected of DiagramKind
    | MetricSelected of string

let init statsData : State * Cmd<Msg> =
    let cmd = Cmd.none
    let state = {
        StatsData = statsData
        DiagramKind = TotalVsWeek
        Metric = Cases
    }
    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DiagramKindSelected diagramKind ->
        { state with DiagramKind = diagramKind }, Cmd.none
    | MetricSelected metric ->
        match metric with
        | CasesMetric ->
            { state with Metric = Cases }, Cmd.none
        | DeceasedMetric ->
            { state with Metric = Deceased }, Cmd.none
        | UnknownMetric ->
            state, Cmd.none

let totalVsWeekData metric statsData =
    let windowSize = 7

    statsData
    |> List.map (fun (dp : StatsDataPoint) ->
        match metric with
        | Cases ->
            {| Date = dp.Date
               Count = dp.Cases.ConfirmedToday
               CountToDate = dp.Cases.ConfirmedToDate
            |}
        | Deceased ->
            {| Date = dp.Date
               Count = dp.StatePerTreatment.Deceased
               CountToDate = dp.StatePerTreatment.DeceasedToDate
            |}
        )
    |> List.windowed windowSize
    |> List.map (fun window ->
        let last =
            window
            |> List.toArray
            |> Array.last

        match last.CountToDate with
        | None -> None
        | Some countToDate ->
            let countInWindow =
                window
                |> List.map (fun dp -> dp.Count)
                |> List.choose id
                |> List.sum
            {| Date = last.Date
               CountInWindow = countInWindow
               CountToDate = countToDate
            |} |> Some)
    |> List.choose id
    |> List.map (fun dp ->
        pojo {| date = dp.Date
                x = dp.CountToDate
                y = dp.CountInWindow |} )
    |> List.toArray

let weekVsWeekBeforeData metric statsData =
    let windowSize = 7

    statsData
    |> List.map (fun (dp : StatsDataPoint) ->
        {| Date = dp.Date
           Count =
            match metric with
            | Cases -> dp.Cases.ConfirmedToday
            | Deceased -> dp.StatePerTreatment.Deceased
        |} )
    |> List.filter (fun dp -> dp.Count |> Option.isSome)
    |> List.windowed (windowSize * 2)
    |> List.map (fun doubleWindow ->
        let firstWindow, secondWindow = List.splitAt windowSize doubleWindow
        pojo {| date = (List.head secondWindow).Date
                y = secondWindow |> List.map (fun dp -> dp.Count |> Option.defaultValue 0) |> List.sum
                x = firstWindow |> List.map (fun dp -> dp.Count |> Option.defaultValue 0) |> List.sum
             |})
    |> List.toArray

let chartOptions state =
    let xAxisTitle, yAxisTitle =
        match state.DiagramKind with
        | TotalVsWeek ->
            (i18n "totalVsWeek.xAxisTitle", i18n "totalVsWeek.yAxisTitle")
        | WeekVsWeekBefore ->
            (i18n "weekVsWeekBefore.yAxisTitle", i18n "weekVsWeekBefore.xAxisTitle")

    {|
        title = None

        chart = pojo
            {| ``type`` = "scatter" ; animation = false ; zoomType = "x" |}

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

        credits = Highcharts.credictsOptions

        series = [|
            {| data = totalVsWeekData state.Metric state.StatsData
               visible = state.DiagramKind = TotalVsWeek
               color = TotalVsWeek.Color |}
            {| data = weekVsWeekBeforeData state.Metric state.StatsData
               visible = state.DiagramKind = WeekVsWeekBefore
               color = WeekVsWeekBefore.Color |}
        |]
    |}

let renderMetricSelector (selected : Metric) dispatch =
    let options = seq {
        yield Html.option [
            prop.text Metric.Cases.Name
            prop.value (Metric.Cases.ToString())
        ]
        yield Html.option [
            prop.text Metric.Deceased.Name
            prop.value (Metric.Deceased.ToString())
        ]
    }

    Html.select [
        prop.value (selected.ToString())
        prop.className "form-control form-control-sm filters__type"
        prop.children options
        prop.onChange (MetricSelected >> dispatch)
    ]

let renderDiagramKindSelectors (selected : DiagramKind) dispatch =
    let renderDiagramKindSelector diagramKind =
        Html.div [
            prop.onClick (fun _ -> dispatch (DiagramKindSelected diagramKind))
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (diagramKind = selected, "selected") ]
            prop.text diagramKind.Name
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (DiagramKind.All |> List.map renderDiagramKindSelector)
    ]

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            chartOptions state
            |> Highcharts.chart
        ]
    ]

let renderChart state dispatch =
    Html.div [
        prop.children [
            Utils.renderChartTopControls [
                Html.div [
                    prop.className "filters"
                    prop.children [
                        renderMetricSelector state.Metric dispatch
                    ]
                ]
                renderDiagramKindSelectors state.DiagramKind dispatch
            ]
            renderChartContainer state dispatch
        ]
    ]

let chart =
    React.functionComponent(fun (props : {| data : StatsData |}) ->
        let state, dispatch = React.useElmish(init props.data, update, [| |])
        renderChart state dispatch
    )
