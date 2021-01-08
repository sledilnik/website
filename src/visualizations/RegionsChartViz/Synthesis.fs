module RegionsChartViz.Synthesis

open System
open Highcharts
open RegionsChartViz.Analysis
open Types

type RegionsChartConfig = {
    RelativeTo: MetricRelativeTo
    ChartTextsGroup: string
}

type RegionRenderingConfiguration =
    { Key : string
      Color : string
      Visible : bool }

type RegionsChartState =
    {
      ChartConfig: RegionsChartConfig
      ScaleType : ScaleType
      MetricType : MetricType
      RegionsData : RegionsData
      RegionsSorted : AreaCases list
      RegionsConfig : RegionRenderingConfiguration list
      RangeSelectionButtonIndex: int
      ShowAll : bool
    }

type RegionSeriesValues = (JsTimestamp * float)[]

let visibleRegions state =
    state.RegionsConfig
    |> List.filter (fun regionConfig -> regionConfig.Visible)


let newCases (regionMetricData: RegionMetricData): RegionMetricData =
    let totalCasesArray = regionMetricData.MetricValues
    let valuesLength = totalCasesArray.Length

    let newCasesArray =
        Array.init valuesLength
            (fun i ->
                match i with
                | 0 -> totalCasesArray.[i]
                | _ -> totalCasesArray.[i] - totalCasesArray.[i - 1]
            )

    { regionMetricData with MetricValues = newCasesArray }


let allSeries state =
    let startDate =
        match state.RegionsData with
        | head :: _ -> head.Date
        | _ -> raise (InvalidOperationException())

    visibleRegions state
    |> List.map (fun regionConfig ->
        let regionName = regionConfig.Key

        let regionMetrics =
            metricForRegion state.RegionsData
                startDate regionName state.MetricType

        let regionPopulation =
            Utils.Dictionaries.regions.[regionName].Population
            |> Option.get
            |> float
        let regionPopBy100k = regionPopulation / 100000.0

        let regionMetrics2nStep =
            regionMetrics
            |> match state.MetricType with
               | NewCases7Days -> newCases
               | _ -> id

        let seriesValuesHc: RegionSeriesValues =
            regionMetrics2nStep.MetricValues
            |> Array.mapi (fun i metricValue ->
                let ts = startDate |> Days.add i |> jsTime12h

                let finalValue =
                    match state.ChartConfig.RelativeTo with
                    | Absolute -> metricValue |> float
                    | Pop100k -> (float metricValue) / regionPopBy100k

                ts, finalValue
            )

        let seriesValuesHc2nStep =
            seriesValuesHc
            |> match state.MetricType with
               | NewCases7Days -> Statistics.calcRunningAverage
               | _ -> id

        {|
            name = I18N.tt "region" regionConfig.Key
            color = regionConfig.Color
            data = seriesValuesHc2nStep
        |}
        |> pojo
    )
    |> List.toArray
