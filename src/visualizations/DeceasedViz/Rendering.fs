module DeceasedViz.Rendering

open System
open Data.Patients
open DeceasedViz.Analysis
open DeceasedViz.Synthesis
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Highcharts
open Types


let availableDisplayMetrics = [|
    { Id = "deceasedToDate"
      MetricsType = HospitalsToDate; ChartType = "normal" }
    { Id = "deceasedToDateRelative"
      MetricsType = HospitalsToDate; ChartType = "percent" }
    { Id = "deceasedToday"
      MetricsType = HospitalsToday; ChartType = "normal" }
    { Id = "deceasedTodayRelative"
      MetricsType = HospitalsToday; ChartType = "percent" }
    { Id = "deceasedToDateByAge"
      MetricsType = ByAgeToDate; ChartType = "percent" }
|]

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ChangeMetrics of DisplayMetrics
    | RangeSelectionChanged of int

let init(statsData : StatsData) : DeceasedVizState * Cmd<Msg> =
    let state = {
        StatsData = statsData
        PatientsData = [||]
        Metrics = availableDisplayMetrics.[0]
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
    | ChangeMetrics metrics ->
        { state with Metrics=metrics }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let total = pts |> Array.map (fun p -> p?point?y |> Utils.optionToInt) |> Array.sum
    let fmtDate = pts.[0]?point?fmtDate

    fmtDate
    + "<br>"
    + (pts
       |> Seq.map (fun p ->
           sprintf """<span style="color:%s">●</span> %s: <b>%s</b>"""
                p?series?color p?series?name (I18N.NumberFormat.formatNumber(p?point?y : float)))
       |> String.concat "<br>")
    + sprintf """<br><br><span style="color: rgba(0,0,0,0)">●</span> %s: <b>%s</b>"""
        (I18N.t "charts.deceased.deceased-total")
        (total |> I18N.NumberFormat.formatNumber)

let renderChartOptions (state : DeceasedVizState) dispatch =
    let className = "cases-chart"
    let scaleType = ScaleType.Linear

    let renderSeries series =
        {|
            ``type`` = "column"
            visible = true
            color = series.Color
            name = I18N.tt "charts.deceased" series.SeriesId
            data =
                state.PatientsData
                |> Seq.map (fun dataPoint ->
                    {|
                        x = dataPoint.Date |> jsTime12h
                        y = getPoint state series dataPoint
                        seriesId = series.SeriesId
                        fmtDate = I18N.tOptions "days.longerDate"
                                      {| date = dataPoint.Date |}
                        fmtTotal =
                            getPointTotal state series dataPoint |> string
                    |} |> pojo
                )
                |> Array.ofSeq
        |}
        |> pojo

    let allSeries = [|
        for series in hospitalSeries() do
            yield renderSeries series
    |]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Browser.Types.Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions
            scaleType className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with
        series = allSeries
        plotOptions = pojo
            {|
                column = pojo
                        {|
                          dataGrouping = pojo {| enabled = false |}
                          groupPadding = 0
                          pointPadding = 0
                          borderWidth = 0 |}
                series = {| stacking = state.Metrics.ChartType; crisp = true
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

let renderMetricsSelectors activeMetrics dispatch =
    let renderSelector (metrics : DisplayMetrics) =
        let active = metrics = activeMetrics
        Html.div [
            prop.text (I18N.chartText "deceased" metrics.Id)
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected")]
            if not active then prop.onClick (fun _ -> dispatch metrics)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        availableDisplayMetrics
        |> Array.map renderSelector
        |> prop.children
    ]

let render (state: DeceasedVizState) dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
            renderMetricsSelectors state.Metrics (ChangeMetrics >> dispatch)
        ]

let renderChart(statsData: StatsData) =
    React.elmishComponent("CasesChart", init statsData, update, render)
