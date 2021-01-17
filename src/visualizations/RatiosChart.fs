[<RequireQualifiedAccess>]
module RatiosChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

open Data.Patients

type DisplayType =
    | Cases
    | Hospital
    | Mortality
  with
    static member all = [ Cases; Hospital; Mortality ]
    static member getName = function
        | Cases     -> I18N.t "charts.ratios.seriousCases"
        | Hospital  -> I18N.t "charts.ratios.hospitalizations"
        | Mortality -> I18N.t "charts.ratios.mortality"
    static member getClassName = function
        | Cases     -> "covid19-ratios-cases"
        | Hospital  -> "covid19-ratios-hospital"
        | Mortality -> "covid19-ratios-mortality"

type Ratios =
    | HospitalCases
    | IcuCases
    | CriticalCases
    | DeceasedCases
    | IcuHospital
    | CriticalHospital
    | DeceasedHospital
    | DeceasedHospitalC
    | DeceasedIcuC
    | DeceasedIcuDeceasedTotal
    | DeceasedHospitalDeceasedTotal

module Ratios =
    let getSeries = function
        | Cases     -> [ HospitalCases; IcuCases; CriticalCases; DeceasedCases ]
        | Hospital  -> [ IcuHospital; CriticalHospital; DeceasedHospital]
        | Mortality -> [ DeceasedHospitalDeceasedTotal; DeceasedIcuDeceasedTotal; DeceasedIcuC; DeceasedHospitalC; ]

    // color, dash, id
    let getSeriesInfo = function
        | HospitalCases                 -> "#de9a5a", Solid,    "hospitalCases"
        | IcuCases                      -> "#d96756", Solid,    "icuCases"
        | CriticalCases                 -> "#bf5747", Solid,    "ventilatorCases"
        | DeceasedCases                 -> "#666666", Dot,      "deceasedCases"
        | IcuHospital                   -> "#d96756", Solid,    "icuHospital"
        | CriticalHospital              -> "#bf5747", Solid,    "ventilatorHospital"
        | DeceasedHospital              -> "#666666", Dot,      "deceasedHospital"
        | DeceasedHospitalC             -> "#de9a5a", Dot,      "hospitalMortality"
        | DeceasedIcuC                  -> "#d96756", Dot,      "icuMortality"
        | DeceasedIcuDeceasedTotal      -> "#d96756", Solid,    "icuDeceasedShare"
        | DeceasedHospitalDeceasedTotal -> "#de9a5a", Solid,    "hospitalDeceasedShare"

type State = {
    casesMap: Map<DateTime, int option>
    patientsData : PatientsStats []
    error: string option
    displayType: DisplayType
    RangeSelectionButtonIndex: int
}

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

let init (data : StatsData) : State * Cmd<Msg> =
    let state = {
        casesMap = data |> Seq.map (fun dp -> dp.Date, dp.Cases.ConfirmedToDate) |> Map.ofSeq
        patientsData = [||]
        error = None
        displayType = Cases
        RangeSelectionButtonIndex = 0
    }
    let cmd = Cmd.OfAsync.either Data.Patients.getOrFetch () ConsumePatientsData ConsumeServerError
    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with patientsData = data }, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with error = Some ex.Message }, Cmd.none
    | ChangeDisplayType dt ->
        { state with displayType = dt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none


let renderRatiosChart (state : State) dispatch =

    let startDate = DateTime(2020,03,10)

    let renderRatiosH ratio =
        let percent (a : int option) (b : int option) =
            match a with
            | None | Some 0 -> None
            | _ ->  Some (float a.Value * 100. / float b.Value |> Utils.roundTo1Decimal)

        let cases (date : DateTime) =
            match state.casesMap.TryFind date with
            | None -> None
            | Some i -> i

        let renderRatioPoint : (Data.Patients.PatientsStats -> JsTimestamp * float option) =
            match ratio with
            | HospitalCases                 -> fun ps -> ps.JsDate12h, percent ps.total.inHospital.toDate (cases ps.Date)
            | IcuCases                      -> fun ps -> ps.JsDate12h, percent ps.total.icu.toDate (cases ps.Date)
            | CriticalCases                 -> fun ps -> ps.JsDate12h, percent ps.total.critical.toDate (cases ps.Date)
            | DeceasedCases                 -> fun ps -> ps.JsDate12h, percent ps.total.deceased.toDate (cases ps.Date)
            | IcuHospital                   -> fun ps -> ps.JsDate12h, percent ps.total.icu.toDate ps.total.inHospital.toDate
            | CriticalHospital              -> fun ps -> ps.JsDate12h, percent ps.total.critical.toDate ps.total.inHospital.toDate
            | DeceasedHospital              -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.toDate ps.total.inHospital.toDate
            | DeceasedHospitalC             -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.toDate ps.total.inHospital.toDate
            | DeceasedIcuC                  -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.icu.toDate ps.total.icu.toDate
            | DeceasedIcuDeceasedTotal      -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.icu.toDate ps.total.deceased.toDate
            | DeceasedHospitalDeceasedTotal -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.toDate ps.total.deceased.toDate

        let color, line, id = Ratios.getSeriesInfo ratio
        {|
            visible = true
            color = color
            dashStyle = line |> DashStyle.toString
            name = I18N.tt "charts.ratios" id
            data =
                state.patientsData
                |> Seq.skipWhile (fun dp -> dp.Date < startDate || dp.total.inHospital.toDate.IsNone)
                |> Seq.map renderRatioPoint
                |> Array.ofSeq
        |}
        |> pojo

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let className = DisplayType.getClassName state.displayType
    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` = "line"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        plotOptions = pojo
            {|
                line = pojo {| dataLabels = pojo {| enabled = false |}; marker = pojo {| enabled = false |} |}
            |}

        series = [|
            for ratio in Ratios.getSeries(state.displayType) do
            yield renderRatiosH ratio
        |]

        tooltip = pojo {| shared = true; split = false ; valueSuffix = " %" ; xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}

        legend = pojo {| enabled = true ; layout = "horizontal" |}
|}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderRatiosChart state dispatch |> Highcharts.chartFromWindow
        ]
    ]

let renderDisplaySelector state dt dispatch =
    Html.div [
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (state.displayType = dt, "metric-selector--selected") ]
        prop.text (dt |> DisplayType.getName)
    ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.all
            |> List.map (fun dt -> renderDisplaySelector state dt dispatch) ) ]

let render (state : State) dispatch =
    match state.patientsData, state.error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
            renderDisplaySelectors state dispatch
        ]

let ratiosChart (props : {| data : StatsData |}) =
    React.elmishComponent("RatiosChart", init props.data, update, render)
