﻿module RegionsChartViz.Synthesis

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
      ContentType : ContentType
      MetricType : MetricType
      RegionsData : RegionsData
      RegionsSorted : AreaCases list
      RegionsConfig : RegionRenderingConfiguration list
      RangeSelectionButtonIndex: int
      ShowAll : bool
    }

type RegionSeriesValues = (DateTime * float)[]

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

let vaccinatedPerDay (regionMetricData: RegionMetricData): RegionMetricData =
    let totalVaccinatedArray = regionMetricData.MetricValues
    let valuesLength = totalVaccinatedArray.Length

    let vaccinatedPerDayArray =
        Array.init valuesLength
            (fun i ->
                match i with
                | 0 -> totalVaccinatedArray.[i]
                | _ -> totalVaccinatedArray.[i] - totalVaccinatedArray.[i - 1]
            )

    { regionMetricData with MetricValues = vaccinatedPerDayArray }


let allSeries state =
    let startDate =
        match state.RegionsData with
        | head :: _ -> head.Date
        | _ -> raise (InvalidOperationException())

    visibleRegions state
    |> List.map (fun regionConfig ->
        let regionName = regionConfig.Key

        let regionMetrics =
            match regionName with
            | "si"  -> metricForAllRegions state.RegionsData startDate regionName state.MetricType
            | _     -> metricForRegion state.RegionsData startDate regionName state.MetricType

        let regionPopulation =
            Utils.Dictionaries.regions.[regionName].Population
            |> Option.get
            |> float
        let regionPopBy100k = regionPopulation / 100000.0

        let regionMetrics2nStep =
            regionMetrics
            |> match state.MetricType with
               | NewCases7Days -> newCases
               | Vaccinated7Days -> vaccinatedPerDay
               | _ -> id

        let seriesValuesHc: RegionSeriesValues =
            regionMetrics2nStep.MetricValues
            |> Array.mapi (fun i metricValue ->
                let date = startDate |> Days.add i

                let finalValue =
                    match state.ChartConfig.RelativeTo with
                    | Absolute -> metricValue |> float
                    | Pop100k -> (float metricValue) / regionPopBy100k

                date, finalValue
            )

        let seriesValuesHc2nStep =
            seriesValuesHc
            |> match state.MetricType with
               | NewCases7Days -> Statistics.calcRunningAverage
               | Vaccinated7Days -> Statistics.calcRunningAverage
               | _ -> id
            |> Array.map (fun value ->
                            let date = value |> fst
                            pojo {|
                                    x = date |> jsTime12h
                                    y = value |> snd
                                    date = I18N.tOptions "days.longerDate" {| date = date |} |})

        {|
            name = I18N.tt "region" regionConfig.Key
            color = regionConfig.Color
            data = seriesValuesHc2nStep
        |}
        |> pojo
    )
    |> List.toArray
