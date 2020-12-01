module RegionsChartViz.Analysis

open System
open Types

type MetricType =
    | ActiveCases
    | ConfirmedCases
    | NewCases7Days
    | Deceased
  with
    static member getName = function
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
    : Region =
    regionsDataPoint.Regions
    |> List.find (fun regionDayData -> regionDayData.Name = regionName)

let metricForRegionForDay
    (regionsDataPoint: RegionsDataPoint)
    (regionName: string)
    (metricType: MetricType)
    : int =

    let regionDayData =
        findRegionData regionsDataPoint regionName

    regionDayData.Municipalities
    |> List.sumBy
           (fun muniData ->
                match metricType with
                | ActiveCases -> muniData.ActiveCases
                | ConfirmedCases -> muniData.ConfirmedToDate
                | NewCases7Days -> muniData.ConfirmedToDate
                | Deceased -> muniData.DeceasedToDate
                |> Utils.optionToInt)

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
