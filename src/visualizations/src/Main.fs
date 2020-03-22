module Main

open Elmish
open Elmish.React
open Elmish.Debug
open Elmish.HMR

let Visualizations (elementId : string) =
    Program.mkProgram App.init App.update App.render
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.withReactSynchronous elementId
    |> Program.run
