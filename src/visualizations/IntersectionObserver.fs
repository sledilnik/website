module IntersectionObserver

open Elmish
open Feliz
open Feliz.ElmishComponents
open Feliz.UseElmish
open Fable.Core
open Fable.Core.JsInterop
open Browser

type IntersectionObserverEntry = {
    isIntersecting : bool
    intersectionRatio : float
}

[<Import("createIntersectionObserver", from="./IntersectionObserver.js")>]
let createIntersectionObserverJS (targetElementId : string, callback : IntersectionObserverEntry array -> obj -> unit) : unit = jsNative

let createIntersectionObserver (targetElementId : string) (callback : IntersectionObserverEntry array -> unit) : unit =
    // printfn "Created observer %s" targetElementId
    createIntersectionObserverJS (targetElementId, fun intersectionObserverEntries _ -> callback intersectionObserverEntries)

type Visibility =
    | Hidden
    | Visible

type State = {
    Visibility : Visibility
}

type Msg =
    | Show

let init data : State * Cmd<Msg> =
    { Visibility = Hidden }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | Show -> { state with Visibility = Visible }, Cmd.none

let intersectionObserver = React.functionComponent(fun (props : {| targetElementId : string ; content : ReactElement|}) ->
    let state, dispatch = React.useElmish(init, update, [||])

    let subscribeToIntersectionObserver () =
        let callback (entries : IntersectionObserverEntry array) =
            match state.Visibility, entries.[0].isIntersecting with
            | Hidden, true -> dispatch Show ; ()
            | _ -> ()
        createIntersectionObserver props.targetElementId callback

    React.useEffect(subscribeToIntersectionObserver)

    match state.Visibility with
    | Hidden -> Html.none
    | Visible -> props.content
)
