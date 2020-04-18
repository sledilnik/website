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
                | filtered -> Some ageGroupsData
        )

    let ageGroups =
        ageGroupsData
        |> List.map (fun ag ->
            match ag.AgeFrom, ag.AgeTo with
            | None, None -> ""
            | None, Some b -> sprintf "0-%d" b
            | Some a, Some b -> sprintf "%d-%d" a b
            | Some a, None -> sprintf "nad %d" a)
        |> List.toArray

    {| chart = pojo {| ``type`` = "bar" |}
       title = pojo {| text = None |}
       xAxis = [|
           {| categories = ageGroups
              reversed = false
              opposite = false
              linkedTo = None |}
           {| categories = ageGroups // mirror axis on right side
              reversed = false
              opposite = true
              linkedTo = Some 0 |}
       |]
       yAxis = pojo
           {| title = {| text = "" |}
              labels = pojo {| formatter = fun () -> abs(jsThis?value) |}
           |}
       plotOptions = pojo
           {| series =
               {| stacking = "normal" |} |}
       tooltip = pojo
           {| formatter = fun () ->
                match displayType with
                | Infections -> sprintf "<b>%s</b><br/>Starost: %s<br/>Potrjeno okuženih: %d" jsThis?series?name jsThis?point?category (abs(jsThis?point?y))
                | Deaths -> sprintf "<b>%s</b><br/>Starost: %s<br/>Umrli: %d" jsThis?series?name jsThis?point?category (abs(jsThis?point?y))
           |}
       series = [|
           {| name = "Moški"
              color = "#73ccd5"
              data =
               ageGroupsData
               |> List.map (fun dp -> dp.Male |> Option.map (fun x -> -x))
               |> List.toArray |}
           {| name = "Ženske"
              color = "#d99a91"
              data =
               ageGroupsData
               |> List.map (fun dp -> dp.Female)
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
        let (displayType, setDisplayType) = React.useState Deaths
        Html.div [
            renderChartContainer data displayType setDisplayType
            renderDisplayTypeSelectors displayType setDisplayType
        ]
    )
