[<RequireQualifiedAccess>]
module SewageChart

open System.Collections.Generic
open Browser
open Fable.Core.JsInterop
open Elmish
open Feliz
open Feliz.UseElmish

open Components.Slider
open Highcharts
open Types
open Data.Sewage
open Utils.Dictionaries

let chartText = I18N.chartText "sewage"

type State =
    { SewageData: SewageStats array
      MunicipalitiesData: MunicipalitiesData
      Error: string option
      ShownWastewaterTreatmentPlants: Set<string>
      RangeSelectionButtonIndex: int }

type Msg =
    | ConsumeSewageData of Result<SewageStats array, string>
    | ConsumeServerError of exn
    | ShowWastewaterTreatmentPlant of string
    | HideWastewaterTreatmentPlant of string
    | RangeSelectionChanged of int



let init municipalitiesData: State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeSewageData ConsumeServerError

    { SewageData = [||]
      MunicipalitiesData = municipalitiesData
      Error = None
      ShownWastewaterTreatmentPlants = Set.singleton "ljubljana"
      (* Utils.Dictionaries.wastewaterTreatmentPlants
          |> Map.toSeq
          |> Seq.map fst
          |> Set.ofSeq*)
      RangeSelectionButtonIndex = 0 },
    cmd

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumeSewageData (Ok data) -> { state with SewageData = data }, Cmd.none
    | ConsumeSewageData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | ShowWastewaterTreatmentPlant key ->
        { state with
              ShownWastewaterTreatmentPlants = Set.singleton key },
        Cmd.none
    //        { state with
//              ShownWastewaterTreatmentPlants = Set.add key state.ShownWastewaterTreatmentPlants },
//        Cmd.none
    | HideWastewaterTreatmentPlant key -> state, Cmd.none
    //{ state with
    //      ShownWastewaterTreatmentPlants = Set.remove key state.ShownWastewaterTreatmentPlants },
    //Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none



let chooseWithSomeYValue (xy: (JsTimestamp * float option) []): (JsTimestamp * float) [] =
    xy
    |> Array.choose (fun v ->
        match v with
        | t, Some v -> Some(t, v)
        | _ -> None)


//let chooseWithSomeYValue = id

let valueOrZero (dict: Dictionary<string, int>) (key: string): int =
    if dict.ContainsKey key then dict.[key] else 0

let connectedMunicipalitiesDataPoints (municipalitiesData: MunicipalitiesData) (wastewaterTreatmentPlantKey: string) =
    let wastewaterTreatmentPlant =
        Utils.Dictionaries.wastewaterTreatmentPlants.Item wastewaterTreatmentPlantKey

    let municipalityNames =
        Array.map snd wastewaterTreatmentPlant.Municipalities
        |> Set.ofArray

    municipalitiesData
    |> List.map (fun municipalityData ->
        municipalityData.Date |> jsTimeMidnight,
        (municipalityData.Regions
         |> List.map (fun region ->
             region.Municipalities
             |> List.filter (fun mun -> municipalityNames.Contains mun.Name)))
        |> List.concat)
    |> Array.ofList
    |> Array.sortBy fst


let connectedMunicipalitiesActiveCasesAsXYSeries (municipalitiesData: MunicipalitiesData)
                                                 (wastewaterTreatmentPlantKey: string)
                                                 =
    let lastSeenActiveCasesByMunicipality = new Dictionary<string, int>()

    connectedMunicipalitiesDataPoints municipalitiesData wastewaterTreatmentPlantKey
    |> Array.map (fun (ts, dps) ->
        for dp in dps do
            lastSeenActiveCasesByMunicipality.[dp.Name] <- (Option.defaultValue
                                                                (valueOrZero lastSeenActiveCasesByMunicipality dp.Name)
                                                                dp.ActiveCases)


        ts,
        lastSeenActiveCasesByMunicipality.Values
        |> Seq.sum)

let connectedMunicipalitiesNewCasesAsXYSeries (municipalitiesData: MunicipalitiesData)
                                              (wastewaterTreatmentPlantKey: string)
                                              =
    let lastSeenConfirmedCasesByMunicipality = new Dictionary<string, int>()
    let mutable before = 0

    connectedMunicipalitiesDataPoints municipalitiesData wastewaterTreatmentPlantKey
    |> Array.map (fun (ts, dps) ->
        for dp in dps do
            lastSeenConfirmedCasesByMunicipality.[dp.Name] <- max
                                                                  (Option.defaultValue 0 dp.ConfirmedToDate)
                                                                  (valueOrZero
                                                                      lastSeenConfirmedCasesByMunicipality
                                                                       dp.Name)

        let now =
            lastSeenConfirmedCasesByMunicipality.Values
            |> Seq.sum

        let diff = now - before
        before <- now
        ts, float diff)
        |> Statistics.calcRunningAverage



let plantCovN1AsXYSeries (sewageData: SewageStats array) wastewaterTreatmentPlantKey =
    sewageData
    |> Array.filter (fun (dp: SewageStats) -> Map.containsKey wastewaterTreatmentPlantKey dp.plants)
    |> Array.map (fun (dp: SewageStats) ->
        dp.Date |> jsTimeMidnight,
        (Map.find wastewaterTreatmentPlantKey dp.plants)
            .covN1Compensated)
    |> chooseWithSomeYValue

let plantCovN2AsXYSeries (sewageData: SewageStats array) wastewaterTreatmentPlantKey =
    sewageData
    |> Array.filter (fun (dp: SewageStats) -> Map.containsKey wastewaterTreatmentPlantKey dp.plants)
    |> Array.map (fun (dp: SewageStats) ->
        dp.Date |> jsTimeMidnight,
        (Map.find wastewaterTreatmentPlantKey dp.plants)
            .covN2Compensated)
    |> chooseWithSomeYValue

let renderSelector (state: State) (wastewaterTreatmentPlant: WastewaterTreatmentPlant) dispatch =
    Html.div [ let isActive =
                   state.ShownWastewaterTreatmentPlants.Contains wastewaterTreatmentPlant.Key

               prop.onClick (fun _ ->
                   (match isActive with
                    | true -> HideWastewaterTreatmentPlant
                    | false -> ShowWastewaterTreatmentPlant) wastewaterTreatmentPlant.Key
                   |> dispatch)

               Utils.classes [ (true, "btn btn-sm metric-selector")
                               (isActive, "metric-selector--selected") ]

               prop.text (wastewaterTreatmentPlant.Name) ]

let renderDisplaySelectors state dispatch =
    Html.div [ prop.className "metrics-selectors"
               prop.children
                   (Utils.Dictionaries.wastewaterTreatmentPlants
                    |> Map.toSeq
                    |> Seq.map (fun dt -> renderSelector state (snd dt) dispatch)) ]

let renderChartOptions (state: State) dispatch =
    let className = "sewage-chart"
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
           |> pojo 
           {| index = 2
              title = {| text = null |}
              labels =
                  pojo
                      {| format = "{value}"
                         align = "center"
                         x = -10
                         reserveSpace = false |}
              showFirstLabel = false
              opposite = true
              visible = true
              crosshair = true |}
           |> pojo |]

    let allSeries =
        Utils.Dictionaries.wastewaterTreatmentPlants
        |> Map.toArray
        |> Array.filter (fun (key, _) -> state.ShownWastewaterTreatmentPlants.Contains key)
        |> Array.map (fun (wastewaterTreatmentPlantKey, wastewaterTreatmentPlant) ->
            [|
               pojo
                   {| name = chartText "newCases7dAve"
                      ``type`` = "line"
                      color = "#bda506"
                      dashStyle = "ShortDot"
                      yAxis = 1
                      data =
                          connectedMunicipalitiesNewCasesAsXYSeries state.MunicipalitiesData wastewaterTreatmentPlantKey |}

               pojo
                   {| name = chartText "activeCases"
                      ``type`` = "line"
                      color = "#dba51d"
                      dashStyle = "Dot"
                      yAxis = 2
                      data =
                          connectedMunicipalitiesActiveCasesAsXYSeries
                              state.MunicipalitiesData
                              wastewaterTreatmentPlantKey |}

               pojo
                   {| name = chartText "concentrationGen1"
                      ``type`` = "line"
                      color = "#d45087"
                      dashStyle = "Solid"
                      yAxis = 0
                      data = plantCovN1AsXYSeries state.SewageData wastewaterTreatmentPlantKey |}

               pojo
                   {| name = chartText "concentrationGen2"
                      ``type`` = "line"
                      color = "#a05195"
                      dashStyle = "Solid"
                      yAxis = 0
                      data = plantCovN2AsXYSeries state.SewageData wastewaterTreatmentPlantKey |} |])
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
                      xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}
           credits =
               {| enabled = true
                  text =
                      sprintf
                          "%s: %s, %s"
                          (I18N.t "charts.common.dataSource")
                          (I18N.tOptions ("charts.common.dsNIB") {| context = localStorage.getItem ("contextCountry") |})
                          (I18N.tOptions
                              ("charts.common.dsNIJZ")
                               {| context = localStorage.getItem ("contextCountry") |})
                  href =
                      "https://www.nib.si/aktualno/novice/1474-sporocilo-za-javnost-merjenje-prisotnosti-sars-cov-2-v-odpadni-vodi-slovenskih-cistilnih-naprav-je-lahko-ucinkovito-orodje-za-spremljanje-epidemije" |}
               |> pojo
           responsive =
               pojo
                   {| rules =
                          [| {| condition = {| maxWidth = 768 |}
                                chartOptions =
                                    {| yAxis =
                                           [| {| labels = {| enabled = false |} |}
                                              {| labels = {| enabled = false |} |} |] |} |} |] |} |}
    |> pojo

let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderChartOptions state dispatch
                               |> chartFromWindow ] ]

let renderChart state dispatch =
    Html.div [ prop.children [ renderChartContainer state dispatch ] ]


let render (state: State) dispatch =
    match state.SewageData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [ renderChartContainer state dispatch
                   renderDisplaySelectors state dispatch
                   Html.div [ Html.text (chartText "muncipalitiesIncluded")
                              Html.ul
                                  (state.ShownWastewaterTreatmentPlants
                                   |> Set.toSeq
                                   |> Seq.map (fun treatmentPlantKey ->
                                       Utils.Dictionaries.wastewaterTreatmentPlants.[treatmentPlantKey]
                                           .Municipalities
                                       |> Array.toSeq
                                       |> Seq.map (fun municipality ->
                                           Html.li [ prop.text
                                                         ((municipality |> snd |> Map.find)
                                                             Utils.Dictionaries.municipalities)
                                                             .Name ]))
                                   |> Seq.concat) ] ]

let chart =
    React.functionComponent (fun (props: {| data: MunicipalitiesData |}) ->
        let state, dispatch =
            React.useElmish (init props.data, update, [||])

        render state dispatch)

// TODO: convert bulleted list of municipalities to simple enumeration with commas? 