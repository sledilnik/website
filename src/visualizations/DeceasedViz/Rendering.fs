module DeceasedViz.Rendering

open System
open Data.Patients
open DataVisualization.ChartingTypes
open DeceasedViz.Analysis
open DeceasedViz.Synthesis
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Highcharts
open Types

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | MetricTypeChanged of MetricType
    | ChartTypeChanged of ChartType
    | ChangePage of PageType
    | RangeSelectionChanged of int

let init(statsData : StatsData) : DeceasedVizState * Cmd<Msg> =
    let state = {
        StatsData = statsData
        PatientsData = [||]
        MetricType = MetricType.Default
        ChartType = ChartType.Default
        Page = PageType.Default
        RangeSelectionButtonIndex = 0
        Error = None
    }

    let cmd = Cmd.OfAsync.either getOrFetch ()
                   ConsumePatientsData ConsumeServerError

    state, cmd

let update (msg: Msg) (state: DeceasedVizState) : DeceasedVizState * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with PatientsData = data }, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | MetricTypeChanged mt ->
        { state with MetricType = mt }, Cmd.none
    | ChartTypeChanged ct ->
        { state with ChartType = ct }, Cmd.none
    | ChangePage page ->
        { state with Page = page }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let total =
        pts |> Array.map (fun p -> p?point?y |> Utils.optionToInt) |> Array.sum
    let fmtDate = pts.[0]?point?date

    "<b>"
    + fmtDate
    + "</b><br>"
    + (pts
       |> Seq.map (fun p ->
           sprintf """<span style="color:%s">●</span> %s: <b>%s</b>"""
                p?series?color p?series?name
                (I18N.NumberFormat.formatNumber(p?point?y : float)))
       |> String.concat "<br>")
    + sprintf """<br><br><span style="color: rgba(0,0,0,0)">●</span> %s: <b>%s</b>"""
        (I18N.t "charts.deceased.deceased-total")
        (total |> I18N.NumberFormat.formatNumber)

let renderChartOptions (state : DeceasedVizState) dispatch =
    let className = "cases-chart"
    let scaleType = ScaleType.Linear

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Browser.Types.Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let stacking =
        match state.ChartType with
        | StackedNormal -> "normal"
        | StackedPercent -> "percent"

    let getLastSunday (d : System.DateTime) =
        let mutable date = d
        while date.DayOfWeek <> System.DayOfWeek.Sunday do
          date <- date.AddDays -1.0
        date

    let lastDataPoint = state.StatsData |> List.last
    let previousSunday = getLastSunday (lastDataPoint.Date.AddDays(-7.))

    let baseOptions =
        basicChartOptions
            scaleType className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    let xAxis =
            baseOptions.xAxis
            |> Array.map(fun xAxis -> 
               {| xAxis with
                      plotBands =
                        match state.Page with
                        | AgeGroupsPage | PersonTypePage ->
                            [|
                               {| from=jsTime <| previousSunday
                                  ``to``=jsTime <| lastDataPoint.Date
                                  color="#ffffe0"
                                |}
                            |]
                        | _ -> [| |]
                 |})

    {| baseOptions with
        series = renderAllSeriesData state
        xAxis = xAxis
        plotOptions = pojo
            {|
                column = pojo
                        {|
                          dataGrouping = pojo {| enabled = false |}
                          groupPadding = 0
                          pointPadding = 0
                          borderWidth = 0 |}
                series = {| stacking = stacking; crisp = true
                            borderWidth = 0
                            pointPadding = 0; groupPadding = 0
                            |}
            |}

        tooltip = pojo
            {|
                shared = true
                formatter = fun () -> tooltipFormatter jsThis
            |}

        legend = pojo {| enabled = true ; layout = "horizontal" |}

    |}

let renderChartContainer (state : DeceasedVizState) dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> chartFromWindow
        ]
    ]

let renderMetricTypeSelectors (activeMetric: MetricType) dispatch =
    let renderMetricTypeSelector (metric: MetricType) =
        let active = metric = activeMetric
        Html.div [
            prop.text metric.GetName
            prop.onClick (fun _ -> dispatch metric)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
        ]

    let metricTypesSelectors =
        MetricType.All
        |> List.map renderMetricTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (metricTypesSelectors)
    ]

let renderChartTypeSelectors (activeChartType: ChartType) dispatch =
    let renderChartTypeSelector (chartType: ChartType) =
        let active = chartType = activeChartType
        Html.div [
            prop.text chartType.GetName
            prop.onClick (fun _ -> dispatch chartType)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
        ]

    let chartTypesSelectors =
        ChartType.All
        |> List.map renderChartTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (chartTypesSelectors)
    ]

let renderPagesSwitchers activePage dispatch =
    let renderPageSwitcher (page: PageType) =
        let active = page = activePage
        Html.div [
            prop.text page.GetName
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected")]
            if not active then prop.onClick (fun _ -> dispatch page)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        PageType.All
        |> List.map renderPageSwitcher
        |> prop.children
    ]

let render (state: DeceasedVizState) dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            Utils.renderChartTopControls [
                renderMetricTypeSelectors
                    state.MetricType (MetricTypeChanged >> dispatch)
                renderChartTypeSelectors
                    state.ChartType (ChartTypeChanged >> dispatch)
            ]
            renderChartContainer state dispatch
            renderPagesSwitchers state.Page (ChangePage >> dispatch)

            match state.Page with
            | AgeGroupsPage | PersonTypePage ->
                Html.div [
                    prop.className "disclaimer"
                    prop.children [
                        Html.text (chartText "disclaimer")
                    ]
                ]
            | _ -> Html.none

        ]

let renderChart(statsData: StatsData) =
    React.elmishComponent("CasesChart", init statsData, update, render)
