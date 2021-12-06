[<RequireQualifiedAccess>]
module VaccineEffectAgeChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

open Data.Vaccinations

let chartText = I18N.chartText "vaccineEffectAge"

type ChartType =
    | Absolute
    | Absolute100k
    static member All = [ Absolute; Absolute100k ]

    static member Default = Absolute100k

    member this.GetName =
        match this with
        | Absolute -> chartText "absolute"
        | Absolute100k -> chartText "absolute100k"

type HospitalizedData =
    {
        Date: DateTime
        DateTo: DateTime
        VaccinatedIn: float array
        OtherIn: float array
        VaccinatedIn100k: float array
        OtherIn100k: float array
    }
    static member get_Zero () =
        {
            Date = DateTime.MaxValue
            DateTo = DateTime.MinValue
            VaccinatedIn = [| 0.; 0.; 0. |]
            OtherIn = [| 0.; 0.; 0. |]
            VaccinatedIn100k = [| 0.; 0.; 0. |]
            OtherIn100k = [| 0.; 0.; 0. |]
        }
    static member (+) (h1 : HospitalizedData, h2 : HospitalizedData) =
        {
            Date = if DateTime.Compare(h1.Date, h2.Date) < 1 then h1.Date else h2.Date
            DateTo = if DateTime.Compare(h1.DateTo, h2.DateTo) < 1 then h2.DateTo else h1.DateTo
            VaccinatedIn = h1.VaccinatedIn |> Array.mapi (fun i v -> v + h2.VaccinatedIn.[i])
            OtherIn = h1.OtherIn |> Array.mapi (fun i v -> v + h2.OtherIn.[i])
            VaccinatedIn100k = h1.VaccinatedIn100k |> Array.mapi (fun i v -> v + h2.VaccinatedIn100k.[i])
            OtherIn100k = h1.OtherIn100k |> Array.mapi (fun i v -> v + h2.OtherIn100k.[i])
        }

type State =
    { Data: WeeklyEpisariData
      VaccinationData: VaccinationStats array
      Error: string option
      ChartType: ChartType }

type Msg =
    | ConsumeVaccinationData of Result<VaccinationStats array, string>
    | ConsumeServerError of exn
    | ChartTypeChanged of ChartType


let init data : State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeVaccinationData ConsumeServerError
    let state =
        { Data = data
          VaccinationData = [||]
          Error = None
          ChartType = ChartType.Default }
    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeVaccinationData (Ok data) -> { state with VaccinationData = data }, Cmd.none
    | ConsumeVaccinationData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | ChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none


let renderChartOptions (state: State) dispatch =

    let protectedWithVaccineMap =
        state.VaccinationData
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = // protected 14 days after 2nd dose
                    if i >= 15 then
                        state.VaccinationData.[i - 15].administered2nd.toDate |> Option.defaultValue 0
                    else
                        0

                dp.Date, protectedWithVaccine)
        |> Map.ofSeq


    let protectedWithVaccineAbove65Map =
        state.VaccinationData
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = // protected 14 days after 2nd dose
                    if i >= 15 then
                        state.VaccinationData.[i - 15].administeredPerAge
                        |> List.sumBy
                            (fun ag ->
                                match ag.ageFrom, ag.ageTo with
                                | Some f, _ -> if f >= 65 then (ag.administered2nd |> Option.defaultValue 0) else 0
                                | _, _ -> 0)
                    else
                        0

                dp.Date, protectedWithVaccine)
        |> Map.ofSeq

    let averageOnWeek (dataMap : Map<DateTime, int>) (date: DateTime) (dateTo: DateTime)=
        match dataMap.TryFind(date), dataMap.TryFind(date) with
        | Some v1, Some v2 ->
            Math.Round((((v1 + v2) |> float) / 2.), 0) |> int
        | Some v1, None -> v1
        | None, Some v2 -> v2
        | None, None -> 0

    let get100k value population =
        value * 100000. / (float)population

    let getSummaryData (dp: WeeklyEpisariDataPoint) : HospitalizedData =

        let vaccinatedIn = dp.CovidInVaccinated |> Option.defaultValue 0 |> float
        let otherIn = ((dp.CovidIn |> Option.defaultValue 0) |> float) - vaccinatedIn
        let protectedWithVaccine = averageOnWeek protectedWithVaccineMap dp.Date dp.DateTo
        let otherPopulation =
            (Utils.Dictionaries.regions.["si"].Population |> Option.defaultValue 0)
            - protectedWithVaccine

        let vaccinatedInAbove65 =
            (dp.PerAge
             |> List.sumBy
                (fun ag ->
                    match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
                    | Some f, None -> if f = 65 then (ag.VaccinatedIn |> Option.defaultValue 0) else 0
                    | _, _ -> 0)) |> float
        let otherInAbove65 =
            ((dp.PerAge
              |> List.sumBy
                (fun ag ->
                    match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
                    | Some f, _ -> if f >= 65 then (ag.CovidIn |> Option.defaultValue 0) else 0
                    | _, _ -> 0)) |> float) - vaccinatedInAbove65
        let protectedWithVaccineAbove65 = averageOnWeek protectedWithVaccineAbove65Map dp.Date dp.DateTo
        let otherAbove65Population =
            (Utils.AgePopulationStats.agePopulationStats
             |> Map.toList
             |> List.sumBy
                (fun (key, ag) ->
                    match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
                    | Some f, _ -> if f >= 65 then ag.Population else 0
                    | _, _ -> 0)) - protectedWithVaccineAbove65

        {
            Date = dp.Date
            DateTo = dp.DateTo
            VaccinatedIn =
                [| vaccinatedIn - vaccinatedInAbove65
                   vaccinatedInAbove65
                   vaccinatedIn |]
            OtherIn =
                [| otherIn - otherInAbove65
                   otherInAbove65
                   otherIn |]
            VaccinatedIn100k =
                [| get100k (vaccinatedIn - vaccinatedInAbove65) (protectedWithVaccine - otherAbove65Population)
                   get100k vaccinatedInAbove65 otherAbove65Population
                   get100k vaccinatedIn protectedWithVaccine |]
            OtherIn100k =
                [| get100k (otherIn - otherInAbove65) (otherPopulation - otherAbove65Population)
                   get100k otherInAbove65 otherAbove65Population
                   get100k otherIn otherPopulation |]
        }

    let summaryData =
        state.Data
        |> Array.skipWhile (fun dp -> dp.CovidInVaccinated.IsNone)
        |> Array.map (fun dp -> getSummaryData dp)
        |> Array.sum

    let allSeries =
        [
          yield
                pojo
                    {| name = chartText "vaccinatedIn"
                       ``type`` = "column"
                       color = "#0e5842"
                       data =
                            match state.ChartType with
                            | Absolute -> summaryData.VaccinatedIn
                            | Absolute100k -> summaryData.VaccinatedIn100k
                            |> Array.map
                                (fun dp -> {| y = dp |}) |}
          yield
                pojo
                    {| name = chartText "otherIn"
                       ``type`` = "column"
                       color = "#de9a5a"
                       data =
                            match state.ChartType with
                            | Absolute -> summaryData.OtherIn
                            | Absolute100k -> summaryData.OtherIn100k
                            |> Array.map
                                (fun dp -> {| y = dp |}) |}
        ]

    let label = I18N.tOptions
                    "charts.vaccineEffectAge.hospitalizedIn"
                    {| startDate = summaryData.Date
                       endDate = summaryData.DateTo |}

    label,
    {| optionsWithOnLoadEvent "covid19-vaccine-effect" with
           chart = pojo {| ``type`` = "column" |}
           title = pojo {| text = None |}
           xAxis =
               [| {| ``type`` = "category"
                     categories =
                         [| chartText "ageBelow65"
                            chartText "ageAbove65"
                            chartText "ageAll"|] |} |]
           yAxis =
               [| {| opposite = true
                     title = {| text = null |} |} |]
           series = List.toArray allSeries
           plotOptions = pojo {| series = {| groupPadding = 0.05 |} |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           tooltip =
               pojo
                   {| // formatter = fun () -> tooltipFormatter state jsThis
                      headerFormat = "<b>{point.x}</b><br>"
                      valueDecimals =
                          match state.ChartType with
                          | Absolute -> 0
                          | Absolute100k -> 1
                      shared = true
                      split = false
                      useHTML = true |}
           credits = chartCreditsNIJZ
           responsive =
               pojo
                   {| rules =
                          [| {| condition = {| maxWidth = 768 |}
                                chartOptions = {| yAxis = [| {| labels = pojo {| enabled = false |} |} |] |} |} |] |} |> pojo
           navigator = pojo {| enabled = false |}
           scrollbar = pojo {| enabled = false |}
           rangeSelector = pojo {| enabled = false |} |}


let renderChartContainer state dispatch =
    let label, chart = renderChartOptions state dispatch

    Html.div [ Html.div [ prop.style [ style.height 480 ]
                          prop.className "highcharts-wrapper"
                          prop.children [ chart |> Highcharts.chart ] ]
               Html.div [ prop.className "disclaimer"
                          prop.children [ Utils.Markdown.render (label + chartText "disclaimer") ] ] ]

let renderChartTypeSelector (activeChartType: ChartType) dispatch =
    let renderSelector (chartType: ChartType) =
        let active = chartType = activeChartType

        Html.div [ prop.text chartType.GetName
                   prop.onClick (fun _ -> dispatch chartType)
                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (active, "selected") ] ]

    Html.div [ prop.className "chart-display-property-selector"
               ChartType.All
               |> List.map renderSelector
               |> prop.children ]

let render state dispatch =
    match state.VaccinationData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [ Utils.renderChartTopControls [ renderChartTypeSelector state.ChartType (ChartTypeChanged >> dispatch) ]
                   renderChartContainer state dispatch ]

let vaccineEffectAgeChart
    (props: {| data: WeeklyEpisariData |})
    =
    React.elmishComponent ("VaccineEffectAgeChart", init props.data, update, render)