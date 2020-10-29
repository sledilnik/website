module PhaseDiagram.Types

open Browser

open Types

let i18n = I18N.tt "charts.phaseDiagram"

type DiagramKind =
    | TotalVsWeek
    | WeekVsWeekBefore

    with

    member this.Name =
        match this with
        | TotalVsWeek -> i18n "totalVsWeek.name"
        | WeekVsWeekBefore -> i18n "weekVsWeekBefore.name"

    static member All = [TotalVsWeek ; WeekVsWeekBefore]

type Color = {
    Dark : string
    Light : string
}

type Metric =
    | Cases
    | Deceased

    with

    member this.Name =
        match this with
        | Cases -> i18n "cases"
        | Deceased -> i18n "deceased"

    member this.Color =
        match this with
        | Cases ->
            { Dark = "#dba51d"
              Light = "#f2dba2" }
        | Deceased ->
            { Dark = "#000000"
              Light = "#999999" }

let (|CasesMetric|DeceasedMetric|UnknownMetric|) str =
    if str = Metric.Cases.ToString()
    then CasesMetric
    elif str = Metric.Deceased.ToString()
    then DeceasedMetric
    else UnknownMetric

type DisplayData = {
    x : int
    y : int
    date : System.DateTime
}

type State = {
    StatsData : StatsData
    DisplayData : DisplayData array
    DiagramKind : DiagramKind
    Metric : Metric
    Day : int
}

type Msg =
    | DiagramKindSelected of DiagramKind
    | MetricSelected of string
    | DayChanged of int
