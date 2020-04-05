module App

open Browser
open Elmish
open Feliz

open Types
let init (visualization : string option) =
    let inner () =
        let renderingMode =
            match visualization with
            | None -> Normal
            | Some viz ->
                match viz with
                | "Map" -> Some Map
                | "MetricsComparison" -> Some MetricsComparison
                | "Patients" -> Some Patients
                | "Spread" -> Some Spread
                | "Regions" -> Some Regions
                | "Municipalities" -> Some Municipalities
                | "AgeGroups" -> Some AgeGroups
                | "Hospitals" -> Some Hospitals
                | "Infections" -> Some Infections
                | _ -> None
                |> Embeded

        let initialState =
            { StatsData = NotAsked
              RegionsData = NotAsked
              RenderingMode = renderingMode }

        initialState, Cmd.batch [Cmd.ofMsg StatsDataRequested ; Cmd.ofMsg RegionsDataRequest ]
    inner

let update (msg: Msg) (state: State) =
    match msg with
    | StatsDataRequested ->
        match state.StatsData with
        | Loading -> state, Cmd.none
        | _ -> { state with StatsData = Loading }, Cmd.OfAsync.result Data.Stats.load
    | StatsDataLoaded data ->
        { state with StatsData = data }, Cmd.none
    | RegionsDataRequest ->
        match state.RegionsData with
        | Loading -> state, Cmd.none
        | _ -> { state with RegionsData = Loading }, Cmd.OfAsync.result Data.Regions.load
    | RegionsDataLoaded data ->
        { state with RegionsData = data }, Cmd.none

open Elmish.React

let render (state : State) (dispatch : Msg -> unit) =
    let allVisualizations =
        [ {| Visualization = Hospitals
             ClassName = "patients-chart"
             Label = "Kapacitete bolnišnic"
             Explicit = true
             Renderer = fun _ -> lazyView HospitalsChart.hospitalsChart () |}
          {| Visualization = MetricsComparison
             ClassName = "metrics-comparison-chart"
             Label = "Širjenje COVID-19 v Sloveniji"
             Explicit = false
             Renderer = fun state ->
                match state.StatsData with
                | NotAsked -> Html.none
                | Loading -> Utils.renderLoading
                | Failure error -> Utils.renderErrorLoading error
                | Success data -> lazyView MetricsComparisonChart.metricsComparisonChart {| data = data |} |}
          {| Visualization = Patients
             ClassName = "patients-chart"
             Label = "Obravnava hospitaliziranih"
             Explicit = false
             Renderer = fun _ -> lazyView PatientsChart.patientsChart () |}
          {| Visualization = Spread
             ClassName = "spread-chart"
             Label = "Prirast potrjeno okuženih"
             Explicit = false
             Renderer = fun state ->
                match state.StatsData with
                | NotAsked -> Html.none
                | Loading -> Utils.renderLoading
                | Failure error -> Utils.renderErrorLoading error
                | Success data -> lazyView SpreadChart.spreadChart {| data = data |} |}
          {| Visualization = Infections
             ClassName = "metrics-comparison-chart"
             Label = "Struktura potrjeno okuženih"
             Explicit = false
             Renderer = fun state ->
               match state.StatsData with
               | NotAsked -> Html.none
               | Loading -> Utils.renderLoading
               | Failure error -> Utils.renderErrorLoading error
               | Success data -> lazyView InfectionsChart.infectionsChart {| data = data |} |}
          {| Visualization = Regions
             ClassName = "regions-chart"
             Label = "Potrjeno okuženi po regijah"
             Explicit = false
             Renderer = fun state ->
                match state.RegionsData with
                | NotAsked -> Html.none
                | Loading -> Utils.renderLoading
                | Failure error -> Utils.renderErrorLoading error
                | Success data -> lazyView RegionsChart.regionsChart {| data = data |} |}
          {| Visualization = Municipalities
             ClassName = "municipalities-chart"
             Label = "Potrjeno okuženi po občinah"
             Explicit = false
             Renderer = fun state ->
                match state.RegionsData with
                | NotAsked -> Html.none
                | Loading -> Utils.renderLoading
                | Failure error -> Utils.renderErrorLoading error
                | Success data -> lazyView MunicipalitiesChart.municipalitiesChart {| data = data |} |}
          {| Visualization = Map
             ClassName = "map-chart"
             Label = "Zemljevid potrjeno okuženih po občinah"
             Explicit = false
             Renderer = fun state ->
                match state.RegionsData with
                | NotAsked -> Html.none
                | Loading -> Utils.renderLoading
                | Failure error -> Utils.renderErrorLoading error
                | Success data -> lazyView Map.mapChart {| data = data |} |}
          {| Visualization = AgeGroups
             ClassName = "age-groups-chart"
             Label = "Potrjeno okuženi po starostnih skupinah"
             Explicit = false
             Renderer = fun state ->
                match state.StatsData with
                | NotAsked -> Html.none
                | Loading -> Utils.renderLoading
                | Failure error -> Utils.renderErrorLoading error
                | Success data -> lazyView (AgeGroupsChart.render data) () |}
        ]

    let embeded, visualizations =
        match state.RenderingMode with
        | Normal -> false, allVisualizations |> List.filter (fun viz -> not viz.Explicit)
        | Embeded visualization ->
            match visualization with
            | None -> true, []
            | Some visualization -> true, allVisualizations |> List.filter (fun viz -> viz.Visualization = visualization)

    Html.div
        [ prop.className [ true, "visualization container" ; embeded, "embeded" ]
          prop.children (
              visualizations
              |> List.map (fun viz ->
                  Html.section
                    [ prop.className [ true, viz.ClassName; true, "visualization-chart" ]
                      prop.id viz.ClassName
                      prop.children
                        [ Html.h2 viz.Label
                          state |> viz.Renderer ] ] )
          )
        ]
