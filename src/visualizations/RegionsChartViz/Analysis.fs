module RegionsChartViz.Analysis

open System
open Types

type MetricType =
    | ActiveCases
    | ConfirmedCases
    | NewCases7Days
    | Deceased
  with
    static member Default = MetricType.ActiveCases
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

    match metricType with
    | ActiveCases -> regionDayData.ActiveCases
    | ConfirmedCases -> regionDayData.ConfirmedToDate
    | NewCases7Days -> regionDayData.ConfirmedToDate
    | Deceased -> regionDayData.DeceasedToDate
    |> Utils.optionToInt

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
