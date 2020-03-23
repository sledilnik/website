module App

open Elmish
open Feliz

open Types

let init() =
    let initialState =
        { StatsData = NotAsked
          Metrics =
            { Tests =               { Color = "#ffa600" ; Visible = false ; Label = "Testiranja" }
              TotalTests =          { Color = "#bda535" ; Visible = false ; Label = "Testiranja skupaj" }
              PositiveTests =       { Color = "#7aa469" ; Visible = false ; Label = "Pozitivni testi" }
              TotalPositiveTests =  { Color = "#38a39e" ; Visible = true  ; Label = "Pozitivni testi skupaj" }
              Hospitalized =        { Color = "#1494ab" ; Visible = true  ; Label = "Hospitalizirani" }
              HospitalizedIcu =     { Color = "#0d7891" ; Visible = false ; Label = "Intenzivna nega" }
              Deaths =              { Color = "#075b76" ; Visible = false ; Label = "Umrli" }
              TotalDeaths =         { Color = "#003f5c" ; Visible = false ; Label = "Umrli skupaj" } } }

    initialState, Cmd.OfAsync.result SourceData.loadData

let update (msg: Msg) (state: State) =
    match msg with
    | StatsDataLoaded data ->
        { state with StatsData = data }, Cmd.none
    | ToggleMetricVisible metric ->
        let newMetrics =
            match metric with
            | Tests -> { state.Metrics with Tests = { state.Metrics.Tests with Visible = not state.Metrics.Tests.Visible } }
            | TotalTests -> { state.Metrics with TotalTests = { state.Metrics.TotalTests with Visible = not state.Metrics.TotalTests.Visible } }
            | PositiveTests -> { state.Metrics with PositiveTests = { state.Metrics.PositiveTests with Visible = not state.Metrics.PositiveTests.Visible } }
            | TotalPositiveTests -> { state.Metrics with TotalPositiveTests = { state.Metrics.TotalPositiveTests with Visible = not state.Metrics.TotalPositiveTests.Visible } }
            | Hospitalized -> { state.Metrics with Hospitalized = { state.Metrics.Hospitalized with Visible = not state.Metrics.Hospitalized.Visible } }
            | HospitalizedIcu -> { state.Metrics with HospitalizedIcu = { state.Metrics.HospitalizedIcu with Visible = not state.Metrics.HospitalizedIcu.Visible } }
            | Deaths -> { state.Metrics with Deaths = { state.Metrics.Deaths with Visible = not state.Metrics.Deaths.Visible } }
            | TotalDeaths -> { state.Metrics with TotalDeaths = { state.Metrics.TotalDeaths with Visible = not state.Metrics.TotalDeaths.Visible } }
        { state with Metrics = newMetrics }, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    match state.StatsData with
    | NotAsked -> Html.none
    | Loading -> Html.text "Nalagam podatke ..."
    | Failure error -> Html.text error
    | Success data ->
        Html.div
            [ prop.className "visualization container"
              prop.children
                [ Html.section
                    [ prop.className "metric-comparison-chart"
                      prop.children
                        [ Html.h2 "Pregled in primerjava podatkov COVID-19 za Slovenijo"
                          MetricComparisonChart.render data state.Metrics dispatch ] ]
                  Html.section
                    [ prop.className "age-group-chart"
                      prop.children
                        [ Html.h2 "Pozitivni testi po starostnih skupinah"
                          AgeGroupChart.render data ] ]
                  Html.section
                    [ prop.className "data-table"
                      prop.children
                        [ Html.h2 "Tabelarični prikaz podatkov"
                          DataTable.render data ] ]
                ]
            ]
