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
      Regions : Region list
      RegionsConfig : RegionRenderingConfiguration list
      RangeSelectionButtonIndex: int
    }

type RegionSeriesValues = (JsTimestamp * float)[]

let visibleRegions state =
    state.RegionsConfig
    |> List.filter (fun regionConfig -> regionConfig.Visible)


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

        let seriesValuesHc: RegionSeriesValues =
            regionMetrics.MetricValues
            |> Array.mapi (fun i metricValue ->
                let ts = startDate |> Days.add i |> jsTime12h

                let finalValue =
                    match state.ChartConfig.RelativeTo with
                    | Absolute -> metricValue |> float
                    | Pop100k -> (float metricValue) / regionPopBy100k

                ts, finalValue
            )

        {|
            name = I18N.tt "region" regionConfig.Key
            color = regionConfig.Color
            data = seriesValuesHc
        |}
        |> pojo
    )
    |> List.toArray
