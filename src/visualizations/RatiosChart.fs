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
    | Mortality
  with
    static member all = [ Cases; Mortality ]
    static member getName = function
        | Cases     -> "Resni primeri"
        | Mortality -> "Smrtnost"

type Ratios =
    | IcuHospital
    | CriticalHospital
    | DeceasedIcu
    | DeceasedHospital
    | DeceasedIcuDeceasedHospital
    | DeceasedHospitalDeceasedTotal

module Ratios =
    let all =
        [ IcuHospital; CriticalHospital; DeceasedIcu; DeceasedHospital; DeceasedIcuDeceasedHospital; DeceasedHospitalDeceasedTotal; ]

    // color, dash, name
    let getSeriesInfo = function
        | IcuHospital                   -> "#d99a91", Solid,    "Delež v intenzivni enoti"
        | CriticalHospital              -> "#bf5747", Solid,    "Delež na repiratorju"
        | DeceasedIcu                   -> "#d5c768", Solid,    "Smrtnost v intenzivni enoti"
        | DeceasedHospital              -> "#de9a5a", Dot,      "Smrtnost v bolnišnici"
        | DeceasedIcuDeceasedHospital   -> "#de9a5a", Dot,      "Delež smrti v intenzivni enoti"
        | DeceasedHospitalDeceasedTotal -> "#666666", Dot,      "Delež smrti v bolnišnici"

type State = {
    statsData: StatsData
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
        statsData = data
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

        let renderRatioPoint : (Data.Patients.PatientsStats -> JsTimestamp * float option) =
            match ratio with
            | IcuHospital                   -> fun ps -> ps.JsDate12h, percent ps.total.icu.toDate ps.total.inHospital.toDate
            | CriticalHospital              -> fun ps -> ps.JsDate12h, percent ps.total.critical.toDate ps.total.inHospital.toDate
            | DeceasedIcu                   -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.icu.toDate ps.total.icu.toDate
            | DeceasedHospital              -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.toDate ps.total.inHospital.toDate 
            | DeceasedIcuDeceasedHospital   -> fun ps -> ps.JsDate12h, percent ps.total.deceased.hospital.icu.toDate ps.total.deceased.hospital.toDate 
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

    let baseOptions = Highcharts.basicChartOptions ScaleType.Linear "covid19-ratios-mortality"
    {| baseOptions with
        chart = pojo
            {|
                ``type`` ="spline"
                zoomType = "x"
            |}
        plotOptions = pojo
            {|
                spline = pojo {| dataLabels = pojo {| enabled = false |}; marker = pojo {| enabled = false |} |}
            |}
        yAxis = baseOptions.yAxis |> Array.map (fun ax -> {| ax with max = 55 ; labels = pojo {| format = "{value}%" |} |} )

        series = [| for ratio in Ratios.all do yield renderRatiosH ratio |]

        tooltip = pojo {| shared = true; formatter = None |} 

        legend = pojo
            {|
                enabled = Some true
                title = {| text= "" |}
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                //labelFormatter = string //fun series -> series.name
                layout = "vertical"
                floating = true
                x = 20
                y = 30
                backgroundColor = "#FFF"
            |}
|}

let renderChartContainer state =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            match state.displayType with 
            | Cases     -> renderRatiosChart state  |> Highcharts.chartFromWindow
            | Mortality -> renderRatiosChart state  |> Highcharts.chartFromWindow
        ]
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
