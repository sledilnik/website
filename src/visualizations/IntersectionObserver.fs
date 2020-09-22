module IntersectionObserver

open System
open Fable.Core
open Elmish
open Feliz
open Feliz.UseElmish

type Options = {
    root : Browser.Types.Element option
    rootMargin : string
    threshold: float
}

let defaultOptions = {
   root = None
   rootMargin = "0px"
   threshold = 0.0
}

type IntersectionObserver = obj

type IntersectionObserverEntry = {
    isIntersecting : bool
    intersectionRatio : float
}

[<Import("createIntersectionObserver", from="./IntersectionObserver.js")>]
let private createIntersectionObserverJS (targetElementId : string, callback : IntersectionObserverEntry array -> obj -> unit, options : Options option) : IntersectionObserver = jsNative

let createIntersectionObserver (targetElementId : string) (callback : IntersectionObserverEntry array -> unit) (options : Options option) : IntersectionObserver =
    let callbackWrapper = fun intersectionObserverEntries _ -> callback intersectionObserverEntries
    createIntersectionObserverJS (targetElementId, callbackWrapper, options)

module Component =
    open Fable.Core.JsInterop

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

    let intersectionObserver = React.functionComponent(fun (props : {| targetElementId : string ; content : ReactElement ; options : Options |}) ->
        let state, dispatch = React.useElmish(init, update, [||])

        let subscribeToIntersectionObserver () =
            let callback (entries : IntersectionObserverEntry array) =
                match state.Visibility, entries.[0].isIntersecting with
                | Hidden, true -> dispatch Show ; ()
                | _ -> ()
            let observer = createIntersectionObserver props.targetElementId callback (Some props.options)
            { new IDisposable with member this.Dispose() = observer?disconnect() }

        React.useEffect(subscribeToIntersectionObserver)

        match state.Visibility with
        | Hidden -> Html.none
        | Visible -> props.content
    )
