module App

open Browser
open Elmish
open Feliz

open Types
open CountriesChartViz.Synthesis
open I18N

let init (query: obj) (visualization: string option) (page: string) =
    let inner () =
        let renderingMode =
            match visualization with
            | None -> Normal
            | Some viz ->
                match viz with
                | "Map" -> Some Map
                | "EuropeMap" -> Some EuropeMap
                | "WorldMap" -> Some WorldMap
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
                | "CountriesCasesPer1M" -> Some CountriesCasesPer1M
                | "CountriesDeathsPer1M" -> Some CountriesDeathsPer1M
                | _ -> None
                |> Embedded

        let initialState =
            {
              Page = page
              Query = query
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
    let hospitals =
          { VisualizationType = Hospitals
            ClassName = "hospitals-chart"
            ChartTextsGroup = "hospitals"
            Explicit = true
            Renderer = fun _ -> lazyView HospitalsChart.hospitalsChart () }
    let metricsComparison =
          { VisualizationType = MetricsComparison
            ClassName = "metrics-comparison-chart"
            ChartTextsGroup = "metricsComparison"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView MetricsComparisonChart.metricsComparisonChart {| data = data |} }
    let spread =
          { VisualizationType = Spread
            ClassName = "spread-chart"
            ChartTextsGroup = "spread"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView SpreadChart.spreadChart {| data = data |} }

    let map =
          { VisualizationType = Map
            ClassName = "map-chart"
            ChartTextsGroup = "map"
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView Map.mapChart {| data = data |} }

    let municipalities =
          { VisualizationType = Municipalities
            ClassName = "municipalities-chart"
            ChartTextsGroup = "municipalities"
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView MunicipalitiesChart.municipalitiesChart {| query = state.Query; data = data |} }

    let europeMap =
          { VisualizationType = EuropeMap
            ClassName = "europe-chart"
            ChartTextsGroup = "europe"
            Explicit = false
            Renderer = fun _ -> lazyView EuropeMap.mapChart EuropeMap.MapToDisplay.Europe }

    let worldMap =
          { VisualizationType = WorldMap
            ClassName = "world-chart"
            ChartTextsGroup = "world"
            Explicit = false
            Renderer = fun _ -> lazyView EuropeMap.mapChart EuropeMap.MapToDisplay.World }

    let ageGroupsTimeline =
          { VisualizationType = AgeGroupsTimeline
            ClassName = "age-groups-trends-chart"
            ChartTextsGroup = "ageGroupsTimeline"
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

    let tests =
          { VisualizationType = Tests
            ClassName = "tests-chart"
            ChartTextsGroup = "tests"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView TestsChart.testsChart {| data = data |} }

    let hCenters =
          { VisualizationType = HCenters
            ClassName = "hcenters-chart"
            ChartTextsGroup = "hCenters"
            Explicit = false
            Renderer = fun _ -> lazyView HCentersChart.hCentersChart () }

    let infections =
          { VisualizationType = Infections
            ClassName = "infections-chart"
            ChartTextsGroup = "infections"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView InfectionsChart.infectionsChart {| data = data |} }

    let cases =
          { VisualizationType = Cases
            ClassName = "cases-chart"
            ChartTextsGroup = "cases"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView CasesChart.casesChart {| data = data |} }

    let patients =
          { VisualizationType = Patients
            ClassName = "patients-chart"
            ChartTextsGroup = "patients"
            Explicit = false
            Renderer = fun _ -> lazyView PatientsChart.patientsChart () }

    let ratios =
          { VisualizationType = Ratios
            ClassName = "ratios-chart"
            ChartTextsGroup = "ratios"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView RatiosChart.ratiosChart {| data = data |} }

    let ageGroups =
          { VisualizationType = AgeGroups
            ClassName = "age-groups-chart"
            ChartTextsGroup = "ageGroups"
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView AgeGroupsChart.renderChart {| data = data |} }

    let regions =
          { VisualizationType = Regions
            ClassName = "regions-chart"
            ChartTextsGroup = "regions"
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView RegionsChart.regionsChart {| data = data |} }

    let countriesCasesPer1M =
          { VisualizationType = CountriesCasesPer1M
            ClassName = "countries-cases-chart"
            ChartTextsGroup = "countriesNewCasesPer1M"
            Explicit = false
            Renderer =
                fun _ ->
                    lazyView CountriesChartViz.Rendering.renderChart
                        { MetricToDisplay = NewCasesPer1M
                          ChartTextsGroup = "countriesNewCasesPer1M"
                        }
          }

    let countriesDeathsPer1M =
          { VisualizationType = CountriesDeathsPer1M
            ClassName = "countries-deaths-chart"
            ChartTextsGroup = "countriesTotalDeathsPer1M"
            Explicit = false
            Renderer =
                fun _ ->
                    lazyView CountriesChartViz.Rendering.renderChart
                        { MetricToDisplay = TotalDeathsPer1M
                          ChartTextsGroup = "countriesTotalDeathsPer1M"
                        }
          }

    let localVisualizations =
        [ hospitals; metricsComparison; spread; map; municipalities
          europeMap; ageGroupsTimeline; tests; hCenters; infections
          cases; patients; ratios; ageGroups; regions
        ]

    let worldVisualizations =
        [ worldMap; countriesCasesPer1M; countriesDeathsPer1M ]

    let allVisualizations =
        [ hospitals; metricsComparison; spread; map; municipalities
          europeMap; worldMap; ageGroupsTimeline; tests; hCenters; infections
          cases; patients; ratios; ageGroups; regions
          countriesCasesPer1M; countriesDeathsPer1M
        ]

    let embedded, visualizations =
        match state.Page, state.RenderingMode with
        | ("local", Normal) ->
            false,
            localVisualizations
            |> List.filter (fun viz -> not viz.Explicit)
        | ("world", Normal) ->
            false,
            worldVisualizations
            |> List.filter (fun viz -> not viz.Explicit)
        | (_, Embedded visualizationType) ->
            match visualizationType with
            | None -> true, []
            | Some visualizationType ->
                true,
                allVisualizations
                |> List.filter
                       (fun viz -> viz.VisualizationType = visualizationType)
        | _ -> invalidOp "BUG: this should never happen."

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

        let context = localStorage.getItem ("contextCountry")

        Html.div
            [ prop.className "title-brand-wrapper"
              prop.children
                  [ Html.h2
                      [ prop.children
                          [ Html.a
                              [ prop.href ("#" + visualization.ClassName)
                                prop.text (tOptions ("charts." + visualization.ChartTextsGroup + ".title") {| context = context |} )
                                prop.onClick (fun e -> scrollToElement e visualization.ClassName) ] ] ] ] ]

    Html.div
        [ Utils.classes
            [(true, "visualization container")
             (embedded, "embeded") ]
          prop.children
              (visualizations
               |> List.map (fun viz ->
                   Html.section
                       [ prop.className [ viz.ClassName; "visualization-chart" ]
                         prop.id viz.ClassName
                         prop.children
                             [ Html.div
                                 [ prop.className "title-chart-wrapper"
                                   prop.children
                                       [ renderChartTitle viz
                                         renderFaqAndShareBtn viz ] ]
                               state |> viz.Renderer ] ])) ]
