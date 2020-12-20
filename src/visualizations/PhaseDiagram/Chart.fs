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

let defaultDiagramKind = TotalVsWeek
let defaultMetric = Cases


let diagramKindQueryParam =
    DiagramKind.All
    |> List.map (fun dk ->
        ((match dk with
          | TotalVsWeek -> "total-vs-week"
          | WeekVsWeekBefore -> "week-vs-week-before"),
         dk))
    |> Map.ofList

let metricQueryParam =
    Metric.AllMetrics
    |> List.map (fun m ->
        ((match m with
          | Cases -> "cases"
          | Hospitalized -> "hospitalized"
          | Deceased -> "deceased"),
         m))
    |> Map.ofList


let stateToQueryParams (state: State) (queryParams: QueryParams.State) =
    { queryParams with
          PhaseDiagramKind =
              if state.DiagramKind = defaultDiagramKind
              then None
              else Map.tryFindKey (fun k v -> v = state.DiagramKind) diagramKindQueryParam

          PhaseDiagramMetric =
              if state.Metric = defaultMetric
              then None
              else Map.tryFindKey (fun k v -> v = state.Metric) metricQueryParam
              }

let init statsData : State * Cmd<Msg> =
    let metric = defaultMetric
    let diagramKind = defaultDiagramKind
    let displayData = totalVsWeekData metric statsData

    let state = {
        StatsData = statsData
        DisplayData = displayData
        DiagramKind = diagramKind
        Metric = metric
        Day = displayData.Length - 1
    }
    state, Cmd.none

let rec update (msg: Msg) (state: State) : State * Cmd<Msg> =
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
        let newState =
            match metric with
            | CasesMetric -> { state with Metric = Cases }
            | HospitalizedMetric -> { state with Metric = Hospitalized }
            | DeceasedMetric -> { state with Metric = Deceased }
            | UnknownMetric -> state
        let newDisplayData = displayData newState.Metric state.DiagramKind state.StatsData
        { newState with
            DisplayData = newDisplayData
            Day = newDisplayData.Length - 1 }, Cmd.none
    | QueryParamsUpdated queryParams -> (state, Cmd.none) |> incorporateQueryParams queryParams

and incorporateQueryParams (queryParams: QueryParams.State) (state: State, commands: Cmd<Msg>): State * Cmd<Msg> =
    // TODO: This is ugly
    //  - cannot directly update state because diagramKind and Metric have extra logic
    //  - need to call `update` instead of Cmd.ofMsg or dispatching to avoid flickering
    //
    List.fold (fun s m -> update m s |> fst) state (List.concat [
                  match queryParams.PhaseDiagramKind with
                  | Some (q: string) ->
                      match q.ToLower() |> diagramKindQueryParam.TryFind with
                      | Some v -> [ Msg.DiagramKindSelected v ]
                      | None -> []
                  | _ -> []
                  match queryParams.PhaseDiagramMetric with
                  | Some (q: string) ->
                      match q.ToLower() |> metricQueryParam.TryFind with
                      | Some v -> [ Msg.MetricSelected(v.ToString()) ]
                      | None -> []
                  | _ -> [] ])
    , commands


type QueryParamStateMapping<'S, 'M> = {
    toQueryParam: 'S -> string option
    toMsg: 'S -> string option -> 'M option
    getQueryParam: QueryParams.State -> string option
    updateQueryParam: QueryParams.State -> string option -> QueryParams.State
}


// TODO: Refactor query params to use Query Param Definitions instead of a pair of functions
let qpDefinitions =
    [ { toQueryParam =
            (fun s ->
                match s.DiagramKind with
                | k when k = defaultDiagramKind -> None
                | TotalVsWeek -> Some "total-vs-week"
                | WeekVsWeekBefore -> Some "week-vs-week-before")
        toMsg =
            fun s dk ->
                match dk with
                | Some "total-vs-week" -> Msg.DiagramKindSelected TotalVsWeek |> Some
                | Some "week-vs-week-before" -> Msg.DiagramKindSelected WeekVsWeekBefore |> Some
                | _ -> None
        getQueryParam = fun qp -> qp.PhaseDiagramKind
        updateQueryParam = fun qp v -> { qp with PhaseDiagramKind = v } } ]

let queryParamsToMessages<'S, 'M> (qpDefinitions: QueryParamStateMapping<'S, 'M> list)
                                  (qp: QueryParams.State)
                                  (state: 'S)
                                  =
    qpDefinitions
    |> List.choose (fun qpd ->
        let currentQp = qpd.getQueryParam qp
        let oldQp = qpd.toQueryParam state
        if currentQp <> oldQp then qpd.toMsg state currentQp else None)

let stateToQueryParams_<'S, 'M> (qpDefinitions: QueryParamStateMapping<'S, 'M> list) (qp: QueryParams.State) (state: 'S) =
    qpDefinitions
    |> List.fold (fun qp qpd -> qpd.updateQueryParam qp (qpd.toQueryParam state)) qp

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
               color = state.Metric.Color.Light
               marker = pojo {| symbol = "circle" ; radius = 2 |}
               states = pojo {| hover = pojo {| lineWidth = 0 |} |}
            |} |> pojo
            {| data = [| data.[state.Day] |]
               color = state.Metric.Color.Dark
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
               color = state.Metric.Color.Light
               marker = pojo {| symbol = "circle" ; radius = 3 |}
               states = pojo {| hover = pojo {| lineWidth = 0 |} |}
            |} |> pojo
            {| data = [| data.[state.Day] |]
               color = state.Metric.Color.Dark
               marker = pojo {| symbol = "circle" ; radius = 8 |}
               states = pojo {| hover = pojo {| lineWidth = 0 |} |}
            |} |> pojo
        |]
    |} |> pojo

let renderMetricSelector (selected : Metric) dispatch =
    let options =
        Metric.AllMetrics
        |> List.map (fun metric ->
            Html.option [
                prop.text metric.Name
                prop.value (metric.ToString())
            ]
        )

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
    React.functionComponent (fun (props: {| data: StatsData |}) ->
        let state, dispatch =
            QueryParams.useElmishWithQueryParams
                (init props.data)
                update
                stateToQueryParams
                Msg.QueryParamsUpdated

        React.useMemo ((fun () -> renderChart state dispatch), [| state |]))
