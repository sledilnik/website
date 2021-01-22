[<RequireQualifiedAccess>]
module SewageChart

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



let municipalityCasesXY (municipalitiesData: MunicipalitiesData) (wastewaterTreatmentPlantKey: string) =
    let wastewaterTreatmentPlant =
        Utils.Dictionaries.wastewaterTreatmentPlants.Item wastewaterTreatmentPlantKey

    let municipalityNames =
        Array.map snd wastewaterTreatmentPlant.Municipalities
        |> Set.ofArray

    municipalitiesData
    |> List.map (fun municipalityData ->
        municipalityData.Date |> jsTimeMidnight,
        municipalityData.Regions
        |> List.sumBy (fun region ->
            region.Municipalities
            |> List.filter (fun mun -> municipalityNames.Contains mun.Name)
            |> List.sumBy (fun mun -> Option.defaultValue 0 mun.ActiveCases)))
    |> Array.ofList

let municipalitySewageXY (sewageData: SewageStats array) wastewaterTreatmentPlantKey =
    sewageData
    |> Array.filter (fun (dp: SewageStats) -> Map.containsKey wastewaterTreatmentPlantKey dp.wastewaterTreatmentPlants)
    |> Array.map (fun (dp: SewageStats) ->
        dp.Date |> jsTimeMidnight,
        (Map.find wastewaterTreatmentPlantKey dp.wastewaterTreatmentPlants)
            .covN1)

let renderSelector (state: State) (wastewaterTreatmentPlant: WastewaterTreatmentPlant) dispatch =
    Html.div [ let isActive =
                   state.ShownWastewaterTreatmentPlants.Contains wastewaterTreatmentPlant.Key

               let style =
                   if isActive then
                       [ style.backgroundColor wastewaterTreatmentPlant.Color
                         style.borderColor wastewaterTreatmentPlant.Color ]
                   else
                       []

               prop.onClick (fun _ ->
                   (match isActive with
                    | true -> HideWastewaterTreatmentPlant
                    | false -> ShowWastewaterTreatmentPlant) wastewaterTreatmentPlant.Key
                   |> dispatch)

               Utils.classes [ (true, "btn btn-sm metric-selector")
                               (isActive, "metric-selector--selected") ]

               prop.style style

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
              visible = true
              crosshair = true |}
           |> pojo
           {| index = 1
              title = {| text = null |}
              labels =
                  pojo
                      {| format = "{value}%"
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
        |> Array.filter (fun (key, _) -> state.ShownWastewaterTreatmentPlants.Contains key)
        |> Array.map (fun (wastewaterTreatmentPlantKey, wastewaterTreatmentPlant) ->
            [|

               pojo
                   {| name = "Aktivni primeri"
                      ``type`` = "line"
                      color = wastewaterTreatmentPlant.Color
                      yAxis = 1
                      dashStyle = DashStyle.Dot.ToString()
                      data = municipalityCasesXY state.MunicipalitiesData wastewaterTreatmentPlantKey |}
               pojo
                   {| name = "cp-luc-pmmov-rawpmmov-n1"
                      ``type`` = "line"
                      color = wastewaterTreatmentPlant.Color
                      yAxis = 0
                      data = municipalitySewageXY state.SewageData wastewaterTreatmentPlantKey |} |])
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
                  href = "https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19" |}
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
                   renderDisplaySelectors state dispatch ]

let chart =
    React.functionComponent (fun (props: {| data: MunicipalitiesData |}) ->
        let state, dispatch =
            React.useElmish (init props.data, update, [||])

        render state dispatch)
