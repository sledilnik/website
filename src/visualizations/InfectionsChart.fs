[<RequireQualifiedAccess>]
module InfectionsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Browser

open Highcharts
open Types

type Metric =
    | HospitalStaff
    | RestHomeStaff
    | RestHomeOccupant
    | OtherPeople
    | AllConfirmed

type MetricCfg = {
    Metric : Metric
    Color : string
    Label : string
}

type Metrics = MetricCfg list

type DayValueIntMaybe = JsTimestamp*int option
type DayValueFloat = JsTimestamp*float

type ShowAllOrOthers = ShowAllConfirmed | ShowOthers

module Metrics  =
    let all = [
        { Metric=AllConfirmed;      Color="#bda506"; Label="Vsi potrjeni" }
        { Metric=OtherPeople;       Color="#FFDBA3"; Label="Ostale osebe" }
        { Metric=HospitalStaff;     Color="#73ccd5"; Label="Zaposleni v zdravstvu" }
        { Metric=RestHomeStaff;     Color="#20b16d"; Label="Zaposleni v domovih za starejše občane" }
        { Metric=RestHomeOccupant;  Color="#bf5747"; Label="Oskrbovanci domov za starejše občane" }
    ]

    let metricsToDisplay filter =
        let without metricType =
            all |> List.filter (fun metric -> metric.Metric <> metricType)

        match filter with
        | ShowAllConfirmed -> without OtherPeople
        | ShowOthers -> without AllConfirmed

type ValueTypes = RunningTotals | MovingAverages
type ChartType =
    | StackedBarNormal
    | StackedBarPercent
    | SplineChart

type DisplayType = {
    Label: string
    ValueTypes: ValueTypes
    ShowAllOrOthers: ShowAllOrOthers
    ChartType: ChartType
    ShowPhases: bool
    ShowLegend: bool
}

[<Literal>]
let DisplayTypeAverageLabel = "Po dnevih (povprečno)"

[<Literal>]
let DaysOfMovingAverage = 5

/// <summary>
/// A function that calculates the moving average value for a given array of
/// day values.
/// </summary>
type MovingAverageFunc = (DayValueIntMaybe[]) -> DayValueFloat

/// <summary>
/// Calculates the trailing moving average for a given array of day values.
/// </summary>
/// <remarks>
/// The trailing moving average takes the last day as the target day of the
/// average.
/// </remarks>
let movingAverageTrailing: MovingAverageFunc = fun (daysValues) ->
    let (targetDate, _) = daysValues |> Array.last
    let averageValue =
        daysValues
        |> Seq.averageBy(
            fun (_, value) ->
                value |> Option.defaultValue 0 |> float)
    (targetDate, averageValue)

/// <summary>
/// Calculates the centered moving average for a given array of day values.
/// </summary>
/// <remarks>
/// The centered moving average takes the day that is at the center of the
/// values array as the target day of the average.
/// </remarks>
let movingAverageCentered: MovingAverageFunc = fun (daysValues) ->
    match (daysValues |> Seq.length) % 2 with
    | 1 ->
        let centerIndex = (daysValues |> Seq.length) / 2
        let (targetDate, _) = daysValues.[centerIndex]
        let averageValue =
            daysValues
            |> Seq.averageBy(
                fun (_, value) ->
                    value |> Option.defaultValue 0 |> float)
        (targetDate, averageValue)
    | _ -> ArgumentException "daysValues needs to be an odd number" |> raise

/// <summary>
/// Calculates the moving averages array for a given array of day values.
/// </summary>
let movingAverages
    (averageFunc: MovingAverageFunc)
    (daysOfMovingAverage: int)
    (series: DayValueIntMaybe[]): DayValueFloat[] =
    series
    |> Array.windowed daysOfMovingAverage
    |> Array.map averageFunc

let availableDisplayTypes: DisplayType array = [|
    {   Label = DisplayTypeAverageLabel
        ValueTypes = MovingAverages
        ShowAllOrOthers = ShowAllConfirmed
        ChartType = SplineChart
        ShowPhases = true
        ShowLegend = true
    }
    {   Label = "Skupaj";
        ValueTypes = RunningTotals
        ShowAllOrOthers = ShowOthers
        ChartType = StackedBarNormal
        ShowPhases = false
        ShowLegend = true
    }
    {   Label = "Relativno";
        ValueTypes = RunningTotals
        ShowAllOrOthers = ShowOthers
        ChartType = StackedBarPercent
        ShowPhases = false
        ShowLegend = false
    }
|]

type State = {
    DisplayType : DisplayType
    Data : StatsData
}

type Msg =
    | ChangeDisplayType of DisplayType

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        DisplayType = availableDisplayTypes.[0]
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType rt ->
        { state with DisplayType=rt }, Cmd.none

let renderChartOptions displayType (data : StatsData) =

    let xAxisPoint (dp: StatsDataPoint) = dp.Date

    let metricDataGenerator mc : (StatsDataPoint -> int option) =
        let metricFunc =
            match mc.Metric with
            | HospitalStaff -> fun pt -> pt.HospitalEmployeePositiveTestsToDate
            | RestHomeStaff -> fun pt -> pt.RestHomeEmployeePositiveTestsToDate
            | RestHomeOccupant -> fun pt -> pt.RestHomeOccupantPositiveTestsToDate
            | OtherPeople -> fun pt -> pt.UnclassifiedPositiveTestsToDate
            | AllConfirmed -> fun pt -> pt.Cases.ConfirmedToDate

        fun pt -> (pt |> metricFunc |> Utils.zeroToNone)

    /// <summary>
    /// Calculates running totals for a given metric.
    /// </summary>
    let calcRunningTotals metric =
        let pointData = metricDataGenerator metric

        let skipLeadingMissing data =
            data |> List.skipWhile (fun (_,value: 'T option) -> value.IsNone)

        let skipTrailingMissing data =
            data
            |> List.rev
            |> skipLeadingMissing
            |> List.rev

        data
        |> List.map (fun dp -> ((xAxisPoint dp |> jsTime12h), pointData dp))
        |> skipLeadingMissing
        |> skipTrailingMissing
        |> Seq.toArray

    /// <summary>
    /// Converts running total series to daily (delta) values.
    /// </summary>
    let toDailyValues (series: DayValueIntMaybe[]) =
        let mutable last = 0
        Array.init series.Length (fun i ->
            match series.[i] with
            | ts, None -> ts, None
            | ts, Some current ->
                let result = current - last
                last <- current
                ts, Some result
        )

    let toFloatValues (series: DayValueIntMaybe[]) =
        series
        |> Array.map (fun (date, value) ->
            (date, value |> Option.defaultValue 0 |> float))

    let allSeries = [
        for metric in (Metrics.metricsToDisplay displayType.ShowAllOrOthers) do
            yield pojo
                {|
                visible = true
                color = metric.Color
                name = metric.Label
                data =
                    let runningTotals = calcRunningTotals metric
                    match displayType.ValueTypes with
                    | RunningTotals -> runningTotals |> toFloatValues
                    | MovingAverages ->
                        runningTotals |> toDailyValues
                        |> (movingAverages
                                movingAverageCentered DaysOfMovingAverage)
                marker = pojo {| enabled = false |}
                |}
    ]

    let legend =
        {|
            enabled = true
            title = ""
            align = "left"
            verticalAlign = "top"
            borderColor = "#ddd"
            borderWidth = 1
            //labelFormatter = string //fun series -> series.name
            layout = "vertical"
            floating = true
            x = 20
            y = 30
            backgroundColor = "rgba(255,255,255,0.5)"
            reversed = true
        |}

    let myLoadEvent(_: String) =
        let ret(_: Event) =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let baseOptions = basicChartOptions Linear "covid19-metrics-comparison"

    let axisWithPhases() = baseOptions.xAxis

    let axisWithWithoutPhases() =
        baseOptions.xAxis
        |> Array.map (fun ax ->
        {| ax with
            plotBands = shadedWeekendPlotBands
            plotLines = [||]
        |})


    {| baseOptions with
        chart = pojo
            {|
                ``type`` =
                    match displayType.ChartType with
                    | SplineChart -> "spline"
                    | StackedBarNormal -> "column"
                    | StackedBarPercent -> "column"
                zoomType = "x"
                events = {| load = myLoadEvent("infections") |}
            |}
        title = pojo {| text = None |}
        series = List.toArray allSeries
        xAxis =
            if displayType.ShowPhases then axisWithPhases()
            else axisWithWithoutPhases()
        plotOptions = pojo
            {|
                series =
                    match displayType.ChartType with
                    | SplineChart -> pojo {| stacking = ""; |}
                    | StackedBarNormal -> pojo {| stacking = "normal" |}
                    | StackedBarPercent -> pojo {| stacking = "percent" |}
            |}
        legend = pojo {| legend with enabled = displayType.ShowLegend |}
    |}

let renderChartContainer data metrics =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions data metrics
            |> chart
        ]
    ]

let renderDisplaySelectors activeDisplayType dispatch =
    let renderSelector (displayType : DisplayType) =
        let active = displayType = activeDisplayType
        Html.div [
            prop.text displayType.Label
            prop.className [
                true, "btn btn-sm metric-selector"
                active, "metric-selector--selected selected" ]
            if not active then prop.onClick (fun _ -> dispatch displayType)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        availableDisplayTypes
        |> Array.map renderSelector
        |> prop.children
    ]

let disclaimer1 =
    @"Prirast okuženih zdravstvenih delavcev ne pomeni, da so bili odkriti točno
    na ta dan; lahko so bili pozitivni že prej in se je samo podatek o njihovem
    statusu pridobil naknadno. Postavka Zaposleni v DSO vključuje zdravstvene
    delavce, sodelavce in zunanjo pomoč (študentje zdravstvenih smeri), zato so
    dnevni podatki o zdravstvenih delavcih (modri stolpci) ustrezno zmanjšani
    na račun zaposlenih v DSO. To pomeni, da je število zdravstvenih delavcev
    zelo konzervativna ocena."

let halfDaysOfMovingAverage = DaysOfMovingAverage / 2

// TODO: the last date of the graph should be determined from the actual data
// because the graph cuts trailing days without any data. This requires some
// bit of refactoring of the code, to first prepare the data and only then
// render everything. Also the series arrays should work with native DateTime
// so it can be used in arithmetic calculations, instead of JsTimestamp (it should be
// transformed to JsTimestamp at the moment of setting the Highcharts objects).
let lastDateOfGraph =
    DateTime.Now.AddDays(-(halfDaysOfMovingAverage + 1) |> float)

let disclaimer2 =
    sprintf
        @"Zaradi časovno ne dovolj natančnih vhodnih podatkov o potrjeno
        okuženih so dnevne vrednosti prikazane kot drseče povprečje %d dni.
        Seštevek vrednosti tega dneva, %d dni pred dnevom
        in %d dni po tem dnevu je deljen s %d. Zato graf kaže stanje samo do %s,
        na ta način pa dobimo boljšo predstavo o trendih po posameznih skupinah."
        DaysOfMovingAverage
        halfDaysOfMovingAverage
        halfDaysOfMovingAverage
        DaysOfMovingAverage
        (lastDateOfGraph.ToString("dd.MM"))

let render state dispatch =
    Html.div [
        renderChartContainer state.DisplayType state.Data
        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)

        let fullDisclaimer =
            match state.DisplayType.ValueTypes with
            | MovingAverages -> [ Html.p disclaimer2; Html.p disclaimer1 ]
            | _ -> [ Html.p disclaimer1 ]

        Html.div [
            prop.className "disclaimer"
            prop.children fullDisclaimer
        ]
    ]

let infectionsChart (props : {| data : StatsData |}) =
    React.elmishComponent("InfectionsChart", init props.data, update, render)
