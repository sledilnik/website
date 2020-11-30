module RegionsChartViz.Synthesis

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
