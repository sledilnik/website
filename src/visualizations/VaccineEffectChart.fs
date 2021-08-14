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

type DataPoint =
    { Date: DateTime
      ProtectedWithVaccineToDate: int option
      ConfirmedProtectedWithVaccine: int option
      ConfirmedOther: int option }

type State =
    { Data: StatsData
      ChartType: BarChartType
      RangeSelectionButtonIndex: int }

type Msg =
    | BarChartTypeChanged of BarChartType
    | RangeSelectionChanged of int


let init data : State * Cmd<Msg> =
    let state =
        { Data = data
          ChartType = AbsoluteChart
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | BarChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none


let renderChartOptions state dispatch =

// - ProtectedWithVaccineToDate:
//   - Administered2nd + 14 dni (zamik)
// - pairwise to Today:
//   - ProtectedWithVaccine: VaccinatedConfirmedToDate
//   - Others: ConfirmedToDate - VaccinatedConfirmedToDate
// - calcRunningAverage
// - per 100k:
//    - ProtectedWithVaccine * 100k / ProtectedWithVaccineToDate
//    - Others * 100k / (Population - ProtectedWithVaccineToDate)

    let sloPopulation =
        Utils.Dictionaries.regions.["si"].Population
        |> Utils.optionToInt
        |> float

    let dailyData =
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
                let get100k value population =
                    match value with
                    | Some v -> Some ((float)v * 100000. / (float) (population |> Utils.optionToInt))
                    | None -> None

                {| Date = dp.Date
                   ProtectedWithVaccineToDate = protectedWithVaccine
                   ConfirmedProtectedWithVaccine = get100k confirmedProtectedWithVaccine protectedWithVaccine
                   ConfirmedOther = get100k confirmedOther (Utils.Dictionaries.regions.["si"].Population |>Utils.subtractIntOption protectedWithVaccine) |})

    let allSeries =
        [ yield
              pojo
                  {| name = chartText "confirmedOther"
                     ``type`` = "column"
                     color = "#d5c768"
                     yAxis = 0
                     data =
                         dailyData
                         |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.ConfirmedOther))
                         |> Seq.toArray |}
          yield
            pojo
                {| name = chartText "confirmedProtected"
                   ``type`` = "column"
                   color = "#0e5842"
                   yAxis = 0
                   data =
                       dailyData
                       |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.ConfirmedProtectedWithVaccine))
                       |> Seq.toArray |}
          yield
              pojo
                  {| name = chartText "protectedWithVaccine"
                     ``type`` = "line"
                     color = "#0e5842"
                     yAxis = 1
                     data =
                         dailyData
                         |> Seq.map (fun dp -> (dp.Date |> jsTime12h, Math.Round((dp.ProtectedWithVaccineToDate |> Utils.optionToInt |> float) * 100. / sloPopulation, 1)))
                         |> Seq.toArray |}

          ]

    let allYAxis =
        [| {| index = 0
              title = {| text = null |}
              labels =
                  pojo
                      {| format = "{value}"
                         align = "center"
                         x = -15
                         reserveSpace = false |}
              max = None
              showFirstLabel = false
              opposite = true
              visible = true
              crosshair = true |}
           |> pojo
           {| index = 1
              title = {| text = null |}
              labels =
                  pojo
                      {| format = "{value}%"
                         align = "center"
                         x = 10
                         reserveSpace = false |}
              max = 100
              showFirstLabel = false
              opposite = false
              visible = true
              crosshair = true |}
           |> pojo |]

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccine-effect" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           yAxis = allYAxis
           series = List.toArray allSeries
           plotOptions =
               pojo
                   {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                      series =
                          {| crisp = false
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

let render state dispatch =
    Html.div [ Utils.renderChartTopControls [ Utils.renderBarChartTypeSelector
                                                  state.ChartType
                                                  (BarChartTypeChanged >> dispatch) ]
               renderChartContainer state dispatch ]

let vaccineEffectChart (props: {| data: StatsData |}) =
    React.elmishComponent ("VaccineEffectChart", init props.data, update, render)
