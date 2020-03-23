module AgeGroupChart

open Feliz
open Feliz.Recharts

open Types

// type Point = { name: string; uv: int; pv: int; }

// let data = [
//     { name = "Page A"; uv = 4000; pv = 2400 }
//     { name = "Page B"; uv = 3000; pv = 1398 }
//     { name = "Page C"; uv = 2000; pv = 9800 }
//     { name = "Page D"; uv = 2780; pv = 3908 }
//     { name = "Page E"; uv = 1890; pv = 4800 }
//     { name = "Page F"; uv = 2390; pv = 3800 }
//     { name = "Page G"; uv = 3490; pv = 4300 }
// ]

let renderChart (data : StatsData) =
    let lastDataPoint = List.last data
    let ageGroupData =
        [ lastDataPoint.AgeGroups.Below16
          lastDataPoint.AgeGroups.From16to29
          lastDataPoint.AgeGroups.From30to49
          lastDataPoint.AgeGroups.From50to59
          lastDataPoint.AgeGroups.Above60 ]

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
