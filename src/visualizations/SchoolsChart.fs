[<RequireQualifiedAccess>]
module SchoolsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser
open Fable.Core.JsInterop

open Types
open Highcharts

open Data.Schools

let chartText = I18N.chartText "schools"

type MetricType =
    | Active
    | Today
with    
    static member All = [ Active; Today ]
    static member Default = Active
    static member GetName =
        function
        | Active -> chartText "active"
        | Today -> chartText "today"


let DefaultDisplayType = "kindergarten" 
let AllDisplayTypes = [ 
    "kindergarten"
    "elementary"
    "highschool"
    "elementary_special"
    "music"
    "dormitory"
    "institutions"
]

type State =
    { SchoolsData: SchoolsStats array
      Error: string option
      ScaleType: ScaleType
      MetricType: MetricType
      DisplayType: string
      RangeSelectionButtonIndex: int }


type Msg =
    | ConsumeSchoolsData of Result<SchoolsStats array, string>
    | ConsumeServerError of exn
    | ScaleTypeChanged of ScaleType
    | MetricTypeChanged of MetricType
    | ChangeDisplayType of string
    | RangeSelectionChanged of int

let init: State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeSchoolsData ConsumeServerError

    let state =
        { SchoolsData = [||]
          Error = None
          ScaleType = Linear
          MetricType = MetricType.Default
          DisplayType = DefaultDisplayType
          RangeSelectionButtonIndex = 0 }

    state, cmd

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumeSchoolsData (Ok data) ->
        { state with SchoolsData = data }, Cmd.none
    | ConsumeSchoolsData (Error err) -> 
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> 
        { state with Error = Some ex.Message }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none
    | MetricTypeChanged metricType ->
        { state with MetricType = metricType }, Cmd.none
    | ChangeDisplayType dt -> 
        { state with DisplayType = dt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions state dispatch =

    let allSeries =
        match state.MetricType with
        | Active ->
            [ yield
                pojo
                    {| name = chartText "employeesCases"
                       color = "#dba51d"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].employees.active |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "attendeesCases"
                       color = "#bda506"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].attendees.active |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "attendeesQuarantine"
                       color = "#cccccc"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].attendees.quarantined |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "unitsQuarantine"
                       color = "#f4b2e0"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].units.quarantined |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "unitsRemote"
                       color = "#d559b0"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].units.remote |> Utils.zeroToNone))  |}
            ]
        | Today ->
            [ yield
                pojo
                    {| name = chartText "employeesCases"
                       color = "#dba51d"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].employees.confirmed)) |}
              yield
                pojo
                    {| name = chartText "attendeesCases"
                       color = "#bda506"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].attendees.confirmed)) |}
              yield
                pojo
                    {| name = chartText "attendeesQuarantine"
                       color = "#cccccc"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].attendees.to_quarantine)) |}
              yield
                pojo
                    {| name = chartText "unitsQuarantine"
                       color = "#f4b2e0"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[state.DisplayType].units.to_quarantine)) |}
            ]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions state.ScaleType "covid19-schools"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = List.toArray allSeries
        yAxis =
            let showFirstLabel = state.ScaleType <> Linear
            baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel |})
        legend = pojo {| enabled = true ; layout = "horizontal" |}
        credits =
            {| enabled = true
               text = sprintf "%s: %s, %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsMIZS") {| context = localStorage.getItem ("contextCountry") |})
                    (I18N.tOptions ("charts.common.dsNIJZ") {| context = localStorage.getItem ("contextCountry") |})
               href = "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-izobrazevanje-znanost-in-sport/" |} |> pojo
    |}

let renderChartContainer (state: State) dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderChartOptions state dispatch |> Highcharts.chart ] ]


let renderMetricTypeSelectors (activeMetricType: MetricType) dispatch =
    let renderMetricTypeSelector (metricTypeToRender: MetricType) =
        let active = metricTypeToRender = activeMetricType
        Html.div [
            prop.onClick (fun _ -> dispatch metricTypeToRender)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (MetricType.GetName metricTypeToRender)
        ]

    let metricTypesSelectors =
        MetricType.All
        |> List.map renderMetricTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (metricTypesSelectors)
    ]


let renderDisplaySelectors state dispatch =
    let renderSelector state (dt: string) dispatch =
        Html.div [ let isActive = state.DisplayType = dt
                   prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
                   Utils.classes [ (true, "btn btn-sm metric-selector")
                                   (isActive, "metric-selector--selected") ]
                   prop.text (chartText dt) ]

    Html.div [ prop.className "metrics-selectors"
               prop.children
                   (AllDisplayTypes
                    |> Seq.map (fun dt -> renderSelector state dt dispatch)) ]


let render (state: State) dispatch =
    match state.SchoolsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [ 
            Utils.renderChartTopControls [
                renderMetricTypeSelectors
                    state.MetricType (MetricTypeChanged >> dispatch)
                Utils.renderScaleSelector
                    state.ScaleType (ScaleTypeChanged >> dispatch)
            ]
            renderChartContainer state dispatch
            renderDisplaySelectors state dispatch ]

let schoolsChart () =
    React.elmishComponent ("SchoolsChart", init, update, render)
