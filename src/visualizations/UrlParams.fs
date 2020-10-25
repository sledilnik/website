[<RequireQualifiedAccess>]
module UrlParams

type State = {
    Greeting : string option
    DateFrom : System.DateTime option
    DateTo : System.DateTime option
}

let initialState = {
    Greeting = Some "Hello!"
    DateFrom = Some System.DateTime.Today
    DateTo = None
}
