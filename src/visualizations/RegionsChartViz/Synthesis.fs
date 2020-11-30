module RegionsChartViz.Synthesis

open Highcharts
open RegionsChartViz.Analysis
open Types

type RegionsChartConfig = {
    RelativeTo: MetricRelativeTo
    ChartTextsGroup: string
}

type RegionsChartMetric =
    { Key : string
      Color : string
      Visible : bool }

type RegionsChartState =
    {
      ChartConfig: RegionsChartConfig
      ScaleType : ScaleType
      MetricType : MetricType
      Data : RegionsData
      Regions : Region list
      Metrics : RegionsChartMetric list
      RangeSelectionButtonIndex: int
    }

type RegionSeries = {
    Name: string
    Color: string
    Values: (JsTimestamp * float)[]
}

let metricsToRender state =
    state.Metrics
    |> List.filter (fun metric -> metric.Visible)

let renderRegionPoint state metricToRender (point : RegionsDataPoint) =
    let ts = point.Date |> jsTime12h
    let region =
        point.Regions
        |> List.find (fun reg -> reg.Name = metricToRender.Key)

    let municipalityMetricValue muni =
        match state.MetricType with
        | ActiveCases -> muni.ActiveCases
        | ConfirmedCases -> muni.ConfirmedToDate
        | NewCases7Days -> muni.ConfirmedToDate
        | MetricType.Deceased -> muni.DeceasedToDate
        |> Option.defaultValue 0

    let totalSum =
        region.Municipalities
        |> Seq.sumBy municipalityMetricValue
        |> float

    let finalValue =
        match state.ChartConfig.RelativeTo with
        | Absolute -> totalSum
        | Pop100k ->
            let regionPopulation =
                Utils.Dictionaries.regions.[region.Name].Population
                |> Option.get
                |> float

            let regionPopBy100k = regionPopulation / 100000.0
            totalSum / regionPopBy100k

    ts, finalValue

let allSeries state =
    metricsToRender state
    |> List.map (fun metric ->
        let renderPoint = renderRegionPoint state metric

        let data =
            state.Data
            |> Seq.map renderPoint
            |> Array.ofSeq

        {|
            visible = metric.Visible
            color = metric.Color
            name = I18N.tt "region" metric.Key
            data = data
        |}
        |> pojo
    )
    |> List.toArray
