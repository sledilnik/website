module AgeGroupsTimelineViz.Synthesis

open Types
open AgeGroupsTimelineViz.Analysis
open System
open System.Collections.Generic
open System.Text
open Fable.Core
open JsInterop

type CasesInAgeGroupForDay = { Date: DateTime; Cases: int option }
type CasesInAgeGroupTimeline = CasesInAgeGroupForDay list
type CasesInAgeGroupSeries = {
    AgeGroupKey: AgeGroupKey
    Timeline: CasesInAgeGroupTimeline
}

type AllCasesInAgeGroupSeries = IDictionary<AgeGroupKey, CasesInAgeGroupSeries>

let listAgeGroups (timeline: CasesByAgeGroupsTimeline): AgeGroupKey list  =
    timeline.[0].Cases
    |> List.map (fun group -> group.GroupKey)
    |> List.sortBy (fun groupKey -> groupKey.AgeFrom)

let extractTimelineForAgeGroup
    ageGroupKey
    (casesTimeline: CasesByAgeGroupsTimeline)
    : CasesInAgeGroupTimeline =
    casesTimeline
    |> List.map (fun dayData ->
            let date = dayData.Date
            let dataForGroup =
                dayData.Cases
                |> List.find(fun group -> group.GroupKey = ageGroupKey)
            { Date = date; Cases = dataForGroup.All }
            )

let tooltipFormatter jsThis =
    let points: obj[] = jsThis?points

    match points with
    | [||] -> ""
    | _ ->
        let s = StringBuilder()

        let date = points.[0]?point?date
        s.AppendFormat ("<b>{0}</b><br/>", date.ToString()) |> ignore

        s.Append "<table>" |> ignore

        points
        |> Array.iter
               (fun ageGroup ->
                    let ageGroupLabel = ageGroup?series?name
                    let ageGroupColor = ageGroup?series?color
                    let dataPoint = ageGroup?point

                    let dataValue: int = dataPoint?y

                    match dataValue with
                    | 0 -> ignore()
                    | _ ->
                        s.Append "<tr>" |> ignore
                        let ageGroupTooltip =
                            sprintf
                                "<td style='color: %s'>●</td><td style='padding-left: 6px'>%s:</td><td style='text-align: right; padding-left: 6px'><b>%A</b></td>"
                                ageGroupColor
                                ageGroupLabel
                                dataValue
                        s.Append ageGroupTooltip |> ignore
                        s.Append "</tr>" |> ignore
                )

        s.Append "</table>" |> ignore
        s.ToString()
