[<RequireQualifiedAccess>]
module PhaseDiagram.Chart

open Browser
open Fable.Core.JsInterop
open Elmish
open Feliz
open Feliz.UseElmish

open Components.Slider
open Highcharts

open Types
open Data

let init statsData : State * Cmd<Msg> =
    let metric = Cases
    let diagramKind = TotalVsWeek
    let displayData = totalVsWeekData metric statsData

    let state = {
        StatsData = statsData
        DisplayData = displayData
        DiagramKind = diagramKind
        Metric = metric
        Day = displayData.Length - 1
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DayChanged day ->
        { state with Day = day }, Cmd.none
    | DiagramKindSelected diagramKind ->
        let displayData = displayData state.Metric diagramKind state.StatsData
        { state with
            DiagramKind = diagramKind
            DisplayData = displayData
            Day = displayData.Length - 1 }, Cmd.none
    | MetricSelected metric ->
        let newDisplayData = displayData metric state.DiagramKind state.StatsData
        { state with
            Metric = metric
            DisplayData = newDisplayData
            Day = newDisplayData.Length - 1 }, Cmd.none

let sharedChartOptions displayData =
    {| title = None
       chart = pojo {| ``type`` = "scatter" ; animation = false ; zoomType = None |}
       legend = {| |}
       credits = Highcharts.credictsOptions |}

let totalVsWeekChartOptions state =
    let sharedOptions = sharedChartOptions()

    let data = state.DisplayData |> Array.map (fun dp -> dp |> pojo)

    {| sharedOptions with
        xAxis = pojo
            {| ``type`` = "logarithmic"
               gridLineWidth = 1
               title = pojo {| text = i18n "totalVsWeek.xAxisTitle" |}
               min = 1
               max = (state.DisplayData |> Array.maxBy (fun dp -> dp.x)).x
            |}

        yAxis = pojo
            {| ``type`` = "logarithmic"
               title = pojo {| text = i18n "totalVsWeek.yAxisTitle" |}
               min = 1
               max = (state.DisplayData |> Array.maxBy (fun dp -> dp.y)).y
               plotLines = [| |]
            |}

        tooltip = pojo
            {| formatter = fun () ->
                    let date = I18N.tOptions "days.longerDate" {| date = jsThis?point?date |}
                    sprintf "<b>%s</b><br>%s: %s<br>%s: %s"
                        date
                        (i18n "totalVsWeek.xAxisLabel") (I18N.NumberFormat.formatNumber(jsThis?x : int))
                        (i18n "totalVsWeek.yAxisLabel") (I18N.NumberFormat.formatNumber(jsThis?y : int)) |}

        series = [|
            {| data = data
               color = state.Metric.GetColor.Light
               marker = pojo {| symbol = "circle" ; radius = 2 |}
               states = pojo {| hover = pojo {| lineWidth = 0 |} |}
            |} |> pojo
            {| data = [| data.[state.Day] |]
               color = state.Metric.GetColor.Dark
               marker = pojo {| symbol = "circle" ; radius = 8 |}
               states = pojo {| hover = pojo {| lineWidth = 0 |} |}
            |} |> pojo
        |]
    |} |> pojo

let weekVsWeekBeforeOptions state =
    let sharedOptions = sharedChartOptions()

    let data = state.DisplayData |> Array.map (fun dp -> dp |> pojo)

    {| sharedOptions with
        xAxis = pojo
            {| ``type`` = "logarithmic"
               gridLineWidth = 1
               title = pojo {| text = i18n "weekVsWeekBefore.xAxisTitle" |}
               min = 1
               max = (state.DisplayData |> Array.maxBy (fun dp -> dp.x)).x
            |}

        yAxis = pojo
            {| ``type`` = "logarithmic"
               title = pojo {| text = i18n "weekVsWeekBefore.yAxisTitle" |}
               max = (state.DisplayData |> Array.maxBy (fun dp -> dp.y)).y
               plotLines = [| {| color = "#e03030" ; value = 100 ; width = 2 ; dashStyle = "LongDash" |} |> pojo |]
            |}

        tooltip = pojo
            {| formatter = fun () ->
                    let date = I18N.tOptions "days.longerDate" {| date = jsThis?point?date |}
                    sprintf "<b>%s</b><br>%s: %d<br>%s: %d %%"
                        date
                        (i18n "weekVsWeekBefore.xAxisLabel") jsThis?x
                        (i18n "weekVsWeekBefore.yAxisLabel") jsThis?y |}

        series = [|
            {| data = data
               color = state.Metric.GetColor.Light
               marker = pojo {| symbol = "circle" ; radius = 3 |}
               states = pojo {| hover = pojo {| lineWidth = 0 |} |}
            |} |> pojo
            {| data = [| data.[state.Day] |]
               color = state.Metric.GetColor.Dark
               marker = pojo {| symbol = "circle" ; radius = 8 |}
               states = pojo {| hover = pojo {| lineWidth = 0 |} |}
            |} |> pojo
        |]
    |} |> pojo

let renderMetricSelector (selected : Metric) dispatch =
    let metrics = Metric.All |> List.map (fun metric -> (metric.ToString(), metric))
    let options = metrics |> List.map (fun (metricString, metric) ->
            Html.option [
                prop.text metric.GetName
                prop.value (metricString)
            ]
        )

    Html.select [
        prop.value (selected.ToString())
        prop.className "form-control form-control-sm filters__type"
        prop.children options
        prop.onChange ( (fun ct -> Map.find ct (metrics |> Map.ofList)) >> MetricSelected >> dispatch)
    ]

let renderDiagramKindSelectors (selected : DiagramKind) dispatch =
    let renderDiagramKindSelector diagramKind =
        Html.div [
            prop.onClick (fun _ -> dispatch (DiagramKindSelected diagramKind))
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (diagramKind = selected, "selected") ]
            prop.text diagramKind.GetName
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (DiagramKind.All |> List.map renderDiagramKindSelector)
    ]

let renderChartContainer state dispatch =
    let chartOptions =
        match state.DiagramKind with
        | TotalVsWeek -> totalVsWeekChartOptions
        | WeekVsWeekBefore -> weekVsWeekBeforeOptions
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            Highcharts.chart (chartOptions state)
        ]
    ]

let renderChart state dispatch =
    let date = state.DisplayData.[state.Day].date

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
            Html.div [
                prop.className "slider"
                prop.children [
                    Html.span [
                        prop.className "date"
                        prop.children [
                            Html.text (sprintf "%02d. %02d. %d" date.Day date.Month date.Year)
                        ]
                    ]
                    Slider [
                        Props.Min 0
                        Props.Max (state.DisplayData.Length - 1)
                        Props.Value state.Day
                        Props.OnChange (fun value -> dispatch (DayChanged value))
                    ] []
                ]
            ]
        ]
    ]

let chart =
    React.functionComponent(fun (props : {| data : StatsData |}) ->
        let state, dispatch = React.useElmish(init props.data, update, [| |])
        renderChart state dispatch
    )
