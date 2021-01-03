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


let availablePages = [|
    { Id = "deceasedToDate"
      MetricsType = HospitalsToDate; ChartType = StackedBarNormal }
    { Id = "deceasedToDateRelative"
      MetricsType = HospitalsToDate; ChartType = StackedBarPercent }
    { Id = "deceasedToday"
      MetricsType = HospitalsToday; ChartType = StackedBarNormal }
    { Id = "deceasedTodayRelative"
      MetricsType = HospitalsToday; ChartType = StackedBarPercent }
    { Id = "deceasedByAgeToDate"
      MetricsType = ByAgeToDate; ChartType = StackedBarNormal }
    { Id = "deceasedByAgeToDateRelative"
      MetricsType = ByAgeToDate; ChartType = StackedBarPercent }
    { Id = "deceasedByAgeToday"
      MetricsType = ByAgeToday; ChartType = StackedBarNormal }
    { Id = "deceasedByAgeTodayRelative"
      MetricsType = ByAgeToday; ChartType = StackedBarPercent }
|]

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ChangePage of VisualizationPage
    | RangeSelectionChanged of int

let init(statsData : StatsData) : DeceasedVizState * Cmd<Msg> =
    let state = {
        StatsData = statsData
        PatientsData = [||]
        Page = availablePages.[0]
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
    | ChangePage page ->
        { state with Page = page }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let total =
        pts |> Array.map (fun p -> p?point?y |> Utils.optionToInt) |> Array.sum
    let fmtDate = pts.[0]?point?date

    fmtDate
    + "<br>"
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
        match state.Page.ChartType with
        | StackedBarNormal -> "normal"
        | StackedBarPercent -> "percent"
        | _ -> invalidOp "not supported"

    let baseOptions =
        basicChartOptions
            scaleType className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with
        series = renderAllSeriesData state
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

let renderPagesSwitchers activePage dispatch =
    let renderPageSwitcher (page: VisualizationPage) =
        let active = page = activePage
        Html.div [
            prop.text (I18N.chartText "deceased" page.Id)
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected")]
            if not active then prop.onClick (fun _ -> dispatch page)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        availablePages
        |> Array.map renderPageSwitcher
        |> prop.children
    ]

let render (state: DeceasedVizState) dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
            renderPagesSwitchers state.Page (ChangePage >> dispatch)
        ]

let renderChart(statsData: StatsData) =
    React.elmishComponent("CasesChart", init statsData, update, render)
