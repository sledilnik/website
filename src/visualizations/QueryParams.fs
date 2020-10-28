[<RequireQualifiedAccess>]
module QueryParams

open System
open Browser
open Fable.Extras.Web
open Feliz
open Feliz.UseElmish

let queryParamsChangedEvent = "queryParamsChanged"

type State =
    {
      MunicipalitiesChartRegion: string option
      MunicipalitiesChartSearch : string option
      MunicipalitiesChartSort : string option
      Greeting : string option
      DateFrom : string option }

    with

    static member Empty =
        {
          MunicipalitiesChartRegion = None
          MunicipalitiesChartSearch = None
          MunicipalitiesChartSort = None
          Greeting = None
          DateFrom = None }

let getState () =
    JSe.URLSearchParams(window.location.search).Entries()
    |> Seq.fold (fun state (key, value) ->
        match key with
        | "region" ->
            { state with MunicipalitiesChartRegion = Some value }
        | "search" ->
            { state with MunicipalitiesChartSearch = Some value }
        | "sort" ->
            { state with MunicipalitiesChartSort = Some value }
        | "greeting" ->
            { state with Greeting = Some value }
        | "dateFrom" ->
            { state with DateFrom = Some value }
        | _ -> state
    ) State.Empty

let setState state =
    let urlSearchParams =
        seq {
            yield state.MunicipalitiesChartRegion |> Option.map (fun value -> ("region", value))
            yield state.MunicipalitiesChartSearch |> Option.map (fun value -> ("search", value))
            yield state.MunicipalitiesChartSort |> Option.map (fun value -> ("sort", value))
            yield state.Greeting |> Option.map (fun value -> ("greeting", value))
            yield state.DateFrom |> Option.map (fun value -> ("dateFrom", value))
        }
        |> Seq.choose id
        |> JSe.URLSearchParams
    history.replaceState(null, null, "?" + urlSearchParams.ToString() + window.location.hash)
    //window.setTimeout((fun unit -> window.dispatchEvent(CustomEvent.Create queryParamsChangedEvent)), 0) |> ignore
    window.dispatchEvent(CustomEvent.Create queryParamsChangedEvent) |> ignore

/// Bind state to query params
let useQueryParams<'M, 'S when 'S: equality> (state: 'S) (onQueryParamsUpdated: State -> unit )
                       (stateToQueryParams: 'S -> State -> State)
                       =
    // From state to query params
    let previousState, setPreviousState = React.useState(state)
    if state <> previousState then (
                                    let newQueryParams = stateToQueryParams state (getState())
                                    setState ( newQueryParams )
                                    setPreviousState(state)
                                   )




    // From query params to state
    let onQueryParamsChanged =
        React.useCallback (fun (_:obj) ->
            (
             //let queryParams = getState()
             //onQueryParamsUpdated queryParams
             ))

    React.useEffectOnce (fun unit ->
        window.addEventListener (queryParamsChangedEvent, onQueryParamsChanged)
        { new IDisposable with
            member x.Dispose() =
                window.removeEventListener (queryParamsChangedEvent, onQueryParamsChanged) })


let useQueryParamsOneWay<'M, 'S when 'S: equality> (state: 'S) (dispatch: 'M -> unit)
                       (queryParamsToMsg:  State -> 'M)
                       = useQueryParams state (queryParamsToMsg >> dispatch) (fun _ s -> s)


let useElmishWithQueryParams<'M, 'S when 'S: equality> (init: 'S*Elmish.Cmd<'M>) (update: 'M -> 'S -> 'S*Elmish.Cmd<'M>) (stateToQueryParams: 'S -> State -> State) (queyParamsUpdatedMsg: State -> 'M) =
    // To have two-way binding between state <-> query params useElmish() needs to be in a sandwich:
    //  - query params must be injected into the Elmish initial state
    //  - query params must be dispatched as a Msg into the Elmish loop

    let _initAndApplyQueryParams () =
        let state, commands = init
        let state2, commands2 = update ((getState() |> queyParamsUpdatedMsg)) state
        state2, List.append commands commands2

    let state, dispatch = React.useElmish (_initAndApplyQueryParams, update, [||])

    let previousState, setPreviousState = React.useState(state)
    if state <> previousState then (
                                    let newQueryParams = stateToQueryParams state (getState())
                                    setState ( newQueryParams )
                                    setPreviousState(state)
                                   )

    // From query params to state
    let onQueryParamsChanged =
        React.useCallback (fun (_:obj) ->
            (
             let queryParams = getState()
             dispatch(queyParamsUpdatedMsg queryParams)
             ))

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


