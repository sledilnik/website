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


let extractTimelineForAgeGroup ageGroupKey
                               (metricsType: DisplayMetricsType)
                               (casesTimeline: CasesByAgeGroupsTimeline)
                               : CasesInAgeGroupTimeline =

    newExtractTimelineForAgeGroup ageGroupKey casesTimeline |> ignore

    let populationStats = populationStatsForAgeGroup ageGroupKey
    
    match metricsType with
    | RelativeCases ->
        let newCasesTimeline =
            casesTimeline
            |> mapDatedArrayItems
                (fun dayGroupsData ->
                    let dataForGroup =
                        dayGroupsData
                        |> List.find (fun group -> group.GroupKey = ageGroupKey)

                    dataForGroup.All |> Utils.optionToInt |> float)

        newCasesTimeline

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
        
        let difference = Array.map2 ( / ) newCasesTimelineFemale.Data newCasesTimelineMale.Data


        {StartDate = newCasesTimelineFemale.StartDate; Data = difference}




let accumulateWeeklyCases (casesTimeline: CasesInAgeGroupTimeline): CasesInAgeGroupTimeline =

    //hack to ensure that the data starts on a Monday instead of Wednesday
    let padding = [| 0.; 0. |]

    let paddedCases =
        Array.concat [ padding
                       casesTimeline.Data ]

    let paddedLen = Array.length paddedCases
    
    let cases = 
        let chunks = Seq.chunkBySize 7 paddedCases

        let trimmedChunks = 
            match paddedLen % 7 with
            | 0 -> chunks
            | _ -> Seq.truncate ((Seq.length chunks) - 1) chunks // truncate the end if not full week's worth of data 
        
        trimmedChunks
        |> Seq.map Seq.sum
        |> Seq.toArray


    let shiftedStartDate = casesTimeline.StartDate |> Days.add -2 // shift the start date to Monday

    { StartDate = shiftedStartDate
      Data = cases }



let sparklineFormatter jsThis = 0

// let sparklineFormatter jsThis allSeries = 
//     0

// let sparklineFormatter newCases state =
//     let desaturateColor (rgb:string) (sat:float) =
//         let argb = Int32.Parse (rgb.Replace("#", ""), Globalization.NumberStyles.HexNumber)
//         let r = (argb &&& 0x00FF0000) >>> 16
//         let g = (argb &&& 0x0000FF00) >>> 8
//         let b = (argb &&& 0x000000FF)
//         let avg = (float(r + g + b) / 3.0) * 1.6
//         let newR = int (Math.Round (float(r) * sat + avg * (1.0 - sat)))
//         let newG = int (Math.Round (float(g) * sat + avg * (1.0 - sat)))
//         let newB = int (Math.Round (float(b) * sat + avg * (1.0 - sat)))
//         sprintf "#%02x%02x%02x" newR newG newB

//     let color1 = "#bda506"
//     let color2 = desaturateColor color1 0.6
//     let color3 = desaturateColor color1 0.3

//     let temp = [|([| color3 |] |> Array.replicate 42 |> Array.concat )
//                  ([|color2 |] |> Array.replicate 7 |> Array.concat)|]
//                |> Array.concat
//     let columnColors =
//         [| temp; ([| color1 |] |> Array.replicate 7 |> Array.concat)  |]
//         |> Array.concat

//     let options =
//         {|
//             chart =
//                 {|
//                     ``type`` = "column"
//                     backgroundColor = "transparent"
//                 |} |> pojo
//             credits = {| enabled = false |}
//             xAxis =
//                 {|
//                     visible = true
//                     labels = {| enabled = false |}
//                     title = {| enabled = false |}
//                     tickInterval = 7
//                     lineColor = "#696969"
//                     tickColor = "#696969"
//                     tickLength = 4
//                 |}
//             yAxis =
//                 {|
//                     title = {| enabled = false |}
//                     visible = true
//                     opposite = true
//                     min = 0.
//                     max = newCases |> Array.max
//                     tickInterval = 5
//                     endOnTick = true
//                     startOnTick = false
//                     allowDecimals = false
//                     showFirstLabel = true
//                     showLastLabel = true
//                     gridLineColor = "#000000"
//                     gridLineDashStyle = "dot"
//                 |} |> pojo
//             title = {| text = "" |}
//             legend = {| enabled = false |}
//             series =
//                 [|
//                     {|
//                         data = newCases |> Array.map ( max 0.)
//                         animation = false
//                         colors = columnColors
//                         borderColor = columnColors
//                         pointWidth = 2
//                         colorByPoint = true
//                     |} |> pojo
//                 |]
//         |} |> pojo
//     match state.MapToDisplay with
//     | Municipality ->
//         Fable.Core.JS.setTimeout
//             (fun () -> sparklineChart("tooltip-chart-mun", options)) 10
//         |> ignore
//         """<div id="tooltip-chart-mun"; class="tooltip-chart";></div>"""
//     | Region ->
//         Fable.Core.JS.setTimeout
//             (fun () -> sparklineChart("tooltip-chart-reg", options)) 10
//         |> ignore
//         """<div id="tooltip-chart-reg"; class="tooltip-chart";></div>"""

let tooltipFormatter jsThis=

    let point = jsThis?point
    let date = point?dateSpan
    let weeks = point?weeks
    let ageGroupKey:AgeGroupKey = point?ageGroupKey

    let colorsCategories = {| Male = "#73CCD5" ; Female = "#D99A91" |}


    let label = sprintf "<b> %s </b>" (date.ToString())

    label 
        + sprintf "<br>%s: <b>%s</b>" (I18N.t "charts.heatmap.age") (ageGroupKey.Label)
        + sprintf "<br>%s: <b>%s</b> %s" (I18N.t "charts.heatmap.confirmedCases") ("confirmed cases") ("per 100 000")
        + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorsCategories.Male) (I18N.t "charts.heatmap.male") ("male cases") ("per 100 000")
        + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorsCategories.Female) (I18N.t "charts.heatmap.female") ("female cases") ("per 100 000")
    
    
