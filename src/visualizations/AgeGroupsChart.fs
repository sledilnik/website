[<RequireQualifiedAccess>]
module AgeGroupsChart

open Feliz

open Types
open Recharts
open Browser

type Breakdown =
    | Gender
    | Total
  with
    static member all = [ Gender; Total ]
    static member getName = function
        | Gender -> "Po spolu"
        | Total -> "Vsi"

let renderBars (data : StatsData) breakdown =
    let ageGroupData =
        data
        |> List.rev
        |> List.pick (fun dataPoint ->
            dataPoint.StatePerAgeToDate
            |> List.filter (fun ageGroup -> // keep non-empty
                match ageGroup.TestedPositiveMale, ageGroup.TestedPositiveFemale, ageGroup.TestedPositiveAll with
                | None, None, None -> false
                | _ -> true)
            |> function // take most recent day with some data
                | [] -> None
                | filtered -> Some filtered
        )

    let bars =
        match breakdown with
        | Total -> [
            Recharts.bar [
                bar.name "Vsi"
                bar.fill "#d5c768"
                bar.dataKey (fun (point : AgeGroup) -> point.TestedPositiveAll |> Option.defaultValue 0)
            ]]
        | Gender -> [
            Recharts.bar [
                bar.name "Ženske"
                bar.fill "#d99a91"
                bar.dataKey (fun (point : AgeGroup) -> point.TestedPositiveFemale |> Option.defaultValue 0)
            ]

            Recharts.bar [
                bar.name "Moški"
                bar.fill "#73ccd5"
                bar.dataKey (fun (point : AgeGroup) -> point.TestedPositiveMale |> Option.defaultValue 0)
            ]]

    Recharts.barChart [
        barChart.data ageGroupData
        barChart.maxBarSize 40
        barChart.barCategoryGapPercentage 15
        barChart.children ([
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
        ] @ bars)
    ]

let renderChart data breakdown =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 450
        responsiveContainer.chart (renderBars data breakdown) ]

let renderBreakdownSelector breakdown current choose =
    Html.div [
        prop.onClick (fun _ -> choose breakdown)
        prop.className [ true, "btn btn-sm metric-selector"; breakdown = current, "metric-selector--selected" ]
        prop.text (breakdown |> Breakdown.getName)
    ]

let renderBreakdownSelectors current choose =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            Breakdown.all
            |> List.map (fun breakdown ->
                renderBreakdownSelector breakdown current choose
            ) ) ]

let render data =
    let elm = React.functionComponent (fun () ->
        let (breakdown, setBreakdown) = React.useState Gender
        Html.div [
            renderChart data breakdown
            renderBreakdownSelectors breakdown setBreakdown
        ]
    )

    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true);
    document.dispatchEvent(evt) |> ignore
    elm
