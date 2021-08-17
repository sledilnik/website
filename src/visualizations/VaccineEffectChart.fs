[<RequireQualifiedAccess>]
module VaccineEffectChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

let chartText = I18N.chartText "vaccineEffect"

type DisplayType =
    | ConfirmedCases
    | HospitalizedCases
  with
    static member All =
        [ ConfirmedCases
          HospitalizedCases ]

    static member Default = ConfirmedCases

    member this.GetName =
        match this with
        | ConfirmedCases    -> chartText "confirmedCases"
        | HospitalizedCases -> chartText "hospitalizedCases"

type DataPoint =
    { Date: DateTime
      ProtectedWithVaccineToDate: int option
      ConfirmedProtectedWithVaccine: int option
      ConfirmedOther: int option }

type State =
    { DisplayType: DisplayType
      ChartType: BarChartType
      Data: StatsData
      WeeklyData : WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg =
    | DisplayTypeChanged of DisplayType
    | BarChartTypeChanged of BarChartType
    | RangeSelectionChanged of int


let init data weeklyData : State * Cmd<Msg> =
    let state =
        { DisplayType = DisplayType.Default
          ChartType = AbsoluteChart
          Data = data
          WeeklyData = weeklyData
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none
    | BarChartTypeChanged chartType ->
        { state with ChartType = chartType }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none


let renderChartOptions state dispatch =

    let sloPopulation =
        Utils.Dictionaries.regions.["si"].Population
        |> Utils.optionToInt
        |> float

    let get100k value population =
        match value with
        | Some v -> Some ((float)v * 100000. / (float) (population |> Utils.optionToInt))
        | None -> None

    let protectedWithVaccineMap =
        state.Data
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = // protected 14 days after 2nd dose
                    if i >= 14 then
                        state.Data.[i - 14]
                            .Vaccination
                            .Administered2nd
                            .ToDate
                    else
                        None
                dp.Date, protectedWithVaccine)
        |> Map.ofSeq

    let protectedWithVaccineOnDay date =
        match protectedWithVaccineMap.TryFind(date) with
        | Some v -> v
        | None -> None

    let dailyData =
        state.Data
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = protectedWithVaccineOnDay(dp.Date)

                let confirmedProtectedWithVaccine =
                    match dp.Cases.VaccinatedConfirmedToDate with
                    | Some v ->
                        if i >= 1 then
                            dp.Cases.VaccinatedConfirmedToDate
                            |> Utils.subtractIntOption state.Data.[i - 1].Cases.VaccinatedConfirmedToDate
                        else
                            dp.Cases.VaccinatedConfirmedToDate
                    | _ -> None
                let confirmedOther =
                    match dp.Cases.ConfirmedToDate with
                    | Some v ->
                        if i >= 1 then
                            dp.Cases.ConfirmedToDate
                            |> Utils.subtractIntOption state.Data.[i - 1].Cases.ConfirmedToDate
                            |> Utils.subtractIntOption confirmedProtectedWithVaccine
                        else
                            dp.Cases.ConfirmedToDate
                            |> Utils.subtractIntOption confirmedProtectedWithVaccine
                    | _ -> None

                {| Date = dp.Date
                   ProtectedWithVaccineToDate = protectedWithVaccine
                   CasesProtectedWithVaccine = get100k confirmedProtectedWithVaccine protectedWithVaccine
                   CasesOther = get100k confirmedOther (Utils.Dictionaries.regions.["si"].Population |>Utils.subtractIntOption protectedWithVaccine) |})

    let weeklyData =
        state.WeeklyData
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = protectedWithVaccineOnDay(dp.DateTo)

                {| Date = dp.Date
                   ProtectedWithVaccineToDate = protectedWithVaccine
                   CasesProtectedWithVaccine = get100k dp.HospitalizedVaccinated protectedWithVaccine
                   CasesOther = get100k dp.HospitalizedOther (Utils.Dictionaries.regions.["si"].Population |>Utils.subtractIntOption protectedWithVaccine) |})

    let allSeries =
        let data, color =
            match state.DisplayType with
            | ConfirmedCases -> dailyData |> Seq.filter (fun dp -> dp.CasesOther.IsSome), "#d5c768"
            | HospitalizedCases -> weeklyData |> Seq.filter (fun dp -> dp.CasesOther.IsSome), "#de9a5a"
        [ yield
            pojo
            {| name = chartText "casesProtected"
               ``type`` = "column"
               color = "#0e5842"
               yAxis = 0
               data =
                   data
                   |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.CasesProtectedWithVaccine))
                   |> Seq.toArray |}
          yield
            pojo
            {| name = chartText "casesOther"
               ``type`` = "column"
               color = color
               yAxis = 0
               data =
                   data
                   |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.CasesOther))
                   |> Seq.toArray |}
        ]


    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccine-effect" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           series = List.toArray allSeries
           plotOptions =
               pojo
                   {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                      series =
                          {| stacking =
                                match state.ChartType with
                                | AbsoluteChart -> "normal"
                                | RelativeChart -> "percent"
                             crisp = false
                             borderWidth = 0
                             pointPadding = 0
                             groupPadding = 0 |} |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           tooltip =
               pojo
                   {| shared = true
                      split = false
                      formatter = None
                      valueSuffix = ""
                      xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}
           responsive =
               pojo
                   {| rules =
                          [| {| condition = {| maxWidth = 768 |}
                                chartOptions =
                                    {| yAxis =
                                           [| {| labels = {| enabled = false |} |}
                                              {| labels = {| enabled = false |} |} |] |} |} |] |} |}



let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderChartOptions state dispatch
                               //renderBarChart state dispatch
                               |> chartFromWindow ] ]

let renderDisplaySelectors (activeDisplayType: DisplayType) dispatch =
    let renderDisplayTypeSelector (displayTypeToRender: DisplayType) =
        let active = displayTypeToRender = activeDisplayType
        Html.div [
            prop.onClick (fun _ -> dispatch displayTypeToRender)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text displayTypeToRender.GetName
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        DisplayType.All
        |> List.map renderDisplayTypeSelector
        |> prop.children
    ]

let render state dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderDisplaySelectors
                state.DisplayType (DisplayTypeChanged >> dispatch)
            Utils.renderBarChartTypeSelector
                state.ChartType (BarChartTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch ]

let vaccineEffectChart (props: {| data: StatsData ; weeklyData: WeeklyStatsData |}) =
    React.elmishComponent ("VaccineEffectChart", init props.data props.weeklyData, update, render)
