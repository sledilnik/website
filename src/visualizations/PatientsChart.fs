[<RequireQualifiedAccess>]
module PatientsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Highcharts
open Types

open Data.Patients

type Breakdown =
    | ByHospital
    | AllHospitals
    | Facility of string
  with
    static member getName = function
        | ByHospital -> I18N.t "charts.patients.byHospital"
        | AllHospitals -> I18N.t "charts.patients.allHospitals"
        | Facility fcode ->
            let _, name = Data.Hospitals.facilitySeriesInfo fcode
            name

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

    let getSeriesInfo = function
        | InHospital            -> "#de9a5a", "hospitalized"
        | Icu                   -> "#d99a91", "icu"
        | Critical              -> "#bf5747", "ventilator"
        | InHospitalIn          -> "#d5c768", "admitted"
        | InHospitalOut         -> "#8cd4b2", "discharged"
        | InHospitalDeceased    -> "#666666", "deceased"

type State = {
    PatientsData : PatientsStats []
    Error : string option
    AllFacilities : string list
    Breakdown : Breakdown
    RangeSelectionButtonIndex: int
  } with
    static member initial =
        {
            PatientsData = [||]
            Error = None
            AllFacilities = []
            Breakdown = AllHospitals
            RangeSelectionButtonIndex = 0
        }

let getAllBreakdowns state = seq {
        yield ByHospital
        yield AllHospitals
        for fcode in state.AllFacilities do
            yield Facility fcode
    }

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | SwitchBreakdown of Breakdown
    | RangeSelectionChanged of int

let init () : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either Data.Patients.getOrFetch () ConsumePatientsData ConsumeServerError
    State.initial, cmd

let getFacilitiesList (data : PatientsStats array) =
    seq { // take few samples
        data.[data.Length/2]
        data.[data.Length-2]
        data.[data.Length-1]
    }
    |> Seq.collect (fun stats -> stats.facilities |> Map.toSeq |> Seq.map (fun (facility, stats) -> facility,stats.inHospital.today)) // hospital name
    |> Seq.fold (fun hospitals (hospital,cnt) -> hospitals |> Map.add hospital cnt) Map.empty // all
    |> Map.toList
    |> List.sortBy (fun (_,cnt) -> cnt |> Option.defaultValue -1 |> ( * ) -1)
    |> List.map (fst)

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with PatientsData = data; AllFacilities = getFacilitiesList data }, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | SwitchBreakdown breakdown ->
        { state with Breakdown = breakdown }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderByHospitalChart (state : State) dispatch =

    let startDate = DateTime(2020,03,10)

    let renderSources fcode =
        let renderPoint ps : (JsTimestamp * int option) =
            let value =
                ps.facilities
                |> Map.tryFind fcode
                |> Option.bind (fun stats -> stats.inHospital.today)
                |> Utils.zeroToNone
            ps.JsDate12h, value

        let color, name = Data.Hospitals.facilitySeriesInfo fcode
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
        |} |> pojo

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear "covid19-patients-by-hospital"
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with

        series = [| for fcode in state.AllFacilities do yield renderSources fcode |]

        tooltip = pojo {| shared = true; formatter = None ; xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>"|}

        legend = pojo {| enabled = true ; layout = "horizontal" |}

    |} |> pojo


let renderStructureChart (state : State) dispatch =

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
                match p?point?seriesId with
                | "hospitalized" | "discharged" | "deceased"  -> fmtUnder <- ""
                | _ -> fmtUnder <- fmtUnder + "↳ "
                fmtLine <- sprintf """<br>%s<span style="color:%s">●</span> %s: <b>%s</b>"""
                    fmtUnder
                    p?series?color
                    p?series?name
                    p?point?fmtTotal
                if fmtStr.Length > 0 && p?point?seriesId = "hospitalized" then
                    fmtStr <- fmtLine + fmtStr // if we got Admitted before, then put it after Hospitalized
                else
                    fmtStr <- fmtStr + fmtLine
        sprintf "<b>%s</b>" fmtDate + fmtStr

    let psData =
        match state.Breakdown with
        | Facility fcode ->
            state.PatientsData |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map (fun ps -> (ps.Date, ps.facilities |> Map.find fcode)) |> Seq.toArray
        | _ ->
            state.PatientsData |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map (fun ps -> (ps.Date, ps.total.ToFacilityStats)) |> Seq.toArray


    let renderBarSeries series =
        let subtract (a : int option) (b : int option) = Some (b.Value - a.Value)
        let negative (a : int option) = Some (- a.Value)

        let getPoint : (Data.Patients.FacilityPatientStats -> int option) =
            match series with
            | InHospital            -> fun ps -> ps.inHospital.today |> subtract ps.icu.today |> subtract ps.inHospital.``in`` |> Utils.zeroToNone
            | Icu                   -> fun ps -> ps.icu.today |> subtract ps.critical.today |> Utils.zeroToNone
            | Critical              -> fun ps -> ps.critical.today |> Utils.zeroToNone
            | InHospitalIn          -> fun ps -> ps.inHospital.``in`` |> Utils.zeroToNone
            | InHospitalOut         -> fun ps -> negative ps.inHospital.out |> Utils.zeroToNone
            | InHospitalDeceased    -> fun ps -> negative ps.deceased.today |> Utils.zeroToNone

        let getPointTotal : (Data.Patients.FacilityPatientStats -> int option) =
            match series with
            | InHospital            -> fun ps -> ps.inHospital.today |> Utils.zeroToNone
            | Icu                   -> fun ps -> ps.icu.today |> Utils.zeroToNone
            | Critical              -> fun ps -> ps.critical.today |> Utils.zeroToNone
            | InHospitalIn          -> fun ps -> ps.inHospital.``in`` |> Utils.zeroToNone
            | InHospitalOut         -> fun ps -> ps.inHospital.out |> Utils.zeroToNone
            | InHospitalDeceased    -> fun ps -> ps.deceased.today |> Utils.zeroToNone

        let color, seriesId = Series.getSeriesInfo series
        {|
            color = color
            name = I18N.tt "charts.patients" seriesId
            data =
                psData
                |> Seq.map (fun (date,ps) ->
                    {|
                        x = date |> jsTime12h
                        y = getPoint ps
                        fmtTotal = getPointTotal ps |> string
                        fmtDate = I18N.tOptions "days.longerDate" {| date = date |}
                        seriesId = seriesId
                    |} )
                |> Seq.toArray
        |} |> pojo

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let className = "covid19-patients-structure"
    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with
        chart = pojo
            {|
                animation = false
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

        legend = pojo {| enabled = true ; layout = "horizontal" |}

    |} |> pojo


let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            match state.Breakdown with
            | ByHospital ->
                renderByHospitalChart state dispatch
                |> Highcharts.chartFromWindow
            | _ ->
                renderStructureChart state dispatch
                |> Highcharts.chartFromWindow
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
            getAllBreakdowns state
            |> Seq.map (fun breakdown -> renderBreakdownSelector state breakdown dispatch) ) ]

let render (state : State) dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
            renderBreakdownSelectors state dispatch
        ]

let patientsChart () =
    React.elmishComponent("PatientsChart", init (), update, render)
