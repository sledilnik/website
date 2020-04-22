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

type AgeGroupKey = {
    AgeFrom : int option
    AgeTo : int option
}

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

    merged |> List.sortBy (fun group -> group.GroupKey) |> List.rev

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
    AgeFrom: int option
    AgeTo: int option
}

type AgesChartData = {
    Categories: AgeCategoryChartData list
    } with

    member this.AgeGroupsLabels =
        this.Categories
        |> List.map (fun ag ->
            match ag.AgeFrom, ag.AgeTo with
            | None, None -> ""
            | None, Some b -> sprintf "0-%d" b
            | Some a, Some b -> sprintf "%d-%d" a b
            | Some a, None -> sprintf "nad %d" a)
        |> List.toArray

let calculateChartData infectionsAndDeathsPerAge chartMode =
    raise (NotImplementedException "")
    // match chartMode with
    // | AbsoluteInfections ->
    // | AbsoluteDeaths
    // | InfectionsPerPopulation
    // | DeathsPerPopulation
    // | DeathsPerInfections

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

let renderDisplayTypeSelector
    (displayType: ChartMode)
    (activeDisplayType: ChartMode)
    dispatch =

    let isActive = displayType = activeDisplayType

    Html.div [
        prop.onClick (fun _ -> ChartModeChanged displayType |> dispatch)
        prop.className [
            true, "btn btn-sm metric-selector";
            isActive, "metric-selector--selected" ]
        prop.text (
            match displayType with
            | AbsoluteInfections -> "Potrjeno okuženi"
            | AbsoluteDeaths -> "Umrli"
            | InfectionsPerPopulation -> "Delež potrjeno okuženih"
            | DeathsPerPopulation -> "Delež umrlih"
            | DeathsPerInfections -> "Umrli glede na okužene"
            )
    ]

let renderDisplayTypeSelectors activeChartMode dispatch =
    let displayTypesForChartMode chartMode =
        match ChartMode.ScaleType chartMode with
        | Absolute -> [ AbsoluteInfections; AbsoluteDeaths ]
        | Relative ->
            [ InfectionsPerPopulation;
            DeathsPerPopulation;
            DeathsPerInfections; ]

    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            displayTypesForChartMode activeChartMode
            |> List.map (fun displayType ->
                renderDisplayTypeSelector
                    displayType activeChartMode dispatch
            ) ) ]

let renderChartOptions
    (state : State) (infectionsAndDeathsPerAge: InfectionsAndDeathsPerAge) =

    let percentageOfPopulation affected total =
        let rawPercentage = (float affected) / (float total) * 100.
        System.Math.Round(rawPercentage, 3)

    let femaleValue (ageGroupData: AgeGroup) =
        match ChartMode.ScaleType state.ChartMode with
        | Absolute ->
            match ageGroupData.Female with
            | Some x -> float x |> Some
            | None -> None
        | Relative ->
            let populationStats =
                Utils.AgePopulationStats.populationStatsForAgeGroup
                    ageGroupData.AgeFrom ageGroupData.AgeTo

            match ageGroupData.Female with
            | Some x -> percentageOfPopulation x populationStats.Female |> Some
            | None -> None

    let maleValue (ageGroupData: AgeGroup) =
        match ChartMode.ScaleType state.ChartMode with
        | Absolute ->
            match ageGroupData.Male with
            | Some x -> float -x |> Some
            | None -> None
        | Relative ->
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
        match ChartMode.ScaleType state.ChartMode with
        | Absolute -> (abs value).ToString()
        | Relative -> percentageValuesLabelFormatter value

    {| chart = pojo {| ``type`` = "bar" |}
       title = pojo {| text = None |}
       xAxis = [|
           {| categories = "" // TODO: ageGroupsLabels ageGroupsData
              reversed = false
              opposite = false
              linkedTo = None |}
           {| categories = "" // TODO: ageGroupsLabels ageGroupsData // mirror axis on right side
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
                        "<b>%s</b><br/>Starost: %s<br/>Potrjeno okuženih: %d"
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
                | DeathsPerInfections -> "" // TODO
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
              data = [| |] // TODO
            //    ageGroupsData
            //    |> List.map maleValue
            //    |> List.toArray
               |}
           {| name = LabelFemale
              color = "#D99A91"
              dataLabels = pojo
                {| enabled = true
                   formatter = fun () -> valuesLabelFormatter jsThis?y
                   align = "left"
                   style = pojo {| textOutline = false |}
                   padding = 10 |}
              data = [| |] // TODO
            //    ageGroupsData
            //    |> List.map femaleValue
            //    |> List.toArray
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
        match scaleType with
        | Absolute -> { state with ChartMode = AbsoluteInfections }, Cmd.none
        | Relative -> { state with ChartMode = InfectionsPerPopulation }, Cmd.none

let renderChartContainer state =
    let infectionsAndDeathsPerAge = latestAgeData state
    let chartData = calculateChartData infectionsAndDeathsPerAge state.ChartMode

    Html.div [
        prop.style [ style.height 450 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state infectionsAndDeathsPerAge |> chart ]
    ]

let render (state : State) dispatch =
    let activeScaleType = ChartMode.ScaleType state.ChartMode

    Html.div [
        renderScaleTypeSelectors activeScaleType (ScaleTypeChanged >> dispatch)
        renderChartContainer state
        renderDisplayTypeSelectors state.ChartMode dispatch
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent("AgeGroupsChart", init props.data, update, render)
