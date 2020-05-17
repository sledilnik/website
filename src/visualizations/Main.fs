module Main

open Elmish
open Elmish.React
open Elmish.Debug
open Elmish.HMR

let Visualizations (elementId : string, query : obj, language : string, visualization : string option) =
    Program.mkProgram (App.init query visualization language) App.update App.render
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.withReactSynchronous elementId
    |> Program.run
