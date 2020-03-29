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
                    [ prop.className "metrics-comparison-chart"
                      prop.children
                        [ Html.h2 "Kazalniki COVID-19 v Sloveniji"
                          MetricsComparisonChart.metricsComparisonChart { data = data.StatsData } ] ]
                  Html.section
                    [ prop.className "patients-chart"
                      prop.children
                        [ Html.h2 "Obravnava hospitaliziranih"
                          PatientsChart.patientsChart () ] ]
                  Html.section
                      [ prop.className "patients-chart"
                        prop.children
                          [ Html.h2 "Hitrost širjenja okužbe"
                            SpreadChart.spreadChart { data = data.StatsData } ] ]
                  Html.section
                    [ prop.className "regions-chart"
                      prop.children
                        [ Html.h2 "Potrjeno okuženi po regijah"
                          RegionsChart.regionsChart { data = data.RegionsData } ] ]
                  Html.section
                    [ prop.className "municipalities-chart"
                      prop.children
                        [ Html.h2 "Potrjeno okuženi po občinah"
                          MunicipalitiesChart.municipalitiesChart { data = data.RegionsData } ] ]
                  Html.section
                    [ prop.className "age-groups-chart"
                      prop.children
                        [ Html.h2 "Potrjeno okuženi po starostnih skupinah"
                          AgeGroupsChart.render data.StatsData () ] ]
                ]
            ]
