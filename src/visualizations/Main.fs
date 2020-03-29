module Main

open Elmish
open Elmish.React
open Elmish.Debug
open Elmish.HMR

open Types

let Visualizations (elementId : string, visualization : string option) =
    Program.mkProgram (App.init visualization) App.update App.render
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.withReactSynchronous elementId
    |> Program.run
