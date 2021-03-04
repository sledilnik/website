[<RequireQualifiedAccess>]
module VaccinationChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser
open Fable.Core.JsInterop

open Types
open Highcharts

open Data.Vaccinations

let chartText = I18N.chartText "vaccination"

type DisplayType =
    | Used
    | ByManufacturer
    static member All = [ Used ; ByManufacturer ]
    static member Default = Used
    static member GetName =
        function
        | Used -> chartText "used"
        | ByManufacturer -> chartText "byManufacturer"

let AllVaccinationTypes = [
    "az",          "#ffa600"
    "moderna",     "#f95d6a"
    "pfizer",      "#73ccd5"
]

type State =
    { VaccinationData: VaccinationStats array
      Error: string option
      DisplayType: DisplayType
      RangeSelectionButtonIndex: int }


type Msg =
    | ConsumeVaccinationData of Result<VaccinationStats array, string>
    | ConsumeServerError of exn
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

let init: State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeVaccinationData ConsumeServerError

    let state =
        { VaccinationData = [||]
          Error = None
          DisplayType = DisplayType.Default
          RangeSelectionButtonIndex = 0 }

    state, cmd

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumeVaccinationData (Ok data) ->
        { state with VaccinationData = data }, Cmd.none
    | ConsumeVaccinationData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | ChangeDisplayType dt ->
        { state with DisplayType = dt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderVaccinationChart state dispatch =

    let allSeries =
        match state.DisplayType with
        | Used ->
            [ yield
                pojo
                    {| name = chartText "deliveredDoses"
                       ``type`` = "line"
                       color = "#73ccd5"
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.deliveredToDate)) |}
              yield
                pojo
                    {| name = chartText "usedDoses"
                       ``type`` = "line"
                       color = "#20b16d"
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.usedToDate)) |}
              yield
                pojo
                    {| name = chartText "administered"
                       ``type`` = "line"
                       color = "#189a73"
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.administered.toDate)) |}
              yield
                pojo
                    {| name = chartText "administered2nd"
                       ``type`` = "line"
                       color = "#0e5842"
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.administered2nd.toDate)) |}
            ]
        | _ -> []

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccination"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = List.toArray allSeries
        yAxis =
            baseOptions.yAxis
            |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
        plotOptions =
            pojo
               {| line = pojo {| dataLabels = pojo {| enabled = false |}; marker = pojo {| enabled = false |} |}
                  series = pojo {| stacking = None |} |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
    |}

let renderStackedChart state dispatch =

    let allSeries = seq {
        for vType, vColor in AllVaccinationTypes do
            yield
                pojo
                    {| name = chartText vType
                       ``type`` = "column"
                       color = vColor
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp ->
                                         (dp.JsDate12h, dp.deliveredByManufacturer.TryFind(vType))) |}
    }

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccination-stacked"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = Seq.toArray allSeries
        yAxis =
            baseOptions.yAxis
            |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
        plotOptions =
            pojo
               {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                  series =
                      {| stacking = "normal"
                         crisp = false
                         borderWidth = 0
                         pointPadding = 0
                         groupPadding = 0 |} |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
    |}


let renderChartContainer (state: State) dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [
                    match state.DisplayType with
                    | Used ->
                        renderVaccinationChart state dispatch |> Highcharts.chartFromWindow
                    | ByManufacturer ->
                        renderStackedChart state dispatch |> Highcharts.chartFromWindow ] ]

let renderDisplaySelectors state dispatch =
    let renderSelector (dt: DisplayType) dispatch =
        Html.div [ let isActive = state.DisplayType = dt
                   prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
                   Utils.classes [ (true, "btn btn-sm metric-selector")
                                   (isActive, "metric-selector--selected") ]
                   prop.text (DisplayType.GetName dt) ]

    Html.div [ prop.className "metrics-selectors"
               prop.children
                   (DisplayType.All
                    |> Seq.map (fun dt -> renderSelector dt dispatch)) ]


let render (state: State) dispatch =
    match state.VaccinationData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
            renderDisplaySelectors state dispatch ]

let vaccinationChart () =
    React.elmishComponent ("VaccinationChart", init, update, render)
