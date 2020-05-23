[<RequireQualifiedAccess>]
module PatientsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

open Highcharts
open Types

open Data.Patients

type Breakdown =
    | Structure
    | ByHospital
  with
    static member all = [ Structure; ByHospital; ]
    static member getName = function
        | Structure     -> I18N.t "charts.patients.structure"
        | ByHospital    -> I18N.t "charts.patients.byHospital"

type Series =
    | InHospital
    | Icu
    | Critical
    | InHospitalIn
    | InHospitalOut
    | InHospitalDeceased

module Series =
    let structure =
        [ InHospitalIn; InHospital; Icu; Critical; InHospitalOut; InHospitalDeceased; ]

    let byHospital =
        [ InHospital; ]

    // color, dash, id
    let getSeriesInfo = function
        | InHospital            -> "#de9a5a", Solid, "hospitalized"
        | Icu                   -> "#d99a91", Solid, "icu"
        | Critical              -> "#bf5747", Solid, "ventilator"
        | InHospitalIn          -> "#d5c768", Solid, "admitted"
        | InHospitalOut         -> "#8cd4b2", Solid, "discharged"
        | InHospitalDeceased    -> "#666666", Solid, "deceased"

type State = {
    PatientsData : PatientsStats []
    Error : string option
    AllFacilities : string list
    Breakdown : Breakdown
  } with
    static member SwitchBreakdown breakdown state =
        match breakdown with
        | Structure ->
            { state with
                Breakdown = breakdown
            }
        | ByHospital ->
            { state with
                Breakdown=breakdown
            }
    static member initial =
        {
            PatientsData = [||]
            Error = None
            AllFacilities = []
            Breakdown = Structure
        }
        |> State.SwitchBreakdown Structure

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | SwitchBreakdown of Breakdown

let init () : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either Data.Patients.getOrFetch () ConsumePatientsData ConsumeServerError
    State.initial, cmd

let getFacilitiesList (data : PatientsStats array) =
    seq { // take few samples
        data.[data.Length/2]
        data.[data.Length-2]
        data.[data.Length-1]
    }
    |> Seq.collect (fun stats -> stats.facilities |> Map.toSeq |> Seq.map (fun (facility, stats) -> facility,stats.inHospital.toDate)) // hospital name
    |> Seq.fold (fun hospitals (hospital,cnt) -> hospitals |> Map.add hospital cnt) Map.empty // all
    |> Map.toList
    |> List.sortBy (fun (_,cnt) -> cnt |> Option.defaultValue -1 |> ( * ) -1)
    |> List.map (fst)
 
let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with PatientsData = data; AllFacilities = getFacilitiesList data } |> State.SwitchBreakdown state.Breakdown, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | SwitchBreakdown breakdown ->
        (state |> State.SwitchBreakdown breakdown), Cmd.none

let renderByHospitalChart (state : State) =

    let startDate = DateTime(2020,03,10)

    let renderSources fac =
        let renderPoint ps : (JsTimestamp * int option) =
            let value =
                ps.facilities
                |> Map.tryFind fac
                |> Option.bind (fun stats -> stats.inHospital.today)
                |> Utils.zeroToNone
            ps.JsDate12h, value

        let color, name = Data.Hospitals.facilitySeriesInfo fac
        {|
            visible = true
            color = color
            name = name
            dashStyle = Solid |> DashStyle.toString
            data =
                state.PatientsData
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map renderPoint
                |> Array.ofSeq
            showInLegend = true
        |}
        |> pojo

    let baseOptions = Highcharts.basicChartOptions ScaleType.Linear "covid19-patients-by-hospital"
    {| baseOptions with

        series = [| for fac in state.AllFacilities do yield renderSources fac |]

        tooltip = pojo {| shared = true; formatter = None ; xDateFormat = @"%A, %e. %B %Y" |} 

        legend = pojo
            {|
                enabled = Some true
                title = {| text=(I18N.t "charts.patients.hospitalizedIn") |}
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                layout = "vertical"
                floating = true
                x = 20
                y = 30
                backgroundColor = "#FFF"
            |}
|}

let renderStructureChart (state : State) =

    let startDate = DateTime(2020,03,10)

    let legendFormatter jsThis =
        let pts: obj[] = jsThis?points
        let fmtDate = pts.[0]?point?fmtDate

        let mutable fmtStr = ""
        let mutable fmtLine = ""
        let mutable fmtUnder = ""
        for p in pts do
            match p?point?fmtTotal with
            | "null" -> ()
            | _ ->
                match p?point?id with
                | "hospitalized" | "discharged" | "deceased"  -> fmtUnder <- ""
                | _ -> fmtUnder <- fmtUnder + "↳ "
                fmtLine <- sprintf """<br>%s<span style="color:%s">⬤</span> %s: <b>%s</b>"""
                    fmtUnder
                    p?series?color
                    p?series?name
                    p?point?fmtTotal
                if fmtStr.Length > 0 && p?point?id = "hospitalized" then
                    fmtStr <- fmtLine + fmtStr // if we got Admitted before, then put it after Hospitalized
                else
                    fmtStr <- fmtStr + fmtLine
        sprintf "<b>%s</b>" fmtDate + fmtStr

    let renderBarSeries series =
        let subtract (a : int option) (b : int option) = Some (b.Value - a.Value)
        let negative (a : int option) = Some (- a.Value)

        let getPoint : (Data.Patients.PatientsStats -> int option) =
            match series with
            | InHospital            -> fun ps -> ps.total.inHospital.today |> subtract ps.total.icu.today |> subtract ps.total.inHospital.``in`` |> Utils.zeroToNone
            | Icu                   -> fun ps -> ps.total.icu.today |> subtract ps.total.critical.today |> Utils.zeroToNone
            | Critical              -> fun ps -> ps.total.critical.today |> Utils.zeroToNone
            | InHospitalIn          -> fun ps -> ps.total.inHospital.``in`` |> Utils.zeroToNone
            | InHospitalOut         -> fun ps -> negative ps.total.inHospital.out |> Utils.zeroToNone
            | InHospitalDeceased    -> fun ps -> negative ps.total.deceased.hospital.today |> Utils.zeroToNone

        let getPointTotal : (Data.Patients.PatientsStats -> int option) =
            match series with
            | InHospital            -> fun ps -> ps.total.inHospital.today |> Utils.zeroToNone
            | Icu                   -> fun ps -> ps.total.icu.today |> Utils.zeroToNone
            | Critical              -> fun ps -> ps.total.critical.today |> Utils.zeroToNone
            | InHospitalIn          -> fun ps -> ps.total.inHospital.``in`` |> Utils.zeroToNone
            | InHospitalOut         -> fun ps -> ps.total.inHospital.out |> Utils.zeroToNone
            | InHospitalDeceased    -> fun ps -> ps.total.deceased.hospital.today |> Utils.zeroToNone

        let color, line, id = Series.getSeriesInfo series
        {|
            color = color
            name = I18N.tt "charts.patients" id
            data =
                state.PatientsData
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map (fun dp ->
                    {|
                        x = dp.Date |> jsTime12h
                        y = getPoint dp
                        id = id
                        fmtDate = dp.Date.ToString "d. M. yyyy"
                        fmtTotal = getPointTotal dp |> string
                    |}
                )
                |> Array.ofSeq
        |}
        |> pojo

    let className = "covid19-patients-structure"
    let baseOptions = Highcharts.basicChartOptions ScaleType.Linear className
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = "column"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        plotOptions = pojo
            {|
                column = pojo {| stacking = "normal"; crisp = false; borderWidth = 0; pointPadding = 0; groupPadding = 0 |}
            |}

        series = [| for series in Series.structure do yield renderBarSeries series |]

        tooltip = pojo {| shared = true; formatter = (fun () -> legendFormatter jsThis) |} 

        legend = pojo
            {|
                enabled = Some true
                title = {| text="" |}
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
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
            match state.Breakdown with 
            | Structure     -> renderStructureChart state   |> Highcharts.chartFromWindow
            | ByHospital    -> renderByHospitalChart state  |> Highcharts.chartFromWindow
        ]
    ]

let renderBreakdownSelector state breakdown dispatch =
    Html.div [
        prop.onClick (fun _ -> SwitchBreakdown breakdown |> dispatch)
        prop.className [ true, "btn btn-sm metric-selector"; state.Breakdown = breakdown, "metric-selector--selected" ]
        prop.text (breakdown |> Breakdown.getName)
    ]

let renderBreakdownSelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            Breakdown.all
            |> List.map (fun breakdown -> renderBreakdownSelector state breakdown dispatch) ) ]

let render (state : State) dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state
            renderBreakdownSelectors state dispatch
        ]

let patientsChart () =
    React.elmishComponent("PatientsChart", init (), update, render)
