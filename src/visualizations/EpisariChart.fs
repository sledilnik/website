[<RequireQualifiedAccess>]
module EpisariChart

open System
open System.Text
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser
open Fable.Core.JsInterop

open Highcharts
open Types

let chartText = I18N.chartText "episari"

type DisplayType =
    | HospitalIn
    | IcuIn
    | Deceased
    static member All =
        [ HospitalIn
          IcuIn
          Deceased ]

    static member Default = HospitalIn

    member this.GetName =
        match this with
        | HospitalIn -> chartText "hospitalIn"
        | IcuIn -> chartText "icuIn"
        | Deceased -> chartText "deceased"

type State =
    { DisplayType: DisplayType
      ChartType: BarChartType
      Data: WeeklyEpisariData
      RangeSelectionButtonIndex: int }

type Msg =
    | DisplayTypeChanged of DisplayType
    | BarChartTypeChanged of BarChartType
    | RangeSelectionChanged of int


let init data : State * Cmd<Msg> =
    let state =
        { DisplayType = DisplayType.Default
          ChartType = AbsoluteChart
          Data = data
          RangeSelectionButtonIndex = 3 }

    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DisplayTypeChanged displayType -> { state with DisplayType = displayType }, Cmd.none
    | BarChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none
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

// Highcharts will sum columns together when there aren't enough pixels to draw them individually
// As data in some of the vaccination charts is cumulative already, the aggregation method must be "high"
// instead of the default "sum"
// Docs: https://api.highcharts.com/highstock/series.column.dataGrouping.approximation
// This fixes https://github.com/sledilnik/website/issues/927
let dataGroupingConfigurationForCumulativeData = pojo {| approximation = "high" |}


let renderAgeChart state dispatch =

    let tooltipFormatter jsThis =
        let points: obj [] = jsThis?points

        match points with
        | [||] -> ""
        | _ ->
            let totalCases =
                points
                |> Array.sumBy (fun point -> float point?point?y)

            let s = StringBuilder()

            let fmtHeader : string = points.[0]?point?fmtHeader

            s.AppendFormat("<b>{0}</b><br/>", fmtHeader)
            |> ignore

            s.Append "<table>" |> ignore


            points
            |> Array.iter
                (fun ageGroup ->
                    let ageGroupLabel = ageGroup?series?name
                    let ageGroupColor = ageGroup?series?color
                    let dataPoint = ageGroup?point

                    let dataValue: int = dataPoint?y

                    match dataValue with
                    | 0 -> ()
                    | _ ->
                        let format =
                            "<td style='color: {0}'>●</td>"+
                            "<td style='text-align: center; padding-left: 6px'>{1}:</td>"+
                            "<td style='text-align: right; padding-left: 6px'>"+
                            "<b>{2}</b></td>" +
                            "<td style='text-align: right; padding-left: 10px'>" +
                            "{3}</td>"

                        let percentage =
                            (float dataValue) * 100. / totalCases
                            |> Utils.percentWith1DecimalFormatter

                        s.Append "<tr>" |> ignore
                        let ageGroupTooltip =
                            System.String.Format
                                (format,
                                 ageGroupColor,
                                 ageGroupLabel,
                                 I18N.NumberFormat.formatNumber(dataValue),
                                 percentage)
                        s.Append ageGroupTooltip |> ignore
                        s.Append "</tr>" |> ignore
                )

            let format =
                "<td></td>"
                + "<td style='text-align: center; padding-left: 6px'><b>{0}:</b></td>"
                + "<td style='text-align: right; padding-left: 6px'><b>{1}</b></td>"
                + "<td style='text-align: right; padding-left: 10px'></td>"

            s.Append "<tr>" |> ignore

            let ageGroupTooltip =
                String.Format(
                    format,
                    I18N.t "charts.common.total",
                    I18N.NumberFormat.formatNumber (totalCases)
                )

            s.Append ageGroupTooltip |> ignore
            s.Append "</tr>" |> ignore

            s.Append "</table>" |> ignore
            s.ToString()


    let getAgeGroupRec (dp: WeeklyEpisariDataPoint) ageGroup population =
        let getValue dp ageGroup =
            let aG =
                dp.PerAge
                |> List.tryFind (fun aG -> aG.GroupKey.AgeFrom = ageGroup.AgeFrom && aG.GroupKey.AgeTo = ageGroup.AgeTo)
            match aG with
            | None -> None
            | Some aG ->
                match state.DisplayType with
                | HospitalIn -> aG.CovidIn
                | IcuIn -> aG.IcuIn
                | Deceased -> aG.Deceased

        let value = getValue dp ageGroup

        {| x = jsDatesMiddle dp.Date dp.DateTo
           y = value
           fmtHeader = I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |}
        |> pojo

    let allAgeGroups =
        state.Data
        |> Array.tryLast
        |> Option.map (fun dp -> dp.PerAge)
        |> Option.defaultValue List.empty
        |> List.filter
            (fun aG ->
                aG.GroupKey.AgeFrom.IsSome // skip mean
                && aG.GroupKey.AgeTo.IsSome || aG.GroupKey.AgeFrom = Some 85) // skip all but 85+
        |> List.mapi
            (fun idx aG ->
                { AgeFrom = aG.GroupKey.AgeFrom
                  AgeTo = aG.GroupKey.AgeTo },
                idx)

    let allSeries =
        seq {
            for ageGroup, idx in allAgeGroups do
                let popStats = Utils.AgePopulationStats.populationStatsForAgeGroup ageGroup
                let population = popStats.Male + popStats.Female
                yield
                    pojo
                        {| name = ageGroup.Label
                           ``type`` = "column"
                           color = AgeGroup.colorOfAgeGroup idx
                           data =
                               state.Data
                               |> Array.skipWhile (fun dp -> dp.PerAge.Length <= 1) // skip if we do not have per AG data, just mean value
                               |> Array.map (fun dp -> getAgeGroupRec dp ageGroup population) |}
        }

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let stackType =
        match state.ChartType with
        | AbsoluteChart -> Some "normal"
        | RelativeChart -> Some "percent"

    let baseOptions =
        basicChartOptions
            Linear
            "covid19-episari-chart"
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
                      series = defaultSeriesOptions stackType |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           tooltip = defaultTooltip "" (fun () -> tooltipFormatter jsThis) |}


let renderChartContainer state dispatch =
    Html.div [ Html.div [ prop.style [ style.height 480 ]
                          prop.className "highcharts-wrapper"
                          prop.children [ renderAgeChart state dispatch |> chartFromWindow ] ] ]

let renderDisplaySelectors (activeDisplayType: DisplayType) dispatch =
    let renderDisplayTypeSelector (displayTypeToRender: DisplayType) =
        let active = displayTypeToRender = activeDisplayType

        Html.div [ prop.onClick (fun _ -> dispatch displayTypeToRender)
                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (active, "selected") ]
                   prop.text displayTypeToRender.GetName ]

    Html.div [ prop.className "chart-display-property-selector"
               DisplayType.All
               |> List.map renderDisplayTypeSelector
               |> prop.children ]

let render state dispatch =
    Html.div [ Utils.renderChartTopControls [ renderDisplaySelectors state.DisplayType (DisplayTypeChanged >> dispatch)
                                              Utils.renderBarChartTypeSelector state.ChartType (BarChartTypeChanged >> dispatch) ]
               renderChartContainer state dispatch ]

let episariChart (props: {| data: WeeklyEpisariData |}) =
    React.elmishComponent ("EpisariChart", init props.data, update, render)
