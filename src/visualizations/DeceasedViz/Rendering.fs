module DeceasedViz.Rendering

open System
open Data.Patients
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Types
open Highcharts

type DisplayMetricsType = Today | ToDate

type DisplayMetrics = {
    Id: string
    MetricsType: DisplayMetricsType
    ChartType: string
}

let availableDisplayMetrics = [|
    { Id = "deceasedToDate"; MetricsType = ToDate; ChartType = "normal" }
    { Id = "deceasedToDateRelative"; MetricsType = ToDate; ChartType = "percent" }
    { Id = "deceasedToday"; MetricsType = Today; ChartType = "normal" }
    { Id = "deceasedTodayRelative"; MetricsType = Today; ChartType = "percent" }
|]

let displayMetricsQueryParam =
    [ ("to-date", availableDisplayMetrics.[0])
      ("to-date-relative", availableDisplayMetrics.[1])
      ("today", availableDisplayMetrics.[2])
      ("today-relative", availableDisplayMetrics.[3]) ]
    |> Map.ofList

type State = {
    PatientsData : PatientsStats []
    Metrics: DisplayMetrics
    RangeSelectionButtonIndex: int
    Error : string option
}

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ChangeMetrics of DisplayMetrics
    | RangeSelectionChanged of int
    | QueryParamsUpdated of QueryParams.State

type Series =
    | DeceasedInIcu
    | DeceasedAcute
    | DeceasedCare
    | DeceasedOther

module Series =
    let all =
        [ DeceasedOther; DeceasedCare; DeceasedAcute; DeceasedInIcu ]

    let getSeriesInfo = function
        | DeceasedInIcu  -> true,  "#6d5b80",   "deceased-icu"
        | DeceasedAcute  -> true,  "#8c71a8",   "deceased-acute"
        | DeceasedCare   -> true,  "#a483c7",   "deceased-care"
        | DeceasedOther  -> true,  "#c59eef",   "deceased-rest"

let incorporateQueryParams (queryParams: QueryParams.State) (state: State, commands: Cmd<Msg>): State * Cmd<Msg> =
    let state =
        match queryParams.DeceasedMetrics with
        | Some (q: string) ->
            match q.ToLower() |> displayMetricsQueryParam.TryFind with
            | Some v -> { state with Metrics = v }
            | _ -> state
        | _ -> state

    state, commands

let stateToQueryParams (state: State) (queryParams: QueryParams.State) =
    { queryParams with
          DeceasedMetrics = Map.tryFindKey (fun k v -> v = state.Metrics) displayMetricsQueryParam }

let init() : State * Cmd<Msg> =
    let state = {
        PatientsData = [||]
        Metrics = availableDisplayMetrics.[0]
        RangeSelectionButtonIndex = 0
        Error = None
    }

    let cmd = Cmd.OfAsync.either getOrFetch ()
                   ConsumePatientsData ConsumeServerError

    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
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
    | QueryParamsUpdated queryParams -> (state, Cmd.none) |> incorporateQueryParams queryParams

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

let renderChartOptions (state : State) dispatch =
    let className = "cases-chart"
    let scaleType = ScaleType.Linear

    let subtract (a : int option) (b : int option) =
        match a, b with
        | Some aa, Some bb -> Some (bb - aa)
        | Some aa, None -> -aa |> Some
        | None, Some _ -> b
        | _ -> None

    let renderSeries series =

        let getPoint dataPoint : int option =
            match state.Metrics.MetricsType with
            | Today ->
                match series with
                | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.today
                | DeceasedAcute ->
                        dataPoint.total.deceased.hospital.today
                        |> subtract dataPoint.total.deceased.hospital.icu.today
                | DeceasedCare -> dataPoint.total.deceasedCare.today
                | DeceasedOther ->
                        dataPoint.total.deceased.today
                        |> subtract dataPoint.total.deceased.hospital.today
                        |> subtract dataPoint.total.deceasedCare.today
            | ToDate ->
                match series with
                | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
                | DeceasedAcute ->
                        dataPoint.total.deceased.hospital.toDate
                        |> subtract dataPoint.total.deceased.hospital.icu.toDate
                | DeceasedCare -> dataPoint.total.deceasedCare.toDate
                | DeceasedOther ->
                        dataPoint.total.deceased.toDate
                        |> subtract dataPoint.total.deceased.hospital.toDate
                        |> subtract dataPoint.total.deceasedCare.toDate

        let getPointTotal dataPoint : int option =
            match state.Metrics.MetricsType with
            | Today ->
                match series with
                | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.today
                | DeceasedAcute -> dataPoint.total.deceased.hospital.today
                | DeceasedCare -> dataPoint.total.deceasedCare.today
                | DeceasedOther -> dataPoint.total.deceased.today
            | ToDate ->
                match series with
                | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
                | DeceasedAcute -> dataPoint.total.deceased.hospital.toDate
                | DeceasedCare -> dataPoint.total.deceasedCare.toDate
                | DeceasedOther -> dataPoint.total.deceased.toDate

        let visible, color, seriesId = Series.getSeriesInfo series
        {|
            ``type`` = "column"
            visible = visible
            color = color
            name = I18N.tt "charts.deceased" seriesId
            data =
                state.PatientsData
                |> Seq.map (fun dataPoint ->
                    {|
                        x = dataPoint.Date |> jsTime12h
                        y = getPoint dataPoint
                        seriesId = seriesId
                        fmtDate = I18N.tOptions "days.longerDate"
                                      {| date = dataPoint.Date |}
                        fmtTotal = getPointTotal dataPoint |> string
                    |} |> pojo
                )
                |> Array.ofSeq
        |}
        |> pojo

    let allSeries = [|
        for series in Series.all do
            yield renderSeries series
    |]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
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

let renderChartContainer (state : State) dispatch =
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

let render (state: State) dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
            renderMetricsSelectors state.Metrics (ChangeMetrics >> dispatch)
        ]

let renderChart =
    React.functionComponent (fun () ->
        let state, dispatch =
            QueryParams.useElmishWithQueryParams (init ()) update stateToQueryParams Msg.QueryParamsUpdated

        React.useMemo ((fun () -> render state dispatch), [| state |]))
