module HcCasesChart

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser
open Types
open Highcharts

let chartText = I18N.chartText "hcCases"

type DisplayType =
    | Healthcare
    | HealthcareRelative
  with
    static member all = [ Healthcare; HealthcareRelative; ]
    static member getName = function
        | Healthcare                -> chartText "healthcareEmployees"
        | HealthcareRelative        -> chartText "healthcareEmployeesRelative"

// ---------------------------
// State management
// ---------------------------
type State =
    { scaleType : ScaleType
      displayType: DisplayType
      data: WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg =
    | RangeSelectionChanged of int
    | ScaleTypeChanged of ScaleType
    | ChangeDisplayType of DisplayType

let init data: State * Cmd<Msg> =
    let state =
        { scaleType = Linear
          displayType = Healthcare
          data = data
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with scaleType = scaleType }, Cmd.none
    | ChangeDisplayType displayType ->
        { state with displayType = displayType },
        Cmd.none

// ---------------------------
// Display Type Selection
// ---------------------------
let renderDisplaySelector state dt dispatch =
    Html.div [
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (state.displayType = dt, "metric-selector--selected") ]
        prop.text (dt |> DisplayType.getName)
    ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.all
            |> List.map (fun dt -> renderDisplaySelector state dt dispatch) ) ]

type Series =
    | ConfirmedCases
    | HealthcareCases

module Series =
    let healthcare = [ ConfirmedCases; HealthcareCases ]

    let getSeriesInfo =
        function
        | HealthcareCases ->  "#73ccd5", "healthcareEmployeesCases", 1
        | ConfirmedCases ->  "#d5c768", "totalConfirmed", 1

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let fmtWeekYearFromTo = pts.[0]?point?fmtWeekYearFromTo
    let arrows p = match p?point?seriesId with
                                   | "healthcareEmployeesCases" -> "↳ "
                                   |_ -> ""

    fmtWeekYearFromTo
    + "<br>"
    + (pts
       |> Seq.map (fun p ->
           sprintf """%s<span style="color:%s">●</span> %s: <b>%s</b>""" (arrows p) p?series?color p?series?name p?point?fmtTotal)
       |> String.concat "<br>")


// ---------------------------
// Data Massaging
// ---------------------------
let splitOutFromTotal (split : int option) (total : int option)  =
    match split, total with
    | Some split_, Some total_ -> Some (total_ - split_)
    | None, Some _ -> total
    | _ -> None

let renderSeries state = Seq.mapi (fun legendIndex series ->
    let getPoint: (WeeklyStatsDataPoint -> int option) =
        match series with
        | ConfirmedCases -> fun dp -> (match state.displayType with
                                       | Healthcare -> dp.ConfirmedCases
                                       | HealthcareRelative -> dp.ConfirmedCases |> splitOutFromTotal dp.HealthcareCases) // Because "relative" is a stacked bar chart

        | HealthcareCases -> fun dp -> dp.HealthcareCases

    let getPointTotal: (WeeklyStatsDataPoint -> int option) =
        match series with
        | ConfirmedCases -> fun dp -> dp.ConfirmedCases
        | HealthcareCases -> fun dp -> dp.HealthcareCases

    let color, seriesId, stack = Series.getSeriesInfo (series)
    {|
       color = color
       name = chartText seriesId
       stack = stack
       animation = false
       legendIndex = legendIndex
       data =
           state.data
           |> Seq.map (fun dp ->
               {| x = jsDatesMiddle dp.Date dp.DateTo
                  y = getPoint dp
                  fmtTotal = getPointTotal dp |> string
                  seriesId = seriesId
                  fmtWeekYearFromTo =
                      I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |}
               |> pojo)
           |> Array.ofSeq |}
    |> pojo)

let scaleType (state:State) =
    match state.displayType with
    | HealthcareRelative -> ScaleType.Linear
    | _ -> state.scaleType

let renderChartOptions (state: State) dispatch =
    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let className = "covid19-healthcare-employees"
    let baseOptions =
        basicChartOptions (scaleType state) className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    let lastWeek = state.data.[state.data.Length-1]

    {| baseOptions with
           chart = pojo
            {|
                animation = false
                ``type`` = (match state.displayType with
                            | HealthcareRelative -> "column"
                            | _ -> "line")
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
           series = (match state.displayType with
                    | Healthcare -> Series.healthcare |> renderSeries state
                    | HealthcareRelative -> Series.healthcare |> renderSeries state
                    ) |> Seq.toArray
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun yAxis -> {| yAxis with
                                              min = match (scaleType state) with
                                                    | Linear -> Some 0
                                                    | _ -> None
                                              labels = match state.displayType with
                                                       |  HealthcareRelative  -> pojo {| format = "{value} %" |}
                                                       | _ -> pojo {| format = "{value}" |}

                                              reversedStacks = true
                                              |})
           xAxis =
               baseOptions.xAxis
               |> Array.map (fun xAxis ->
                   {| xAxis with
                          tickInterval = 86400000 * 7
                          plotBands =
                                [|
                                   {| from=jsTime <| lastWeek.Date
                                      ``to``=jsTime <| lastWeek.DateTo
                                      color="#ffffe0"
                                    |}
                                |]
                             |})
           tooltip =
               pojo
                   {| shared = true
                      split = false
                      useHTML = true
                      formatter = fun () -> tooltipFormatter jsThis
                      |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           plotOptions = pojo {|
                                column = pojo {|
                                                stacking = match state.displayType with
                                                           |  HealthcareRelative -> "percent"
                                                           | _ -> "normal" |}

                                |}
           rangeSelector = configureRangeSelector state.RangeSelectionButtonIndex
                                        [|
                                            {|
                                                ``type`` = "week" // Customized to align x axis to exactly 9 weeks
                                                count = 9
                                                text = I18N.tOptions "charts.common.x_months" {| count = 2 |}
                                                events = pojo {| click = onRangeSelectorButtonClick 0 |}
                                            |}
                                            {|
                                                ``type`` = "month"
                                                count = 3
                                                text = I18N.tOptions "charts.common.x_months" {| count = 3 |}
                                                events = pojo {| click = onRangeSelectorButtonClick 1 |}
                                            |}
                                            {|
                                                ``type`` = "all"
                                                count = 1
                                                text = I18N.t "charts.common.all"
                                                events = pojo {| click = onRangeSelectorButtonClick 2 |}
                                            |}
                                        |]

                                    |}

let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 450 ]
               prop.className "highcharts-wrapper"
               prop.children
                   [ renderChartOptions state dispatch
                     |> chartFromWindow ] ]



let render (state: State) dispatch =
    Html.div [
        Utils.renderChartTopControlRight (
            Utils.renderMaybeVisible (match state.displayType with
                                      | HealthcareRelative -> false
                                      | _ -> true) [
                Utils.renderScaleSelector
                    state.scaleType (ScaleTypeChanged >> dispatch)])
        renderChartContainer state dispatch
        renderDisplaySelectors state dispatch

        Html.div [
            prop.className "disclaimer"
            prop.children [
                Html.text (chartText "disclaimer")
            ]
        ]
    ]

let hcCasesChart (props: {| data: WeeklyStatsData |}) =
    React.elmishComponent ("hcCasesChart", init props.data, update, render)
