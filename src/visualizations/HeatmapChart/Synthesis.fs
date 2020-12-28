module HeatmapChart.Synthesis

open Types
open HeatmapChart.Analysis
open System.Collections.Generic
open Fable.Core
open JsInterop

open Utils.AgePopulationStats


type CasesInAgeGroupForDay = float

type CasesInAgeGroupTimeline = DatedArray<CasesInAgeGroupForDay>

type CasesInAgeGroupSeries =
    { AgeGroupKey: AgeGroupKey
      Timeline: CasesInAgeGroupTimeline }

type AllCasesInAgeGroupSeries = IDictionary<AgeGroupKey, CasesInAgeGroupSeries>


type DisplayMetricsType =
    | DifferenceInCases
    | RelativeCases

type DisplayMetrics =
    { Id: string
      MetricsType: DisplayMetricsType }

let availableDisplayMetrics =
    [| 
       { Id = "relative"; MetricsType = RelativeCases } 
       { Id = "difference"; MetricsType = DifferenceInCases }
    |]

let listAgeGroups (timeline: CasesByAgeGroupsTimeline): AgeGroupKey list =
    timeline.Data.[0]
    |> List.map (fun group -> group.GroupKey)
    |> List.sortBy (fun groupKey -> groupKey.AgeFrom)

let extractTimelineForAgeGroup ageGroupKey
                               (metricsType: DisplayMetricsType)
                               (casesTimeline: CasesByAgeGroupsTimeline)
                               : CasesInAgeGroupTimeline =

    let populationStats = populationStatsForAgeGroup ageGroupKey
    
    match metricsType with
    | RelativeCases ->

        let totalPopulation =
            float (populationStats.Female + populationStats.Male)

        let newCasesTimeline =
            casesTimeline
            |> mapDatedArrayItems
                (fun dayGroupsData ->
                    let dataForGroup =
                        dayGroupsData
                        |> List.find (fun group -> group.GroupKey = ageGroupKey)

                    dataForGroup.All |> Utils.optionToInt |> float)

        newCasesTimeline
        |> mapDatedArrayItems (fun x -> float x * 100000. / totalPopulation)

    | DifferenceInCases ->

        let femalePopulation = populationStats.Female |> float
        let malePopulation = populationStats.Male |> float

        let newCasesTimelineMale =
            casesTimeline
            |> mapDatedArrayItems
                (fun dayGroupsData ->
                    let dataForGroup =
                        dayGroupsData
                        |> List.find (fun group -> group.GroupKey = ageGroupKey)

                    dataForGroup.Male |> Utils.optionToInt |> float) 
            |> mapDatedArrayItems ((*) (100000./malePopulation))
        
        let newCasesTimelineFemale =
            casesTimeline
            |> mapDatedArrayItems
                (fun dayGroupsData ->
                    let dataForGroup =
                        dayGroupsData
                        |> List.find (fun group -> group.GroupKey = ageGroupKey)

                    dataForGroup.Female |> Utils.optionToInt |> float) 
            |> mapDatedArrayItems ((*) (100000./femalePopulation))
        
        let difference = 
            newCasesTimelineMale.Data
            |> Array.map2 ( - ) newCasesTimelineFemale.Data

        let minimumOfDifference = difference |> Array.min |> abs

        let shiftedDifference = difference |> Array.map ((+) (minimumOfDifference)) //to avoid negative numbers when taking logs

        {StartDate = newCasesTimelineFemale.StartDate; Data = shiftedDifference}





let accumulateWeeklyCases (casesTimeline: CasesInAgeGroupTimeline): CasesInAgeGroupTimeline =

    //an ugly hack to ensure that the data starts on a Monday instead of Wednesday
    let padding = [| 0.; 0. |]

    let paddedCases =
        Array.concat [ padding
                       casesTimeline.Data ]

    let shiftedStartDate = casesTimeline.StartDate |> Days.add -2

    let cases =
        paddedCases
        |> Seq.chunkBySize 7
        |> Seq.map Seq.sum
        |> Seq.toArray



    { StartDate = shiftedStartDate
      Data = cases }


let tooltipFormatter jsThis =
    let point = jsThis?point
    let date = point?dateSpan
    let name = point?name
    let cases = point?cases
    let color = point?z

    let label = sprintf "<b> %s </b><br/>" (date.ToString())

    label 
        + sprintf "%s<br/>" point?x 
        + sprintf "%s<br/>" cases 
        + sprintf "%s<br/>" point?value 
        + sprintf "%s" point?y
        // + sprintf "<br>%s: <b>%s</b> %s" (I18N.t "charts.map.confirmedCases") (I18N.NumberFormat.formatNumber(value100k:float)) (I18N.t "charts.map.per100k")


