module SourcesChart

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser
open Types
open Highcharts

let countryColors =
    [
      "#dba51d"
      "#afa53f"
      "#777c29"
      "#70a471"
      "#457844"
      "#f95d6a"
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
      ]

type DisplayType =
    | Quarantine
    | QuarantineRelative
    | BySource
    | BySourceRelative
    | BySourceCountry
    | BySourceCountryRelative
  with
    static member all = [ Quarantine; QuarantineRelative; BySource; BySourceRelative; BySourceCountry; BySourceCountryRelative ]
    static member getName = function
        | Quarantine     -> I18N.t "charts.sources.quarantine"
        | QuarantineRelative     -> I18N.t "charts.sources.quarantineRelative"
        | BySource     -> I18N.t "charts.sources.bySource"
        | BySourceRelative     -> I18N.t "charts.sources.bySourceRelative"
        | BySourceCountry     -> I18N.t "charts.sources.bySourceCountry"
        | BySourceCountryRelative     -> I18N.t "charts.sources.bySourceCountryRelative"

// ---------------------------
// State management
// ---------------------------
type State =
    { displayType: DisplayType
      data: WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg =
    | RangeSelectionChanged of int
    | ChangeDisplayType of DisplayType

let init data: State * Cmd<Msg> =
    let state =
        { displayType = Quarantine
          data = data
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none
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
    | SentToQuarantine
    | ConfirmedFromQuarantine
    | ImportedCases
    | ImportRelatedCases
    | SourceUnknown
    | LocalSource

module Series =
    let quarantine = [ SentToQuarantine; ConfirmedCases; ConfirmedFromQuarantine ]
    let quarantineRelative = [  ConfirmedCases; ConfirmedFromQuarantine ]
    let bySource = [ImportedCases; ImportRelatedCases; LocalSource; SourceUnknown; ]

    let getSeriesInfo =
        function
        | SentToQuarantine ->  "#cccccc", "sentToQuarantine", 0
        | ConfirmedFromQuarantine ->  "#665191", "confirmedFromQuarantine", 1
        | ConfirmedCases ->  "#bda506", "confirmedCases", 1

        | ImportedCases -> "#d559b0", "importedCases", 0
        | ImportRelatedCases -> "#f4b2e0", "importRelatedCases", 0
        | SourceUnknown -> "#f95d6a", "sourceUnknown", 0
        | LocalSource ->"#F59C9C", "localSource", 0


let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let fmtWeekYearFromTo = pts.[0]?point?fmtWeekYearFromTo
    let arrows p = match p?point?seriesId with
                                   | "confirmedFromQuarantine" -> "↳ "
                                   |_ -> ""

    fmtWeekYearFromTo
    + "<br>"
    + (pts
       |> Seq.map (fun p ->
           sprintf """%s<span style="color:%s">●</span> %s: <b>%s</b>""" (arrows p) p?series?color p?series?name p?point?fmtTotal)
       |> String.concat "<br>")

let tooltipFormatterWithTotal totalText jsThis =
    let pts: obj [] = jsThis?points
    let total = pts |> Array.map (fun p -> p?point?y |> Utils.optionToInt) |> Array.sum
    tooltipFormatter jsThis + sprintf """<br><br><span style="color: rgba(0,0,0,0)">●</span> %s: <b>%s</b>""" totalText (total |> string)



// ---------------------------
// Data Massaging
// ---------------------------
let splitOutFromTotal (split : int option) (total : int option)  =
    match split, total with
    | Some split_, Some total_ -> Some (total_ - split_)
    | None, Some _ -> total
    | _ -> None


// ---------------------------
// Chart Rendering w Highcharts
// ---------------------------
let renderSeriesImportedByCountry (state: State) =
    let countryCodesSortedByTotal = state.data |> Data.WeeklyStats.countryTotals |> Array.map (fun (countryCode, _) -> countryCode)
    let countryToSeries (countryIndex:int) (countryCode:string) =
                                                                      {|
                                                                      stack = 0
                                                                      animation = false
                                                                      legendIndex = countryIndex
                                                                      color = countryColors.[countryIndex% countryColors.Length]
                                                                      name = I18N.tt "country" countryCode
                                                                      pointPlacement = "between"
                                                                      data = state.data |> Seq.map (fun dp -> {|
                                                                                                               x = dp.Date |> jsTime
                                                                                                               y = dp.ImportedFrom.Item countryCode
                                                                                                               fmtTotal = dp.ImportedFrom.Item countryCode |> string
                                                                                                               fmtWeekYearFromTo =
                                                                                                                  I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |}

                                                                                                              |} |> pojo) |> Array.ofSeq
                                                                      |} |> pojo

    countryCodesSortedByTotal |> Seq.mapi countryToSeries

let renderSeries state = Seq.mapi (fun legendIndex series ->
    let getPoint: (WeeklyStatsDataPoint -> int option) =
        match series with
        | ConfirmedCases -> fun dp -> dp.ConfirmedCases |> splitOutFromTotal dp.Source.FromQuarantine
        | SentToQuarantine -> fun dp -> dp.SentToQuarantine
        | ConfirmedFromQuarantine -> fun dp -> dp.Source.FromQuarantine
        | ImportedCases -> fun dp -> dp.Source.Import
        | ImportRelatedCases -> fun dp -> dp.Source.ImportRelated
        | SourceUnknown -> fun dp -> dp.Source.Unknown
        | LocalSource -> fun dp -> dp.Source.Local

    let getPointTotal: (WeeklyStatsDataPoint -> int option) =
        match series with
        | ConfirmedCases -> fun dp -> dp.ConfirmedCases
        | SentToQuarantine -> fun dp -> dp.SentToQuarantine
        | ConfirmedFromQuarantine -> fun dp -> dp.Source.FromQuarantine
        | ImportedCases -> fun dp -> dp.Source.Import
        | ImportRelatedCases -> fun dp -> dp.Source.ImportRelated
        | SourceUnknown -> fun dp -> dp.Source.Unknown
        | LocalSource -> fun dp -> dp.Source.Local

    let color, seriesId, stack = Series.getSeriesInfo (series)
    {|
       color = color
       name = I18N.tt "charts.sources" seriesId
       stack = stack
       animation = false
       legendIndex = legendIndex
       pointPlacement = "between"
       data =
           state.data
           |> Seq.map (fun dp ->
               {| x = dp.Date |> jsTime
                  y = getPoint dp
                  fmtTotal = getPointTotal dp |> string
                  seriesId = seriesId
                  fmtWeekYearFromTo =
                      I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |}
               |> pojo)
           |> Array.ofSeq |}
    |> pojo)

let renderChartOptions (state: State) dispatch =
    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let className = "covid19-weekly-stats"
    let baseOptions =
        basicChartOptions ScaleType.Linear className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           chart = pojo
            {|
                animation = false
                ``type`` = "column"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
           series = (match state.displayType with
                    | Quarantine -> Series.quarantine |> renderSeries state
                    | QuarantineRelative -> Series.quarantineRelative |> renderSeries state
                    | BySource | BySourceRelative -> Series.bySource |> renderSeries state
                    | BySourceCountry | BySourceCountryRelative -> renderSeriesImportedByCountry state
                    ) |> Seq.toArray
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun yAxis -> {| yAxis with
                                              min = None
                                              labels = match state.displayType with
                                                       | QuarantineRelative | BySourceRelative | BySourceCountryRelative ->pojo {| format = "{value} %" |}
                                                       | _ -> pojo {| format = "{value}" |}

                                              reversedStacks = match state.displayType with
                                                               | Quarantine | QuarantineRelative -> true
                                                               | _ -> false |})
           xAxis =
               baseOptions.xAxis
               |> Array.map (fun xAxis ->
                   {| xAxis with
                          tickInterval = 86400000 * 7
                         |})
           tooltip =
               pojo
                   {| shared = true
                      split = false
                      useHTML = true
                      formatter = match state.displayType with
                                  | Quarantine | QuarantineRelative -> fun () -> tooltipFormatter jsThis
                                  | BySource | BySourceRelative -> fun () -> tooltipFormatterWithTotal (I18N.t "charts.sources.totalConfirmed") jsThis
                                  | BySourceCountry | BySourceCountryRelative -> fun () -> tooltipFormatterWithTotal (I18N.t "charts.sources.totalImported") jsThis
                      |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           plotOptions = pojo {|
                                column = pojo {|
                                                stacking = match state.displayType with
                                                           | QuarantineRelative | BySourceRelative | BySourceCountryRelative -> "percent"
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
        renderChartContainer state dispatch
        renderDisplaySelectors state dispatch
    ]

let sourcesChart (props: {| data: WeeklyStatsData |}) =
    React.elmishComponent ("sourcesChart", init props.data, update, render)
