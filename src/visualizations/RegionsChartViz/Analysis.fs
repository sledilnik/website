module RegionsChartViz.Analysis

open System
open Types

type ContentType =
    | ViewConfirmed
    | ViewDeceased
    | ViewVaccinated
    with
    static member Default = ViewConfirmed
    static member All = [ ViewConfirmed; ViewVaccinated; ViewDeceased ]
    static member GetName = function
       | ViewConfirmed      -> I18N.chartText "regions" "confirmedCases"
       | ViewDeceased       -> I18N.chartText "regions" "deceased"
       | ViewVaccinated     -> I18N.chartText "regions" "vaccinated"

type MetricType =
    | ActiveCases
    | ConfirmedCases
    | NewCases7Days
    | Deceased
    | Vaccinated7Days
    | Vaccinated1st
    | Vaccinated2nd
    | Vaccinated3rd
  with
    static member Default (ct : ContentType) =
        match ct with
        | ViewConfirmed -> NewCases7Days
        | ViewDeceased -> Deceased
        | ViewVaccinated -> Vaccinated7Days
    static member All (ct : ContentType) =
        match ct with
        | ViewConfirmed -> [ NewCases7Days; ActiveCases; ConfirmedCases ]
        | ViewDeceased -> [ ]
        | ViewVaccinated -> [ Vaccinated7Days; Vaccinated2nd; Vaccinated1st; Vaccinated3rd ]
    static member GetName = function
        | ActiveCases -> I18N.chartText "regions" "activeCases"
        | ConfirmedCases -> I18N.chartText "regions" "allCases"
        | NewCases7Days -> I18N.chartText "regions" "newCases7Days"
        | Deceased -> I18N.chartText "regions" "deceased"
        | Vaccinated7Days -> I18N.chartText "regions" "vaccinated7Days"
        | Vaccinated1st -> I18N.chartText "regions" "vaccinated1st"
        | Vaccinated2nd -> I18N.chartText "regions" "vaccinated2nd"
        | Vaccinated3rd -> I18N.chartText "regions" "vaccinated3rd"

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
    | Vaccinated7Days -> regionDayData.Vaccinated1stToDate |> Utils.sumIntOption regionDayData.Vaccinated2ndToDate |> Utils.sumIntOption regionDayData.Vaccinated3rdToDate
    | Vaccinated1st -> regionDayData.Vaccinated1stToDate
    | Vaccinated2nd -> regionDayData.Vaccinated2ndToDate
    | Vaccinated3rd -> regionDayData.Vaccinated3rdToDate
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
