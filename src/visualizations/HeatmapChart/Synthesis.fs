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


let sparklineFormatter jsThis casesMale casesFemale = 

    let options =
        {|
            credits = {| enabled = false |}
            xAxis = {| visible = false |}
            yAxis = {| visible = false |}
            title = {| text = "" |}
            legend = {| enabled = false |}
            series = [| {| data = [| 1 ; 2 ; 3 |] |} |]
        |} |> pojo
    Fable.Core.JS.setTimeout (fun () -> sparklineChart("tooltip-chart", options)) 10 |> ignore
    """<div style="width: 100px; height: 60px;" id="tooltip-chart">"""

    // let options = 
    //         {|
    //             chart = {|``type`` = "column"|} |> pojo

    //             plotOptions = 
    //                 {|
    //                     stacking = "normal"
    //                 |} |> pojo
                
    //             series = 
    //                 [|
    //                     {| data = casesFemale |} |> pojo
    //                     {| data = casesMale |} |> pojo
    //                 |]
    //         |}

    // Fable.Core.JS.setTimeout (fun () -> sparklineChart("tooltip-chart-mun", options)) 10 |> ignore

    // """<div id="tooltip-chart-heatmap"; class="tooltip-chart";></div>"""

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

let tooltipFormatter jsThis =

    let point = jsThis?point
    let date = point?dateSpan
    // let weeks = point?weeks
    let value = point?value
    // let ageGroupId = point?y
    let (ageGroupKey:AgeGroupKey) = point?ageGroupKey


    // printfn "%A" processedTimelineData

    // let ageGroupData:ProcessedTimeline =processedTimelineData.[ageGroupId].Data

    // let casesMale = ageGroupData.Male
    // let casesFemale = ageGroupData.Male

    let colorsCategories = {| Male = "#73CCD5" ; Female = "#D99A91" |}

    // let sparkline = sparklineFormatter jsThis casesFemale casesMale

    let label = sprintf "<b> %s </b>" (date.ToString())

    label 
        + sprintf "<br>%s: <b>%s</b>" (I18N.t "charts.heatmap.age") (ageGroupKey.Label)
        // + sprintf "<br>%s: <b>%s</b> %s" (I18N.t "charts.heatmap.confirmedCases") ("confirmed cases") (I18N.t "charts.heatmap.per100k")
        + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorsCategories.Male) (I18N.t "charts.heatmap.male") ("male cases") (I18N.t "charts.heatmap.per100k")
        + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorsCategories.Female) (I18N.t "charts.heatmap.female") ("female cases") (I18N.t "charts.heatmap.per100k")
        + sprintf "<br>%s" value
        // + sparkline
    
    
