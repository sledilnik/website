module HeatmapChart.Synthesis

open Types
open HeatmapChart.Analysis
open System.Collections.Generic
open System.Text
open Fable.Core
open JsInterop

open Utils.AgePopulationStats

//TODO: might wanna think about displaying the infections relative to the total population
// and having a switch for male/female/all


type CasesInAgeGroupForDay = float

type CasesInAgeGroupTimeline = DatedArray<CasesInAgeGroupForDay>

type CasesInAgeGroupSeries =
    { AgeGroupKey: AgeGroupKey
      Timeline: CasesInAgeGroupTimeline }

type AllCasesInAgeGroupSeries = IDictionary<AgeGroupKey, CasesInAgeGroupSeries>


type DisplayMetricsType =
    | AbsoluteCases
    | RelativeCases

type DisplayMetrics =
    { Id: string
      MetricsType: DisplayMetricsType }

let availableDisplayMetrics =
    [| { Id = "absolute"
         MetricsType = AbsoluteCases }
       { Id = "relative"
         MetricsType = RelativeCases } |]

let listAgeGroups (timeline: CasesByAgeGroupsTimeline): AgeGroupKey list =
    timeline.Data.[0]
    |> List.map (fun group -> group.GroupKey)
    |> List.sortBy (fun groupKey -> groupKey.AgeFrom)

let extractTimelineForAgeGroup ageGroupKey
                               (metricsType: DisplayMetricsType)
                               (casesTimeline: CasesByAgeGroupsTimeline)
                               : CasesInAgeGroupTimeline =

    let newCasesTimeline =
        casesTimeline
        |> mapDatedArrayItems
            (fun dayGroupsData ->
                let dataForGroup =
                    dayGroupsData
                    |> List.find (fun group -> group.GroupKey = ageGroupKey)

                dataForGroup.All |> Utils.optionToInt |> float)

    match metricsType with
    | AbsoluteCases -> newCasesTimeline
    | RelativeCases ->
        let populationStats = populationStatsForAgeGroup ageGroupKey

        let totalPopulation =
            float (populationStats.Female + populationStats.Male)

        newCasesTimeline
        |> mapDatedArrayItems (fun x -> float x * 100000. / totalPopulation)




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

    let label = sprintf "<b> %s </b><br/>" (date.ToString())

    label 
        // + sprintf "<br>%s: <b>%s</b> %s" (I18N.t "charts.map.confirmedCases") (I18N.NumberFormat.formatNumber(value100k:float)) (I18N.t "charts.map.per100k")


