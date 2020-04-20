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
        { Metric=OtherPeople;       Color="#d5c768"; Label="Ostale osebe" }
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

type ValueTypes = Daily | RunningTotals | MovingAverages
type ChartType = 
    | StackedBarNormal 
    | StackedBarPercent 
    | LineChart 
    | SplineChart
    | SplineFatLineChart
    | SplineDottedChart
    | SplineChartWithDataLabels

let chartDashStyle chartType =
    match chartType with
    | SplineDottedChart -> Dot
    | _ -> Solid

type DisplayType = {
    Label: string
    ValueTypes: ValueTypes
    ShowAllOrOthers: ShowAllOrOthers
    ChartType: ChartType
    ShowLegend: bool
}

[<Literal>]
let DisplayTypeAverageLabel = "Po dnevih povprečno"

[<Literal>]
let DaysOfMovingAverage = 5


let availableDisplayTypes: DisplayType array = [|
    {   Label = "Po dnevih"; 
        ValueTypes = Daily; 
        ShowAllOrOthers = ShowOthers;
        ChartType = StackedBarNormal; 
        ShowLegend = true 
    }
    {   Label = "Skupaj"; 
        ValueTypes = RunningTotals; 
        ShowAllOrOthers = ShowOthers;
        ChartType = StackedBarNormal; 
        ShowLegend = true 
    }
    {   Label = "Relativno"; 
        ValueTypes = RunningTotals;  
        ShowAllOrOthers = ShowOthers;
        ChartType = StackedBarPercent; 
        ShowLegend = false 
    }
    {   Label = DisplayTypeAverageLabel + " 1"; 
        ValueTypes = MovingAverages; 
        ShowAllOrOthers = ShowAllConfirmed;
        ChartType = SplineFatLineChart; 
        ShowLegend = true 
    }
    {   Label = DisplayTypeAverageLabel + " 2";
        ValueTypes = MovingAverages;
        ShowAllOrOthers = ShowAllConfirmed;
        ChartType = SplineDottedChart; 
        ShowLegend = true 
    }
    {   Label = DisplayTypeAverageLabel + " 3";
        ValueTypes = MovingAverages;
        ShowAllOrOthers = ShowAllConfirmed;
        ChartType = SplineChart; 
        ShowLegend = true 
    }
    {   Label = DisplayTypeAverageLabel + " 4";
        ValueTypes = MovingAverages;
        ShowAllOrOthers = ShowAllConfirmed;
        ChartType = SplineChartWithDataLabels; 
        ShowLegend = true 
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

    let maxOption a b =
        match a, b with
        | None, None -> None
        | Some x, None -> Some x
        | None, Some y -> Some y
        | Some x, Some y -> Some (max x y)

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

    let toMovingAverages (series: DayValueIntMaybe[]): DayValueFloat[] =
        let calculateDayAverage (daysValues: DayValueIntMaybe[]) =
            let (targetDate, _) = daysValues |> Array.last
            let averageValue = 
                daysValues
                |> Seq.averageBy(
                    fun (_, value) -> 
                        value |> Option.defaultValue 0 |> float)
            (targetDate, averageValue)

        series
        |> Array.windowed DaysOfMovingAverage
        |> Array.map calculateDayAverage

    let allSeries = [
        for metric in (Metrics.metricsToDisplay displayType.ShowAllOrOthers) do
            yield pojo 
                {|
                visible = true
                color = match displayType.ChartType with
                        // This was the only way I could find to achieve half-
                        // transparency, "opacity" parameters just wouldn't 
                        // work.
                        | SplineFatLineChart -> metric.Color + "80"
                        | _ -> metric.Color
                name = metric.Label
                dashStyle = 
                    chartDashStyle displayType.ChartType |> DashStyle.toString
                data =
                    let runningTotals = calcRunningTotals metric
                    match displayType.ValueTypes with
                    | Daily -> toDailyValues runningTotals |> toFloatValues
                    | RunningTotals -> runningTotals |> toFloatValues
                    | MovingAverages -> 
                        runningTotals |> toDailyValues |> toMovingAverages
                marker = 
                    pojo {| enabled = 
                            match displayType.ChartType with
                            | SplineChartWithDataLabels -> true
                            | _ -> false 
                        |}
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

    let myLoadEvent(name: String) =
        let ret(event: Event) =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let baseOptions = basicChartOptions Linear "covid19-metrics-comparison"
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = 
                    match displayType.ChartType with
                    | LineChart -> "line"
                    | SplineChart -> "spline"
                    | SplineFatLineChart -> "spline"
                    | SplineDottedChart -> "spline"
                    | SplineChartWithDataLabels -> "spline"
                    | StackedBarNormal -> "column"
                    | StackedBarPercent -> "column"
                zoomType = "x"
                events = {| load = myLoadEvent("infections") |}
            |}
        title = pojo {| text = None |}
        series = List.toArray allSeries
        xAxis = baseOptions.xAxis |> Array.map (fun ax ->
            {| ax with
                plotBands = shadedWeekendPlotBands
                plotLines = [||]
            |})
        plotOptions = pojo
            {|
                series = 
                    match displayType.ChartType with
                    | LineChart -> 
                        pojo {| stacking = ""; lineWidth = 2 |}
                    | SplineChart -> 
                        pojo {| stacking = ""; lineWidth = 2 |}
                    | SplineFatLineChart -> 
                        pojo {| stacking = ""; lineWidth = 5 |}
                    | SplineDottedChart -> 
                        pojo {| stacking = ""; lineWidth = 2 |}
                    | SplineChartWithDataLabels -> 
                        pojo {| stacking = ""; lineWidth = 2 |}
                    | StackedBarNormal -> pojo {| stacking = "normal" |}
                    | StackedBarPercent -> pojo {| stacking = "percent" |}

                spline =
                    match displayType.ChartType with
                    | SplineChartWithDataLabels -> 
                        pojo {| 
                                dataLabels = pojo {| enabled = true |} 
                                marker = pojo {| enabled = true |}
                            |}
                    | _ -> pojo {| dataLabels = pojo {| enabled = false |} |}
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

let disclaimer2 = 
    sprintf 
        @"Pri grafu '%s' podatki predstavljajo drseča povprečja zadnjih %d dni."
        DisplayTypeAverageLabel
        DaysOfMovingAverage

let render state dispatch =
    Html.div [
        renderChartContainer state.DisplayType state.Data
        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)
        Html.div [
            prop.className "disclaimer"
            prop.children [ 
                Html.p disclaimer1
                Html.p disclaimer2 ]
        ]
    ]

let infectionsChart (props : {| data : StatsData |}) =
    React.elmishComponent("InfectionsChart", init props.data, update, render)
