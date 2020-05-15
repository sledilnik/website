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

type Segmentation =
    | Totals
    | Facility of string

type Breakdown =
    | Structure
    | BySeries
    | ByHospital
  with
    static member all = [ Structure; ByHospital; ]
    static member getName = function
        | Structure     -> "Struktura"
        | BySeries      -> "Obravnava po pacientih"
        | ByHospital    -> "Po bolnišnicah"

type Series =
    | InHospital
    | Icu
    | Critical
    | InHospitalIn
    | InHospitalOut
    | InHospitalDeceased
    | AllInHospital
    | OutOfHospital
    | Deceased

module Series =
    let all =
        [ InHospital; Icu; Critical; InHospitalIn; InHospitalOut; InHospitalDeceased; AllInHospital; OutOfHospital; Deceased; ]

    let structure =
        [ InHospitalIn; InHospital; Icu; Critical; InHospitalOut; InHospitalDeceased; ]

    let bySeries =
        [ InHospital; Icu; Critical; AllInHospital; OutOfHospital; Deceased; ]

    let byHospital =
        [ InHospital; ]

    // color, dash, name
    let getSeriesInfo = function
        | InHospital            -> "#de9a5a", Solid, "cs-inHospital", "Hospitalizirani"
        | Icu                   -> "#d99a91", Solid, "cs-inHospitalICU", "V intenzivni enoti"
        | Critical              -> "#bf5747", Solid, "cs-critical", "Na respiratorju"
        | InHospitalIn          -> "#d5c768", Solid, "cs-inHospitalIn", "Sprejeti"
        | InHospitalOut         -> "#8cd4b2", Solid, "cs-inHospitalOut", "Odpuščeni"
        | InHospitalDeceased    -> "#666666", Solid, "cs-inHospitalDeceased", "Umrli"
        | AllInHospital         -> "#de9a5a", Dot,   "cs-inHospitalToDate", "Hospitalizirani (skupaj)"
        | OutOfHospital         -> "#20b16d", Dot,   "cs-outOfHospitalToDate", "Odpuščeni iz bolnišnice (skupaj)"
        | Deceased              -> "#666666", Dot,   "cs-deceasedToDate", "Umrli (skupaj)"

type State = {
    scaleType : ScaleType
    data : PatientsStats []
    error: string option
    allSegmentations: Segmentation list
    activeSegmentations: Set<Segmentation>
    allSeries: Series list
    activeSeries: Set<Series>
    breakdown: Breakdown
  } with
    static member switchBreakdown breakdown state =
        match breakdown with
        | Structure ->
            { state with
                breakdown=breakdown
                allSegmentations = [ Totals ]
                activeSegmentations = Set [ Totals ]
                activeSeries = Set Series.structure
            }
        | BySeries ->
            { state with
                breakdown=breakdown
                allSegmentations = [ Totals ]
                activeSegmentations = Set [ Totals ]
                activeSeries = Set Series.bySeries
            }
        | ByHospital ->
            let segmentations =
                match state.data with
                | [||] -> [Totals]
                | [| _ |] -> [Totals]
                | data ->
                    // TODO: in future we'll need more
                    seq { // take few samples
                        data.[data.Length/2]
                        data.[data.Length-2]
                        data.[data.Length-1]
                    }
                    |> Seq.collect (fun stats -> stats.facilities |> Map.toSeq |> Seq.map (fun (facility, stats) -> facility,stats.inHospital.toDate)) // hospital name
                    |> Seq.fold (fun hospitals (hospital,cnt) -> hospitals |> Map.add hospital cnt) Map.empty // all
                    |> Map.toList
                    |> List.sortBy (fun (_,cnt) -> cnt |> Option.defaultValue -1 |> ( * ) -1)
                    |> List.map (fst >> Facility)
            { state with
                breakdown=breakdown
                allSegmentations = segmentations
                activeSegmentations = Set segmentations
                activeSeries = Set Series.byHospital
            }
    static member initial =
        {
            scaleType = Linear
            data = [||]
            error = None
            allSegmentations = [ Totals ]
            activeSegmentations = Set [ Totals ]
            allSeries = Series.all
            activeSeries = Set Series.all
            breakdown = Structure
        }
        |> State.switchBreakdown Structure



module Set =
    let toggle x s =
        match s |> Set.contains x with
        | true -> s |> Set.remove x
        | false -> s |> Set.add x

type Msg =
    | ConsumeServerData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ToggleSegmentation of Segmentation
    | ToggleSeries of Series
    | ScaleTypeChanged of ScaleType
    | SwitchBreakdown of Breakdown

let init () : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either Data.Patients.getOrFetch () ConsumeServerData ConsumeServerError
    State.initial, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeServerData (Ok data) ->
        { state with data = data } |> State.switchBreakdown state.breakdown, Cmd.none
    | ConsumeServerData (Error err) ->
        { state with error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with error = Some ex.Message }, Cmd.none
    | ToggleSegmentation s ->
        { state with activeSegmentations = state.activeSegmentations |> Set.toggle s }, Cmd.none
    | ToggleSeries s ->
        { state with activeSeries = state.activeSeries |> Set.toggle s }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with scaleType = scaleType }, Cmd.none
    | SwitchBreakdown breakdown ->
        (state |> State.switchBreakdown breakdown), Cmd.none

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
            match p?series?name with
            | "Hospitalizirani" | "Odpuščeni" | "Umrli"  -> fmtUnder <- ""
            | _ -> fmtUnder <- fmtUnder + "↳ "
            fmtLine <- sprintf """<br>%s<span style="color:%s">⬤</span> %s: <b>%s</b>"""
                fmtUnder
                p?series?color
                p?series?name
                p?point?fmtTotal
            if fmtStr.Length > 0 && p?series?name = "Hospitalizirani" then
                fmtStr <- fmtLine + fmtStr // if we got Sprejeti before, then put it after Hospitalizirani
            else
                fmtStr <- fmtStr + fmtLine

    sprintf "<b>%s</b>" fmtDate + fmtStr

let renderChartOptions (state : State) =

    let startDate = DateTime(2020,03,10)

    let zeroToNone value =
        match value with
        | None -> None
        | Some x ->
            if x = 0 then None
            else Some x

    let renderBarSeries series =
        let subtract (a : int option) (b : int option) = Some (b.Value - a.Value)
        let negative (a : int option) = Some (- a.Value)

        let getPoint : (Data.Patients.PatientsStats -> int option) =
            match series with
            | InHospital            -> fun ps -> ps.total.inHospital.today |> subtract ps.total.icu.today |> subtract ps.total.inHospital.``in`` |> zeroToNone
            | Icu                   -> fun ps -> ps.total.icu.today |> subtract ps.total.critical.today |> zeroToNone
            | Critical              -> fun ps -> ps.total.critical.today |> zeroToNone
            | InHospitalIn          -> fun ps -> ps.total.inHospital.``in`` |> zeroToNone
            | InHospitalOut         -> fun ps -> negative ps.total.inHospital.out |> zeroToNone
            | InHospitalDeceased    -> fun ps -> negative ps.total.deceased.hospital.today |> zeroToNone
            | _ -> fun ps -> None

        let getPointTotal : (Data.Patients.PatientsStats -> int option) =
            match series with
            | InHospital            -> fun ps -> ps.total.inHospital.today |> zeroToNone
            | Icu                   -> fun ps -> ps.total.icu.today |> zeroToNone
            | Critical              -> fun ps -> ps.total.critical.today |> zeroToNone
            | InHospitalIn          -> fun ps -> ps.total.inHospital.``in`` |> zeroToNone
            | InHospitalOut         -> fun ps -> ps.total.inHospital.out |> zeroToNone
            | InHospitalDeceased    -> fun ps -> ps.total.deceased.hospital.today |> zeroToNone
            | _ -> fun ps -> None

        let color, line, className, name = Series.getSeriesInfo series

        {|
            visible = state.activeSeries |> Set.contains series
            color = color
            name = name
            data =
                state.data
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map (fun dp ->
                    {|
                        x = dp.Date |> jsTime12h
                        y = getPoint dp
                        fmtDate = dp.Date.ToString "d. M. yyyy"
                        fmtTotal = getPointTotal dp |> string
                    |}
                )
                |> Array.ofSeq
        |}
        |> pojo

    let renderSeries series =
        let renderPoint : (Data.Patients.PatientsStats -> JsTimestamp * int option) =
            match series with
            | InHospital            -> fun ps -> ps.JsDate12h, ps.total.inHospital.today |> zeroToNone
            | Icu                   -> fun ps -> ps.JsDate12h, ps.total.icu.today |> zeroToNone
            | Critical              -> fun ps -> ps.JsDate12h, ps.total.critical.today |> zeroToNone
            | InHospitalIn          -> fun ps -> ps.JsDate12h, ps.total.inHospital.``in`` |> zeroToNone
            | InHospitalOut         -> fun ps -> ps.JsDate12h, ps.total.inHospital.out |> zeroToNone
            | InHospitalDeceased    -> fun ps -> ps.JsDate12h, ps.total.deceased.hospital.today |> zeroToNone
            | AllInHospital         -> fun ps -> ps.JsDate12h, ps.total.inHospital.toDate |> zeroToNone
            | OutOfHospital         -> fun ps -> ps.JsDate12h, ps.total.outOfHospital.toDate |> zeroToNone
            | Deceased              -> fun ps -> ps.JsDate12h, ps.total.deceased.toDate |> zeroToNone

        let color, line, className, name = Series.getSeriesInfo series

        {|
            visible = state.activeSeries |> Set.contains series
            color = color
            dashStyle = line |> DashStyle.toString
            name = name
            data =
                state.data
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map renderPoint
                |> Array.ofSeq
        |}
        |> pojo

    let renderSources segmentation =
        let facility, (renderPoint: Data.Patients.PatientsStats -> JsTimestamp * int option) =
            match segmentation with
            | Totals -> "Skupaj", fun ps -> ps.JsDate12h, ps.total.inHospital.today |> zeroToNone
            | Facility f ->
                f, (fun ps ->
                    let value =
                        ps.facilities
                        |> Map.tryFind f
                        |> Option.bind (fun stats -> stats.inHospital.today)
                        |> zeroToNone
                    ps.JsDate12h, value)

        let color, name = Data.Hospitals.facilitySeriesInfo facility
        {|
            visible = true
            color = color
            name = name
            dashStyle = Solid |> DashStyle.toString
            data =
                state.data
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map renderPoint
                |> Array.ofSeq
            showInLegend = true
        |}
        |> pojo

    let allSeries = [|
        match state.breakdown with
        | Structure ->
            for series in Series.structure do
                yield renderBarSeries series
        | BySeries ->
            for _ in state.allSegmentations do // |> Seq.filter (fun s -> Set.contains s state.activeSegmentations) do
                for series in state.allSeries |> Seq.filter (fun s -> Set.contains s state.activeSeries) do
                    yield renderSeries series
        | ByHospital ->
            for segmentation in state.allSegmentations do // |> Seq.filter (fun s -> Set.contains s state.activeSegmentations) do
                yield renderSources segmentation
    |]

    let className =
        match state.breakdown with
        | Structure     -> "covid19-patients-inout"
        | BySeries      -> "covid19-patients"
        | ByHospital    -> "covid19-hospitals"

    let tooltipOpt = {|
                         shared = true
                         formatter =
                             if state.breakdown = Structure then
                                 (fun () -> legendFormatter jsThis) |> Some
                             else None
                      |} |> pojo

    let baseOptions = Highcharts.basicChartOptions state.scaleType className
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = if state.breakdown = Structure then "column" else "line"
                zoomType = "x"
            |}
        plotOptions = pojo
            {|
                series =
                    if state.breakdown = Structure
                    then pojo {| stacking = "normal"; crisp = false; borderWidth = 0; pointPadding = 0; groupPadding = 0 |}
                    else pojo {| stacking = ""; |}
            |}
        series = allSeries

        tooltip = tooltipOpt

        legend = pojo
            {|
                enabled = Some true
                title = {| text=if state.breakdown=ByHospital then "Hospitalizirani v:" else "" |}
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
            renderChartOptions state
            |> Highcharts.chartFromWindow
        ]
    ]

let renderBreakdownSelector state breakdown dispatch =
    Html.div [
        prop.onClick (fun _ -> SwitchBreakdown breakdown |> dispatch)
        prop.className [ true, "btn btn-sm metric-selector"; state.breakdown = breakdown, "metric-selector--selected" ]
        prop.text (breakdown |> Breakdown.getName)
    ]

let renderBreakdownSelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            Breakdown.all
            |> List.map (fun breakdown ->
                renderBreakdownSelector state breakdown dispatch
            ) ) ]

let render (state : State) dispatch =
    match state.data, state.error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            //Utils.renderChartTopControlRight
            //    (Utils.renderScaleSelector
            //        state.scaleType (ScaleTypeChanged >> dispatch))

            renderChartContainer state
            renderBreakdownSelectors state dispatch
        ]

let patientsChart () =
    React.elmishComponent("PatientsChart", init (), update, render)
