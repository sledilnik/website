module DeceasedViz.Analysis

open DataVisualization.ChartingTypes

type DisplayMetricsType =
    | HospitalsToDate
    | HospitalsToday
    | ByAgeToDate
    | ByAgeToday
    | ByTypeToDate
    | ByTypeToday

type VisualizationPage = {
    Id: string
    MetricsType: DisplayMetricsType
    ChartType: ChartType
}

let (|HospitalMetricsType|_|) (metricsType: DisplayMetricsType) =
    match metricsType with
    | HospitalsToDate -> Some HospitalMetricsType
    | HospitalsToday -> Some HospitalMetricsType
    | _ -> None
let (|AgeGroupsMetricsType|_|) (metricsType: DisplayMetricsType) =
    match metricsType with
    | ByAgeToDate -> Some AgeGroupsMetricsType
    | ByAgeToday -> Some AgeGroupsMetricsType
    | _ -> None
let (|PersonTypeMetricsType|_|) (metricsType: DisplayMetricsType) =
    match metricsType with
    | ByTypeToDate -> Some PersonTypeMetricsType
    | ByTypeToday -> Some PersonTypeMetricsType
    | _ -> None
