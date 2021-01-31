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

let AllSchoolTypes = [ 
    "kindergarten",         "#ffa600" 
    "elementary",           "#70a471"
    "highschool",           "#a05195"
    "elementary_special",   "#457844"
    "music",                "#f95d6a"
    "dormitory",            "#665191"
    "institutions",         "#10829a"
]

type DisplayType =
    | Attendees
    | Employees
    | Type of string
    static member All =
        seq {
            yield Attendees
            yield Employees
            for typ, color in AllSchoolTypes do
                yield Type typ
        }
    static member Default = Attendees
    static member GetName =
        function
        | Attendees -> chartText "attendeeCases"
        | Employees -> chartText "employeeCases"
        | Type typ  -> chartText typ


type State =
    { SchoolsData: SchoolsStats array
      Error: string option
      ScaleType: ScaleType
      MetricType: MetricType
      DisplayType: DisplayType
      RangeSelectionButtonIndex: int }


type Msg =
    | ConsumeSchoolsData of Result<SchoolsStats array, string>
    | ConsumeServerError of exn
    | ScaleTypeChanged of ScaleType
    | MetricTypeChanged of MetricType
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

let init: State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeSchoolsData ConsumeServerError

    let state =
        { SchoolsData = [||]
          Error = None
          ScaleType = Linear
          MetricType = MetricType.Default
          DisplayType = DisplayType.Default
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


let chartCredits =
    {| 
        enabled = true
        text = sprintf "%s: %s, %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsMIZS") {| context = localStorage.getItem ("contextCountry") |})
                    (I18N.tOptions ("charts.common.dsNIJZ") {| context = localStorage.getItem ("contextCountry") |})
        href = "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-izobrazevanje-znanost-in-sport/" 
    |}

let renderPerSchoolChart state dispatch =

    let allSeries =
        let sType =
            match state.DisplayType with
            | Type typ -> typ
            | _ -> ""

        match state.MetricType with
        | Active ->
            [ yield
                pojo
                    {| name = chartText "employeesCases"
                       ``type`` = "line"
                       color = "#dba51d"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].employees.active |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "attendeesCases"
                       ``type`` = "line"
                       color = "#bda506"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].attendees.active |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "attendeesQuarantine"
                       ``type`` = "line"
                       color = "#cccccc"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].attendees.quarantined |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "unitsQuarantine"
                       ``type`` = "line"
                       color = "#f4b2e0"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].units.quarantined |> Utils.zeroToNone)) |}
              yield
                pojo
                    {| name = chartText "unitsRemote"
                       ``type`` = "line"
                       color = "#d559b0"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].units.remote |> Utils.zeroToNone))  |}
            ]
        | Today ->
            [ yield
                pojo
                    {| name = chartText "employeesCases"
                       ``type`` = "line"
                       color = "#dba51d"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].employees.confirmed)) |}
              yield
                pojo
                    {| name = chartText "attendeesCases"
                       ``type`` = "line"
                       color = "#bda506"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].attendees.confirmed)) |}
              yield
                pojo
                    {| name = chartText "attendeesQuarantine"
                       ``type`` = "line"
                       color = "#cccccc"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].attendees.to_quarantine)) |}
              yield
                pojo
                    {| name = chartText "unitsQuarantine"
                       ``type`` = "line"
                       color = "#f4b2e0"
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.schoolType.[sType].units.to_quarantine)) |}
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
        credits = pojo chartCredits
    |}


let renderStackedChart state dispatch =

    let allSeries = seq {
        for sType, sColor in AllSchoolTypes do
            yield
                pojo
                    {| name = chartText sType
                       ``type`` = "column"
                       color = sColor
                       data =
                           state.SchoolsData
                           |> Array.map (fun dp -> (dp.JsDate12h, 
                                                    match state.DisplayType, state.MetricType with
                                                    | Attendees, Active -> 
                                                        dp.schoolType.[sType].attendees.active |> Utils.zeroToNone
                                                    | Attendees, Today -> 
                                                        dp.schoolType.[sType].attendees.confirmed |> Utils.zeroToNone
                                                    | Employees, Active -> 
                                                        dp.schoolType.[sType].employees.active |> Utils.zeroToNone 
                                                    | Employees, Today -> 
                                                        dp.schoolType.[sType].employees.confirmed |> Utils.zeroToNone 
                                                    | _, _ -> None)) |} 
    }

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
        series = Seq.toArray allSeries
        yAxis =
            let showFirstLabel = state.ScaleType <> Linear
            baseOptions.yAxis 
            |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel; reversedStacks = false; |})
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
        credits = pojo chartCredits
    |}

let renderChartContainer (state: State) dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ 
                    match state.DisplayType with
                    | Type typ ->
                        renderPerSchoolChart state dispatch |> Highcharts.chartFromWindow
                    | _ ->
                        renderStackedChart state dispatch |> Highcharts.chartFromWindow
               ] ]

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
