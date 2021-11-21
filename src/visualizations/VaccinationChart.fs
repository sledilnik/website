[<RequireQualifiedAccess>]
module VaccinationChart

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

let chartText = I18N.chartText "vaccination"

type ScaleType =
    | Absolute
    | Relative
    | PopulationShare

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
    | Protected
    | ByAge1st
    | ByAgeAll
    | ByAge3rd
    static member All = [ ByAgeAll; ByAge1st; ByAge3rd; Protected ]
    static member Default = ByAgeAll

    static member GetName =
        function
        | ByAge1st -> chartText "byAge1st"
        | ByAgeAll -> chartText "byAgeAll"
        | ByAge3rd -> chartText "byAge3rd"
        | Protected -> chartText "protectedEstimated"

type State =
    { VaccinationData: VaccinationStats array
      Error: string option
      DisplayType: DisplayType
      MetricType: MetricType
      ScaleType: ScaleType
      RangeSelectionButtonIndex: int }

type Msg =
    | ConsumeVaccinationData of Result<VaccinationStats array, string>
    | ConsumeServerError of exn
    | DisplayTypeChanged of DisplayType
    | MetricTypeChanged of MetricType
    | ScaleTypeChanged of ScaleType
    | RangeSelectionChanged of int

let currentScaleType state =
    match state.DisplayType with
    | Protected -> PopulationShare
    | _ -> state.ScaleType

let currentMetricType state =
    match state.DisplayType with
    | Protected -> ToDate
    | _ -> state.MetricType

let init: State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeVaccinationData ConsumeServerError

    let state =
        { VaccinationData = [||]
          Error = None
          DisplayType = DisplayType.Default
          MetricType = MetricType.Default
          ScaleType = ScaleType.Absolute
          RangeSelectionButtonIndex = 0 }

    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeVaccinationData (Ok data) -> { state with VaccinationData = data }, Cmd.none
    | ConsumeVaccinationData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | DisplayTypeChanged dt -> { state with DisplayType = dt }, Cmd.none
    | MetricTypeChanged mt -> { state with MetricType = mt }, Cmd.none
    | ScaleTypeChanged st -> { state with ScaleType = st }, Cmd.none
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

let subtractSafely prev curr  =
    match curr, prev with
    | Some c, Some p -> Some(c - p)
    | Some c, None -> Some c
    | _ -> None

let addSafely prev curr  =
    match curr, prev with
    | Some c, Some p -> Some(c + p)
    | Some c, None -> Some c
    | None, Some p -> Some p
    | _ -> None

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
            let totalVaccinated =
                points
                |> Array.sumBy (fun point -> float point?point?vaccinated)

            let totalPopulation =
                points
                |> Array.sumBy (fun point -> float point?point?population)

            let s = StringBuilder()

            let date = points.[0]?point?date

            s.AppendFormat("<b>{0}</b><br/>", date.ToString())
            |> ignore

            s.Append "<table>" |> ignore

            points
            |> Array.sortByDescending
                (fun ag ->
                    match currentScaleType state with
                    | Absolute -> 0.
                    | Relative -> 0.
                    | PopulationShare ->
                        ((float) ag?point?vaccinated
                         / (float) ag?point?population))
            |> Array.iter
                (fun ageGroup ->
                    let ageGroupLabel = ageGroup?series?name
                    let ageGroupColor = ageGroup?series?color
                    let dataPoint = ageGroup?point

                    let dataValue: int = dataPoint?vaccinated
                    let population: int = dataPoint?population

                    match dataValue with
                    | 0 -> ()
                    | _ ->
                        let format =
                            "<td style='color: {0}'>●</td>"
                            + "<td style='text-align: center; padding-left: 6px'>{1}:</td>"
                            + "<td style='text-align: right; padding-left: 6px'>{2}</td>"
                            + "<td style='text-align: right; padding-left: 10px'><b>{3}</b></td>"

                        let percentage =
                            float dataValue * 100. / float population
                            |> Utils.percentWith1DecimalFormatter

                        s.Append "<tr>" |> ignore

                        let ageGroupTooltip =
                            String.Format(
                                format,
                                ageGroupColor,
                                ageGroupLabel,
                                I18N.NumberFormat.formatNumber (dataValue),
                                percentage
                            )

                        s.Append ageGroupTooltip |> ignore
                        s.Append "</tr>" |> ignore)

            let format =
                "<td></td>"
                + "<td style='text-align: center; padding-left: 6px'><b>{0}:</b></td>"
                + "<td style='text-align: right; padding-left: 6px'>{1}</td>"
                + "<td style='text-align: right; padding-left: 10px'><b>{2}</b></td>"

            let percentage =
                float totalVaccinated * 100.
                / float totalPopulation
                |> Utils.percentWith1DecimalFormatter

            s.Append "<tr>" |> ignore

            let ageGroupTooltip =
                String.Format(
                    format,
                    I18N.t "charts.common.total",
                    I18N.NumberFormat.formatNumber (totalVaccinated),
                    percentage
                )

            s.Append ageGroupTooltip |> ignore
            s.Append "</tr>" |> ignore

            s.Append "</table>" |> ignore
            s.ToString()

    let getAgeGroupRec (i: int) (dp: VaccinationStats) ageGroup population =
        let getAgeGroup dp ageGroup =
            dp.administeredPerAge
            |> List.find
                (fun aG ->
                    aG.ageFrom = ageGroup.AgeFrom
                    && aG.ageTo = ageGroup.AgeTo)

        let getValue dp ageGroup =
            let aG = getAgeGroup dp ageGroup
            match state.DisplayType with
            | Protected
            | ByAgeAll -> aG.administered2nd
            | ByAge1st -> aG.administered
            | ByAge3rd -> aG.administered3rd

        let value =
            match state.DisplayType, currentMetricType state with
            | Protected, _ ->
                if i >= 15 then             // need 14 days for protection
                    if i >= 15 + 183 then   // waning protection after 6 months + 3rd dose protected
                        getValue state.VaccinationData.[i-15] ageGroup
                        |> subtractSafely (getValue state.VaccinationData.[i-15-183] ageGroup)
                        |> addSafely (getAgeGroup state.VaccinationData.[i-15] ageGroup).administered3rd
                    else
                        getValue state.VaccinationData.[i-15] ageGroup
                else
                    None
            | _, Today ->
                if i > 0 then
                    getValue dp ageGroup
                    |> subtractSafely (getValue state.VaccinationData.[i-1] ageGroup)
                else
                    getValue dp ageGroup
            | _, ToDate ->
                getValue dp ageGroup

        let y =
            match currentScaleType state with
            | Absolute
            | Relative ->
                match value with
                | Some v -> Some((float) v)
                | _ -> None
            | PopulationShare ->
                match value with
                | Some v -> Some((float) v / (float) population * 100.)
                | _ -> None

        {| x = dp.JsDate12h
           y = y
           vaccinated = value
           population = population
           date = I18N.tOptions "days.longerDate" {| date = dp.Date |} |}
        |> pojo

    let allAgeGroups =
        state.VaccinationData
        |> Array.tryLast
        |> Option.map (fun dp -> dp.administeredPerAge)
        |> Option.defaultValue List.empty
        |> List.mapi
            (fun idx aG ->
                { AgeFrom = aG.ageFrom
                  AgeTo = aG.ageTo },
                idx)

    let ageGroupColors =
        [| "#FFDA6B"
           "#E9B825"
           "#AEEFDB"
           "#80DABF"
           "#52C4A2"
           "#43B895"
           "#33AB87"
           "#2DA782"
           "#26A37D"
           "#189A73"
           "#F4B2E0"
           "#E586C8"
           "#D559B0"
           "#C33B9A"
           "#B01C83"
           "#9e1975" |]

    // SURS 2021-H1: see https://docs.google.com/spreadsheets/d/1aBOp6GW7RpUg3GcrHThWb-6Czj8mlgATlHF2dUw9rTU/edit#gid=2092138705
    let ageGroupPopulation =
        [| 374210
           141126
           113005
           133642
           150074
           161814
           152766
           149801
           152352
           144472
           136527
           107823
           75738
           61052
           36642
           17933 |]

    let allSeries =
        seq {
            for ageGroup, idx in allAgeGroups do
                yield
                    pojo
                        {| name = ageGroup.Label
                           ``type`` =
                               match currentScaleType state with
                               | Absolute | Relative -> "column"
                               | PopulationShare -> "line"
                           color = ageGroupColors.[idx]
                           data =
                               state.VaccinationData
                               |> Array.mapi (fun i dp -> getAgeGroupRec i dp ageGroup ageGroupPopulation.[idx]) |}
        }

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let stackType =
        match currentScaleType state with
        | Absolute -> Some "normal"
        | Relative -> Some "percent"
        | PopulationShare -> None

    let baseOptions =
        basicChartOptions
            Linear
            "covid19-vaccination-stacked"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick

    {| baseOptions with
           series = Seq.toArray allSeries
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
           plotOptions =
               pojo
                   {| line = pojo {| marker = pojo {| enabled = false |} |}
                      column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                      series = defaultSeriesOptions stackType |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           tooltip = defaultTooltip "" (fun () -> tooltipFormatter jsThis) |}

let renderChartContainer (state: State) dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderAgeChart state dispatch
                               |> Highcharts.chartFromWindow ] ]

let renderScaleTypeSelectors state dispatch =
    let renderScaleTypeSelector (scaleType: ScaleType) (activeScaleType: ScaleType) (label: string) =
        let defaultProps =
            [ prop.text label
              Utils.classes [ (true, "chart-display-property-selector__item")
                              (scaleType = activeScaleType, "selected") ] ]

        if scaleType = activeScaleType then
            Html.div defaultProps
        else
            Html.div (
                (prop.onClick (fun _ -> dispatch scaleType))
                :: defaultProps
            )

    Html.div [ prop.className "chart-display-property-selector"
               prop.children [ renderScaleTypeSelector Absolute state.ScaleType (I18N.t "charts.common.absolute")
                               renderScaleTypeSelector Relative state.ScaleType (I18N.t "charts.common.relative")
                               renderScaleTypeSelector PopulationShare state.ScaleType (I18N.t "charts.common.populationShare") ] ]

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

                                                  if state.DisplayType <> Protected then
                                                    renderMetricTypeSelectors state (MetricTypeChanged >> dispatch)

                                                    renderScaleTypeSelectors state (ScaleTypeChanged >> dispatch)
                                                ]
                   renderChartContainer state dispatch

                   if state.DisplayType = Protected then
                       Html.div [ prop.className "disclaimer"
                                  prop.children [ Utils.Markdown.render (chartText "disclaimer") ] ]
                ]

let vaccinationChart () =
    React.elmishComponent ("VaccinationChart", init, update, render)
