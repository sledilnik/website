module WeeklyDemographicsViz.Synthesis

open System
open System.Collections.Generic
open Types
open WeeklyDemographicsViz.Analysis


type CasesInAgeGroupForDay = float

type CasesInAgeGroupTimeline = DatedArray<CasesInAgeGroupForDay>

type CasesInAgeGroupSeries =
    { AgeGroupKey: AgeGroupKey
      Timeline: CasesInAgeGroupTimeline }

type AllCasesInAgeGroupSeries = IDictionary<AgeGroupKey, CasesInAgeGroupSeries>

type ProcessedTimeline =
    { All:float[]; Male:float[]; Female:float[]}

type ProcessedAgeGroupData ={
    AgeGroupKey: AgeGroupKey
    Data: ProcessedTimeline
    StartDate:DateTime
}

type DisplayMetricsType =
    | CasesRatio
    | NewCases

type DisplayMetrics =
    { Id: string
      MetricsType: DisplayMetricsType }

let availableDisplayMetrics =
    [|
       { Id = "newCases"; MetricsType = NewCases }
       { Id = "ratio"; MetricsType = CasesRatio }
    |]

let listAgeGroups (timeline: CasesByAgeGroupsTimeline): AgeGroupKey list =
    timeline.Data.[0]
    |> List.map (fun group -> group.GroupKey)
    |> List.sortBy (fun groupKey -> groupKey.AgeFrom)


let newExtractTimelineForAgeGroup
    (ageGroupKey: AgeGroupKey)
    (casesTimeline: CasesByAgeGroupsTimeline)=

    let newCasesTimeline =
        casesTimeline
        |> mapDatedArrayItems
            (fun dayGroupsData ->
                dayGroupsData
                |> List.find (fun group -> group.GroupKey = ageGroupKey))

    let optionToFloat (value: int option) : float =
        match value with
        | Some x -> float x
        | None -> 0.

    let maleCaseCount = newCasesTimeline.Data |> Array.map ((fun dp -> dp.Male) >> optionToFloat)
    let femaleCaseCount = newCasesTimeline.Data |> Array.map ((fun dp -> dp.Female) >> optionToFloat)
    let totalCaseCount = newCasesTimeline.Data |> Array.map ((fun dp -> dp.All) >> optionToFloat)

    // a hack to ensure that the weekly counts start on Monday
    let padCases (cases: CasesInAgeGroupForDay[]): CasesInAgeGroupForDay[] =
        Array.concat [|[|0.;0.|]; cases|]

    let accumulateWeekly (cases: CasesInAgeGroupForDay[]): float[] =
        let chunks =
            cases
            |> Seq.chunkBySize 7

        let numberOfWeeks = Seq.length chunks

        let lastWeek = chunks |> Seq.skip (numberOfWeeks - 2) |> Seq.concat

        let truncatedChunks = // check if there's complete data available for last week otherwise truncate
            match Seq.length lastWeek with
            | 7 -> chunks
            | _ -> Seq.truncate (numberOfWeeks - 1) chunks

        truncatedChunks
        |> Seq.map Seq.sum
        |> Seq.toArray

    let (shiftedStartDate:DateTime) = casesTimeline.StartDate |> Days.add -2

    let data =
        {
            All = totalCaseCount |> padCases |> accumulateWeekly
            Male = maleCaseCount |> padCases |> accumulateWeekly
            Female = femaleCaseCount |> padCases |> accumulateWeekly
        }

    {
       AgeGroupKey = ageGroupKey
       Data = data
       StartDate = shiftedStartDate
    }





