module DeceasedViz.Analysis

type DisplayMetricsType =
    | HospitalsToDate
    | HospitalsToday
    | ByAgeToDate
//    | ByAgeToday

type DisplayMetrics = {
    Id: string
    MetricsType: DisplayMetricsType
    ChartType: string
}
