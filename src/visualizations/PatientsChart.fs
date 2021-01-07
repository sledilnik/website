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

type HospitalType =
    | CovidHospitals
    | CareHospitals

    static member All = [ CovidHospitals; CareHospitals ]

type Breakdown =
    | ByHospital
    | AllHospitals
    | Facility of string
  with
    static member All state = seq {
        yield ByHospital
        yield AllHospitals
        for fcode in state.AllFacilities do
            yield Facility fcode
    }
    static member Default = AllHospitals
    member this.GetName =
        match this with
        | ByHospital -> I18N.t "charts.patients.byHospital"
        | AllHospitals -> I18N.t "charts.patients.allHospitals"
        | Facility facility -> Utils.Dictionaries.GetFacilityName(facility)

and State = {
    PatientsData : PatientsStats []
    Error : string option
    AllFacilities : string list
    HTypeToDisplay : HospitalType
    Breakdown : Breakdown
    RangeSelectionButtonIndex: int
  } with
    static member initial hTypeToDisplay =
        {
            PatientsData = [||]
            Error = None
            AllFacilities = []
            HTypeToDisplay = hTypeToDisplay
            Breakdown = Breakdown.Default
            RangeSelectionButtonIndex = 0
        }

type Series =
    | InHospital
    | Icu
    | IcuDeceased
    | Critical
    | InHospitalIn
    | InHospitalOut
    | InHospitalDeceased
    | Care
    | CareIn
    | CareOut
    | CareDeceased

module Series =
    let structure hTypeToDisplay =
        if hTypeToDisplay = CareHospitals
        then [ CareIn; Care; CareOut; CareDeceased; ]
        else [ InHospitalIn; InHospital; Icu
               Critical; InHospitalOut; InHospitalDeceased; IcuDeceased ]

    let byHospital =
        [ InHospital; ]

    let getSeriesInfo = function
        | InHospital            -> "#de9a5a", "hospitalized"
        | Icu                   -> "#d96756", "icu"
        | IcuDeceased           -> "#6d5b80", "icu-deceased"
        | Critical              -> "#bf5747", "ventilator"
        | InHospitalIn          -> "#d5c768", "admitted"
        | InHospitalOut         -> "#8cd4b2", "discharged"
        | InHospitalDeceased    -> "#8c71a8", "deceased"
        | Care                  -> "#dba51d", "care"
        | CareIn                -> "#d5c768", "admitted"
        | CareOut               -> "#8cd4b2", "discharged"
        | CareDeceased          -> "#a483c7", "deceased"



type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | SwitchBreakdown of Breakdown
    | RangeSelectionChanged of int

let init (hTypeToDisplay : HospitalType) : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either getOrFetch () ConsumePatientsData ConsumeServerError
    State.initial hTypeToDisplay, cmd

let getFacilitiesList (state : State) (data : PatientsStats array) =
    data.[data.Length-1].facilities
    |> Map.toSeq
    |> Seq.filter
       (fun (_, stats) ->
            if state.HTypeToDisplay = CareHospitals
            then stats.care.toDate.IsSome
            else stats.inHospital.toDate.IsSome)
    |> Seq.map
       (fun (facility, stats) ->
            facility,
            if state.HTypeToDisplay = CareHospitals
            then stats.care.today
            else stats.inHospital.today)
    |> Seq.fold (fun hospitals (hospital,cnt) -> hospitals |> Map.add hospital cnt) Map.empty // all
    |> Map.toList
    |> List.sortBy (fun (_,cnt) -> cnt |> Option.defaultValue -1 |> ( * ) -1)
    |> List.map (fst)

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with PatientsData = data; AllFacilities = getFacilitiesList state data }, Cmd.none
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
                |> Option.bind (fun stats ->
                    if state.HTypeToDisplay = CareHospitals
                    then stats.care.today
                    else stats.inHospital.today
                )
            ps.JsDate12h, value

        {|
            visible = true
            name = Utils.Dictionaries.GetFacilityName(fcode)
            color = Utils.Dictionaries.GetFacilityColor(fcode)
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
        basicChartOptions
            ScaleType.Linear "covid19-patients-by-hospital"
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with

        series = [| for fcode in state.AllFacilities do yield renderSources fcode |]

        tooltip = pojo {| shared = true; formatter = None ; xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>"|}

        legend = pojo {| enabled = true ; layout = "horizontal" |}

    |} |> pojo


let renderStructureChart (state : State) dispatch =

    let startDate = DateTime(2020,03,10)

    let tooltipFormatter jsThis =
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
                | "hospitalized" | "care" | "discharged" | "deceased"  -> fmtUnder <- ""
                | _ -> fmtUnder <- fmtUnder + "↳ "
                fmtLine <- sprintf """<br>%s<span style="color:%s">●</span> %s: <b>%s</b>"""
                    fmtUnder
                    p?series?color
                    p?series?name
                    (I18N.NumberFormat.formatNumber(p?point?fmtTotal : int))
                if fmtStr.Length > 0 && List.contains p?point?seriesId [ "hospitalized"; "care" ] then
                    fmtStr <- fmtLine + fmtStr // if we got Admitted before, then put it after Hospitalized
                else
                    fmtStr <- fmtStr + fmtLine
        sprintf "<b>%s</b>" fmtDate + fmtStr

    let psData: (DateTime * FacilityPatientStats)[] =
        match state.Breakdown with
        | Facility fcode ->
            state.PatientsData
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map (
                        fun ps -> (ps.Date, ps.facilities |> Map.find fcode)
                       )
                |> Seq.toArray
        | _ ->
            state.PatientsData
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map
                       (fun ps -> (ps.Date, ps.total.ToFacilityStats))
                |> Seq.toArray

    let renderBarSeries series =
        let subtract (a : int option) (b : int option) =
            match a, b with
            | Some aa, Some bb -> Some (bb - aa)
            | Some aa, None -> -aa |> Some
            | None, Some _ -> b
            | _ -> None
        let negative (a : int option) =
            match a with
            | Some aa -> -aa |> Some
            | None -> None

        let getPoint (ps: FacilityPatientStats): int option =
            match series with
            | InHospital ->
                ps.inHospital.today
                |> subtract ps.icu.today
                |> subtract ps.inHospital.``in``
            | Icu ->
                ps.icu.today
                |> subtract ps.critical.today
            | IcuDeceased -> negative ps.deceased.icu.today
            | Critical -> ps.critical.today
            | InHospitalIn -> ps.inHospital.``in``
            | InHospitalOut -> negative ps.inHospital.out
            | InHospitalDeceased ->
                ps.deceased.today
                |> subtract ps.deceased.icu.today
                |> negative
            | Care ->
                ps.care.today
                |> subtract ps.care.``in``
            | CareIn -> ps.care.``in``
            | CareOut -> negative ps.care.out
            | CareDeceased -> negative ps.deceasedCare.today

        let getPointTotal : (FacilityPatientStats -> int option) =
            match series with
            | InHospital            -> fun ps -> ps.inHospital.today
            | Icu                   -> fun ps -> ps.icu.today
            | IcuDeceased           -> fun ps -> ps.deceased.icu.today
            | Critical              -> fun ps -> ps.critical.today
            | InHospitalIn          -> fun ps -> ps.inHospital.``in``
            | InHospitalOut         -> fun ps -> ps.inHospital.out
            | InHospitalDeceased    -> fun ps -> ps.deceased.today
            | Care                  -> fun ps -> ps.care.today
            | CareIn                -> fun ps -> ps.care.``in``
            | CareOut               -> fun ps -> ps.care.out
            | CareDeceased          -> fun ps -> ps.deceasedCare.today

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
        basicChartOptions
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
                column = pojo {| dataGrouping = pojo {| enabled = false |} ; stacking = "normal"; crisp = false; borderWidth = 0; pointPadding = 0; groupPadding = 0 |}
            |}

        series = [| for series in Series.structure state.HTypeToDisplay
                        do yield renderBarSeries series |]

        tooltip = pojo {| shared = true; split = false ; formatter = (fun () -> tooltipFormatter jsThis) |}

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
                |> chartFromWindow
            | _ ->
                renderStructureChart state dispatch
                |> chartFromWindow
        ]
    ]

let renderBreakdownSelector state breakdown dispatch =
    Html.div [
        prop.onClick (fun _ -> SwitchBreakdown breakdown |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (state.Breakdown = breakdown, "metric-selector--selected") ]
        prop.text breakdown.GetName
    ]

let renderBreakdownSelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            Breakdown.All state
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

let patientsChart (props : {| hTypeToDisplay : HospitalType |}) =
    React.elmishComponent("PatientsChart", init props.hTypeToDisplay, update, render)
