module App

open Elmish
open Fable.React
open Fable.React.Props

open Types
let init() =
    let initialState =
        { Data = NotAsked
          Metrics =
            { Tests =               { Color = "#ffa600" ; Visible = false ; Label = "Testiranja" }
              TotalTests =          { Color = "#bda535" ; Visible = false ; Label = "Testiranja skupaj" }
              Cases =               { Color = "#7aa469" ; Visible = false ; Label = "Pozitivni testi" }
              TotalCases =          { Color = "#38a39e" ; Visible = true  ; Label = "Pozitivni testi skupaj" }
              Hospitalized =        { Color = "#1494ab" ; Visible = true  ; Label = "Hospitalizirani" }
              HospitalizedIcu =     { Color = "#0d7891" ; Visible = false ; Label = "Intenzivna nega" }
              Deaths =              { Color = "#075b76" ; Visible = false ; Label = "Umrli" }
              TotalDeaths =         { Color = "#003f5c" ; Visible = false ; Label = "Umrli skupaj" } } }

    initialState, Cmd.OfAsync.result SourceData.loadData

let update (msg: Msg) (state: State) =
    match msg with
    | DataLoaded data ->
        { state with Data = data }, Cmd.none
    | ToggleMetricVisible metric ->
        let newMetrics =
            match metric with
            | Tests -> { state.Metrics with Tests = { state.Metrics.Tests with Visible = not state.Metrics.Tests.Visible } }
            | TotalTests -> { state.Metrics with TotalTests = { state.Metrics.TotalTests with Visible = not state.Metrics.TotalTests.Visible } }
            | Cases -> { state.Metrics with Cases = { state.Metrics.Cases with Visible = not state.Metrics.Cases.Visible } }
            | TotalCases -> { state.Metrics with TotalCases = { state.Metrics.TotalCases with Visible = not state.Metrics.TotalCases.Visible } }
            | Hospitalized -> { state.Metrics with Hospitalized = { state.Metrics.Hospitalized with Visible = not state.Metrics.Hospitalized.Visible } }
            | HospitalizedIcu -> { state.Metrics with HospitalizedIcu = { state.Metrics.HospitalizedIcu with Visible = not state.Metrics.HospitalizedIcu.Visible } }
            | Deaths -> { state.Metrics with Deaths = { state.Metrics.Deaths with Visible = not state.Metrics.Deaths.Visible } }
            | TotalDeaths -> { state.Metrics with TotalDeaths = { state.Metrics.TotalDeaths with Visible = not state.Metrics.TotalDeaths.Visible } }
        { state with Metrics = newMetrics }, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    match state.Data with
    | NotAsked -> nothing
    | Loading -> str "Nalagam podatke ..."
    | Failure error -> str (error)
    | Success data ->
        div [ Class "visualization container-fluid" ] [
            section [ Class "content" ]
                [ Chart.render data state.Metrics dispatch
                  DataTable.render data state.Metrics ]
        ]
