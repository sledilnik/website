[<RequireQualifiedAccess>]
module AgeGroupsChart

open Browser
open Feliz
open Fable.Core.JsInterop

open Types
open Highcharts

type DisplayType =
    | Infections
    | Deaths

    static member all = [ Infections ; Deaths ]

    static member getName = function
        | Infections -> "Potrjeno okuženi"
        | Deaths -> "Umrli"

// let renderBreakdownSelector breakdown current choose =
//     Html.div [
//         prop.onClick (fun _ -> choose breakdown)
//         prop.className [ true, "btn btn-sm metric-selector"; breakdown = current, "metric-selector--selected" ]
//         prop.text (breakdown |> Breakdown.getName)
//     ]

// let renderBreakdownSelectors current choose =
//     Html.div [
//         prop.className "metrics-selectors"
//         prop.children (
//             Breakdown.all
//             |> List.map (fun breakdown ->
//                 renderBreakdownSelector breakdown current choose
//             ) ) ]

let chartOptions (data : StatsData) (displayType : DisplayType) setDisplayType =

    let ageGroupsData =
        data
        |> List.rev
        |> List.pick (fun dataPoint ->
            dataPoint.StatePerAgeToDate
            |> List.filter (fun ageGroup ->
                match ageGroup.TestedPositiveMale, ageGroup.TestedPositiveFemale, ageGroup.TestedPositiveAll with
                | None, None, None -> false
                | _ -> true)
            |> function // take the most recent day with some data
                | [] -> None
                | filtered -> Some filtered
        )

    let xAxisDataKey (dp : AgeGroup) =
        (fun (dp : AgeGroup) ->
            match dp.AgeFrom, dp.AgeTo with
            | None, None -> ""
            | None, Some b -> sprintf "0-%d" b
            | Some a, Some b -> sprintf "%d-%d" a b
            | Some a, None -> sprintf "nad %d" a )

    let categories =
        [ "0-4" ; "5-9" ; "10-14" ; "15-19"
          "20-24" ; "25-29" ; "30-34" ; "35-39" ; "40-44"
          "45-49" ; "50-54" ; "55-59" ; "60-64" ; "65-69"
          "70-74" ; "75-79" ; "80-84" ; "85-89" ; "90-94"
          "95-99" ; "100 +" ]

    let baseOptions = basicChartOptions Linear "covid19-age-groups-comparison"

    {| baseOptions with
        chart = pojo {| ``type`` = "bar" |}
        title = pojo {| text = None |}
        xAxis = [|
            {| categories = List.toArray categories
               reversed = false
               opposite = false
               labels = {| step = 1 |}
               linkedTo = None |}
            {| categories = List.toArray categories // mirror axis on right side
               reversed = false
               opposite = true
               labels = {| step = 1 |}
               linkedTo = Some 0 |}
        |]

        yAxis = pojo
            {| title = {| text = "" |}
               labels = pojo {| formatter = fun () -> sprintf "%.1f%%" (abs(float jsThis?value)) |}
            |}

        plotOptions = pojo
            {| series =
                {| stacking = "normal" |} |}

        //tooltip = {| enabled = false |}
        // tooltip =
        //     {| formatter = fun () ->
        //         return "<b>" + this.series.name + ", age " + this.point.category + "</b><br/>" +
        //             "Population = " + Highcharts.numberFormat(Math.abs(this.point.y), 1) + "%";
        //     |}
        tooltip = pojo
            {| formatter = fun () ->
                sprintf "<b>%s, age %s</b><br/>Population = %.1f%%" jsThis?series?name jsThis?point?category (abs(float jsThis?point?y))
            |}

        series = [|
            {| name = "Male"
               xAxis = 0
               data =
                [ -2.2 ; -2.1 ; -2.2 ; -2.4
                  -2.7 ; -3.0 ; -3.3 ; -3.2
                  -2.9 ; -3.5 ; -4.4 ; -4.1
                  -3.4 ; -2.7 ; -2.3 ; -2.2
                  -1.6 ; -0.6 ; -0.3 ; -0.0
                  -0.0 ] |> List.toArray |}
            {| name = "Female"
               xAxis = 1
               data =
                [ 2.1 ; 2.0 ; 2.1 ; 2.3 ; 2.6
                  2.9 ; 3.2 ; 3.1 ; 2.9 ; 3.4
                  4.3 ; 4.0 ; 3.5 ; 2.9 ; 2.5
                  2.7 ; 2.2 ; 1.1 ; 0.6 ; 0.2
                  0.0 ] |> List.toArray |}
        |]
    |}

let renderChartContainer data displayType setDisplayType =
    Html.div [
        prop.style [ style.height 450 ]
        prop.className "highcharts-wrapper"
        prop.children [
            chartOptions data displayType setDisplayType
            |> Highcharts.chart
        ]
    ]

let render data =
    React.functionComponent (fun () ->
        let (displayType, setDisplayType) = React.useState Infections
        Html.div [
            renderChartContainer data displayType setDisplayType
            // renderBreakdownSelectors breakdown setBreakdown
        ]
    )
