[<RequireQualifiedAccess>]
module RatiosChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

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
        | Cases     -> "Resni primeri"
        | Hospital  -> "Hospitalizirani"
        | Mortality -> "Smrtnost"
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

    // color, dash, name
    let getSeriesInfo = function
        | HospitalCases                 -> "#de9a5a", Solid,    "Hospitalizirirani"
        | IcuCases                      -> "#d99a91", Solid,    "V intenzivni enoti"
        | CriticalCases                 -> "#bf5747", Solid,    "Na respiratorju"
        | DeceasedCases                 -> "#666666", Dot,      "Umrli"
        | IcuHospital                   -> "#d99a91", Solid,    "V intenzivni enoti"
        | CriticalHospital              -> "#bf5747", Solid,    "Na repiratorju"
        | DeceasedHospital              -> "#666666", Dot,      "Umrli"
        | DeceasedHospitalC             -> "#de9a5a", Dot,      "Smrtnost v bolnišnici"
        | DeceasedIcuC                  -> "#d99a91", Dot,      "Smrtnost v intenzivni enoti"
        | DeceasedIcuDeceasedTotal      -> "#d99a91", Solid,    "Delež smrti v intenzivni enoti"
        | DeceasedHospitalDeceasedTotal -> "#de9a5a", Solid,    "Delež smrti v bolnišnici"


type State = {
    casesMap: Map<DateTime, int option>
    patientsData : PatientsStats []
    error: string option
    displayType: DisplayType
}

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ChangeDisplayType of DisplayType

let init (data : StatsData) : State * Cmd<Msg> =
    let state = {
        casesMap = data |> Seq.map (fun dp -> dp.Date, dp.Cases.ConfirmedToDate) |> Map.ofSeq
        patientsData = [||]
        error = None
        displayType = Cases
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


let renderRatiosChart (state : State) =

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

        let color, line, name = Ratios.getSeriesInfo ratio

        {|
            visible = true
            color = color
            dashStyle = line |> DashStyle.toString
            name = name
            data =
                state.patientsData
                |> Seq.skipWhile (fun dp -> dp.Date < startDate || dp.total.inHospital.toDate.IsNone)
                |> Seq.map renderRatioPoint
                |> Array.ofSeq
        |}
        |> pojo

        
    let maxValue = if state.displayType = Mortality then Some 100 else None
    let className = DisplayType.getClassName state.displayType
    let baseOptions = Highcharts.basicChartOptions ScaleType.Linear className
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = "spline"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        plotOptions = pojo
            {|
                spline = pojo {| dataLabels = pojo {| enabled = false |}; marker = pojo {| enabled = false |} |}
            |}
        yAxis = baseOptions.yAxis 
            |> Array.map (fun ax -> {| ax with max = maxValue ; labels = pojo {| format = "{value}%" |} |} )

        series = [| 
            for ratio in Ratios.getSeries(state.displayType) do 
            yield renderRatiosH ratio 
        |]

        tooltip = pojo {| shared = true; valueSuffix = " %" ; xDateFormat = @"%A, %e. %B %Y" |}

        legend = pojo {| enabled = true ; layout = "horizontal" |}
|}

let renderChartContainer state =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [ renderRatiosChart state  |> Highcharts.chartFromWindow ]
    ]

let renderDisplaySelector state dt dispatch =
    Html.div [
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        prop.className [ true, "btn btn-sm metric-selector"; state.displayType = dt, "metric-selector--selected" ]
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
            renderChartContainer state
            renderDisplaySelectors state dispatch
        ]

let ratiosChart (props : {| data : StatsData |}) =
    React.elmishComponent("RatiosChart", init props.data, update, render)
