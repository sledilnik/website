module App

open Elmish
open Feliz

open Types

let init() =
    let initialState =
        { Data = NotAsked }

    initialState, Cmd.OfAsync.result SourceData.loadData

let update (msg: Msg) (state: State) =
    match msg with
    | DataLoaded data ->
        { state with Data = data }, Cmd.none

let render (state : State) (dispatch : Msg -> unit) =
    match state.Data with
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
                        [ Html.h2 "Pregled in primerjava podatkov"
                          MetricsComparisonChart.metricsComparisonChart { data = data.StatsData } ] ]
                  Html.section
                    [ prop.className "regions-chart"
                      prop.children
                        [ Html.h2 "Stanje obolelih"
                          PatientsChart.patientsChart () ] ]
                  Html.section
                    [ prop.className "regions-chart"
                      prop.children
                        [ Html.h2 "Pozitivni testi po regijah"
                          RegionsChart.regionsChart { data = data.RegionsData } ] ]
                  Html.section
                    [ prop.className "age-group-chart"
                      prop.children
                        [ Html.h2 "Pozitivni testi po starostnih skupinah"
                          AgeGroupsChart.render data.StatsData ] ]
                  Html.section
                    [ prop.className "data-table"
                      prop.children
                        [ Html.h2 "Tabelarični prikaz podatkov"
                          DataTable.render data.StatsData ] ]
                ]
            ]
