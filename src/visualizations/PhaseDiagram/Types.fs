module PhaseDiagram.Types

open Browser

open Types

let i18n = I18N.tt "charts.phaseDiagram"

type DiagramKind =
    | TotalVsWeek
    | WeekVsWeekBefore

    with

    member this.GetName =
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
    | Hospitalized
    | Deceased

    with

    member this.GetName =
        match this with
        | Cases -> i18n "cases"
        | Hospitalized -> i18n "hospitalized"
        | Deceased -> i18n "deceased"

    member this.GetColor =
        match this with
        | Cases ->
            { Dark = "#dba51d"
              Light = "#f2dba2" }
        | Hospitalized ->
            { Dark = "#be7A2a"
              Light = "#eacaa3" }
        | Deceased ->
            { Dark = "#000000"
              Light = "#999999" }

    static member All =
        [ Cases ; Hospitalized ; Deceased ]

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
    | MetricSelected of Metric
    | DayChanged of int
