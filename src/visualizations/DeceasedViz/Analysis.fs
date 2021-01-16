module DeceasedViz.Analysis

open DataVisualization.ChartingTypes

let chartText = I18N.chartText "deceased"

type MetricType =
    | Today
    | ToDate
with
    static member Default = Today
    static member All = [ Today; ToDate ]
    member this.GetName =
        match this with
        | Today  -> chartText "showToday"
        | ToDate -> chartText "showToDate"

type ChartType =
    | StackedNormal
    | StackedPercent
with
    static member Default = StackedNormal
    static member All = [ StackedNormal; StackedPercent ]
    member this.GetName =
        match this with
        | StackedNormal  -> chartText "showNormal"
        | StackedPercent -> chartText "showPercent"

type PageType =
    | HospitalsPage
    | PersonTypePage
    | AgeGroupsPage
with
    static member Default = HospitalsPage
    static member All = [ HospitalsPage; PersonTypePage; AgeGroupsPage; ]
    member this.GetName =
        match this with
        | HospitalsPage  -> chartText "deceasedHospitals"
        | PersonTypePage -> chartText "deceasedByType"
        | AgeGroupsPage  -> chartText "deceasedByAge"
