module RegionsChartViz.Analysis

open System
open Types

type MetricType =
    | ActiveCases
    | ConfirmedCases
    | NewCases7Days
    | Deceased
  with
    static member Default = MetricType.NewCases7Days
    static member GetName = function
        | ActiveCases -> I18N.chartText "regions" "activeCases"
        | ConfirmedCases -> I18N.chartText "regions" "confirmedCases"
        | NewCases7Days -> I18N.chartText "regions" "newCases7Days"
        | Deceased -> I18N.chartText "regions" "deceased"

type MetricRelativeTo = Absolute | Pop100k


type RegionMetricData = {
    Name: string
    StartDate: DateTime
    MetricValues: int []
}

let getMetric regionDayData metricType =
    match metricType with
    | ActiveCases -> regionDayData.ActiveCases
    | ConfirmedCases -> regionDayData.ConfirmedToDate
    | NewCases7Days -> regionDayData.ConfirmedToDate
    | Deceased -> regionDayData.DeceasedToDate
    |> Utils.optionToInt


let findRegionData
    (regionsDataPoint: RegionsDataPoint)
    (regionName: string)
    : AreaCases =
    regionsDataPoint.Regions
    |> List.find (fun regionDayData -> regionDayData.Name = regionName)

let metricForRegionForDay
    (regionsDataPoint: RegionsDataPoint)
    (regionName: string)
    (metricType: MetricType)
    : int =

    let regionDayData =
        findRegionData regionsDataPoint regionName

    getMetric regionDayData metricType

let metricForRegion
    (regionsData: RegionsData)
    (startDate: DateTime)
    (regionName: string)
    (metricType: MetricType)
    : RegionMetricData =

    let metricValues =
        regionsData
        |> List.map (fun regionsDataForDay ->
             metricForRegionForDay regionsDataForDay regionName metricType)

    { Name = regionName
      StartDate = startDate
      MetricValues = metricValues |> List.toArray }

let metricForAllRegionsForDay
    (regionsDataPoint: RegionsDataPoint)
    (metricType: MetricType)
    : int =

    regionsDataPoint.Regions
    |> Seq.sumBy (fun dp -> getMetric dp metricType)

let metricForAllRegions
    (regionsData: RegionsData)
    (startDate: DateTime)
    (regionName: string)
    (metricType: MetricType)
    : RegionMetricData =

    let metricValues =
        regionsData
        |> List.map (fun regionsDataForDay ->
             metricForAllRegionsForDay regionsDataForDay metricType)

    { Name = regionName
      StartDate = startDate
      MetricValues = metricValues |> List.toArray }
