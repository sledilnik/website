module WeeklyStatsChart

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser
open Types
open Highcharts

let countryColors =
    [
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
    | BySource
    | BySourceRelative
    | BySourceCountry
    | BySourceCountryRelative
  with
    static member all = [ Quarantine; BySource; BySourceRelative; BySourceCountry; BySourceCountryRelative ]
    static member getName = function
        | Quarantine     -> I18N.t "charts.weeklyStats.quarantine"
        | BySource     -> I18N.t "charts.weeklyStats.bySource"
        | BySourceRelative     -> I18N.t "charts.weeklyStats.bySourceRelative"
        | BySourceCountry     -> I18N.t "charts.weeklyStats.bySourceCountry"
        | BySourceCountryRelative     -> I18N.t "charts.weeklyStats.bySourceCountryRelative"

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
    let bySource = [ImportedCases; ImportRelatedCases; LocalSource; SourceUnknown; ]

    let getSeriesInfo =
        function
        | SentToQuarantine ->  "#cccccc", "sentToQuarantine", 0
        | ConfirmedFromQuarantine ->  "#665191", "confirmedFromQuarantine", 1
        | ConfirmedCases ->  "#bda506", "confirmedCases", 1

        | ImportedCases -> "#d96756", "importedCases", 0
        | ImportRelatedCases -> "#bf5747", "importRelatedCases", 0
        | SourceUnknown -> "#cccccc", "sourceUnknown", 0
        | LocalSource ->"#d5c768", "localSource", 0


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
                                                                      // color = countryColors.[countryIndex% countryColors.Length]
                                                                      name = I18N.tt "country" countryCode
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
       name = I18N.tt "charts.weeklyStats" seriesId
       stack = stack
       animation = false
       legendIndex = legendIndex
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
                    | BySource | BySourceRelative -> Series.bySource |> renderSeries state
                    | BySourceCountry | BySourceCountryRelative -> renderSeriesImportedByCountry state
                    ) |> Seq.toArray
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun yAxis -> {| yAxis with
                                              min = None
                                              reversedStacks = match state.displayType with
                                                               | Quarantine -> true
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
                                  | Quarantine -> fun () -> tooltipFormatter jsThis
                                  | BySource | BySourceRelative -> fun () -> tooltipFormatterWithTotal (I18N.t "charts.weeklyStats.totalConfirmed") jsThis
                                  | BySourceCountry | BySourceCountryRelative -> fun () -> tooltipFormatterWithTotal (I18N.t "charts.weeklyStats.totalImported") jsThis
                      |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           plotOptions = pojo {|
                                column = pojo {|
                                                stacking = match state.displayType with
                                                           | BySourceRelative | BySourceCountryRelative -> "percent"
                                                           | _ -> "normal" |}

                                |}|}

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

let weeklyStatsChart (props: {| data: WeeklyStatsData |}) =
    React.elmishComponent ("WeeklyStatsChart", init props.data, update, render)
