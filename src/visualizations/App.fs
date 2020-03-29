module App

open Browser
open Elmish
open Elmish.UrlParser
open Feliz

open Types

module Route =

    let private visualizationParam name =
        let inner =
            Option.bind (fun (value : string) ->
                match value with
                | "MetricsComparison" -> Some MetricsComparison
                | "Patients" -> Some Patients
                | "Spread" -> Some Spread
                | "Regions" -> Some Regions
                | "AgeGroups" -> Some AgeGroups
                | _ -> None)
        customParam name inner

    let parseRoute (location : Location) =
        location
        |> parseHash (s "embed" <?> visualizationParam "viz")
        |> Option.flatten

let init() =
    let initialState =
        { Data = NotAsked
          RenderVisualization = window.location |> Route.parseRoute }

    initialState, Cmd.OfAsync.result SourceData.loadData

let update (msg: Msg) (state: State) =
    match msg with
    | DataLoaded data ->
        { state with Data = data }, Cmd.none

let render (state : State) (dispatch : Msg -> unit) =
    let allVisualizations =
        [ {| Visualization = MetricsComparison
             ClassName = "metrics-comparison-chart"
             Label = "Širjenje COVID-19 v Sloveniji"
             Renderer = fun data -> MetricsComparisonChart.metricsComparisonChart { data = data.StatsData } |}
          {| Visualization = Patients
             ClassName = "patients-chart"
             Label = "Obravnava hospitaliziranih"
             Renderer = fun data -> PatientsChart.patientsChart () |}
          {| Visualization = Spread
             ClassName = "spread-chart"
             Label = "Hitrost širjenja okužbe"
             Renderer = fun data -> SpreadChart.spreadChart { data = data.StatsData } |}
          {| Visualization = Regions
             ClassName = "regions-chart"
             Label = "Potrjeno okuženi po regijah"
             Renderer = fun data -> RegionsChart.regionsChart { data = data.RegionsData } |}
          {| Visualization = Municipalities
             ClassName = "municipalities-chart"
             Label = "Potrjeno okuženi po občinah"
             Renderer = fun data -> MunicipalitiesChart.municipalitiesChart { data = data.RegionsData } |}
          {| Visualization = AgeGroups
             ClassName = "age-groups-chart"
             Label = "Potrjeno okuženi po starostnih skupinah"
             Renderer = fun data -> AgeGroupsChart.render data.StatsData () |} ]

    match state.Data with
    | NotAsked -> Html.none
    | Loading -> Html.text "Nalagam podatke..."
    | Failure error -> Html.text error
    | Success data ->
        let visualizations =
            match state.RenderVisualization with
            | Some visualization -> allVisualizations |> List.filter (fun viz -> viz.Visualization = visualization)
            | None -> allVisualizations

        Html.div
            [ prop.className "visualization container"
              prop.children (
                  visualizations
                  |> List.map (fun viz ->
                      Html.section
                        [ prop.className viz.ClassName
                          prop.children
                            [ Html.h2 viz.Label
                              data |> viz.Renderer ] ] )
              )
            ]
