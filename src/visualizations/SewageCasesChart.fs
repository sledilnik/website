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
      GenomeColors: Map<string, string>
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
    let cmdC =
        Cmd.OfAsync.either Data.SewageCases.getOrFetch () ConsumeSewageCasesData ConsumeServerError

    let cmdG =
        Cmd.OfAsync.either Data.SewageGenomes.getOrFetch () ConsumeSewageGenomesData ConsumeServerError

    { DisplayType = DisplayType.Default
      Station = "CČN Ljubljana"
      GenomeColors = Map.empty
      SewageStations = []
      SewageCasesData = [||]
      SewageGenomesData = [||]
      Error = None
      RangeSelectionButtonIndex = 3 },
    (cmdC @ cmdG)

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

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeSewageCasesData(Ok data) ->
        { state with
            SewageCasesData = data
            SewageStations = GetStations data },
        Cmd.none
    | ConsumeSewageCasesData(Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeSewageGenomesData(Ok data) ->
        { state with
            SewageGenomesData = data
            GenomeColors = GetGenomeColors data },
        Cmd.none
    | ConsumeSewageGenomesData(Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | DisplayTypeChanged displayType -> { state with DisplayType = displayType }, Cmd.none
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
        boost = pojo {| enabled = false |} |}


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
                   xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |} |}
    |> pojo

let renderChartContainer state dispatch =
    Html.div
        [ prop.style [ style.height 480 ]
          prop.className "highcharts-wrapper"
          prop.children
              [ match state.DisplayType with
                | EstimatedCases -> renderCasesChart state dispatch |> chartFromWindow
                | GenomesRatio -> renderGenomesChart state dispatch |> chartFromWindow ] ]


let renderStationSelector state dispatch =

    let renderedStations =
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

let renderChart state dispatch =
    Html.div
        [ Utils.renderChartTopControls
              [ renderStationSelector state dispatch
                renderDisplaySelectors state.DisplayType (DisplayTypeChanged >> dispatch) ]
          renderChartContainer state dispatch ]

let render (state: State) dispatch =
    match state.SewageCasesData, state.SewageGenomesData, state.Error with
    | [||], _, None -> Html.div [ Utils.renderLoading ]
    | _, [||], None -> Html.div [ Utils.renderLoading ]
    | _, _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, _, None -> Html.div [ renderChart state dispatch ]

let sewageCasesChart () =
    React.elmishComponent ("SewageCasesChart", init, update, render)
