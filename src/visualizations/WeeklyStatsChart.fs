module WeeklyStatsChart

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser
open Types
open Highcharts


type Series =
    | ConfirmedCases
    | SentToQuarantine

module Series =
    let all = [ SentToQuarantine; ConfirmedCases ]

    let getSeriesInfo =
        function
        | SentToQuarantine -> true, "#d96756", "sentToQuarantine"
        | ConfirmedCases -> true, "#d5c768", "confirmedCases"

type State =
    { data: WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg = RangeSelectionChanged of int

let init data: State * Cmd<Msg> =
    let state =
        { data = data
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let fmtWeekYearFromTo = pts.[0]?point?fmtWeekYearFromTo

    fmtWeekYearFromTo
    + (pts
       |> Seq.map (fun p ->
           sprintf """<br><span style="color:%s">●</span> %s: <b>%s</b>""" p?series?color p?series?name p?point?y)
       |> String.concat "<br>")

let renderChartOptions (state: State) dispatch =
    let renderSeries series =
        let getPoint: (WeeklyStatsDataPoint -> int option) =
            match series with
            | ConfirmedCases -> fun dp -> dp.ConfirmedCases
            | SentToQuarantine -> fun dp -> dp.SentToQuarantine

        let visible, color, seriesId = Series.getSeriesInfo (series)
        {| ``type`` = "line"
           visible = visible
           color = color
           name = I18N.tt "charts.weeklyStats" seriesId
           data =
               state.data
               |> Seq.map (fun dp ->
                   {| x = dp.Date |> jsTime
                      y = getPoint dp
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

    let baseOptions =
        basicChartOptions ScaleType.Linear "covid19-weeks" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           series =
               Series.all
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
                      layout = "horizontal" |} |}

let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 450 ]
               prop.className "highcharts-wrapper"
               prop.children
                   [ renderChartOptions state dispatch
                     |> chartFromWindow ] ]

let render (state: State) dispatch =
    Html.div [ renderChartContainer state dispatch ]

let weeklyStatsChart (props: {| data: WeeklyStatsData |}) =
    React.elmishComponent ("WeeklyStatsChart", init props.data, update, render)
