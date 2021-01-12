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

let chartText = I18N.chartText "ageGroups"

type ScaleType = Absolute | Relative

type ChartMode =
    | AbsoluteInfections
    | AbsoluteDeaths
    | InfectionsPerPopulation
    | DeathsPerPopulation
    | DeathsPerInfections

    static member All =
        [ AbsoluteInfections
          AbsoluteDeaths
          InfectionsPerPopulation
          DeathsPerPopulation
          DeathsPerInfections ]

    static member Default = AbsoluteInfections

    member this.GetName: string =
        match this with
        | AbsoluteInfections -> chartText "confirmedCases"
        | AbsoluteDeaths -> chartText "deceased"
        | InfectionsPerPopulation -> chartText "confirmedCases"
        | DeathsPerPopulation -> chartText "deceased"
        | DeathsPerInfections -> chartText "deceasedPerConfirmedCases"

    member this.ScaleType =
        match this with
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

let maybeFloat = Option.map float

[<Literal>]
let LabelMale = "Moški"
[<Literal>]
let LabelFemale = "Ženske"


let populationOf sexLabel ageGroupLabel =
    let parseAgeGroupLabel (label: string): AgeGroupKey =
        if label.Contains('-') then
            let i = label.IndexOf('-')
            let fromAge = Int32.Parse(label.Substring(0, i))
            let toAge = Int32.Parse(label.Substring(i+1))
            { AgeFrom = Some fromAge; AgeTo =  Some toAge }
        else if label.Contains("+") then
            let i = label.IndexOf('-')
            let fromAge = Int32.Parse(label.Substring(0, i))
            { AgeFrom = Some fromAge; AgeTo =  None }
        else
            sprintf "Invalid age group label: %s" label
            |> ArgumentException |> raise

    let groupKey = parseAgeGroupLabel ageGroupLabel
    let ageGroupStats =
        Utils.AgePopulationStats.populationStatsForAgeGroup groupKey

    match sexLabel with
    | LabelMale -> ageGroupStats.Male
    | LabelFemale -> ageGroupStats.Female
    | _ ->
        sprintf "Invalid sex label: '%s'" sexLabel
        |> ArgumentException |> raise

let percentageOfPopulation affected total =
    let rawPercentage = (float affected) / (float total) * 100.
    rawPercentage |> Utils.roundTo3Decimals

let percentageOfPopulationMaybe infections population =
    infections |> Option.map (fun x -> percentageOfPopulation x population)

let percentageOfInfected deaths infections =
    (float deaths) / (float infections) * 100. |> Utils.roundTo1Decimal

let deathsPerInfectionsMaybe deaths infections =
    match deaths, infections with
    | (_, Some 0) -> None
    | (Some deaths, Some infections) ->
        percentageOfInfected deaths infections |> Some
    | _ -> None

type InfectionsAndDeathsForAgeGroup = {
    GroupKey: AgeGroupKey
    InfectionsMale : int option
    InfectionsFemale : int option
    DeathsMale : int option
    DeathsFemale : int option
}

type InfectionsAndDeathsPerAge = InfectionsAndDeathsForAgeGroup[]

let mergeInfectionsAndDeathsByGroups
    (infections: AgeGroupsList) (deaths: AgeGroupsList)
    : InfectionsAndDeathsPerAge =

    let mappedInfections =
        infections
        |> List.map (fun group ->
            let groupKey = group.GroupKey
            let combined = { GroupKey = groupKey
                             InfectionsMale = group.Male
                             InfectionsFemale = group.Female
                             DeathsMale = None; DeathsFemale = None }
            combined)
        |> List.toArray

    let deathsDict =
        deaths
        |> Seq.map (fun group -> group.GroupKey, group)
        |> dict

    let merged =
        mappedInfections
        |> Array.map (fun combined ->
            match deathsDict.TryGetValue combined.GroupKey with
            | (true, deathsForGroup) ->
                { combined with
                        DeathsMale = deathsForGroup.Male
                        DeathsFemale = deathsForGroup.Female }
            | (false, _) -> combined
            )

    merged |> Array.sortBy (fun group -> group.GroupKey)

/// <summary>
/// Fetches the infections and deaths per age groups for the latest day that
/// has both sets of data.
/// </summary>
let latestAgeData state =
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
             Some (dataPoint.Date, mergeInfectionsAndDeathsByGroups infectionsData deathsData)
        | _ -> None
    )

type AgeCategoryChartData = {
    GroupKey: AgeGroupKey
    Male: float option
    Female: float option
}

type AgesChartData = {
    Categories: AgeCategoryChartData[]
    } with

    member this.AgeGroupsLabels =
        this.Categories
        |> Array.map (fun ag -> ag.GroupKey.Label)

    member this.MaleValues =
        this.Categories |> Array.map (fun ag -> ag.Male)

    member this.FemaleValues =
        this.Categories |> Array.map (fun ag -> ag.Female)

let calculateChartData
    (infectionsAndDeathsPerAge: InfectionsAndDeathsPerAge) chartMode
    : AgesChartData =

    let mapNoneToZero x =
        match x with
        | None -> Some 0
        | _ -> x

    // Since HC bar chart no longer seems to accept Nones for values,
    // we map them to zeros first.
    let infectionsWithoutNones =
        infectionsAndDeathsPerAge
        |> Array.map(fun x ->
                { GroupKey = x.GroupKey
                  InfectionsMale = x.InfectionsMale |> mapNoneToZero
                  InfectionsFemale = x.InfectionsFemale |> mapNoneToZero
                  DeathsMale = x.DeathsMale |> mapNoneToZero
                  DeathsFemale = x.DeathsFemale |> mapNoneToZero
                }
            )

    let categories =
        infectionsWithoutNones
        |> Array.map (fun ageGroupData ->
            let populationStats =
                Utils.AgePopulationStats.populationStatsForAgeGroup
                    ageGroupData.GroupKey

            let (male, female) =
                match chartMode with
                | AbsoluteInfections ->
                    (maybeFloat ageGroupData.InfectionsMale,
                     maybeFloat ageGroupData.InfectionsFemale)
                | AbsoluteDeaths ->
                    (maybeFloat ageGroupData.DeathsMale,
                     maybeFloat ageGroupData.DeathsFemale)
                | InfectionsPerPopulation ->
                    let male =
                        percentageOfPopulationMaybe
                            ageGroupData.InfectionsMale populationStats.Male
                    let female =
                        percentageOfPopulationMaybe
                            ageGroupData.InfectionsFemale populationStats.Female
                    (male, female)

                | DeathsPerPopulation ->
                    let male =
                        percentageOfPopulationMaybe
                            ageGroupData.DeathsMale populationStats.Male
                    let female =
                        percentageOfPopulationMaybe
                            ageGroupData.DeathsFemale populationStats.Female
                    (male, female)

                | DeathsPerInfections ->
                    let male =
                        deathsPerInfectionsMaybe
                            ageGroupData.DeathsMale
                            ageGroupData.InfectionsMale
                    let female =
                        deathsPerInfectionsMaybe
                            ageGroupData.DeathsFemale
                            ageGroupData.InfectionsFemale
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
              Utils.classes [
                  (true, "chart-display-property-selector__item")
                  (scaleType = activeScaleType, "selected") ] ]
        if scaleType = activeScaleType then
            Html.div defaultProps
        else
            Html.div
                ((prop.onClick (fun _ -> dispatch scaleType)) :: defaultProps)

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            renderScaleTypeSelector
                Absolute activeScaleType (chartText "absolute")
            renderScaleTypeSelector
                Relative activeScaleType (chartText "populationShare")
        ]
    ]

let renderChartCategorySelector
    (activeChartMode: ChartMode)
    dispatch
    (chartModeToRender: ChartMode) =

    let isActive = chartModeToRender = activeChartMode

    Html.div [
        prop.onClick (fun _ -> ChartModeChanged chartModeToRender |> dispatch)
        Utils.classes [
            (true, "btn btn-sm metric-selector")
            (isActive, "metric-selector--selected") ]
        prop.text ( chartModeToRender.GetName )
    ]

let renderChartCategorySelectors activeChartMode dispatch =
    let categoriesForChartMode (chartMode: ChartMode) =
        match chartMode.ScaleType with
        | Absolute -> [ AbsoluteInfections; AbsoluteDeaths ]
        | Relative ->
            [
                InfectionsPerPopulation;
                DeathsPerPopulation;
                // DeathsPerInfections;
            ]

    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            categoriesForChartMode activeChartMode
            |> List.map (renderChartCategorySelector activeChartMode dispatch)
            ) ]

let renderChartOptions
    (state : State) (latestDate: DateTime) (chartData: AgesChartData) =

    let valuesLabelFormatter (value: float) =
        match state.ChartMode.ScaleType with
        | Absolute -> (I18N.NumberFormat.formatNumber (abs value))
        | Relative -> Utils.percentWith3DecimalFormatter value

    let dateText = (I18N.tOptions "charts.common.dataDate" {| date = latestDate  |})

    {| optionsWithOnLoadEvent "covid19-age-groups" with
        chart = pojo {| ``type`` = "bar" |}
        title = pojo {| text = None |}
        subtitle = {| text = dateText ; align="left"; verticalAlign="bottom" |}
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
               allowDecimals = state.ChartMode.ScaleType = Relative
            |}
        plotOptions = pojo
            {| series = pojo
                {| stacking = "normal" |}
            |}
        credits = pojo
            {|
                enabled = true
                text =
                    sprintf "%s: %s, %s"
                        (I18N.chartText "common" "dataSource")
                        (I18N.chartText "common" "dsNIJZ")
                        (I18N.chartText "common" "dsMZ")
                href = "https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19"
            |}
        tooltip = pojo
            {| formatter = fun () ->
                 let sex = jsThis?series?name
                 let ageGroup = jsThis?point?category
                 let dataValue: float = jsThis?point?y

                 match state.ChartMode with
                 | AbsoluteInfections ->
                     sprintf
                         "<b>%s</b><br/>%s: %s<br/>%s: %A"
                         sex
                         (chartText "age")
                         ageGroup
                         (chartText "confirmedCases")
                         (I18N.NumberFormat.formatNumber (abs dataValue))
                 | InfectionsPerPopulation ->
                     sprintf
                         "<b>%s</b><br/>%s: %s<br/>%s: %s<br/>%s: %d"
                         sex
                         (chartText "age")
                         ageGroup
                         (chartText "shareOfInfectedPopulation")
                         (Utils.percentWith2DecimalFormatter dataValue)
                         (chartText "populationTotal")
                         (populationOf sex ageGroup)
                 | AbsoluteDeaths ->
                     sprintf
                         "<b>%s</b><br/>%s: %s<br/>%s: %A"
                         sex
                         (chartText "age")
                         ageGroup
                         (chartText "deceased")
                         (I18N.NumberFormat.formatNumber (abs dataValue))
                 | DeathsPerPopulation ->
                     sprintf
                         "<b>%s</b><br/>%s: %s<br/>%s: %s<br/>%s: %d"
                         sex
                         (chartText "age")
                         ageGroup
                         (chartText "shareOfDeceasedPopulation")
                         (Utils.percentWith2DecimalFormatter dataValue)
                         (chartText "populationTotal")
                         (populationOf sex ageGroup)
                 | DeathsPerInfections ->
                     sprintf
                         "<b>%s</b><br/>%s: %s<br/>%s: %s"
                         sex
                         (chartText "age")
                         ageGroup
                         (chartText "shareOfDeceasedConfirmedCases")
                         (Utils.percentWith2DecimalFormatter dataValue)
            |}
        series = [|
            {| name = chartText "male"
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
            {| name = chartText "female"
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
    { ChartMode = ChartMode.Default; Data = data }, Cmd.none

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
    let latest = latestAgeData state
    let latestDate = fst (latest)
    let infectionsAndDeathsPerAge = snd (latest)
    let chartData = calculateChartData infectionsAndDeathsPerAge state.ChartMode

    Html.div [
        prop.style [ style.height 400 ]
        prop.className "highcharts-wrapper"
        prop.children [ renderChartOptions state latestDate chartData |> chart ]
    ]

let render (state : State) dispatch =
    let activeScaleType = state.ChartMode.ScaleType
    Html.div [
        Utils.renderChartTopControlRight
            (renderScaleTypeSelectors activeScaleType (ScaleTypeChanged >> dispatch))
        renderChartContainer state
        renderChartCategorySelectors state.ChartMode dispatch

        match state.ChartMode with
        | AbsoluteDeaths | DeathsPerPopulation | DeathsPerInfections ->
            Html.div [
                prop.className "disclaimer"
                prop.children [
                    Html.text (chartText "disclaimer")
                ]
            ]
        | _ -> Html.none
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent("AgeGroupsChart", init props.data, update, render)
