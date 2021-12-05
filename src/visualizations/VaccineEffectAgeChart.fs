[<RequireQualifiedAccess>]
module VaccineEffectAgeChart

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

    static member Default = Absolute

    member this.GetName =
        match this with
        | Absolute -> chartText "absolute"
        | Absolute100k -> chartText "absolute100k"


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

    let getSummaryData (dp: WeeklyEpisariDataPoint) =

        let vaccinatedIn = dp.CovidInVaccinated |> Option.defaultValue 0
        let unvaccinatedIn = (dp.CovidIn |> Option.defaultValue 0) - vaccinatedIn
        let vaccinatedInAbove65 =
            dp.PerAge
            |> List.sumBy
                (fun ag ->
                    match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
                    | Some f, None -> if f = 65 then (ag.VaccinatedIn |> Option.defaultValue 0) else 0
                    | _, _ -> 0)
        let unvaccinatedInAbove65 =
            (dp.PerAge
             |> List.sumBy
                (fun ag ->
                    match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
                    | Some f, Some t -> if f >= 65 then (ag.CovidIn |> Option.defaultValue 0) else 0
                    | _, _ -> 0)) - vaccinatedInAbove65

        // TODO: protectedWithVaccine per day -> 100k weighted groups
        unvaccinatedIn - unvaccinatedInAbove65, vaccinatedIn - vaccinatedInAbove65, // below 65
        unvaccinatedInAbove65, vaccinatedInAbove65, // above 65
        unvaccinatedIn, vaccinatedIn  // all ages


    let summaryData =
        state.Data
        |> Array.skipWhile (fun dp -> dp.CovidInVaccinated.IsNone)
        |> Array.map (fun dp -> getSummaryData dp)

    let allSeries =
        [
          yield
                pojo
                    {| name = chartText "vaccinatedIn"
                       ``type`` = "column"
                       color = "#0e5842"
                       data = [|
                          summaryData |> Array.sumBy (fun (a,b,c,d,e,f) -> b)
                          summaryData |> Array.sumBy (fun (a,b,c,d,e,f) -> d)
                          summaryData |> Array.sumBy (fun (a,b,c,d,e,f) -> f)
                       |] |}
          yield
                pojo
                    {| name = chartText "unvaccinatedIn"
                       ``type`` = "column"
                       color = "#de9a5a"
                       data = [|
                          summaryData |> Array.sumBy (fun (a,b,c,d,e,f) -> a)
                          summaryData |> Array.sumBy (fun (a,b,c,d,e,f) -> c)
                          summaryData |> Array.sumBy (fun (a,b,c,d,e,f) -> e)
                       |] |}
        ]

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
    Html.div [ Html.div [ prop.style [ style.height 480 ]
                          prop.className "highcharts-wrapper"
                          prop.children [ renderChartOptions state dispatch |> chart ] ]
               Html.div [ prop.className "disclaimer"
                          prop.children [ Utils.Markdown.render (chartText "disclaimer") ] ] ]

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