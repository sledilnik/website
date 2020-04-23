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

type ScaleType = Absolute | Relative

type ChartMode =
    | AbsoluteInfections
    | AbsoluteDeaths
    | InfectionsPerPopulation
    | DeathsPerPopulation
    | DeathsPerInfections

    static member ScaleType mode =
        match mode with
        | AbsoluteInfections -> Absolute
        | AbsoluteDeaths -> Absolute
        | InfectionsPerPopulation -> Relative
        | DeathsPerPopulation -> Relative
        | DeathsPerInfections -> Relative

type State = {
    ChartMode: ChartMode
    Data: StatsData
}

type Msg =
    | ChartModeChanged of ChartMode
    | ScaleTypeChanged of ScaleType

[<Literal>]
let LabelMale = "Moški"
[<Literal>]
let LabelFemale = "Ženske"

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

let roundTo2Decimals (value: float) = System.Math.Round(value, 2)
let roundTo3Decimals (value: float) = System.Math.Round(value, 3)

let percentageOfPopulation affected total =
    let rawPercentage = (float affected) / (float total) * 100.
    rawPercentage |> roundTo3Decimals

let percentageOfInfected deaths infections =
    (float deaths) / (float infections) * 100. |> roundTo2Decimals

type AgeGroupKey = {
    AgeFrom : int option
    AgeTo : int option
    } with

    member this.Label =
        match this.AgeFrom, this.AgeTo with
        | None, None -> ""
        | None, Some b -> sprintf "0-%d" b
        | Some a, Some b -> sprintf "%d-%d" a b
        | Some a, None -> sprintf "nad %d" a

type InfectionsAndDeathsForAgeGroup = {
    GroupKey: AgeGroupKey
    InfectionsMale : int option
    InfectionsFemale : int option
    DeathsMale : int option
    DeathsFemale : int option
}

type InfectionsAndDeathsPerAge = InfectionsAndDeathsForAgeGroup list

let mergeInfectionsAndDeathsByGroups
    (infections: AgeGroupsList) (deaths: AgeGroupsList)
    : InfectionsAndDeathsPerAge =

    let mappedInfections =
        infections
        |> List.map (fun group ->
            let groupKey = { AgeFrom = group.AgeFrom; AgeTo = group.AgeTo }
            let combined = { GroupKey = groupKey
                             InfectionsMale = group.Male
                             InfectionsFemale = group.Female
                             DeathsMale = None; DeathsFemale = None }
            combined)

    let deathsDict =
        deaths
        |> Seq.map (fun group ->
            { AgeFrom = group.AgeFrom; AgeTo = group.AgeTo }, group)
        |> dict

    let merged =
        mappedInfections
        |> List.map (fun combined ->
            match deathsDict.TryGetValue combined.GroupKey with
            | (true, deathsForGroup) ->
                { combined with
                        DeathsMale = deathsForGroup.Male
                        DeathsFemale = deathsForGroup.Female }
            | (false, _) -> combined
            )

    merged |> List.sortBy (fun group -> group.GroupKey)

/// <summary>
/// Fetches the infections and deaths per age groups for the latest day that
/// has both sets of data.
/// </summary>
let latestAgeData state: InfectionsAndDeathsPerAge =
    /// <summary>
    /// Filter function for determining whether the specified AgeGroups
    /// actually has any data or just an empty record.
    /// </summary>
    let extractAgeGroupsDataMaybe (ageGroupsData: AgeGroupsList) =
        ageGroupsData
            |> List.filter (fun ageGroup ->
                match ageGroup.Male, ageGroup.Female with
                | None, None -> false
                | _ -> true)
            |> function // take the most recent day with some data
                | [] -> None
                | _ -> Some ageGroupsData

    state.Data
    |> List.rev
    |> List.pick (fun dataPoint ->
        let infectionsDataMaybe =
            extractAgeGroupsDataMaybe dataPoint.StatePerAgeToDate
        let deathsDataMaybe =
            extractAgeGroupsDataMaybe dataPoint.DeceasedPerAgeToDate

        match infectionsDataMaybe, deathsDataMaybe with
        | Some infectionsData, Some deathsData ->
            mergeInfectionsAndDeathsByGroups infectionsData deathsData |> Some
        | _ -> None
    )

type AgeCategoryChartData = {
    GroupKey: AgeGroupKey
    Male: float option
    Female: float option
}

type AgesChartData = {
    Categories: AgeCategoryChartData list
    } with

    member this.AgeGroupsLabels =
        this.Categories
        |> List.map (fun ag -> ag.GroupKey.Label)
        |> List.toArray

    member this.MaleValues =
        this.Categories
        |> List.map (fun ag -> ag.Male)
        |> List.toArray

    member this.FemaleValues =
        this.Categories
        |> List.map (fun ag -> ag.Female)
        |> List.toArray

let calculateChartData
    (infectionsAndDeathsPerAge: InfectionsAndDeathsPerAge) chartMode
    : AgesChartData =

    let categories =
        infectionsAndDeathsPerAge
        |> List.map (fun ageGroupData ->

            let (male, female) =
                match chartMode with
                | AbsoluteInfections ->
                    (ageGroupData.InfectionsMale |> Option.map float,
                     ageGroupData.InfectionsFemale |> Option.map float)
                | AbsoluteDeaths ->
                    (ageGroupData.DeathsMale |> Option.map float,
                     ageGroupData.DeathsFemale |> Option.map float)
                | InfectionsPerPopulation ->
                    let populationStats =
                        Utils.AgePopulationStats.populationStatsForAgeGroup
                            ageGroupData.GroupKey.AgeFrom
                            ageGroupData.GroupKey.AgeTo

                    let male =
                        match ageGroupData.InfectionsMale with
                        | Some x ->
                            percentageOfPopulation x populationStats.Male
                            |> Some
                        | None -> None
                    let female =
                        match ageGroupData.InfectionsFemale with
                        | Some x ->
                            percentageOfPopulation x populationStats.Female
                            |> Some
                        | None -> None
                    (male, female)

                | DeathsPerPopulation ->
                    let populationStats =
                        Utils.AgePopulationStats.populationStatsForAgeGroup
                            ageGroupData.GroupKey.AgeFrom
                            ageGroupData.GroupKey.AgeTo

                    let male =
                        match ageGroupData.DeathsMale with
                        | Some x ->
                            percentageOfPopulation x populationStats.Male
                            |> Some
                        | None -> None
                    let female =
                        match ageGroupData.DeathsFemale with
                        | Some x ->
                            percentageOfPopulation x populationStats.Female
                            |> Some
                        | None -> None
                    (male, female)

                | DeathsPerInfections ->
                    let male =
                        match ageGroupData.DeathsMale,
                            ageGroupData.InfectionsMale with
                        | (_, Some 0) -> None
                        | (Some deaths, Some infections) ->
                            percentageOfInfected deaths infections |> Some
                        | _ -> None
                    let female =
                        match ageGroupData.DeathsFemale,
                            ageGroupData.InfectionsFemale with
                        | (_, Some 0) -> None
                        | (Some deaths, Some infections) ->
                            percentageOfInfected deaths infections |> Some
                        | _ -> None
                    (male, female)

            { GroupKey = ageGroupData.GroupKey
              Male = male; Female = female }
            )

    { Categories = categories }

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
            renderScaleTypeSelector Relative activeScaleType "Relativne"
        ]
    ]

let renderChartCategorySelector
    (activeChartMode: ChartMode)
    dispatch
    (chartModeToRender: ChartMode) =

    let isActive = chartModeToRender = activeChartMode

    Html.div [
        prop.onClick (fun _ -> ChartModeChanged chartModeToRender |> dispatch)
        prop.className [
            true, "btn btn-sm metric-selector";
            isActive, "metric-selector--selected" ]
        prop.text (
            match chartModeToRender with
            | AbsoluteInfections -> "Potrjeno okuženi"
            | AbsoluteDeaths -> "Umrli"
            | InfectionsPerPopulation -> "Delež potrjeno okuženih"
            | DeathsPerPopulation -> "Delež umrlih"
            | DeathsPerInfections -> "Umrli glede na št. okuženih"
            )
    ]

let renderChartCategorySelectors activeChartMode dispatch =
    let categoriesForChartMode chartMode =
        match ChartMode.ScaleType chartMode with
        | Absolute -> [ AbsoluteInfections; AbsoluteDeaths ]
        | Relative ->
            [ InfectionsPerPopulation;
            DeathsPerPopulation;
            DeathsPerInfections; ]

    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            categoriesForChartMode activeChartMode
            |> List.map (renderChartCategorySelector activeChartMode dispatch)
            ) ]

let renderChartOptions
    (state : State) (chartData: AgesChartData) =

    let percentageValuesLabelFormatter (value: float) =
        // A hack to replace decimal point with decimal comma.
        ((abs value).ToString() + "%").Replace('.', ',')

    let valuesLabelFormatter (value: float) =
        match ChartMode.ScaleType state.ChartMode with
        | Absolute -> (abs value).ToString()
        | Relative -> percentageValuesLabelFormatter value

    {| chart = pojo {| ``type`` = "bar" |}
       title = pojo {| text = None |}
       xAxis = [|
           {| categories = chartData.AgeGroupsLabels
              reversed = false
              opposite = false
              linkedTo = None |}
           {| categories = chartData.AgeGroupsLabels // mirror axis on right side
              reversed = false
              opposite = true
              linkedTo = Some 0 |}
       |]
       yAxis = pojo
           {| title = {| text = "" |}
              labels = pojo
                {| formatter = fun () -> valuesLabelFormatter jsThis?value |}
              // allowDecimals needs to be enabled because the values can be
              // be below 1, otherwise it won't auto-scale to below 1.
              allowDecimals = ChartMode.ScaleType state.ChartMode = Relative
           |}
       plotOptions = pojo
           {| series = pojo
               {| stacking = "normal" |}
           |}
       tooltip = pojo
           {| formatter = fun () ->
                match state.ChartMode with
                | AbsoluteInfections ->
                    sprintf
                        "<b>%s</b><br/>Starost: %s<br/>Potrjeno okuženi: %d"
                        jsThis?series?name
                        jsThis?point?category
                        (abs(jsThis?point?y))
                | InfectionsPerPopulation ->
                    sprintf
                        "<b>%s</b><br/>Starost: %s<br/>Delež okuženega prebivalstva: %s<br/>Prebivalcev skupaj: %d"
                        jsThis?series?name
                        jsThis?point?category
                        (percentageValuesLabelFormatter jsThis?point?y)
                        (populationOf jsThis?series?name jsThis?point?category)

                | AbsoluteDeaths ->
                    sprintf
                        "<b>%s</b><br/>Starost: %s<br/>Umrli: %d"
                        jsThis?series?name
                        jsThis?point?category
                        (abs(jsThis?point?y))
                | DeathsPerPopulation ->
                    sprintf
                        "<b>%s</b><br/>Starost: %s<br/>Delež umrlih med prebivalstvom: %s<br/>Prebivalcev skupaj: %d"
                        jsThis?series?name
                        jsThis?point?category
                        (percentageValuesLabelFormatter jsThis?point?y)
                        (populationOf jsThis?series?name jsThis?point?category)
                | DeathsPerInfections ->
                    sprintf
                        "<b>%s</b><br/>Starost: %s<br/>Delež umrlih glede na št. okuženih: %s"
                        jsThis?series?name
                        jsThis?point?category
                        (percentageValuesLabelFormatter jsThis?point?y)
           |}
       series = [|
           {| name = LabelMale
              color = "#73CCD5"
              dataLabels = pojo
                {| enabled = true
                   formatter = fun() -> valuesLabelFormatter jsThis?y
                   align = "right"
                   style = pojo {| textOutline = false |}
                   padding = 10 |}
              data = chartData.MaleValues
                     |> Array.map (Option.map (fun y -> -y))
               |}
           {| name = LabelFemale
              color = "#D99A91"
              dataLabels = pojo
                {| enabled = true
                   formatter = fun () -> valuesLabelFormatter jsThis?y
                   align = "left"
                   style = pojo {| textOutline = false |}
                   padding = 10 |}
              data = chartData.FemaleValues
               |}
       |]
    |}

let init (data : StatsData) : State * Cmd<Msg> =
    { ChartMode = AbsoluteInfections; Data = data }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChartModeChanged chartMode ->
        { state with ChartMode = chartMode}, Cmd.none
    | ScaleTypeChanged scaleType ->
        let toChartMode =
            match scaleType with
            | Absolute ->
                match state.ChartMode with
                | DeathsPerPopulation -> AbsoluteDeaths
                | DeathsPerInfections -> AbsoluteDeaths
                | _ -> AbsoluteInfections
            | Relative ->
                match state.ChartMode with
                | AbsoluteInfections -> InfectionsPerPopulation
                | _ -> DeathsPerPopulation
        { state with ChartMode = toChartMode }, Cmd.none

let renderChartContainer state =
    let infectionsAndDeathsPerAge = latestAgeData state
    let chartData = calculateChartData infectionsAndDeathsPerAge state.ChartMode

    Html.div [
        prop.style [ style.height 450 ]
        prop.className "highcharts-wrapper"
        prop.children [ renderChartOptions state chartData |> chart ]
    ]

let render (state : State) dispatch =
    let activeScaleType = ChartMode.ScaleType state.ChartMode

    Html.div [
        renderScaleTypeSelectors activeScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state
        renderChartCategorySelectors state.ChartMode dispatch
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent("AgeGroupsChart", init props.data, update, render)
