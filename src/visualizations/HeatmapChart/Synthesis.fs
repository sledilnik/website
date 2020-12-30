module HeatmapChart.Synthesis

open System
open System.Collections.Generic
open Types
open HeatmapChart.Analysis
open Fable.Core
open JsInterop
open Highcharts

open Utils.AgePopulationStats


type CasesInAgeGroupForDay = float

type CasesInAgeGroupTimeline = DatedArray<CasesInAgeGroupForDay>

type CasesInAgeGroupSeries =
    { AgeGroupKey: AgeGroupKey
      Timeline: CasesInAgeGroupTimeline }

type AllCasesInAgeGroupSeries = IDictionary<AgeGroupKey, CasesInAgeGroupSeries>

type ProcessedTimeline = 
    { All:float[]; Male:float[]; Female:float[]}

type ProcessedAgeGroupData =
    {AgeGroupKey: AgeGroupKey; Data: ProcessedTimeline; StartDate:DateTime}

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

    let processedData = 
        {
           AgeGroupKey = ageGroupKey
           Data = data
           StartDate = shiftedStartDate
        }

    processedData


let sparklineFormatter jsThis maleCases femaleCases = 

    let maleData = 
        // [|1;2;3;4|]
        maleCases
        |> Array.mapi (fun i x -> {| x=i; y=x |} |> pojo)

    let femaleData = 
        // [|1;2;3;4|]
        femaleCases
        |> Array.mapi (fun i x -> {| x=i; y=x |} |> pojo)
    
    let options = 
        {|
            chart = 
                {| 
                    ``type`` = "column" 
                    backgroundColor = "transparent"
                |}|> pojo
            legend = {|enable = false|}

            title = {| enable = false|}
            series = 
                [| 
                    {| 
                        animation = false 
                        data = maleData
                        color = "#73CCD5"
                    |}|> pojo 
                    {| 
                        animation = false 
                        data = femaleData 
                        color = "#D99A91"
                    |}|> pojo 
                |]
            
            plotOptions = 
                {|
                    column = {| stacking = "normal"|} |> pojo
                |}|>pojo

            xAxis = 
                {|
                    visible = true
                    labels = {| enabled = false|}
                    title = {| enabled = false|}
                    stack = "A"
                |}
            yAxis = 
                {|
                    visible = true
                    labels = {| enabled = false|}
                    title = {| enabled = false|}
                    stack = "A"
                |}

            credits = {| enable  = false|}
        |}

    Fable.Core.JS.setTimeout (fun () -> sparklineChart("tooltip-chart-heatmap", options)) 10 |> ignore
    """<div style="width: 350px; height: 200px;"id="tooltip-chart-heatmap">"""


let tooltipFormatter jsThis =

    let point = jsThis?point
    let date = point?dateSpan
    let value = point?value
    let (ageGroupKey:AgeGroupKey) = point?ageGroupKey
    let maleCases = point?maleCases
    let femaleCases = point?femaleCases

    let colorCategories = {| Male = "#73CCD5" ; Female = "#D99A91" |}

    let sparkline = sparklineFormatter jsThis maleCases femaleCases

    let label = sprintf "<b> %s </b>" (date.ToString())

    label 
        + sprintf "<br>%s: <b>%s</b>" (I18N.t "charts.heatmap.age") (ageGroupKey.Label)
        // + sprintf "<br>%s: <b>%s</b> %s" (I18N.t "charts.heatmap.confirmedCases") ("confirmed cases") (I18N.t "charts.heatmap.per100k")
        + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorCategories.Male) (I18N.t "charts.heatmap.male") ("male cases") (I18N.t "charts.heatmap.per100k")
        + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorCategories.Female) (I18N.t "charts.heatmap.female") ("female cases") (I18N.t "charts.heatmap.per100k")
        // + sprintf "<br>%s" value
        + sparkline
    
    
