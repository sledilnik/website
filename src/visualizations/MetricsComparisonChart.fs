[<RequireQualifiedAccess>]
module MetricsComparisonChart

open System
open Browser
open Elmish
open Feliz
open Feliz.UseElmish

open Highcharts
open Types

open Data.Patients

let chartText = I18N.chartText "metricsComparison"

type MetricType =
    | Active
    | Today
    | ToDate

type FullMetricType = {
    MetricType: MetricType
    IsAveraged: bool
}
  with
    member this.Name =
        match this.MetricType, this.IsAveraged with
        | Active, _ -> chartText "showActive"
        | Today, false -> chartText "showToday"
        | Today, true -> chartText "show7DaysAverage"
        | ToDate, _ -> chartText "showToDate"

let availableMetricTypes =
    [ { MetricType = Active; IsAveraged = false }
      { MetricType = Today; IsAveraged = false }
      { MetricType = ToDate; IsAveraged = false }
      { MetricType = Today; IsAveraged = true } ]

let MetricQueryParam = [
    ("active", availableMetricTypes.[0]);
    ("today", availableMetricTypes.[1])
    ("7-days-average", availableMetricTypes.[3])
    ("to-date", availableMetricTypes.[2])] |> Map.ofList

type Metric =
    | PerformedTestsToday
    | PerformedTestsToDate
    | ConfirmedCasesToday
    | ConfirmedCasesToDate
    | ActiveCases
    | RecoveredToDate
    | HospitalIn
    | HospitalOut
    | HospitalToday
    | HospitalToDate
    | HospitalOutToDate
    | ICUIn
    | ICUOut
    | ICUToday
    | ICUToDate
    | VentilatorIn
    | VentilatorOut
    | VentilatorToday
    | VentilatorToDate
    | DeceasedToday
    | DeceasedToDate
    with
        static member UseStatsData metric =
            [PerformedTestsToday; PerformedTestsToDate; ConfirmedCasesToday
             ConfirmedCasesToDate; ActiveCases; RecoveredToDate]
            |> List.contains metric

type MetricCfg = {
    Metric: Metric
    Color : string
    Visible : bool
    Type : MetricType
    Id: string
}

type Metrics = MetricCfg list

module Metrics  =
    let initial = [
        { Metric=ActiveCases;           Color="#dba51d"; Visible=true;  Type=Active; Id="activeCases" }
        { Metric=HospitalToday;         Color="#be7A2a"; Visible=true;  Type=Active; Id="hospitalized" }
        { Metric=ICUToday;              Color="#d96756"; Visible=true;  Type=Active; Id="icu" }
        { Metric=VentilatorToday;       Color="#bf5747"; Visible=true;  Type=Active; Id="ventilator" }
        { Metric=PerformedTestsToday;   Color="#19aebd"; Visible=false; Type=Today;  Id="testsPerformed" }
        { Metric=ConfirmedCasesToday;   Color="#bda506"; Visible=true;  Type=Today;  Id="confirmedCases" }
        { Metric=HospitalIn;            Color="#be7A2a"; Visible=true;  Type=Today;  Id="hospitalAdmitted" }
        { Metric=HospitalOut;           Color="#8cd4b2"; Visible=false; Type=Today;  Id="hospitalDischarged" }
        { Metric=ICUIn;                 Color="#d96756"; Visible=true;  Type=Today;  Id="icuAdmitted" }
        { Metric=ICUOut;                Color="#ffb4a2"; Visible=false; Type=Today;  Id="icuDischarged" }
        { Metric=VentilatorIn;          Color="#bf5747"; Visible=true;  Type=Today;  Id="ventilatorAdmitted" }
        { Metric=VentilatorOut;         Color="#d99a91"; Visible=false; Type=Today;  Id="ventilatorDischarged" }
        { Metric=DeceasedToday;         Color="#6d5b80"; Visible=true;  Type=Today;  Id="deceased" }
        { Metric=PerformedTestsToDate;  Color="#19aebd"; Visible=false; Type=ToDate; Id="testsPerformed" }
        { Metric=ConfirmedCasesToDate;  Color="#bda506"; Visible=true;  Type=ToDate; Id="confirmedCases" }
        { Metric=RecoveredToDate;       Color="#20b16d"; Visible=true;  Type=ToDate; Id="recovered" }
        { Metric=HospitalToDate;        Color="#be7A2a"; Visible=true;  Type=ToDate; Id="hospitalAdmitted" }
        { Metric=HospitalOutToDate;     Color="#8cd4b2"; Visible=false; Type=ToDate; Id="hospitalDischarged" }
        { Metric=ICUToDate;             Color="#d96756"; Visible=false; Type=ToDate; Id="icuAdmitted" }
        { Metric=VentilatorToDate;      Color="#d96756"; Visible=false; Type=ToDate; Id="ventilatorAdmitted" }
        { Metric=DeceasedToDate;        Color="#6d5b80"; Visible=true;  Type=ToDate; Id="deceased" }
    ]
    /// Find a metric in the list and apply provided function to modify its value
    let update (fn: MetricCfg -> MetricCfg) metric metrics =
        metrics
        |> List.map (fun mc -> if mc.Metric = metric then fn mc else mc)

type State =
    { ScaleType : ScaleType
      MetricType : FullMetricType
      Metrics : Metrics
      StatsData : StatsData
      PatientsData : PatientsStats []
      Error : string option
      RangeSelectionButtonIndex: int
    }

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ToggleMetricVisible of Metric
    | ScaleTypeChanged of ScaleType
    | MetricTypeChanged of FullMetricType
    | RangeSelectionChanged of int
    | QueryParamsUpdated of QueryParams.State

let init data : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either getOrFetch () ConsumePatientsData ConsumeServerError
    let state = {
        ScaleType = Linear
        MetricType = { MetricType = Active; IsAveraged = false }
        Metrics = Metrics.initial
        StatsData = data
        PatientsData = [||]
        Error = None
        RangeSelectionButtonIndex = 0
    }
    state, cmd



let incorporateQueryParams (queryParams: QueryParams.State) (state: State, commands: Cmd<Msg>): State * Cmd<Msg>=
       let state = match queryParams.MetricsComparisonType with
                   | Some (sort : string) ->
                       match sort.ToLower() |> MetricQueryParam.TryFind with
                       | Some v -> {state with MetricType=v}
                       | _ -> state
                   | _ -> state

       state, commands

let stateToQueryParams (state: State) (queryParams: QueryParams.State)
    = { queryParams with
                        MetricsComparisonType = Map.tryFindKey (fun k v -> v = state.MetricType) MetricQueryParam
                                                     }

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with PatientsData = data; }, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | ToggleMetricVisible metric ->
        { state with
            Metrics = state.Metrics
                      |> Metrics.update (fun mc -> { mc with Visible = not mc.Visible}) metric
        }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none
    | MetricTypeChanged metricType ->
        { state with
            MetricType = metricType
            }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none
    | QueryParamsUpdated queryParams -> (state, Cmd.none) |> incorporateQueryParams queryParams

let statsDataGenerator metric =
    fun point ->
        match metric.Metric with
        | PerformedTestsToday -> point.Tests.Performed.Today
        | PerformedTestsToDate -> point.Tests.Performed.ToDate
        | ConfirmedCasesToday -> point.Cases.ConfirmedToday
        | ConfirmedCasesToDate -> point.Cases.ConfirmedToDate
        | ActiveCases -> point.Cases.Active
        | RecoveredToDate -> point.Cases.RecoveredToDate
        | _ -> None

let patientsDataGenerator metric =
    fun point ->
        match metric.Metric with
        | HospitalToday -> point.total.inHospital.today
        | HospitalIn -> point.total.inHospital.``in``
        | HospitalOut -> point.total.inHospital.out
        | HospitalToDate -> point.total.inHospital.toDate
        | HospitalOutToDate -> point.total.outOfHospital.toDate
        | ICUToday -> point.total.icu.today
        | ICUIn -> point.total.icu.``in``
        | ICUOut -> point.total.icu.out
        | ICUToDate -> point.total.icu.toDate
        | VentilatorToday -> point.total.critical.today
        | VentilatorIn -> point.total.critical.``in``
        | VentilatorOut -> point.total.critical.out
        | VentilatorToDate -> point.total.critical.toDate
        | DeceasedToday -> point.total.deceased.today |> Utils.zeroToNone
        | DeceasedToDate -> point.total.deceased.toDate
        | _ -> None


let prepareMetricsData (metric: MetricCfg) (state: State) =

    let statsData = statsDataGenerator metric
    let patientsData = patientsDataGenerator metric

    let untrimmedData =
        if Metric.UseStatsData metric.Metric then
            state.StatsData
            |> Seq.map (fun dp -> (dp.Date |> jsTime12h, statsData dp))
        else
            state.PatientsData
            |> Seq.map (fun dp -> (dp.Date |> jsTime12h, patientsData dp))

    let isValueMissing ((_, value): (JsTimestamp * int option)) = value.IsNone

    let intOptionToFloat value =
        match value with
        | Some x -> float x
        | None -> 0.

    let trimmedData =
        untrimmedData
        |> Seq.toArray
        |> Array.skipWhile isValueMissing
        |> Array.rev
        |> Array.skipWhile isValueMissing
        |> Array.rev
        |> Array.map(fun (date, value) -> (date, value |> intOptionToFloat))

    let finalData =
        match state.MetricType.IsAveraged with
        | true -> trimmedData |> Statistics.calcRunningAverage
        | false -> trimmedData

    finalData


let renderChartOptions state dispatch =

    let allSeries = [
        let mutable startTime = DateTime.Today |> jsTime

        let visibleMetrics =
            state.Metrics
            |> Seq.filter (fun metric ->
                metric.Type = state.MetricType.MetricType
                && metric.Visible)

        for metric in visibleMetrics do
            let data = prepareMetricsData metric state

            if data |> Array.length > 0 then
                let metricStartTime = data.[0] |> fst
                if metricStartTime < startTime then
                    startTime <- metricStartTime

            yield pojo
                {|
                    visible = true
                    ``type`` =
                        if state.MetricType.IsAveraged then "spline"
                        else "line"
                    color = metric.Color
                    name = chartText metric.Id
                    marker =
                        if metric.Metric = DeceasedToday then
                            pojo {| enabled = true; symbol = "diamond" |}
                        else pojo {| enabled = false |}
                    lineWidth = if metric.Metric = DeceasedToday then 0 else 2
                    states =
                        if metric.Metric = DeceasedToday then
                            pojo {| hover = {| lineWidthPlus = 0 |} |}
                        else pojo {||}
                    dashStyle =
                        match state.MetricType.MetricType with
                        | Active -> "Solid"
                        | Today -> "ShortDot"
                        | ToDate -> "Dot"
                    data = data
                |}

        yield addContainmentMeasuresFlags startTime None |> pojo
    ]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions state.ScaleType "covid19-metrics-comparison"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = List.toArray allSeries
        yAxis =
            let showFirstLabel = state.ScaleType <> Linear
            baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = Some showFirstLabel |})
    |}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> chartFromWindow
        ]
    ]

let renderMetricSelector (metric : MetricCfg) dispatch =
    let style =
        if metric.Visible
        then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleMetricVisible metric.Metric |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (metric.Visible, "metric-selector--selected")]
        prop.style style
        prop.text (chartText metric.Id) ]

let renderMetricsSelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children [
            for mc in state.Metrics do
                if mc.Type = state.MetricType.MetricType then
                    yield renderMetricSelector mc dispatch
        ]
    ]

let renderMetricTypeSelectors (activeMetricType: FullMetricType) dispatch =
    let renderMetricTypeSelector (metricTypeToRender: FullMetricType) =
        let active = metricTypeToRender = activeMetricType
        Html.div [
            prop.onClick (fun _ -> dispatch metricTypeToRender)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (metricTypeToRender.Name)
        ]

    let metricTypesSelectors =
        availableMetricTypes
        |> List.map renderMetricTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (metricTypesSelectors)
    ]

let render (state : State) dispatch =
    match state.PatientsData, state.Error with
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
            renderMetricsSelectors state dispatch
        ]
let chart =
    React.functionComponent(fun (props : {| data : StatsData |}) ->
        let state, dispatch = QueryParams.useElmishWithQueryParams (props.data |> init) update stateToQueryParams Msg.QueryParamsUpdated
        render state dispatch
    )
