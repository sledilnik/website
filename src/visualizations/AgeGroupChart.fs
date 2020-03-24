[<RequireQualifiedAccess>]
module AgeGroupChart

open Feliz
open Feliz.Recharts

open Types

let renderChart (data : StatsData) =
    let latestDataPoint = List.last data
    let ageGroupData =
        [ latestDataPoint.AgeGroups.Below16
          latestDataPoint.AgeGroups.From16to29
          latestDataPoint.AgeGroups.From30to49
          latestDataPoint.AgeGroups.From50to59
          latestDataPoint.AgeGroups.Above60 ]

    Recharts.barChart [
        barChart.data ageGroupData
        barChart.maxBarSize 40
        barChart.barCategoryGapPercentage 15
        barChart.children [
            Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]
            Recharts.xAxis [ xAxis.dataKey (fun (point : AgeGroup) ->
                match point.AgeFrom, point.AgeTo with
                | None, None -> ""
                | None, Some b -> sprintf "0-%d" b
                | Some a, Some b -> sprintf "%d-%d" a b
                | Some a, None -> sprintf "nad %d" a ) ]
            Recharts.yAxis [ ]
            Recharts.tooltip [ ]
            Recharts.legend [ ]

            Recharts.bar [
                bar.name "Vsi"
                bar.fill "#ffa600"
                bar.dataKey (fun (point : AgeGroup) -> point.TestedPositiveAll |> Option.defaultValue 0)
            ]

            Recharts.bar [
                bar.name "Ženske"
                bar.fill "#38a39e"
                bar.dataKey (fun (point : AgeGroup) -> point.TestedPositiveFemale |> Option.defaultValue 0)
            ]

            Recharts.bar [
                bar.name "Moški"
                bar.fill "#003f5c"
                bar.dataKey (fun (point : AgeGroup) -> point.TestedPositiveMale |> Option.defaultValue 0)
            ]
        ] ]

let render data =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 500
        responsiveContainer.chart (renderChart data) ]
