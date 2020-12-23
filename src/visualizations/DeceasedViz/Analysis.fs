module DeceasedViz.Analysis

open AgeGroupsTimelineViz.Synthesis

type DisplayMetricsType =
    | HospitalsToDate
    | HospitalsToday
    | ByAgeToDate
//    | ByAgeToday

type VisualizationPage = {
    Id: string
    MetricsType: DisplayMetricsType
    ChartType: string
}

let (|HospitalMetricsType|_|) (metricsType: DisplayMetricsType) =
    match metricsType with
    | HospitalsToDate -> Some HospitalMetricsType
    | HospitalsToday -> Some HospitalMetricsType
    | _ -> None
