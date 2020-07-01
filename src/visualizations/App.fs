module App

open Browser
open Elmish
open Feliz

open Types

let init (query: obj) (visualization: string option) =
    let inner () =
        let renderingMode =
            match visualization with
            | None -> Normal
            | Some viz ->
                match viz with
                | "Map" -> Some Map
                | "EuropeMap" -> Some EuropeMap
                | "MetricsComparison" -> Some MetricsComparison
                | "Patients" -> Some Patients
                | "Ratios" -> Some Ratios
                | "Tests" -> Some Tests
                | "Cases" -> Some Cases
                | "Spread" -> Some Spread
                | "Regions" -> Some Regions
                | "Municipalities" -> Some Municipalities
                | "AgeGroups" -> Some AgeGroups
                | "AgeGroupsTimeline" -> Some AgeGroupsTimeline
                | "HCenters" -> Some HCenters
                | "Hospitals" -> Some Hospitals
                | "Infections" -> Some Infections
                | "Countries" -> Some Countries
                | _ -> None
                |> Embedded

        let initialState =
            { Query = query
              StatsData = NotAsked
              RegionsData = NotAsked
              RenderingMode = renderingMode }

        initialState,
        Cmd.batch
            [ Cmd.ofMsg StatsDataRequested
              Cmd.ofMsg RegionsDataRequest ]

    inner

let update (msg: Msg) (state: State) =
    match msg with
    | StatsDataRequested ->
        match state.StatsData with
        | Loading -> state, Cmd.none
        | _ -> { state with StatsData = Loading }, Cmd.OfAsync.result Data.Stats.load
    | StatsDataLoaded data -> { state with StatsData = data }, Cmd.none
    | RegionsDataRequest ->
        match state.RegionsData with
        | Loading -> state, Cmd.none
        | _ -> { state with RegionsData = Loading }, Cmd.OfAsync.result Data.Regions.load
    | RegionsDataLoaded data -> { state with RegionsData = data }, Cmd.none

open Elmish.React

let render (state: State) (_: Msg -> unit) =
    let allVisualizations: Visualization list =
        [
          { VisualizationType = Hospitals
            ClassName = "hospitals-chart"
            Label = I18N.t "charts.hospitals.title"
            Explicit = true
            Renderer = fun _ -> lazyView HospitalsChart.hospitalsChart () }
          { VisualizationType = MetricsComparison
            ClassName = "metrics-comparison-chart"
            Label = I18N.t "charts.metricsComparison.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView MetricsComparisonChart.metricsComparisonChart {| data = data |} }
          { VisualizationType = Spread
            ClassName = "spread-chart"
            Label = I18N.t "charts.spread.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView SpreadChart.spreadChart {| data = data |} }
          { VisualizationType = Map
            ClassName = "map-chart"
            Label = I18N.t "charts.map.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView Map.mapChart {| data = data |} }
          { VisualizationType = Municipalities
            ClassName = "municipalities-chart"
            Label = I18N.t "charts.municipalities.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView MunicipalitiesChart.municipalitiesChart {| query = state.Query; data = data |} }
          { VisualizationType = EuropeMap
            ClassName = "europe-chart"
            Label = I18N.t "charts.europe.title"
            Explicit = false
            Renderer =
               fun state ->
                   match state.StatsData with
                   | NotAsked -> Html.none
                   | Loading -> Utils.renderLoading
                   | Failure error -> Utils.renderErrorLoading error
                   | Success data -> lazyView EuropeMap.mapChart {| data = data |} }
          { VisualizationType = AgeGroupsTimeline
            ClassName = "age-groups-trends-chart"
            Label = I18N.t "charts.ageGroupsTimeline.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView AgeGroupsTimelineViz.Rendering.renderChart
                            {| data = data |} }
          { VisualizationType = Tests
            ClassName = "tests-chart"
            Label = I18N.t "charts.tests.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView TestsChart.testsChart {| data = data |} }
          { VisualizationType = HCenters
            ClassName = "hcenters-chart"
            Label = I18N.t "charts.hCenters.title"
            Explicit = false
            Renderer = fun _ -> lazyView HCentersChart.hCentersChart () }
          { VisualizationType = Infections
            ClassName = "infections-chart"
            Label = I18N.t "charts.infections.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView InfectionsChart.infectionsChart {| data = data |} }
          { VisualizationType = Cases
            ClassName = "cases-chart"
            Label = I18N.t "charts.cases.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView CasesChart.casesChart {| data = data |} }
          { VisualizationType = Patients
            ClassName = "patients-chart"
            Label = I18N.t "charts.patients.title"
            Explicit = false
            Renderer = fun _ -> lazyView PatientsChart.patientsChart () }
          { VisualizationType = Ratios
            ClassName = "ratios-chart"
            Label = I18N.t "charts.ratios.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView RatiosChart.ratiosChart {| data = data |} }
          { VisualizationType = AgeGroups
            ClassName = "age-groups-chart"
            Label = I18N.t "charts.ageGroups.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView AgeGroupsChart.renderChart {| data = data |} }
          { VisualizationType = Regions
            ClassName = "regions-chart"
            Label = I18N.t "charts.regions.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView RegionsChart.regionsChart {| data = data |} }
          { VisualizationType = Countries
            ClassName = "countries-chart"
            Label = I18N.t "charts.countries.title"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success _ -> lazyView CountriesChartViz.Rendering.renderChart () } ]

    let embedded, visualizations =
        match state.RenderingMode with
        | Normal ->
            false,
            allVisualizations
            |> List.filter (fun viz -> not viz.Explicit)
        | Embedded visualizationType ->
            match visualizationType with
            | None -> true, []
            | Some visualizationType ->
                true,
                allVisualizations
                |> List.filter (fun viz -> viz.VisualizationType = visualizationType)

    let brandLink =
        match state.RenderingMode with
        | Normal -> Html.none
        | Embedded _ ->
            Html.a
                [ prop.className "brand-link"
                  prop.target "_blank"
                  prop.href "https://covid-19.sledilnik.org/"
                  prop.text (I18N.t "meta.title") ]


    let renderFaqLink (visualization: Visualization) =
        if visualization.Explicit then
            Html.none // we do not have FAQ for hidden charts yet
        else
            Html.div
                [ prop.className "faq-link-wrapper"
                  prop.children
                      [ Html.a
                          [ prop.className "faq-link"
                            prop.key visualization.ClassName
                            prop.target "_blank"
                            prop.href
                                ("/"
                                 + localStorage.getItem ("i18nextLng")
                                 + "/faq#"
                                 + visualization.ClassName)
                            prop.text "?" ]
                        |> Html.div ] ]


    let renderFaqAndShareBtn (visualization: Visualization) =
        match state.RenderingMode with
        | Embedded _ ->
            Html.div
                [ prop.className "faq-and-share-wrapper"
                  prop.children
                      [ renderFaqLink visualization
                        brandLink ] ]
        | Normal ->
            Html.div
                [ prop.className "faq-and-share-wrapper"
                  prop.children
                      [ renderFaqLink visualization
                        ShareButton.dropdown visualization () ] ]


    let renderChartTitle (visualization: Visualization) =

        let scrollToElement (e: MouseEvent) visualizationId =
            e.preventDefault ()

            let element =
                document.getElementById (visualizationId)

            let offset = -100.

            let position =
                element.getBoundingClientRect().top
                + window.pageYOffset
                + offset

            window.scrollTo
                ({| top = position
                    behavior = "smooth" |}
                 |> unbox) // behavior = smooth | auto
            window.history.pushState (null, null, "#" + visualizationId)

        Html.div
            [ prop.className "title-brand-wrapper"
              prop.children
                  [ Html.h2
                      [ prop.children
                          [ Html.a
                              [ prop.href ("#" + visualization.ClassName)
                                prop.text visualization.Label
                                prop.onClick (fun e -> scrollToElement e visualization.ClassName) ] ] ] ] ]

    Html.div
        [ prop.className
            [ true, "visualization container"
              embedded, "embeded" ]
          prop.children
              (visualizations
               |> List.map (fun viz ->
                   Html.section
                       [ prop.className
                           [ true, viz.ClassName
                             true, "visualization-chart" ]
                         prop.id viz.ClassName
                         prop.children
                             [ Html.div
                                 [ prop.className "title-chart-wrapper"
                                   prop.children
                                       [ renderChartTitle viz
                                         renderFaqAndShareBtn viz ] ]
                               state |> viz.Renderer ] ])) ]
