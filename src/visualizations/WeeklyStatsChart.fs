module WeeklyStatsChart

open System.Runtime.InteropServices
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser
open Types
open Highcharts

type DisplayType =
    | Quarantine
    | BySource
    | BySourceCountry
  with
    static member all = [ Quarantine; BySource; BySourceCountry ]
    static member getName = function
        | Quarantine     -> I18N.t "charts.weeklyStats.quarantine"
        | BySource     -> I18N.t "charts.weeklyStats.bySource"
        | BySourceCountry     -> I18N.t "charts.weeklyStats.bySourceCountry"

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
    let bySource = [ImportedCases; ImportRelatedCases;  LocalSource; SourceUnknown]

    let getSeriesInfo =
        function
        | SentToQuarantine ->  "#cccccc", "sentToQuarantine", 0
        | ConfirmedFromQuarantine ->  "#6f42c1", "confirmedFromQuarantine", 1
        | ConfirmedCases ->  "#d5c768", "confirmedCases", 1

        | ImportedCases -> "#007bff", "importedCases", 0
        | ImportRelatedCases -> "#17a2b8", "importRelatedCases", 0
        | SourceUnknown -> "#6610f2", "sourceUnknown", 0
        | LocalSource ->"#ffc107", "localSource", 0

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

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let fmtWeekYearFromTo = pts.[0]?point?fmtWeekYearFromTo
    let arrows p = match p?point?seriesId with
                                   | "confirmedFromQuarantine" -> "↳ "
                                   |_ -> ""

    fmtWeekYearFromTo
    + (pts
       |> Seq.map (fun p ->
           sprintf """<br>%s<span style="color:%s">●</span> %s: <b>%s</b>""" (arrows p) p?series?color p?series?name p?point?fmtTotal)
       |> String.concat "<br>")

let splitOutFromTotal (split : int option) (total : int option)  =
    match split, total with
    | Some split_, Some total_ -> Some (total_ - split_)
    | None, Some _ -> total
    | _ -> None

let renderChartOptions (state: State) dispatch =
    let renderSeries series =
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
        |> pojo

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let className = "covid19-weekly-stats"
    let baseOptions =
        basicChartOptions ScaleType.Linear className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    let selectSeries displayType = match displayType with
                                   | Quarantine -> Series.quarantine
                                   | BySource -> Series.bySource
                                   | BySourceCountry -> Series.bySource

    {| baseOptions with
           chart = pojo
            {|
                animation = false
                ``type`` = "column"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
           series =
               state.displayType
               |> selectSeries
               |> Seq.map (renderSeries)
               |> Seq.toArray
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun yAxis -> {| yAxis with min = None |})
           xAxis =
               baseOptions.xAxis
               |> Array.map (fun xAxis ->
                   {| xAxis with
                          tickInterval = 86400000 * 7
                          startOfWeek = 1 // Monday
                          dateTimeLabelFormats = pojo {| week = I18N.t "charts.common.weekYearDateFormat" |} |})
           tooltip =
               pojo
                   {| shared = true
                      split = false
                      formatter = fun () -> tooltipFormatter jsThis |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           plotOptions = pojo {|
                                column = pojo {|
                                                stacking = "normal" |}

                                |}|}

let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 450 ]
               prop.className "highcharts-wrapper"
               prop.children
                   [ renderChartOptions state dispatch
                     |> chartFromWindow ] ]

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
let render (state: State) dispatch =
    Html.div [
        renderChartContainer state dispatch
        renderDisplaySelectors state dispatch
    ]

let weeklyStatsChart (props: {| data: WeeklyStatsData |}) =
    React.elmishComponent ("WeeklyStatsChart", init props.data, update, render)
