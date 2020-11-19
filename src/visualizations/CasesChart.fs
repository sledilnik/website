[<RequireQualifiedAccess>]
module CasesChart

open System
open System.Collections.Generic
open Data.Patients
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Types
open Highcharts

type DisplayType =
    | MultiChart

type State = {
    data: StatsData
    PatientsData : PatientsStats []
    displayType: DisplayType
    RangeSelectionButtonIndex: int
    Error : string option
}

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

type Series =
    | DeceasedInIcu
    | DeceasedInHospitals
    | DeceasedOther
    | Recovered
    | Active
    | InHospital
    | Icu
    | Critical

module Series =
    let all =
        [ Active; InHospital; Icu; Critical; Recovered
          DeceasedOther; DeceasedInHospitals; DeceasedInIcu ]
    let active =
        [ Active; InHospital; Icu; Critical; ]
    let inHospital =
        [ InHospital; Icu; Critical; ]
    let closed =
        [ DeceasedInHospitals; DeceasedOther; DeceasedInIcu; Recovered;  ]

    let getSeriesInfo = function
        | DeceasedInIcu        -> true,  "#666666",   "deceased-icu"
        | DeceasedInHospitals  -> true,  "#888888",   "deceased-hospital"
        | DeceasedOther        -> true,  "#aaaaaa",   "deceased-rest"
        | Recovered     -> true,  "#8cd4b2",   "recovered"
        | Active        -> true,  "#d5c768",   "active"
        | InHospital    -> true,  "#de9a5a",   "hospitalized"
        | Icu           -> true,  "#d96756",   "icu"
        | Critical      -> true,  "#bf5747",   "ventilator"

let init data : State * Cmd<Msg> =
    let state = {
        data = data
        PatientsData = [||]
        displayType = MultiChart
        RangeSelectionButtonIndex = 0
        Error = None
    }

    let cmd = Cmd.OfAsync.either getOrFetch ()
                   ConsumePatientsData ConsumeServerError

    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with PatientsData = data }, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | ChangeDisplayType rt ->
        { state with displayType=rt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let legendFormatter jsThis =
    let pts: obj[] = jsThis?points
    let fmtDate = pts.[0]?point?fmtDate

    let mutable fmtUnder = ""
    let mutable fmtStr = sprintf "<b>%s</b>" fmtDate
    for p in pts do
        match p?point?fmtTotal with
        | "null" -> ()
        | _ ->
            match p?point?seriesId with
            | "active" | "recovered" | "deceased"  -> fmtUnder <- ""
            | _ -> fmtUnder <- fmtUnder + "↳ "
            fmtStr <- fmtStr + sprintf """<br>%s<span style="color:%s">●</span> %s: <b>%s</b>"""
                fmtUnder
                p?series?color
                p?series?name
                p?point?fmtTotal
    fmtStr

let renderChartOptions (state : State) dispatch =
    let className = "cases-chart"
    let scaleType = ScaleType.Linear

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

    let renderSeries series =

        let getPoint : (StatsDataPoint -> PatientsStats -> int option) =
            match series with
            | Recovered ->
                fun sdp pdp -> negative sdp.Cases.RecoveredToDate
            | DeceasedInIcu ->
                fun sdp pdp -> negative pdp.total.deceased.hospital.icu.toDate
            | DeceasedInHospitals ->
                fun sdp pdp ->
                    pdp.total.deceased.hospital.toDate
                    |> subtract pdp.total.deceased.hospital.icu.toDate
                    |> negative
            | DeceasedOther ->
                fun sdp pdp ->
                    sdp.StatePerTreatment.DeceasedToDate
                    |> subtract pdp.total.deceased.hospital.toDate
                    |> negative
            | Active ->
                fun sdp pdp ->
                    sdp.Cases.Active
                    |> subtract sdp.StatePerTreatment.InHospital
            | InHospital ->
                fun sdp pdp ->
                    sdp.StatePerTreatment.InHospital
                    |> subtract sdp.StatePerTreatment.InICU
            | Icu ->
                fun sdp pdp ->
                    sdp.StatePerTreatment.InICU
                    |> subtract sdp.StatePerTreatment.Critical
            | Critical ->
                fun sdp pdp -> sdp.StatePerTreatment.Critical

        let getPointTotal : (StatsDataPoint -> PatientsStats -> int option) =
            match series with
            | Recovered ->
                fun sdp pdp -> sdp.Cases.RecoveredToDate
            | DeceasedInIcu ->
                fun sdp pdp -> pdp.total.deceased.hospital.icu.toDate
            | DeceasedInHospitals ->
                fun sdp pdp -> pdp.total.deceased.hospital.toDate
            | DeceasedOther ->
                fun sdp pdp -> sdp.StatePerTreatment.DeceasedToDate
            | Active ->
                fun sdp pdp -> sdp.Cases.Active
            | InHospital ->
                fun sdp pdp -> sdp.StatePerTreatment.InHospital
            | Icu ->
                fun sdp pdp -> sdp.StatePerTreatment.InICU
            | Critical ->
                fun sdp pdp -> sdp.StatePerTreatment.Critical

        let statsDataDict = state.data |> Seq.map(fun x -> x.Date, x) |> dict

        let patientsDataDict
            = state.PatientsData |> Seq.map(fun x -> x.Date, x) |> dict

        let mergeFunc (pair: KeyValuePair<DateTime, StatsDataPoint>) =
            let date = pair.Key
            let stateDp = pair.Value
            match patientsDataDict.TryGetValue date with
            | true, patientsDp -> Some (date, stateDp, patientsDp)
            | false, _ -> None

        let mergedData =
            statsDataDict
            |> Seq.map mergeFunc
            |> Seq.choose id
            |> Seq.toList
            |> List.sortBy (fun (date, _, _) -> date)

        let visible, color, seriesId = Series.getSeriesInfo series
        {|
            ``type`` = "column"
            visible = visible
            color = color
            name = I18N.tt "charts.cases" seriesId
            data =
                mergedData
                |> Seq.filter (fun (_, dp, _) -> dp.Cases.Active.IsSome)
                |> Seq.map (fun (date, statsDp, patientsDp) ->
                    {|
                        x = date |> jsTime12h
                        y = getPoint statsDp patientsDp
                        seriesId = seriesId
                        fmtDate = I18N.tOptions "days.longerDate"
                                      {| date = date |}
                        fmtTotal = getPointTotal statsDp patientsDp |> string
                    |} |> pojo
                )
                |> Array.ofSeq
        |}
        |> pojo

    let allSeries = [|
        for series in Series.all do
            yield renderSeries series
    |]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions
            scaleType className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with
        series = allSeries
        plotOptions = pojo
            {|
                series = {| stacking = "normal"; crisp = false; borderWidth = 0; pointPadding = 0; groupPadding = 0  |}
            |}

        tooltip = pojo
            {|
                shared = true
                formatter = fun () -> legendFormatter jsThis
            |}

        legend = pojo {| enabled = true ; layout = "horizontal" |}

    |}

let renderChartContainer (state : State) dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> chartFromWindow
        ]
    ]

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state dispatch
    ]

let casesChart (props : {| data : StatsData |}) =
    React.elmishComponent("CasesChart", init props.data, update, render)
