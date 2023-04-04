[<RequireQualifiedAccess>]
module SewageCasesChart

open System
open Browser
open Elmish
open Feliz
open Feliz.ElmishComponents

open Highcharts
open Types
open Data.SewageCases
open Data.SewageGenomes

let chartText = I18N.chartText "sewageCases"

type DisplayType =
    | EstimatedCases
    | GenomesRatio

    static member All = [ EstimatedCases; GenomesRatio ]
    static member Default = EstimatedCases
    member this.GetName =
        match this with
        | EstimatedCases -> chartText "estimatedCases"
        | GenomesRatio -> chartText "genomesRatio"

type State =
    { DisplayType: DisplayType
      Station: string
      SewageStations: string list
      SewageCasesData: SewageCases array
      SewageGenomesData: SewageGenomes array
      Error: string option
      RangeSelectionButtonIndex: int }

type Msg =
    | ConsumeSewageCasesData of Result<SewageCases array, string>
    | ConsumeSewageGenomesData of Result<SewageGenomes array, string>
    | ConsumeServerError of exn
    | StationChanged of string
    | DisplayTypeChanged of DisplayType
    | RangeSelectionChanged of int


let init: State * Cmd<Msg> =
    let cmdC = Cmd.OfAsync.either Data.SewageCases.getOrFetch () ConsumeSewageCasesData ConsumeServerError
    let cmdG = Cmd.OfAsync.either Data.SewageGenomes.getOrFetch () ConsumeSewageGenomesData ConsumeServerError

    { DisplayType = DisplayType.Default
      Station = "CČN Ljubljana"
      SewageStations = []
      SewageCasesData = [||]
      SewageGenomesData = [||]
      Error = None
      RangeSelectionButtonIndex = 3 },
    (cmdC @ cmdG)

let GetStations (data : SewageCases array) =
    data
    |> Seq.map (fun dp -> dp.station)
    |> Seq.distinct
    |> Seq.sortBy (fun str -> str.Substring(str.IndexOf(' ')))
    |> Seq.toList

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumeSewageCasesData (Ok data) -> { state with SewageCasesData = data; SewageStations = GetStations data }, Cmd.none
    | ConsumeSewageCasesData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeSewageGenomesData (Ok data) -> { state with SewageGenomesData = data }, Cmd.none
    | ConsumeSewageGenomesData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | DisplayTypeChanged displayType -> { state with DisplayType = displayType }, Cmd.none
    | StationChanged station -> { state with Station = station }, Cmd.none
    | RangeSelectionChanged buttonIndex -> { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderCasesChart (state: State) dispatch =

    let allSeries =
        [|
            yield
                {|
                    name = chartText "estimatedCases"
                    ``type`` = "line"
                    color = "#a05195"
                    lineWidth = 0
                    marker = pojo {|
                                    symbol = "diamond"
                                    radius = 5
                                    enabled = true |}
                    data =
                        state.SewageCasesData
                        |> Array.filter (fun dp -> dp.station = state.Station)
                        |> Array.map (fun dp -> (dp.JsDate12h, dp.cases.estimated))
                |} |> pojo

            yield
                {|
                    name = chartText "activeCases"
                    ``type`` = "line"
                    color = "#dba51d"
                    dashStyle = "Dot"
                    marker = pojo {| enabled = false |}
                    data =
                        state.SewageCasesData
                        |> Array.filter (fun dp -> dp.station = state.Station)
                        |> Array.map (fun dp -> (dp.JsDate12h, dp.cases.active100k))
                |} |> pojo
        |]

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        Highcharts.basicChartOptions Linear "sewage-cases-chart" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
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
                      xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |} |}
    |> pojo

let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderCasesChart state dispatch
                               |> chartFromWindow ] ]

let renderStationSelector state dispatch =

    let renderedStations =
        state.SewageStations
        |> List.map (fun station ->
            Html.option [
                prop.text station
                prop.value station
            ])

    Html.select [
        prop.value (state.Station)
        prop.className "form-control form-control-sm filters__type"
        prop.children renderedStations
        prop.onChange (StationChanged >> dispatch)
    ]

let renderDisplaySelectors (activeDisplayType: DisplayType) dispatch =
    let renderDisplayTypeSelector (displayTypeToRender: DisplayType) =
        let active = displayTypeToRender = activeDisplayType

        Html.div [ prop.onClick (fun _ -> dispatch displayTypeToRender)
                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (active, "selected") ]
                   prop.text displayTypeToRender.GetName ]

    Html.div [ prop.className "chart-display-property-selector"
               DisplayType.All
               |> List.map renderDisplayTypeSelector
               |> prop.children ]

let renderChart state dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderStationSelector state dispatch
            renderDisplaySelectors state.DisplayType (DisplayTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch
    ]

let render (state: State) dispatch =
    match state.SewageCasesData, state.SewageGenomesData, state.Error with
    | [||], _, None    -> Html.div [ Utils.renderLoading ]
    | _, [||], None    -> Html.div [ Utils.renderLoading ]
    | _, _, Some err   -> Html.div [ Utils.renderErrorLoading err ]
    | _, _, None       -> Html.div [ renderChart state dispatch ]

let sewageCasesChart() =
    React.elmishComponent("SewageCasesChart", init, update, render)
