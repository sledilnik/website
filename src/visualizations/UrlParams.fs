[<RequireQualifiedAccess>]
module UrlParams

open Feliz.Recoil

type State = {
    Greeting : string option
    DateFrom : System.DateTime option
    DateTo : System.DateTime option
}

type GetSetState = {
    Get : State
    Set : State -> unit
}

let getSet (state, setState) = {
    Get = state
    Set = setState
}

let initialState = {
    Greeting = Some "Hello, world!"
    DateFrom = Some System.DateTime.Today
    DateTo = None
}

let state = Recoil.atom("UrlParams/params", initialState)
