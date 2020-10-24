[<RequireQualifiedAccess>]
module QueryParams

open Browser
open Fable.Extras.Web

let queryParamsChangedEvent = "queryParamsChanged"

type State =
    { Greeting : string option
      DateFrom : string option }

    with

    static member Empty =
        { Greeting = None
          DateFrom = None }

let getState () =
    JSe.URLSearchParams(window.location.search).Entries()
    |> Seq.fold (fun state (key, value) ->
        match key with
        | "greeting" ->
            { state with Greeting = Some value }
        | "dateFrom" ->
            { state with DateFrom = Some value }
        | _ -> state
    ) State.Empty

let setState state =
    let urlSearchParams =
        seq {
            yield state.Greeting |> Option.map (fun value -> ("greeting", value))
            yield state.DateFrom |> Option.map (fun value -> ("dateFrom", value))
        }
        |> Seq.choose id
        |> JSe.URLSearchParams
    history.replaceState(null, null, "?" + urlSearchParams.ToString())
    window.dispatchEvent(CustomEvent.Create queryParamsChangedEvent) |> ignore
