[<RequireQualifiedAccess>]
module VaccinesChart

open System
open System.Text
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser
open Fable.Core.JsInterop

open Types
open Highcharts

open Data.Vaccinations

let chartText = I18N.chartText "vaccines"

type MetricType =
    | Today
    | ToDate
    static member All = [ Today; ToDate ]
    static member Default = Today

    static member GetName =
        function
        | Today -> I18N.t "charts.common.showToday"
        | ToDate -> I18N.t "charts.common.showToDate"

type DisplayType =
    | Used
    | Delivered
    | Unused
    | ByWeekUsage
    | ByWeekSupply
    static member All =
        [ Used
          Delivered
          Unused
          ByWeekUsage
          ByWeekSupply ]

    static member Default = Used

    static member GetName =
        function
        | Used -> chartText "used"
        | Delivered -> chartText "byManufacturer"
        | Unused -> chartText "unused"
        | ByWeekUsage -> chartText "byWeek"
        | ByWeekSupply -> chartText "byWeekSupply"

let AllVaccinationTypes =
    [ "novavax", "#182958"
      "janssen", "#019cdc"
      "az", "#ffa600"
      "moderna", "#f95d6a"
      "pfizer", "#73ccd5" ]

type State =
    { VaccinationData: VaccinationStats array
      Error: string option
      DisplayType: DisplayType
      MetricType: MetricType
      RangeSelectionButtonIndex: int }

type Msg =
    | ConsumeVaccinationData of Result<VaccinationStats array, string>
    | ConsumeServerError of exn
    | DisplayTypeChanged of DisplayType
    | MetricTypeChanged of MetricType
    | RangeSelectionChanged of int

let init: State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeVaccinationData ConsumeServerError

    let state =
        { VaccinationData = [||]
          Error = None
          DisplayType = DisplayType.Default
          MetricType = MetricType.Default
          RangeSelectionButtonIndex = 0 }

    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeVaccinationData (Ok data) -> { state with VaccinationData = data }, Cmd.none
    | ConsumeVaccinationData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | DisplayTypeChanged dt -> { state with DisplayType = dt }, Cmd.none
    | MetricTypeChanged mt -> { state with MetricType = mt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none

let defaultTooltip hdrFormat formatter =
    {| split = false
       shared = true
       useHTML = true
       formatter = formatter
       headerFormat = hdrFormat
       xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}
    |> pojo

let defaultSeriesOptions stackType =
    {| stacking = stackType
       crisp = false
       borderWidth = 0
       pointPadding = 0
       groupPadding = 0 |}

let subtractSafely curr prev =
    match curr, prev with
    | Some c, Some p -> Some(c - p)
    | Some c, None -> Some c
    | _ -> None

let calcUnusedDoses delivered used =
    match delivered, used with
    | Some d, Some u -> Some(d - u)
    | _ -> None


// Highcharts will sum columns together when there aren't enough pixels to draw them individually
// As data in some of the vaccination charts is cumulative already, the aggregation method must be "high"
// instead of the default "sum"
// Docs: https://api.highcharts.com/highstock/series.column.dataGrouping.approximation
// This fixes https://github.com/sledilnik/website/issues/927
let dataGroupingConfigurationForCumulativeData = pojo {| approximation = "high" |}


let renderStackedChart state dispatch =

    let tooltipFormatter jsThis =
        let points: obj [] = jsThis?points

        match points with
        | [||] -> ""
        | _ ->
            let total =
                points
                |> Array.sumBy (fun point -> float point?point?y)

            let s = StringBuilder()

            let date = points.[0]?point?date

            s.AppendFormat("<b>{0}</b><br/>", date.ToString())
            |> ignore

            s.Append "<table>" |> ignore

            points
            |> Array.iter
                (fun dp ->
                    match dp?point?y with
                    | 0 -> ()
                    | value ->
                        let format =
                            "<td style='color: {0}'>●</td>"
                            + "<td style='text-align: left; padding-left: 6px'>{1}:</td>"
                            + "<td style='text-align: right; padding-left: 6px'><b>{2}</b></td>"

                        s.Append "<tr>" |> ignore

                        let dpTooltip =
                            String.Format(
                                format,
                                dp?series?color,
                                dp?series?name,
                                I18N.NumberFormat.formatNumber (value)
                            )

                        s.Append dpTooltip |> ignore
                        s.Append "</tr>" |> ignore)

            let format =
                "<td></td>"
                + "<td style='text-align: left; padding-left: 6px'><b>{0}:</b></td>"
                + "<td style='text-align: right; padding-left: 6px'><b>{1}</b></td>"

            s.Append "<tr>" |> ignore

            let totalTooltip =
                String.Format(format, I18N.t "charts.common.total", I18N.NumberFormat.formatNumber (total))

            s.Append totalTooltip |> ignore
            s.Append "</tr>" |> ignore

            s.Append "</table>" |> ignore
            s.ToString()

    let getValue currDP prevDP vType =
        match state.DisplayType with
        | Used ->
            match state.MetricType with
            | Today ->
                subtractSafely (currDP.usedByManufacturer.TryFind(vType)) (prevDP.usedByManufacturer.TryFind(vType))
            | ToDate -> currDP.usedByManufacturer.TryFind(vType)
        | Delivered ->
            match state.MetricType with
            | Today ->
                subtractSafely
                    (currDP.deliveredByManufacturer.TryFind(vType))
                    (prevDP.deliveredByManufacturer.TryFind(vType))
            | ToDate -> currDP.deliveredByManufacturer.TryFind(vType)
        | Unused ->
            currDP.deliveredByManufacturer.TryFind(vType)
            |> Utils.subtractIntOption (currDP.usedByManufacturer.TryFind(vType))
        | _ -> None

    let allSeries =
        seq {
            for vType, vColor in AllVaccinationTypes do
                yield
                    pojo
                        {| name = chartText vType
                           ``type`` = "column"
                           color = vColor
                           data =
                               state.VaccinationData
                               |> Array.pairwise
                               |> Array.map
                                   (fun (prevDP, currDP) ->
                                       {| x = currDP.JsDate12h
                                          y = getValue currDP prevDP vType
                                          date = I18N.tOptions "days.longerDate" {| date = currDP.Date |} |}
                                       |> pojo) |}
        }

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccines-stacked" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           series = Seq.toArray allSeries
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
           plotOptions =
               pojo
                   {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                      series = defaultSeriesOptions "normal" |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           tooltip = defaultTooltip "" (fun () -> tooltipFormatter jsThis) |}


let renderWeeklyChart state dispatch =

    let valueToWeeklyDataPoint (date: DateTime) (value: int option) =
        let fromDate = date.AddDays(-6.)

        {| x = jsDatesMiddle fromDate date
           y = value
           fmtHeader = I18N.tOptions "days.weekYearFromToDate" {| date = fromDate; dateTo = date |} |}

    let toWeeklyData (dataArray: VaccinationStats array) =
        dataArray
        |> Array.skipWhile (fun dp -> dp.Date.DayOfWeek <> DayOfWeek.Sunday)
        |> Array.mapi (fun i e -> if i % 7 = 0 then Some(e) else None)
        |> Array.choose id
        |> Array.pairwise

    let allSeries =
        seq {
            yield
                pojo
                    {| name = chartText "usedDoses"
                       ``type`` = "line"
                       color = "#20b16d"
                       data =
                           state.VaccinationData
                           |> toWeeklyData
                           |> Array.map
                               (fun (prevW, currW) ->
                                   valueToWeeklyDataPoint currW.Date (subtractSafely currW.usedToDate prevW.usedToDate)) |}

            if state.DisplayType = ByWeekUsage then
                yield
                    pojo
                        {| name = chartText "administered"
                           ``type`` = "column"
                           color = "#189a73"
                           data =
                               state.VaccinationData
                               |> toWeeklyData
                               |> Array.map
                                   (fun (prevW, currW) ->
                                       valueToWeeklyDataPoint
                                           currW.Date
                                           (subtractSafely currW.administered.toDate prevW.administered.toDate)) |}

                yield
                    pojo
                        {| name = chartText "administered2nd"
                           ``type`` = "column"
                           color = "#0e5842"
                           data =
                               state.VaccinationData
                               |> toWeeklyData
                               |> Array.map
                                   (fun (prevW, currW) ->
                                       valueToWeeklyDataPoint
                                           currW.Date
                                           (subtractSafely currW.administered2nd.toDate prevW.administered2nd.toDate)) |}

                yield
                    pojo
                        {| name = chartText "administered3rd"
                           ``type`` = "column"
                           color = "#2ca25f"
                           data =
                               state.VaccinationData
                               |> toWeeklyData
                               |> Array.map
                                   (fun (prevW, currW) ->
                                       valueToWeeklyDataPoint
                                           currW.Date
                                           (subtractSafely currW.administered3rd.toDate prevW.administered3rd.toDate)) |}

            if state.DisplayType = ByWeekSupply then
                yield
                    pojo
                        {| name = chartText "deliveredDoses"
                           ``type`` = "line"
                           color = "#73ccd5"
                           data =
                               state.VaccinationData
                               |> toWeeklyData
                               |> Array.map
                                   (fun (prevW, currW) ->
                                       valueToWeeklyDataPoint
                                           currW.Date
                                           (subtractSafely currW.deliveredToDate prevW.deliveredToDate)) |}

                yield
                    pojo
                        {| name = chartText "unusedDoses"
                           ``type`` = "line"
                           color = "#ffa600"
                           data =
                               state.VaccinationData
                               |> toWeeklyData
                               |> Array.map
                                   (fun (prevW, currW) ->
                                       valueToWeeklyDataPoint
                                           currW.Date
                                           (calcUnusedDoses currW.deliveredToDate currW.usedToDate)) |}
        }

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccines-weekly" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           series = Seq.toArray allSeries
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
           plotOptions =
               pojo
                   {| line = pojo {| marker = pojo {| enabled = false |} |}
                      column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                      series = defaultSeriesOptions None |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           tooltip = defaultTooltip "<b>{point.fmtHeader}</b><br>" None |}


let renderChartContainer (state: State) dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ match state.DisplayType with
                               | ByWeekUsage
                               | ByWeekSupply ->
                                   renderWeeklyChart state dispatch
                                   |> Highcharts.chartFromWindow
                               | Used
                               | Unused
                               | Delivered ->
                                   renderStackedChart state dispatch
                                   |> Highcharts.chartFromWindow ] ]

let renderMetricTypeSelectors state dispatch =
    let renderMetricTypeSelector (metricTypeToRender: MetricType) =
        let active = metricTypeToRender = state.MetricType

        Html.div [ prop.onClick (fun _ -> dispatch metricTypeToRender)
                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (active, "selected") ]
                   prop.text (MetricType.GetName metricTypeToRender) ]

    let metricTypesSelectors =
        MetricType.All
        |> List.map renderMetricTypeSelector

    Html.div [ prop.className "chart-display-property-selector"
               prop.children (metricTypesSelectors) ]

let renderDisplaySelectors state dispatch =
    let renderSelector (dt: DisplayType) dispatch =
        Html.div [ let isActive = state.DisplayType = dt
                   prop.onClick (fun _ -> DisplayTypeChanged dt |> dispatch)

                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (isActive, "selected") ]

                   prop.text (DisplayType.GetName dt) ]

    Html.div [ prop.className "chart-display-property-selector"
               prop.children (
                   DisplayType.All
                   |> Seq.map (fun dt -> renderSelector dt dispatch)
               ) ]


let render (state: State) dispatch =
    match state.VaccinationData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [ Utils.renderChartTopControls [ renderDisplaySelectors state dispatch

                                                  match state.DisplayType with
                                                  | Used
                                                  | Delivered ->
                                                      renderMetricTypeSelectors state (MetricTypeChanged >> dispatch)
                                                  | _ -> Html.none ]
                   renderChartContainer state dispatch ]

let vaccinesChart () =
    React.elmishComponent ("VaccinesChart", init, update, render)
