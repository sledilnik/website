[<RequireQualifiedAccess>]
module QueryParams

open System
open Browser
open Fable.Extras.Web
open Feliz
open Feliz.UseElmish

let queryParamsChangedEvent = "queryParamsChanged"

type State =
    { MunicipalitiesChartRegion: string option
      MunicipalitiesChartSearch: string option
      MunicipalitiesChartSort: string option
      MetricsComparisonMetricType: string option
      MetricsCorrelationDisplayType: string option
      RegionsMetricType: string option
      PatientsBreakdown: string option
      DailyComparisonDisplayType: string option
      DeceasedMetrics: string option
      ExcessDeathsDisplayType: string option
      MapContentType: string option
      MapDisplayType: string option
      AgeGroupsChartMode: string option
      AgeGroupsTimelineMetrics: string option
      TestsDisplayType: string option
      HcCasesDisplayType: string option
      SourcesDisplayType: string option
      PhaseDiagramKind: string option
      PhaseDiagramMetric: string option
      DateFrom: string option }

    static member Empty =
        { MunicipalitiesChartRegion = None
          MunicipalitiesChartSearch = None
          MunicipalitiesChartSort = None
          MetricsComparisonMetricType = None
          MetricsCorrelationDisplayType = None
          RegionsMetricType = None
          PatientsBreakdown = None
          DailyComparisonDisplayType = None
          DeceasedMetrics = None
          ExcessDeathsDisplayType = None
          MapContentType = None
          MapDisplayType = None
          AgeGroupsChartMode = None
          AgeGroupsTimelineMetrics = None
          TestsDisplayType = None
          HcCasesDisplayType = None
          SourcesDisplayType = None
          PhaseDiagramKind = None
          PhaseDiagramMetric = None
          DateFrom = None }

let getState () =
    JSe
        .URLSearchParams(window.location.search)
        .Entries()
    |> Seq.fold (fun state (key, value) ->
        match key with
        | "region" ->
            { state with
                  MunicipalitiesChartRegion = Some value }
        | "search" ->
            { state with
                  MunicipalitiesChartSearch = Some value }
        | "sort" ->
            { state with
                  MunicipalitiesChartSort = Some value }
        | "metrics-comparison" ->
            { state with
                  MetricsComparisonMetricType = Some value }
        | "metrics-correlation" ->
            { state with
                  MetricsCorrelationDisplayType = Some value }
        | "regions-metric" ->
            { state with
                  RegionsMetricType = Some value }
        | "patients" ->
            { state with
                  PatientsBreakdown = Some value }
        | "daily-comparison" ->
            { state with
                  DailyComparisonDisplayType = Some value }
        | "deceased" ->
            { state with
                  DeceasedMetrics = Some value }
        | "excess-deaths" ->
            { state with
                  ExcessDeathsDisplayType = Some value }
        | "map-content" ->
            { state with
                  MapContentType = Some value }
        | "map-display" ->
            { state with
                  MapDisplayType = Some value }
        | "age-groups" ->
            { state with
                  AgeGroupsChartMode = Some value }
        | "age-groups-timeline" ->
            { state with
                  AgeGroupsTimelineMetrics = Some value }
        | "tests" ->
            { state with
                  TestsDisplayType = Some value }
        | "hc-cases" ->
            { state with
                  HcCasesDisplayType = Some value }
        | "sources" ->
            { state with
                  SourcesDisplayType = Some value }
        | "phase-diagram-kind" ->
            { state with
                  PhaseDiagramKind = Some value }
        | "phase-diagram-metric" ->
            { state with
                  PhaseDiagramMetric = Some value }
        | "dateFrom" -> { state with DateFrom = Some value }
        | _ -> state) State.Empty

let setState state =
    let urlSearchParams =
        seq {
            yield
                state.MunicipalitiesChartRegion
                |> Option.map (fun value -> ("region", value))

            yield
                state.MunicipalitiesChartSearch
                |> Option.map (fun value -> ("search", value))

            yield
                state.MunicipalitiesChartSort
                |> Option.map (fun value -> ("sort", value))

            yield
                state.MetricsComparisonMetricType
                |> Option.map (fun value -> ("metrics-comparison", value))

            yield
                state.MetricsCorrelationDisplayType
                |> Option.map (fun value -> ("metrics-correlation", value))

            yield
                state.RegionsMetricType
                |> Option.map (fun value -> ("regions-metric", value))

            yield
                state.PatientsBreakdown
                |> Option.map (fun value -> ("patients", value))

            yield
                state.DailyComparisonDisplayType
                |> Option.map (fun value -> ("daily-comparison", value))

            yield
                state.DeceasedMetrics
                |> Option.map (fun value -> ("deceased", value))

            yield
                state.ExcessDeathsDisplayType
                |> Option.map (fun value -> ("excess-deaths", value))

            yield
                state.MapContentType
                |> Option.map (fun value -> ("map-content", value))

            yield
                state.MapDisplayType
                |> Option.map (fun value -> ("map-display", value))

            yield
                state.AgeGroupsChartMode
                |> Option.map (fun value -> ("age-groups", value))

            yield
                state.AgeGroupsTimelineMetrics
                |> Option.map (fun value -> ("age-groups-timeline", value))

            yield
                state.TestsDisplayType
                |> Option.map (fun value -> ("tests", value))

            yield
                state.HcCasesDisplayType
                |> Option.map (fun value -> ("hc-cases", value))

            yield
                state.SourcesDisplayType
                |> Option.map (fun value -> ("sources", value))

            yield
                state.PhaseDiagramKind
                |> Option.map (fun value -> ("phase-diagram-kind", value))

            yield
                state.PhaseDiagramMetric
                |> Option.map (fun value -> ("phase-diagram-metric", value))

            yield
                state.DateFrom
                |> Option.map (fun value -> ("dateFrom", value))
        }
        |> Seq.choose id
        |> JSe.URLSearchParams

    history.replaceState
        (null,
         null,
         "?"
         + urlSearchParams.ToString()
         + window.location.hash)

    window.setTimeout ((fun unit -> window.dispatchEvent (CustomEvent.Create queryParamsChangedEvent)), 0)
    |> ignore
//window.dispatchEvent(CustomEvent.Create queryParamsChangedEvent) |> ignore

/// Bind state to query params
let useQueryParams<'M, 'S when 'S: equality> (state: 'S)
                                             (onQueryParamsUpdated: State -> unit)
                                             (stateToQueryParams: 'S -> State -> State)
                                             =
    // From state to query params
    let previousState, setPreviousState = React.useState (state)

    if state <> previousState then
        (let newQueryParams = stateToQueryParams state (getState ())
         setState (newQueryParams)
         setPreviousState (state))




    // From query params to state
    let onQueryParamsChanged =
        React.useCallback (fun (_: obj) ->
            (
            //let queryParams = getState()
            //onQueryParamsUpdated queryParams
            ))

    React.useEffectOnce (fun unit ->
        window.addEventListener (queryParamsChangedEvent, onQueryParamsChanged)
        { new IDisposable with
            member x.Dispose() =
                window.removeEventListener (queryParamsChangedEvent, onQueryParamsChanged) })


let useQueryParamsOneWay<'M, 'S when 'S: equality> (state: 'S) (dispatch: 'M -> unit) (queryParamsToMsg: State -> 'M) =
    useQueryParams state (queryParamsToMsg >> dispatch) (fun _ s -> s)


let useElmishWithQueryParams<'M, 'S when 'S: equality> (init: 'S * Elmish.Cmd<'M>)
                                                       (update: 'M -> 'S -> 'S * Elmish.Cmd<'M>)
                                                       (stateToQueryParams: 'S -> State -> State)
                                                       (queyParamsUpdatedMsg: State -> 'M)
                                                       =
    // To have two-way binding between state <-> query params useElmish() needs to be in a sandwich:
    //  - query params must be injected into the Elmish initial state
    //  - query params must be dispatched as a Msg into the Elmish loop

    let _initAndApplyQueryParams () =
        let state, commands = init

        let state2, commands2 =
            update ((getState () |> queyParamsUpdatedMsg)) state

        state2, List.append commands commands2

    let state, dispatch =
        React.useElmish (_initAndApplyQueryParams, update, [||])

    let previousState, setPreviousState = React.useState (state)

    if state <> previousState then
        (let newQueryParams = stateToQueryParams state (getState ())
         setState (newQueryParams)
         setPreviousState (state))

    // From query params to state
    let onQueryParamsChanged =
        React.useCallback (fun (_: obj) ->
            (let queryParams = getState ()
             dispatch (queyParamsUpdatedMsg queryParams)))

    React.useEffectOnce (fun unit ->
        window.addEventListener (queryParamsChangedEvent, onQueryParamsChanged)
        { new IDisposable with
            member x.Dispose() =
                window.removeEventListener (queryParamsChangedEvent, onQueryParamsChanged) })

    // Intentionally returning previousState
    // previousState at this point is up to date
    // previousState is only updated when the state changes according to F#'s equality semantics
    // See conditional call to setPreviousState(state) above
    (previousState, dispatch)

let useReducerWithQueryParams<'M, 'S when 'S: equality> (update: 'S -> 'M -> 'S)
                                                        (init: 'S)
                                                        (stateToQueryParams: 'S -> State -> State)
                                                        (queyParamsUpdatedMsg: State -> 'M)
                                                        =
    // To have two-way binding between state <-> query params useElmish() needs to be in a sandwich:
    //  - query params must be injected into the Elmish initial state
    //  - query params must be dispatched as a Msg into the Elmish loop

    let _initAndApplyQueryParams =
        update init ((getState () |> queyParamsUpdatedMsg))

    let (state, dispatch) =
        React.useReducer (update, _initAndApplyQueryParams)

    let previousState, setPreviousState = React.useState (state)

    if state <> previousState then
        (let newQueryParams = stateToQueryParams state (getState ())
         setState (newQueryParams)
         setPreviousState (state))

    // From query params to state
    let onQueryParamsChanged =
        React.useCallback (fun (_: obj) ->
            (let queryParams = getState ()
             dispatch (queyParamsUpdatedMsg queryParams)))

    React.useEffectOnce (fun unit ->
        window.addEventListener (queryParamsChangedEvent, onQueryParamsChanged)
        { new IDisposable with
            member x.Dispose() =
                window.removeEventListener (queryParamsChangedEvent, onQueryParamsChanged) })

    // Intentionally returning previousState
    // previousState at this point is up to date
    // previousState is only updated when the state changes according to F#'s equality semantics
    // See conditional call to setPreviousState(state) above
    (previousState, dispatch)



// TODO: Newest design prototype in PhaseDiagram/Chart
// queryParamsToMessages, stateToQueryParams defined from query params definition:
// {toQueryParam; toMsg; getQueryParam; updateQueryParam}

// ?query=params -> QueryParams.State -> _list of messages_ -> dispatch/update -> new state
// state change -> ?query=params

//



