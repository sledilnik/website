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

let renderDisplayTypeSelector displaytype current choose =
    Html.div [
        prop.onClick (fun _ -> choose displaytype)
        prop.className [ true, "btn btn-sm metric-selector"; displaytype = current, "metric-selector--selected" ]
        prop.text (displaytype |> DisplayType.getName)
    ]

let renderDisplayTypeSelectors current choose =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.all
            |> List.map (fun displaytype ->
                renderDisplayTypeSelector displaytype current choose
            ) ) ]

let chartOptions (data : StatsData) (displayType : DisplayType) setDisplayType =

    let ageGroupsData =
        data
        |> List.rev
        |> List.pick (fun dataPoint ->
            let ageGroupsData =
                match displayType with
                | Infections -> dataPoint.StatePerAgeToDate
                | Deaths -> dataPoint.DeceasedPerAgeToDate
            ageGroupsData
            |> List.filter (fun ageGroup ->
                match ageGroup.Male, ageGroup.Female, ageGroup.All with
                | None, None, None -> false
                | _ -> true)
            |> function // take the most recent day with some data
                | [] -> None
                | _ -> Some ageGroupsData
        )

    let ageGroupsLabels =
        ageGroupsData
        |> List.map (fun ag ->
            match ag.AgeFrom, ag.AgeTo with
            | None, None -> ""
            | None, Some b -> sprintf "0-%d" b
            | Some a, Some b -> sprintf "%d-%d" a b
            | Some a, None -> sprintf "nad %d" a)
        |> List.toArray

    let percentageOfPopulation affected total =
        let rawPercentage = (float affected) / (float total) * 100.
        System.Math.Round(rawPercentage, 3)

    let femaleValue (ageGroupData: AgeGroup) = 
        let populationStats = 
            Utils.AgePopulationStats.populationStatsForAgeGroup 
                ageGroupData.AgeFrom ageGroupData.AgeTo

        match ageGroupData.Female with
        | Some x -> percentageOfPopulation x populationStats.Female |> Some
        | None -> None

    let maleValue (ageGroupData: AgeGroup) = 
        let populationStats = 
            Utils.AgePopulationStats.populationStatsForAgeGroup 
                ageGroupData.AgeFrom ageGroupData.AgeTo

        match ageGroupData.Male with
        | Some x -> -percentageOfPopulation x populationStats.Male |> Some
        | None -> None

    let labelFormatterPercentage (value: float) = 
        // A hack to replace decimal point with decimal comma.
        ((abs value).ToString() + "%").Replace('.', ',')

    {| chart = pojo {| ``type`` = "bar" |}
       title = pojo {| text = None |}
       xAxis = [|
           {| categories = ageGroupsLabels
              reversed = false
              opposite = false
              linkedTo = None |}
           {| categories = ageGroupsLabels // mirror axis on right side
              reversed = false
              opposite = true
              linkedTo = Some 0 |}
       |]
       yAxis = pojo
           {| title = {| text = "" |}
              labels = pojo {| formatter = fun () -> abs(jsThis?value) |}
              // allowDecimals needs to be enabled because the values can be
              // be below 1, otherwise it won't auto-scale to below 1.
              allowDecimals = true
           |}
       plotOptions = pojo
           {| series = pojo
               {| stacking = "normal" |}
           |}
       tooltip = pojo
           {| formatter = fun () ->
                match displayType with
                | Infections -> sprintf "<b>%s</b><br/>Starost: %s<br/>Potrjeno okuženih: %d" jsThis?series?name jsThis?point?category (abs(jsThis?point?y))
                | Deaths -> sprintf "<b>%s</b><br/>Starost: %s<br/>Umrli: %d" jsThis?series?name jsThis?point?category (abs(jsThis?point?y))
           |}
       series = [|
           {| name = "Moški"
              color =
                match displayType with
                | Infections -> "#73CCD5"
                | Deaths -> "#73CCD5"
              dataLabels = pojo
                {| enabled = true
                   formatter = fun() -> labelFormatterPercentage jsThis?y
                   align = "right"
                   style = pojo {| textOutline = false |}
                   padding = 10 |}
              data =
               ageGroupsData
               |> List.map maleValue
               |> List.toArray |}
           {| name = "Ženske"
              color =
                match displayType with
                | Infections -> "#D99A91"
                | Deaths -> "#D99A91"
              dataLabels = pojo
                {| enabled = true
                   formatter = fun () -> labelFormatterPercentage jsThis?y
                   align = "left"
                   style = pojo {| textOutline = false |}
                   padding = 10 |}
              data =
               ageGroupsData
               |> List.map femaleValue
               |> List.toArray |}
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
            renderDisplayTypeSelectors displayType setDisplayType
        ]
    )
