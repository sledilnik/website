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
    (municipalitiesDataPoint: MunicipalitiesDataPoint)
    (regionName: string)
    : Region =
    municipalitiesDataPoint.Regions
    |> List.find (fun regionDayData -> regionDayData.Name = regionName)

let metricForRegionForDay
    (municipalitiesDataPoint: MunicipalitiesDataPoint)
    (regionName: string)
    (metricType: MetricType)
    : int =

    let regionDayData =
        findRegionData municipalitiesDataPoint regionName

    regionDayData.Municipalities
    |> List.sumBy
           (fun municipalitiesData ->
                match metricType with
                | ActiveCases -> municipalitiesData.ActiveCases
                | ConfirmedCases -> municipalitiesData.ConfirmedToDate
                | NewCases7Days -> municipalitiesData.ConfirmedToDate
                | Deceased -> municipalitiesData.DeceasedToDate
                |> Utils.optionToInt)

let metricForRegion
    (municipalitiesData: MunicipalitiesData)
    (startDate: DateTime)
    (regionName: string)
    (metricType: MetricType)
    : RegionMetricData =

    let metricValues =
        municipalitiesData
        |> List.map (fun municipalitiesDataForDay ->
             metricForRegionForDay municipalitiesDataForDay regionName metricType)

    { Name = regionName
      StartDate = startDate
      MetricValues = metricValues |> List.toArray }
