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

type ChartType =
    | Absolute
    | Absolute100k
    | Relative100k
  with
    static member All =
        [ Absolute
          Absolute100k
          Relative100k ]

    static member Default = Absolute

    member this.GetName =
        match this with
        | Absolute    -> chartText "absolute"
        | Absolute100k -> chartText "absolute100k"
        | Relative100k -> chartText "relative100k"

type DataPoint =
    { Date: DateTime
      ProtectedWithVaccineToDate: int option
      ConfirmedProtectedWithVaccine: int option
      ConfirmedOther: int option }

type State =
    { DisplayType: DisplayType
      ChartType: ChartType
      Data: StatsData
      WeeklyData : WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg =
    | DisplayTypeChanged of DisplayType
    | ChartTypeChanged of ChartType
    | RangeSelectionChanged of int


let init data weeklyData : State * Cmd<Msg> =
    let state =
        { DisplayType = DisplayType.Default
          ChartType = ChartType.Default
          Data = data
          WeeklyData = weeklyData
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none
    | ChartTypeChanged chartType ->
        { state with ChartType = chartType }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none


let renderChartOptions state dispatch =

    let intOptionToFloat opt =
        match opt with
        | Some i -> Some ((float) i)
        | None -> None

    let addFloatOption a b =
        match a,b with
        | Some a, Some b -> Some (a+b)
        | Some a, None -> Some (a)
        | None, Some b -> Some (b)
        | None, None -> None

    let get100k value population =
        match value with
        | Some v -> Some (v * 100000. / (float) (population |> Utils.optionToInt))
        | None -> None

    let checkAndProcess100k dp =
        match state.ChartType with
        | Absolute -> dp
        | Absolute100k | Relative100k ->
            {| Date = dp.Date
               DateTo = dp.DateTo
               ProtectedWithVaccineToDate = dp.ProtectedWithVaccineToDate
               CasesProtectedWithVaccine =
                    get100k
                        dp.CasesProtectedWithVaccine
                        dp.ProtectedWithVaccineToDate
               CasesOther =
                    get100k
                        dp.CasesOther
                        (Utils.Dictionaries.regions.["si"].Population
                         |> Utils.subtractIntOption dp.ProtectedWithVaccineToDate) |}

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

    let dailyConfirmedData =
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
                   DateTo = dp.Date
                   ProtectedWithVaccineToDate = protectedWithVaccine
                   CasesProtectedWithVaccine = confirmedProtectedWithVaccine |> intOptionToFloat
                   CasesOther = confirmedOther |> intOptionToFloat |})

    let weeklyHospitalizedData =
        state.WeeklyData
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = protectedWithVaccineOnDay(dp.DateTo)

                {| Date = dp.Date
                   DateTo = dp.DateTo
                   ProtectedWithVaccineToDate = protectedWithVaccine
                   CasesProtectedWithVaccine = dp.HospitalizedVaccinated |> intOptionToFloat
                   CasesOther = dp.HospitalizedOther |> intOptionToFloat |})

    let allSeries =
        let color, data =
            match state.DisplayType with
            | ConfirmedCases ->
                let daily =
                    dailyConfirmedData
                    |> Seq.filter (fun dp -> dp.CasesProtectedWithVaccine.IsSome)
                    |> Seq.map checkAndProcess100k
                let emptyRec =
                    {| Date = DateTime.MaxValue
                       DateTo = DateTime.MinValue
                       ProtectedWithVaccineToDate = None
                       CasesProtectedWithVaccine = None
                       CasesOther = None |}
                let mutable sumRec = emptyRec
                let weeklyConfirmedData =
                    seq {
                        for dp in daily do
                            sumRec <-
                                {| Date = if dp.Date.CompareTo(sumRec.Date) < 0 then dp.Date else sumRec.Date
                                   DateTo = if dp.DateTo.CompareTo(sumRec.Date) > 0 then dp.DateTo else sumRec.Date
                                   ProtectedWithVaccineToDate = if dp.ProtectedWithVaccineToDate > sumRec.ProtectedWithVaccineToDate then dp.ProtectedWithVaccineToDate else sumRec.ProtectedWithVaccineToDate
                                   CasesProtectedWithVaccine = sumRec.CasesProtectedWithVaccine |> addFloatOption dp.CasesProtectedWithVaccine
                                   CasesOther = sumRec.CasesOther |> addFloatOption dp.CasesOther |}
                            if dp.Date.DayOfWeek = DayOfWeek.Sunday then
                                yield sumRec
                                sumRec <- emptyRec
                        if sumRec.Date <> DateTime.MaxValue then
                            yield sumRec // flush
                    }
                "#d5c768", weeklyConfirmedData
            | HospitalizedCases ->
                "#de9a5a",
                weeklyHospitalizedData
                |> Seq.filter (fun dp -> dp.CasesProtectedWithVaccine.IsSome)
                |> Seq.map checkAndProcess100k
        [ yield
            pojo
            {| name = chartText "casesOther"
               ``type`` = "column"
               color = color
               yAxis = 0
               data =
                   data
                   |> Seq.map (fun dp -> (dp.DateTo |> jsTime12h, dp.CasesOther))
                   |> Seq.toArray |}
          yield
            pojo
            {| name = chartText "casesProtected"
               ``type`` = "column"
               color = "#0e5842"
               yAxis = 0
               data =
                   data
                   |> Seq.map (fun dp -> (dp.DateTo |> jsTime12h, dp.CasesProtectedWithVaccine))
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
                                | Absolute | Absolute100k -> "normal"
                                | Relative100k -> "percent"
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
                               |> chartFromWindow ] ]

let renderChartTypeSelector (activeChartType: ChartType) dispatch =
    let renderSelector (chartType : ChartType) =
        let active = chartType = activeChartType
        Html.div [
            prop.text chartType.GetName
            prop.onClick (fun _ -> dispatch chartType)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        ChartType.All
        |> List.map renderSelector
        |> prop.children
    ]

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
            renderChartTypeSelector
                state.ChartType (ChartTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch ]

let vaccineEffectChart (props: {| data: StatsData ; weeklyData: WeeklyStatsData |}) =
    React.elmishComponent ("VaccineEffectChart", init props.data props.weeklyData, update, render)
