[<RequireQualifiedAccess>]
module SewageChart

open System
open System.Collections.Generic
open Browser
open Elmish
open Feliz
open Feliz.UseElmish

open Highcharts
open Types
open Data.Sewage
open Data.SewageCases
open Data.SewageGenomes
open Utils.Dictionaries

let chartText = I18N.chartText "sewage"

let useColors =
    [ "#f95d6a"
      "#d45087"
      "#a05195"
      "#665191"
      "#10829a"
      "#024a66"
      "#f95d6a"
      "#a05195"
      "#024a66"
      "#665191"
      "#10829a"
      "#dba51d"
      "#afa53f"
      "#777c29"
      "#70a471"
      "#457844"
      "#ffa600"
      "#d45087"
      "#dba51d"
      "#afa53f"
      "#777c29"
      "#70a471"
      "#457844" ]

type DisplayType =
    | Cases100k
    | GenomesRatio
    | MeasurementsNIB

    static member All = [ Cases100k; GenomesRatio; MeasurementsNIB ]
    static member Default = GenomesRatio

    member this.GetName =
        match this with
        | Cases100k -> chartText "cases100k"
        | GenomesRatio -> chartText "genomesRatio"
        | MeasurementsNIB -> chartText "measurementsNIB"

type State =
    { DisplayType: DisplayType
      Station: string
      GenomeColors: Map<string, string>
      SewageStations: string list
      SewageCasesData: SewageCases array
      SewageGenomesData: SewageGenomes array
      SewageData: SewageStats array
      MunicipalitiesData: MunicipalitiesData
      Error: string option
      RangeSelectionButtonIndex: int }

type Msg =
    | ConsumeSewageCasesData of Result<SewageCases array, string>
    | ConsumeSewageGenomesData of Result<SewageGenomes array, string>
    | ConsumeSewageData of Result<SewageStats array, string>
    | ConsumeServerError of exn
    | StationChanged of string
    | DisplayTypeChanged of DisplayType
    | RangeSelectionChanged of int


let DefaultStation = "CČN Ljubljana"
let DefaultKey = "ljubljana"

let StationNameToKey name : string = // convert from station name (NLZOH) to key (NIB)
    let found =
        Utils.Dictionaries.wastewaterTreatmentPlants
        |> Map.toArray
        |> Array.tryFind (fun (key, wp) -> wp.Name = name)

    match found with
    | Some(key, _) -> key
    | _ -> DefaultKey

let StationKeyToName key = // convert from station key (NIB) to name (NLZOH)
    let found = Utils.Dictionaries.wastewaterTreatmentPlants |> Map.tryFind key

    match found with
    | Some wp -> wp.Name
    | _ -> DefaultStation

let GetStations (data: SewageCases array) =
    data
    |> Array.map (fun dp -> dp.station)
    |> Array.distinct
    |> Array.sortBy (fun str -> str.Substring(str.IndexOf(' ')))
    |> Array.toList

let GetGenomeColors (data: SewageGenomes array) =
    data
    |> Array.map (fun dp -> dp.genome)
    |> Array.distinct
    |> Array.sort
    |> Array.mapi (fun idx genome -> (genome, useColors.[idx % useColors.Length]))
    |> Map.ofArray

let FixGenomeNames data =
    data
    |> Array.map (fun dp ->
        match dp.genome with
        | "Omikron" ->
            { dp with
                genome = dp.genome + " " + chartText "other" }
        | "Drugo" -> { dp with genome = chartText "other" }
        | _ -> dp)


let init municipalitiesData : State * Cmd<Msg> =
    let cmdS =
        Cmd.OfAsync.either Data.Sewage.getOrFetch () ConsumeSewageData ConsumeServerError

    let cmdC =
        Cmd.OfAsync.either Data.SewageCases.getOrFetch () ConsumeSewageCasesData ConsumeServerError

    let cmdG =
        Cmd.OfAsync.either Data.SewageGenomes.getOrFetch () ConsumeSewageGenomesData ConsumeServerError


    { DisplayType = DisplayType.Default
      Station = DefaultStation
      GenomeColors = Map.empty
      SewageStations = []
      SewageCasesData = [||]
      SewageGenomesData = [||]
      SewageData = [||]
      MunicipalitiesData = municipalitiesData
      Error = None
      RangeSelectionButtonIndex = 3 }, // all to show history
    (cmdS @ cmdC @ cmdG)

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeSewageCasesData(Ok data) ->
        { state with
            SewageCasesData = data
            SewageStations = GetStations data },
        Cmd.none
    | ConsumeSewageCasesData(Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeSewageGenomesData(Ok data) ->
        let fixdata = FixGenomeNames data

        { state with
            SewageGenomesData = fixdata
            GenomeColors = GetGenomeColors fixdata },
        Cmd.none
    | ConsumeSewageGenomesData(Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeSewageData(Ok data) -> { state with SewageData = data }, Cmd.none
    | ConsumeSewageData(Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | DisplayTypeChanged displayType ->
        let station =
            match state.DisplayType, displayType with
            | Cases100k, MeasurementsNIB
            | GenomesRatio, MeasurementsNIB -> StationNameToKey(state.Station) // NLZOH name -> NIB key
            | MeasurementsNIB, Cases100k
            | MeasurementsNIB, GenomesRatio -> StationKeyToName(state.Station) // NIB key -> to NLZOH name
            | _, _ -> state.Station

        { state with
            DisplayType = displayType
            Station = station },
        Cmd.none
    | StationChanged station -> { state with Station = station }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with
            RangeSelectionButtonIndex = buttonIndex },
        Cmd.none


let renderGenomesChart (state: State) dispatch =

    let className = "covid-19-sewage-genomes"

    let renderGenomeSeries =

        let foundGenomes =
            state.SewageGenomesData
            |> Array.filter (fun dp -> dp.station = state.Station)
            |> Array.map (fun dp -> dp.genome)
            |> Array.distinct
            |> Array.sort
            |> Array.toList

        let genomeToSeries genomeName =
            {| ``type`` = "column"
               name = genomeName
               color = state.GenomeColors.[genomeName]
               stack = 0
               animation = false
               yAxis = 0
               data =
                state.SewageGenomesData
                |> Array.filter (fun dp -> (dp.station = state.Station && dp.genome = genomeName))
                |> Array.map (fun dp -> (dp.JsDate12h, dp.ratio * 100.)) |}
            |> pojo

        foundGenomes |> Seq.map genomeToSeries |> Seq.toArray


    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions ScaleType.Linear className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
        chart =
            pojo
                {| animation = false
                   ``type`` = "column"
                   zoomType = "x"
                   className = className
                   events = pojo {| load = onLoadEvent (className) |} |}

        series = renderGenomeSeries

        yAxis =
            baseOptions.yAxis
            |> Array.map (fun yAxis ->
                {| yAxis with
                    min = None
                    labels = pojo {| format = "{value} %" |}
                    reversedStacks = true |})

        legend =
            pojo
                {| enabled = true
                   layout = "horizontal" |}

        tooltip =
            pojo
                {| shared = true
                   split = false
                   formatter = None
                   snap = 50
                   valueSuffix = "%"
                   valueDecimals = 1
                   xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}

        plotOptions =
            pojo
                {| column =
                    pojo
                        {| dataGrouping = pojo {| enabled = false |}
                           stacking = "percent" |}

                |}
        // As number of data points grow over time, HighCharts will kick into boost mode.
        // For boost mode to work correctly, data points must be [x, y] pairs.
        // Right now are data points are objects in order to shove in extra data for tooltips
        // When performance without boost mode becomes a problem refactor tooltip formatting and use data points in [x, y] form.
        //
        // See:
        //  - https://api.highcharts.com/highcharts/boost.seriesThreshold
        //  - https://assets.highcharts.com/errors/12/
        boost = pojo {| enabled = false |}

        credits = chartCreditsNIJZNLZOH |}



let renderCasesChart (state: State) dispatch =

    let className = "covid19-sewage-cases"

    let allSeries =
        [| yield
               {| name = chartText "estimatedCases"
                  ``type`` = "line"
                  color = "#a05195"
                  lineWidth = 0
                  marker =
                   pojo
                       {| symbol = "diamond"
                          radius = 5
                          enabled = true |}
                  yAxis = 0
                  data =
                   state.SewageCasesData
                   |> Array.filter (fun dp -> dp.station = state.Station)
                   |> Array.map (fun dp -> (dp.JsDate12h, dp.cases.estimated)) |}
               |> pojo

           yield
               {| name = chartText "activeCases"
                  ``type`` = "line"
                  color = "#dba51d"
                  dashStyle = "Dot"
                  marker = pojo {| enabled = false |}
                  yAxis = 0
                  data =
                   state.SewageCasesData
                   |> Array.filter (fun dp -> dp.station = state.Station)
                   |> Array.map (fun dp -> (dp.JsDate12h, dp.cases.active100k)) |}
               |> pojo |]

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        Highcharts.basicChartOptions Linear className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
        series = allSeries

        yAxis =
            baseOptions.yAxis
            |> Array.map (fun yAxis ->
                {| yAxis with
                    labels = pojo {| format = "{value}" |} |})

        legend =
            pojo
                {| enabled = true
                   layout = "horizontal" |}

        tooltip =
            pojo
                {| shared = true
                   split = false
                   formatter = None
                   snap = 50
                   valueSuffix = ""
                   valueDecimals = 1
                   xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}

        credits = chartCreditsNIJZNLZOH |}
    |> pojo


let chooseWithSomeYValue (xy: (JsTimestamp * float option)[]) : (JsTimestamp * float)[] =
    xy
    |> Array.choose (fun v ->
        match v with
        | t, Some v -> Some(t, v)
        | _ -> None)


let valueOrZero (dict: Dictionary<string, int>) (key: string) : int =
    if dict.ContainsKey key then dict.[key] else 0

let connectedMunicipalitiesDataPoints (municipalitiesData: MunicipalitiesData) (wastewaterTreatmentPlantKey: string) =
    let wastewaterTreatmentPlant =
        Utils.Dictionaries.wastewaterTreatmentPlants.Item wastewaterTreatmentPlantKey

    let municipalityNames =
        Array.map snd wastewaterTreatmentPlant.Municipalities |> Set.ofArray

    municipalitiesData
    |> List.filter (fun municipalityData -> municipalityData.Date < DateTime(2023, 3, 1)) // FILTER: we have sewage data from NIB only until end 28.2.2023
    |> List.map (fun municipalityData ->
        municipalityData.Date |> jsTimeMidnight,
        (municipalityData.Regions
         |> List.map (fun region ->
             region.Municipalities
             |> List.filter (fun mun -> municipalityNames.Contains mun.Name)))
        |> List.concat)
    |> Array.ofList
    |> Array.sortBy fst


let connectedMunicipalitiesActiveCasesAsXYSeries
    (municipalitiesData: MunicipalitiesData)
    (wastewaterTreatmentPlantKey: string)
    =
    let lastSeenActiveCasesByMunicipality = new Dictionary<string, int>()

    connectedMunicipalitiesDataPoints municipalitiesData wastewaterTreatmentPlantKey
    |> Array.map (fun (ts, dps) ->
        for dp in dps do
            lastSeenActiveCasesByMunicipality.[dp.Name] <-
                (Option.defaultValue (valueOrZero lastSeenActiveCasesByMunicipality dp.Name) dp.ActiveCases)


        ts, lastSeenActiveCasesByMunicipality.Values |> Seq.sum)

let connectedMunicipalitiesNewCasesAsXYSeries
    (municipalitiesData: MunicipalitiesData)
    (wastewaterTreatmentPlantKey: string)
    =
    let lastSeenConfirmedCasesByMunicipality = new Dictionary<string, int>()
    let mutable before = 0

    connectedMunicipalitiesDataPoints municipalitiesData wastewaterTreatmentPlantKey
    |> Array.map (fun (ts, dps) ->
        for dp in dps do
            lastSeenConfirmedCasesByMunicipality.[dp.Name] <-
                max
                    (Option.defaultValue 0 dp.ConfirmedToDate)
                    (valueOrZero lastSeenConfirmedCasesByMunicipality dp.Name)

        let now = lastSeenConfirmedCasesByMunicipality.Values |> Seq.sum

        let diff = now - before
        before <- now
        ts, float diff)
    |> Statistics.calcRunningAverage


let plantCovN2AsXYSeries (sewageData: SewageStats array) wastewaterTreatmentPlantKey =
    sewageData
    |> Array.filter (fun (dp: SewageStats) -> Map.containsKey wastewaterTreatmentPlantKey dp.plants)
    |> Array.map (fun (dp: SewageStats) ->
        dp.Date |> jsTimeMidnight, (Map.find wastewaterTreatmentPlantKey dp.plants).covN2Compensated)
    |> chooseWithSomeYValue


let renderMeasurementNIBChart (state: State) dispatch =
    let className = "covid19-sewage-nib"
    let scaleType = ScaleType.Linear

    let allYAxis =
        [| {| index = 0
              title = {| text = null |}
              labels =
               pojo
                   {| format = "{value}"
                      align = "center"
                      x = -15
                      reserveSpace = false |}
              showFirstLabel = false
              opposite = true
              visible = false
              ``type`` = "logarithmic"
              crosshair = true |}
           |> pojo
           {| index = 1
              title = {| text = null |}
              labels =
               pojo
                   {| format = "{value}"
                      align = "center"
                      x = 10
                      reserveSpace = false |}
              showFirstLabel = false
              opposite = false
              visible = true
              crosshair = true |}
           |> pojo |]

    let allSeries =
        Utils.Dictionaries.wastewaterTreatmentPlants
        |> Map.toArray
        |> Array.filter (fun (key, wp) -> key = state.Station) // TODO
        |> Array.map (fun (wastewaterTreatmentPlantKey, wastewaterTreatmentPlant) ->
            [| pojo
                   {| name = chartText "concentrationGen2"
                      ``type`` = "line"
                      color = "#a05195"
                      lineWidth = 0
                      marker =
                       pojo
                           {| symbol = "diamond"
                              radius = 5
                              enabled = true |}
                      yAxis = 0
                      data = plantCovN2AsXYSeries state.SewageData wastewaterTreatmentPlantKey |}

               pojo
                   {| name = chartText "activeCases"
                      ``type`` = "line"
                      color = "#dba51d"
                      dashStyle = "Dot"
                      yAxis = 1
                      data =
                       connectedMunicipalitiesActiveCasesAsXYSeries
                           state.MunicipalitiesData
                           wastewaterTreatmentPlantKey |} |])

        |> Array.concat


    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        Highcharts.basicChartOptions scaleType className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
        yAxis = allYAxis
        series = allSeries
        legend =
            pojo
                {| enabled = true
                   layout = "horizontal" |}
        tooltip =
            pojo
                {| shared = true
                   split = false
                   formatter = None
                   snap = 50
                   valueSuffix = ""
                   valueDecimals = None
                   xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}
        credits =
            {| enabled = true
               text =
                sprintf
                    "%s: %s, %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsNIB") {| context = localStorage.getItem ("contextCountry") |})
                    (I18N.tOptions ("charts.common.dsNIJZ") {| context = localStorage.getItem ("contextCountry") |})
               href = "https://www.nib.si/aktualno/novice/1500-monitoring-sars-cov-2-v-odpadnih-vodah" |}
            |> pojo
        responsive =
            pojo
                {| rules =
                    [| {| condition = {| maxWidth = 768 |}
                          chartOptions =
                           {| yAxis =
                               [| {| labels = pojo {| enabled = false |} |}
                                  {| labels = pojo {| enabled = false |} |}
                                  {| labels = pojo {| enabled = false |} |} |] |} |} |] |} |}
    |> pojo

let renderStationSelector state dispatch =

    let renderedStations =
        match state.DisplayType with
        | MeasurementsNIB ->
            Utils.Dictionaries.wastewaterTreatmentPlants
            |> Map.toList
            |> List.sortBy (fun (key, wp) -> wp.Name.Substring(wp.Name.IndexOf(' ')))
            |> List.map (fun (key, wp) -> Html.option [ prop.text wp.Name; prop.value key ])
        | _ ->
            state.SewageStations
            |> List.map (fun station -> Html.option [ prop.text station; prop.value station ])

    Html.select
        [ prop.value (state.Station)
          prop.className "form-control form-control-sm filters__type"
          prop.children renderedStations
          prop.onChange (StationChanged >> dispatch) ]

let renderDisplaySelectors (activeDisplayType: DisplayType) dispatch =
    let renderDisplayTypeSelector (displayTypeToRender: DisplayType) =
        let active = displayTypeToRender = activeDisplayType

        Html.div
            [ prop.onClick (fun _ -> dispatch displayTypeToRender)
              Utils.classes [ (true, "chart-display-property-selector__item"); (active, "selected") ]
              prop.text displayTypeToRender.GetName ]

    Html.div
        [ prop.className "chart-display-property-selector"
          DisplayType.All |> List.map renderDisplayTypeSelector |> prop.children ]


let renderChartContainer state dispatch =
    Html.div
        [ prop.style [ style.height 480 ]
          prop.className "highcharts-wrapper"
          prop.children
              [ match state.DisplayType with
                | Cases100k -> renderCasesChart state dispatch |> chartFromWindow
                | GenomesRatio -> renderGenomesChart state dispatch |> chartFromWindow
                | MeasurementsNIB -> renderMeasurementNIBChart state dispatch |> chartFromWindow ] ]

let renderChart state dispatch =
    Html.div
        [ Utils.renderChartTopControls
              [ renderStationSelector state dispatch
                renderDisplaySelectors state.DisplayType (DisplayTypeChanged >> dispatch) ]
          renderChartContainer state dispatch ]

let render (state: State) dispatch =
    match state.SewageData, state.SewageCasesData, state.SewageGenomesData, state.Error with
    | [||], _, _, None -> Html.div [ Utils.renderLoading ]
    | _, [||], _, None -> Html.div [ Utils.renderLoading ]
    | _, _, [||], None -> Html.div [ Utils.renderLoading ]
    | _, _, _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, _, _, None -> Html.div [ renderChart state dispatch ]


let chart =
    React.functionComponent (fun (props: {| data: MunicipalitiesData |}) ->
        let state, dispatch = React.useElmish (init props.data, update, [||])

        render state dispatch)
