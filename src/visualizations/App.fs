module App

open Browser
open Elmish
open Feliz

open RegionsChartViz.Synthesis
open Types
open CountriesChartViz.Analysis
open I18N

let init (query: obj) (visualization: string option) (page: string) (apiEndpoint: string)=
    let renderingMode =
        match visualization with
        | None -> Normal
        | Some viz ->
            match viz with
            | "Map" -> Some Map
            | "RegionMap" -> Some RegionMap
            | "EuropeMap" -> Some EuropeMap
            | "WorldMap" -> Some WorldMap
            | "MetricsComparison" -> Some MetricsComparison
            | "DailyComparison" -> Some DailyComparison
            | "Patients" -> Some Patients
            | "IcuPatients" -> Some IcuPatients
            | "CarePatients" -> Some CarePatients
            | "Ratios" -> Some Ratios
            | "Tests" -> Some Tests
            | "Cases" -> Some Cases
            | "Spread" -> Some Spread
            | "Regions" -> Some Regions
            | "Regions100k" -> Some Regions100k
            | "Vaccination" -> Some Vaccination
            | "Schools" -> Some Schools
            | "SchoolStatus" -> Some SchoolStatus
            | "Sewage" -> Some Sewage
            | "Sources" -> Some Sources
            | "HcCases" -> Some HcCases
            | "Municipalities" -> Some Municipalities
            | "AgeGroups" -> Some AgeGroups
            | "AgeGroupsTimeline" -> Some AgeGroupsTimeline
            | "HCenters" -> Some HCenters
            | "Hospitals" -> Some Hospitals
            | "Infections" -> Some Infections
            | "CountriesCasesPer100k" -> Some CountriesCasesPer100k
            | "CountriesActiveCasesPer100k" -> Some CountriesActiveCasesPer100k
            | "CountriesNewDeathsPer100k" -> Some CountriesNewDeathsPer100k
            | "CountriesTotalDeathsPer100k" -> Some CountriesTotalDeathsPer100k
            | "PhaseDiagram" -> Some PhaseDiagram
            | "Deceased" -> Some Deceased
            | "ExcessDeaths" -> Some ExcessDeaths
            | "MetricsCorrelation" -> Some MetricsCorrelation
            | "WeeklyDemographics" -> Some WeeklyDemographics
            | _ -> None
            |> Embedded

    let initialState =
        {
          ApiEndpoint = apiEndpoint
          Page = page
          Query = query
          StatsData = NotAsked
          WeeklyStatsData = NotAsked
          RegionsData = NotAsked
          MunicipalitiesData = NotAsked
          RenderingMode = renderingMode }

    // Request data loading based on the page we are on
    let cmd =
        match page with
        | "local" ->
            Cmd.batch
                [ Cmd.ofMsg StatsDataRequested
                  Cmd.ofMsg WeeklyStatsDataRequested
                  Cmd.ofMsg RegionsDataRequest
                  Cmd.ofMsg MunicipalitiesDataRequest ]
        | "world" ->
            Cmd.batch
                [ Cmd.ofMsg StatsDataRequested
                  Cmd.ofMsg WeeklyStatsDataRequested ]
        | _ ->
            Cmd.batch
                [ Cmd.ofMsg StatsDataRequested
                  Cmd.ofMsg WeeklyStatsDataRequested
                  Cmd.ofMsg RegionsDataRequest
                  Cmd.ofMsg MunicipalitiesDataRequest ]

    initialState, cmd

let update (msg: Msg) (state: State) =
    match msg with
    | StatsDataRequested ->
        match state.StatsData with
        | Loading -> state, Cmd.none
        | _ -> { state with StatsData = Loading }, Cmd.OfAsync.result Data.Stats.load
    | StatsDataLoaded data -> { state with StatsData = data }, Cmd.none
    | WeeklyStatsDataRequested ->
        match state.WeeklyStatsData with
        | Loading -> state, Cmd.none
        | _ -> { state with WeeklyStatsData = Loading }, Cmd.OfAsync.result Data.WeeklyStats.load
    | WeeklyStatsDataLoaded data -> { state with WeeklyStatsData = data }, Cmd.none
    | RegionsDataRequest ->
        match state.RegionsData with
        | Loading -> state, Cmd.none
        | _ -> { state with RegionsData = Loading }, Cmd.OfAsync.result (Data.Regions.load state.ApiEndpoint)
    | RegionsDataLoaded data -> { state with RegionsData = data }, Cmd.none
    | MunicipalitiesDataRequest ->
        match state.MunicipalitiesData with
        | Loading -> state, Cmd.none
        | _ -> { state with MunicipalitiesData = Loading }, Cmd.OfAsync.result (Data.Municipalities.load state.ApiEndpoint)
    | MunicipalitiesDataLoaded data -> { state with MunicipalitiesData = data }, Cmd.none

open Elmish.React

let render (state: State) (_: Msg -> unit) =
    let hospitals =
          { VisualizationType = Hospitals
            ClassName = "hospitals-chart"
            ChartTextsGroup = "hospitals"
            ChartEnabled = true
            Explicit = true
            Renderer = fun _ -> lazyView HospitalsChart.hospitalsChart () }

    let metricsComparison =
          { VisualizationType = MetricsComparison
            ClassName = "metrics-comparison-chart"
            ChartTextsGroup = "metricsComparison"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView MetricsComparisonChart.metricsComparisonChart {| data = data |} }

    let dailyComparison =
          { VisualizationType = DailyComparison
            ClassName = "daily-comparison-chart"
            ChartTextsGroup = "dailyComparison"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView DailyComparisonChart.dailyComparisonChart {| data = data |} }

    let spread =
          { VisualizationType = Spread
            ClassName = "spread-chart"
            ChartTextsGroup = "spread"
            ChartEnabled = true
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
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.MunicipalitiesData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView Map.mapMunicipalitiesChart {| data = data |} }

    let regionMap =
          { VisualizationType = RegionMap
            ClassName = "rmap-chart"
            ChartTextsGroup = "rmap"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView Map.mapRegionChart {| data = data |} }

    let municipalities =
          { VisualizationType = Municipalities
            ClassName = "municipalities-chart"
            ChartTextsGroup = "municipalities"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.MunicipalitiesData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView MunicipalitiesChart.municipalitiesChart {| query = state.Query; data = data |} }

    let europeMap =
          { VisualizationType = EuropeMap
            ClassName = "europe-chart"
            ChartTextsGroup = "europe"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.WeeklyStatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView EuropeMap.mapChart {| mapToDisplay = EuropeMap.MapToDisplay.Europe; data = data |} }

    let worldMap =
          { VisualizationType = WorldMap
            ClassName = "world-chart"
            ChartTextsGroup = "world"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.WeeklyStatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView EuropeMap.mapChart {| mapToDisplay = EuropeMap.MapToDisplay.World; data = data |} }

    let ageGroupsTimeline =
          { VisualizationType = AgeGroupsTimeline
            ClassName = "age-groups-trends-chart"
            ChartTextsGroup = "ageGroupsTimeline"
            ChartEnabled = true
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
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView TestsChart.testsChart () }

    let hCenters =
          { VisualizationType = HCenters
            ClassName = "hcenters-chart"
            ChartTextsGroup = "hCenters"
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView HCentersChart.hCentersChart state.ApiEndpoint }

    let infections =
          { VisualizationType = Infections
            ClassName = "infections-chart"
            ChartTextsGroup = "infections"
            ChartEnabled = true
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
            ChartEnabled = true
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
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView PatientsChart.patientsChart {| hTypeToDisplay = PatientsChart.HospitalType.CovidHospitals |} }

    let patientsICU =
          { VisualizationType = IcuPatients
            ClassName = "icu-patients-chart"
            ChartTextsGroup = "icuPatients"
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView PatientsChart.patientsChart {| hTypeToDisplay = PatientsChart.HospitalType.CovidHospitalsICU |} }

    let patientsCare =
          { VisualizationType = CarePatients
            ClassName = "care-patients-chart"
            ChartTextsGroup = "carePatients"
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView PatientsChart.patientsChart {| hTypeToDisplay = PatientsChart.HospitalType.CareHospitals |} }

    let ratios =
          { VisualizationType = Ratios
            ClassName = "ratios-chart"
            ChartTextsGroup = "ratios"
            ChartEnabled = true
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
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView AgeGroupsChart.renderChart {| data = data |} }

    let regionsAbs =
          { VisualizationType = Regions
            ClassName = "regions-chart"
            ChartTextsGroup = "regions"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        let config: RegionsChartConfig =
                            { RelativeTo = RegionsChartViz.Analysis
                                               .MetricRelativeTo.Absolute
                              ChartTextsGroup = "regions"
                            }
                        let props = {| data = data |}
                        lazyView
                            (RegionsChartViz.Rendering.renderChart config) props
            }

    let regions100k =
          { VisualizationType = Regions100k
            ClassName = "regions-chart-100k"
            ChartTextsGroup = "regions100k"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.RegionsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        let config: RegionsChartConfig =
                            { RelativeTo = RegionsChartViz.Analysis
                                               .MetricRelativeTo.Pop100k
                              ChartTextsGroup = "regions100k"
                            }
                        let props = {| data = data |}
                        lazyView
                            (RegionsChartViz.Rendering.renderChart config) props
         }

    let vaccination =
          { VisualizationType = Vaccination
            ClassName = "vaccination-chart"
            ChartTextsGroup = "vaccination"
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView VaccinationChart.vaccinationChart () }

    let schools =
          { VisualizationType = Schools
            ClassName = "schools-chart"
            ChartTextsGroup = "schools"
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView SchoolsChart.schoolsChart () }

    let schoolStatus =
          { VisualizationType = SchoolStatus
            ClassName = "school-status-chart"
            ChartTextsGroup = "schoolStatus"
            ChartEnabled = true
            Explicit = false
            Renderer = fun _ -> lazyView SchoolStatusChart.schoolStatusChart {| query = state.Query |} }

    let sources =
          { VisualizationType = Sources
            ClassName = "sources-chart"
            ChartTextsGroup = "sources"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.WeeklyStatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView SourcesChart.sourcesChart {| data = data |} }

    let sewage =
          { VisualizationType = Sewage
            ClassName = "sewage-chart"
            ChartTextsGroup = "sewage"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.MunicipalitiesData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView SewageChart.chart {| data = data |} }

    let hcCases =
          { VisualizationType = HcCases
            ClassName = "hc-cases-chart"
            ChartTextsGroup = "hcCases"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.WeeklyStatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView HcCasesChart.hcCasesChart {| data = data |} }

    let deceased =
          { VisualizationType = Deceased
            ClassName = "deceased-chart"
            ChartTextsGroup = "deceased"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success statsData ->
                        lazyView DeceasedViz.Rendering.renderChart statsData
          }

    let countriesCasesPer100k =
          { VisualizationType = CountriesCasesPer100k
            ClassName = "countries-cases-chart"
            ChartTextsGroup = "countriesNewCasesPer100k"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView CountriesChartViz.Rendering.renderChart
                            { StatsData = data
                              MetricToDisplay = NewCasesPer100k
                              ChartTextsGroup = "countriesNewCasesPer100k"
                              DataSource = "dsOWD_NIJZ"
                            }
          }

    let countriesActiveCasesPer100k =
          { VisualizationType = CountriesActiveCasesPer100k
            ClassName = "countries-active-chart"
            ChartTextsGroup = "countriesActiveCasesPer100k"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView CountriesChartViz.Rendering.renderChart
                            { StatsData = data
                              MetricToDisplay = ActiveCasesPer100k
                              ChartTextsGroup = "countriesActiveCasesPer100k"
                              DataSource = "dsOWD_NIJZ"
                            }
          }

    let countriesNewDeathsPer100k =
          { VisualizationType = CountriesNewDeathsPer100k
            ClassName = "countries-new-deaths-chart"
            ChartTextsGroup = "countriesNewDeathsPer100k"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView CountriesChartViz.Rendering.renderChart
                            { StatsData = data
                              MetricToDisplay = NewDeathsPer100k
                              ChartTextsGroup = "countriesNewDeathsPer100k"
                              DataSource = "dsOWD_MZ"
                            }
          }

    let countriesTotalDeathsPer100k =
          { VisualizationType = CountriesTotalDeathsPer100k
            ClassName = "countries-total-deaths-chart"
            ChartTextsGroup = "countriesTotalDeathsPer100k"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView CountriesChartViz.Rendering.renderChart
                            { StatsData = data
                              MetricToDisplay = TotalDeathsPer100k
                              ChartTextsGroup = "countriesTotalDeathsPer100k"
                              DataSource = "dsOWD_MZ"
                            }
          }

//    let countriesDeathsPerCases =
//          { VisualizationType = CountriesDeathsPer100k
//            ClassName = "countries-deaths-per-cases"
//            ChartTextsGroup = "countriesDeathsPerCases"
//            ChartEnabled = true
//            Explicit = false
//            Renderer =
//                fun _ ->
//                    lazyView CountriesChartViz.Rendering.renderChart
//                        { MetricToDisplay = DeathsPerCases
//                          ChartTextsGroup = "countriesDeathsPerCases"
//                        }
//          }

    let phaseDiagram =
          { VisualizationType = PhaseDiagram
            ClassName = "phase-diagram-chart"
            ChartTextsGroup = "phaseDiagram"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView PhaseDiagram.Chart.chart {| data = data |} }

    let excessDeaths =
          { VisualizationType = ExcessDeaths
            ClassName = "excess-deaths-chart"
            ChartTextsGroup = "excessDeaths"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> ExcessDeathsChart.Main.chart {| statsData = data |} }

    let metricsCorrelation =
          { VisualizationType = MetricsCorrelation
            ClassName = "metrics-correlation-chart"
            ChartTextsGroup = "metricsCorrelation"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data ->
                        lazyView MetricsCorrelationViz.Rendering.renderChart
                            {| data = data |} }

    let weeklyDemographics =
          { VisualizationType = WeeklyDemographics
            ClassName = "weekly-demographics-chart"
            ChartTextsGroup = "weeklyDemographics"
            ChartEnabled = true
            Explicit = false
            Renderer =
                fun state ->
                    match state.StatsData with
                    | NotAsked -> Html.none
                    | Loading -> Utils.renderLoading
                    | Failure error -> Utils.renderErrorLoading error
                    | Success data -> lazyView WeeklyDemographicsViz.Rendering.renderChart {| data = data |} }

    let localVisualizations =
        [ hospitals; metricsComparison; dailyComparison; tests; vaccination;
          map; municipalities;
          regions100k; europeMap;
          sewage; schools; (*schoolStatus;*)
          patients; patientsICU; // patientsCare;
          ageGroupsTimeline; weeklyDemographics; ageGroups;
          metricsCorrelation; deceased; excessDeaths
          infections; hcCases;
          sources; cases;
          regionMap; regionsAbs
          phaseDiagram; spread;
          //hCenters
        ]

    let worldVisualizations =
        [ worldMap
          countriesActiveCasesPer100k
          countriesCasesPer100k
          countriesNewDeathsPer100k
          countriesTotalDeathsPer100k
          // countriesDeathsPerCases
        ]

    let allVisualizations =
        [ sewage; metricsCorrelation; hospitals; metricsComparison; spread; dailyComparison; map
          municipalities; sources; vaccination
          europeMap; worldMap; ageGroupsTimeline; tests; hCenters; infections
          cases; patients; patientsICU; patientsCare; deceased; ratios; ageGroups; regionMap; regionsAbs
          regions100k; schools; schoolStatus; hcCases
          countriesCasesPer100k
          countriesActiveCasesPer100k
          countriesNewDeathsPer100k
          countriesTotalDeathsPer100k
          phaseDiagram
          excessDeaths
          weeklyDemographics
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
                  prop.text (t "meta.title") ]


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
                                prop.onClick (fun e -> scrollToElement e visualization.ClassName) ]
                            Html.span
                              [ prop.text ("charts." + visualization.ChartTextsGroup + ".titleMenu")
                                prop.className "hidden" ] ] ] ] ]

    Html.div
        [ Utils.classes [
            (true, "visualization container")
            (embedded, "embeded") ]
          prop.children (
            visualizations
            |> List.map (fun viz ->
                Html.section [
                    prop.className [ viz.ClassName ; "visualization-chart"; (if viz.ChartEnabled then "" else "chart-disabled") ]
                    prop.id viz.ClassName
                    prop.children [
                        Html.div [
                            prop.className "title-chart-wrapper"
                            prop.children [
                                renderChartTitle viz
                                renderFaqAndShareBtn viz
                            ]
                        ]
                        if not viz.ChartEnabled then (
                            Html.div [
                                prop.className "disabled-notice"
                                prop.children [
                                    Utils.Markdown.render (chartText viz.ChartTextsGroup "disabled")
                                ]
                            ]
                        )
                        IntersectionObserver.Component.intersectionObserver
                            {| targetElementId = viz.ClassName
                               content = state |> viz.Renderer
                               options = { IntersectionObserver.defaultOptions with rootMargin = "100px" }
                            |}
                    ] ] ) ) ]
