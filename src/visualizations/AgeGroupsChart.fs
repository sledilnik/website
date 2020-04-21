[<RequireQualifiedAccess>]
module AgeGroupsChart

open Browser
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

open Types
open Highcharts
open System

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

[<Literal>]
let LabelMale = "Moški"
[<Literal>]
let LabelFemale = "Ženske"

let ageGroupsLabels ageGroupsData =
    ageGroupsData
    |> List.map (fun ag ->
        match ag.AgeFrom, ag.AgeTo with
        | None, None -> ""
        | None, Some b -> sprintf "0-%d" b
        | Some a, Some b -> sprintf "%d-%d" a b
        | Some a, None -> sprintf "nad %d" a)
    |> List.toArray

let populationOf sexLabel ageGroupLabel =
    let parseAgeGroupLabel (label: string) =
        if label.Contains('-') then
            let i = label.IndexOf('-')
            let fromAge = Int32.Parse(label.Substring(0, i))
            let toAge = Int32.Parse(label.Substring(i+1))
            (Some fromAge, Some toAge)
        else if label.Contains("nad ") then
            let fromAge = Int32.Parse(label.Substring("nad ".Length))
            (Some fromAge, None)
        else
            sprintf "Invalid age group label: %s" label
            |> ArgumentException |> raise

    let (fromAge, toAge) = parseAgeGroupLabel ageGroupLabel
    let ageGroupStats = 
        Utils.AgePopulationStats.populationStatsForAgeGroup fromAge toAge

    match sexLabel with
    | LabelMale -> ageGroupStats.Male
    | LabelFemale -> ageGroupStats.Female
    | _ -> 
        sprintf "Invalid sex label: '%s'" sexLabel 
        |> ArgumentException |> raise

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

    let percentageOfPopulation affected total =
        let rawPercentage = (float affected) / (float total) * 100.
        System.Math.Round(rawPercentage, 3)

    let femaleValue (ageGroupData: AgeGroup) = 
        match state.ScaleType with
        | Absolute ->
            match ageGroupData.Female with
            | Some x -> float x |> Some
            | None -> None
        | PopulationPercentage -> 
            let populationStats = 
                Utils.AgePopulationStats.populationStatsForAgeGroup 
                    ageGroupData.AgeFrom ageGroupData.AgeTo

            match ageGroupData.Female with
            | Some x -> percentageOfPopulation x populationStats.Female |> Some
            | None -> None

    let maleValue (ageGroupData: AgeGroup) = 
        match state.ScaleType with
        | Absolute ->
            match ageGroupData.Male with
            | Some x -> float -x |> Some
            | None -> None
        | PopulationPercentage -> 
            let populationStats = 
                Utils.AgePopulationStats.populationStatsForAgeGroup 
                    ageGroupData.AgeFrom ageGroupData.AgeTo

            match ageGroupData.Male with
            | Some x -> -percentageOfPopulation x populationStats.Male |> Some
            | None -> None

    let percentageValuesLabelFormatter (value: float) =
        // A hack to replace decimal point with decimal comma.
        ((abs value).ToString() + "%").Replace('.', ',')

    let valuesLabelFormatter (value: float) = 
        match state.ScaleType with
        | Absolute -> (abs value).ToString()
        | PopulationPercentage -> percentageValuesLabelFormatter value

    {| chart = pojo {| ``type`` = "bar" |}
       title = pojo {| text = None |}
       xAxis = [|
           {| categories = ageGroupsLabels ageGroupsData
              reversed = false
              opposite = false
              linkedTo = None |}
           {| categories = ageGroupsLabels  ageGroupsData // mirror axis on right side
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
                match state.DisplayType, state.ScaleType with
                | Infections, Absolute -> 
                    sprintf 
                        "<b>%s</b><br/>Starost: %s<br/>Potrjeno okuženih: %d" 
                        jsThis?series?name 
                        jsThis?point?category 
                        (abs(jsThis?point?y))
                | Infections, PopulationPercentage -> 
                    sprintf 
                        "<b>%s</b><br/>Starost: %s<br/>Delež okuženega prebivalstva: %s<br/>Prebivalcev skupaj: %d" 
                        jsThis?series?name 
                        jsThis?point?category 
                        (percentageValuesLabelFormatter jsThis?point?y)
                        (populationOf jsThis?series?name jsThis?point?category)

                | Deaths, Absolute -> 
                    sprintf 
                        "<b>%s</b><br/>Starost: %s<br/>Umrli: %d" 
                        jsThis?series?name 
                        jsThis?point?category 
                        (abs(jsThis?point?y))
                | Deaths, PopulationPercentage -> 
                    sprintf 
                        "<b>%s</b><br/>Starost: %s<br/>Delež umrlih med prebivalstvom: %s<br/>Prebivalcev skupaj: %d" 
                        jsThis?series?name 
                        jsThis?point?category 
                        (percentageValuesLabelFormatter jsThis?point?y)
                        (populationOf jsThis?series?name jsThis?point?category)
           |}
       series = [|
           {| name = LabelMale
              color =
                match state.DisplayType with
                | Infections -> "#73CCD5"
                | Deaths -> "#73CCD5"
              dataLabels = pojo
                {| enabled = true
                   formatter = fun() -> valuesLabelFormatter jsThis?y
                   align = "right"
                   style = pojo {| textOutline = false |}
                   padding = 10 |}
              data =
               ageGroupsData
               |> List.map maleValue
               |> List.toArray |}
           {| name = LabelFemale
              color =
                match state.DisplayType with
                | Infections -> "#D99A91"
                | Deaths -> "#D99A91"
              dataLabels = pojo
                {| enabled = true
                   formatter = fun () -> valuesLabelFormatter jsThis?y
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
