[<RequireQualifiedAccess>]
module AgeGroupsChart

open Browser
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

open Types
open Highcharts

type ScaleType = Absolute | PopulationPercentage

type DisplayType =
    | Infections
    | Deaths

    static member all = [ Infections ; Deaths ]

    static member getName = function
        | Infections -> "Potrjeno okuženi"
        | Deaths -> "Umrli"

type State = { 
    ScaleType: ScaleType
    DisplayType: DisplayType
    Data: StatsData
}

type Msg =
    | ScaleTypeChanged of ScaleType
    | DisplayTypeChanged of DisplayType

let renderScaleTypeSelectors activeScaleType dispatch =
    let renderScaleTypeSelector 
        (scaleType : ScaleType) 
        (activeScaleType : ScaleType) 
        (label : string) =
        let defaultProps =
            [ prop.text label
              prop.className [
                  true, "chart-display-property-selector__item"
                  scaleType = activeScaleType, "selected" ] ]
        if scaleType = activeScaleType then 
            Html.div defaultProps
        else 
            Html.div 
                ((prop.onClick (fun _ -> dispatch scaleType)) :: defaultProps)

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            Html.text "Prikazane vrednosti: "
            renderScaleTypeSelector Absolute activeScaleType "Absolutne"
            renderScaleTypeSelector 
                PopulationPercentage activeScaleType "Delež prebivalstva"
        ]
    ]

let renderDisplayTypeSelector 
    (displayType: DisplayType) 
    (activeDisplayType: DisplayType) 
    dispatch =

    let isActive = displayType = activeDisplayType
    
    Html.div [
        prop.onClick (fun _ -> DisplayTypeChanged displayType |> dispatch)
        prop.className [ 
            true, "btn btn-sm metric-selector"; 
            isActive, "metric-selector--selected" ]
        prop.text (displayType |> DisplayType.getName)
    ]

let renderDisplayTypeSelectors activeDisplayType dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.all
            |> List.map (fun displayType ->
                renderDisplayTypeSelector 
                    displayType activeDisplayType dispatch
            ) ) ]

let renderChartOptions (state : State) =

    let ageGroupsData =
        state.Data
        |> List.rev
        |> List.pick (fun dataPoint ->
            let ageGroupsData =
                match state.DisplayType with
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
                match state.DisplayType with
                | Infections -> sprintf "<b>%s</b><br/>Starost: %s<br/>Potrjeno okuženih: %d" jsThis?series?name jsThis?point?category (abs(jsThis?point?y))
                | Deaths -> sprintf "<b>%s</b><br/>Starost: %s<br/>Umrli: %d" jsThis?series?name jsThis?point?category (abs(jsThis?point?y))
           |}
       series = [|
           {| name = "Moški"
              color =
                match state.DisplayType with
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
                match state.DisplayType with
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

let init (data : StatsData) : State * Cmd<Msg> =
    {   ScaleType = Absolute
        DisplayType = Infections
        Data = data }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none

let renderChartContainer state =
    Html.div [
        prop.style [ style.height 450 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state 
            |> Highcharts.chart
        ]
    ]

let render (state : State) dispatch =
    Html.div [
        renderScaleTypeSelectors state.ScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state
        renderDisplayTypeSelectors state.DisplayType dispatch
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent("AgeGroupsChart", init props.data, update, render)
