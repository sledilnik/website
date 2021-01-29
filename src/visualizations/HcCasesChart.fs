module HcCasesChart

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser
open Types
open Highcharts

let chartText = I18N.chartText "hcCases"

type ChartType =
    | Absolute
    | Relative
with
    static member Default = Absolute
    static member All = [ Absolute; Relative ]
    member this.GetName =
        match this with
        | Absolute -> chartText "absolute"
        | Relative -> chartText "relative"

type DisplayType =
    | Structure
    | Healthcare
  with
    static member All = [ Structure; Healthcare; ]
    static member Default = Structure
    member this.GetName =
        match this with
        | Structure  -> chartText "structure"
        | Healthcare -> chartText "healthcare"

// ---------------------------
// State management
// ---------------------------
type State =
    { scaleType : ScaleType
      chartType: ChartType
      displayType: DisplayType
      data: WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg =
    | RangeSelectionChanged of int
    | ScaleTypeChanged of ScaleType
    | ChartTypeChanged of ChartType
    | DisplayTypeChanged of DisplayType

let init data: State * Cmd<Msg> =
    let state =
        { scaleType = Linear
          chartType = ChartType.Default
          displayType = DisplayType.Default
          data = data
          RangeSelectionButtonIndex = 3 }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with scaleType = scaleType }, Cmd.none
    | ChartTypeChanged chartType ->
        { state with chartType = chartType }, Cmd.none
    | DisplayTypeChanged displayType ->
        { state with displayType = displayType }, Cmd.none

type Series =
    | ConfirmedCases
    | HealthcareCases
    | HealthcareMaleCases
    | HealthcareFemaleCases
    | RhOccupantCases

module Series =
    let structure = [ ConfirmedCases; HealthcareCases; RhOccupantCases ]
    let healthcareSplit = [ HealthcareMaleCases; HealthcareFemaleCases ]

    let getSeriesInfo (state, series) =
        match series with
        | HealthcareCases       -> true,  "#73ccd5", "healthcareEmployeesCases", 1
        | HealthcareMaleCases   -> true,  "#73ccd5", "healthcareMaleCases", 1
        | HealthcareFemaleCases -> true,  "#d99a91", "healthcareFemaleCases", 1
        | RhOccupantCases       -> true,  "#bf5747", "rhOccupantCases", 1
        | ConfirmedCases ->
            match state.chartType with
            | Absolute -> false, "#d5c768", "totalConfirmed", 1
            | Relative -> true,  "#d5c768", "otherCases", 1

let tooltipFormatter jsThis state =
    let pts: obj [] = jsThis?points
    let fmtWeekYearFromTo = pts.[0]?point?fmtWeekYearFromTo
    let arrows p =
        if state.displayType = Structure && state.chartType = Absolute then
            match p?point?seriesId with
            | "healthcareEmployeesCases" -> "↳ "
            | "rhOccupantCases" -> "↳ "
            |_ -> ""
        else    
            ""

    fmtWeekYearFromTo
    + "<br>"
    + (pts
       |> Seq.map (fun p ->
           sprintf """%s<span style="color:%s">●</span> %s: <b>%s</b>""" (arrows p) p?series?color p?series?name (I18N.NumberFormat.formatNumber(p?point?fmtTotal:int)))
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
        | ConfirmedCases -> fun dp -> (match state.chartType with
                                       | Absolute -> dp.ConfirmedCases
                                       | Relative ->
                                            dp.ConfirmedCases
                                            |> splitOutFromTotal dp.HealthcareCases
                                            |> splitOutFromTotal dp.RetirementHomeOccupantCases)

        | HealthcareCases -> fun dp -> dp.HealthcareCases
        | HealthcareMaleCases -> fun dp -> dp.HealthcareMaleCases
        | HealthcareFemaleCases -> fun dp -> dp.HealthcareFemaleCases
        | RhOccupantCases -> fun dp -> dp.RetirementHomeOccupantCases

    let getPointTotal: (WeeklyStatsDataPoint -> int option) =
        match series with
        | ConfirmedCases -> fun dp -> dp.ConfirmedCases
        | HealthcareCases -> fun dp -> dp.HealthcareCases
        | HealthcareMaleCases -> fun dp -> dp.HealthcareMaleCases
        | HealthcareFemaleCases -> fun dp -> dp.HealthcareFemaleCases
        | RhOccupantCases -> fun dp -> dp.RetirementHomeOccupantCases

    let visible, color, seriesId, stack = Series.getSeriesInfo (state, series)
    {|
       visible = visible
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
    match state.chartType with
    | Relative -> ScaleType.Linear
    | Absolute -> state.scaleType

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
                ``type`` = (match state.chartType with
                            | Relative -> "column"
                            | Absolute -> "line")
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
           series = 
                match state.displayType with
                | Structure -> Series.structure |> renderSeries state |> Seq.toArray
                | Healthcare -> Series.healthcareSplit |> renderSeries state |> Seq.toArray
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun yAxis -> {| yAxis with
                                              min = match (scaleType state) with
                                                    | Linear -> Some 0
                                                    | _ -> None
                                              labels = match state.chartType with
                                                       | Relative -> pojo {| format = "{value} %" |}
                                                       | Absolute -> pojo {| format = "{value}" |}

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
                      formatter = fun () -> tooltipFormatter jsThis state
                      |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           plotOptions = pojo {|
                                column = pojo {|
                                                stacking = match state.chartType with
                                                           | Relative -> "percent"
                                                           | Absolute -> "normal" |}

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


let renderChartTypeSelectors (activeChartType: ChartType) dispatch =
    let renderChartTypeSelector (chartType: ChartType) =
        let active = chartType = activeChartType
        Html.div [
            prop.text chartType.GetName
            prop.onClick (fun _ -> dispatch chartType)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
        ]

    let chartTypesSelectors =
        ChartType.All
        |> List.map renderChartTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (chartTypesSelectors)
    ]

let renderDisplaySelector state dt dispatch =
    Html.div [
        prop.onClick (fun _ -> DisplayTypeChanged dt |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (state.displayType = dt, "metric-selector--selected") ]
        prop.text dt.GetName
    ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.All
            |> List.map (fun dt -> renderDisplaySelector state dt dispatch) ) ]


let render (state: State) dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderChartTypeSelectors
                state.chartType (ChartTypeChanged >> dispatch)
            Utils.renderMaybeVisible (state.chartType = Absolute) [
                Utils.renderScaleSelector
                    state.scaleType (ScaleTypeChanged >> dispatch)]
        ]
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
