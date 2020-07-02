module AgeGroupsTimelineViz.Synthesis

open System
open System.Collections.Generic
open AgeGroupsTimelineViz.Analysis
open Types


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
