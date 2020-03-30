
[<RequireQualifiedAccess>]
module PatientsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Highcharts
open Types

open Data.Patients

type Segmentation =
    | Totals
    | Facility of string

type Breakdown =
    | BySeries
    | BySource
  with
    static member all = [ BySource; BySeries ]
    static member getName = function
        | BySeries -> "Obravnava po pacientih"
        | BySource -> "Hospitalizirani po bolnišnicah"

type Series =
    | InCare
    | OutOfHospital
    | InHospital
    | AllInHospital
    | NeedsO2
    | Icu
    | Critical
    | Deceased
    | Hospital
    | Home

module Series =
    let all =
        [ InCare; AllInHospital; InHospital; OutOfHospital; NeedsO2; Icu; Critical; Deceased; Hospital; Home; ]

    // color, dash, name
    let getSeriesInfo = function
        | InCare        -> "#ffa600", "cs-inCare", "Oskrbovani"
        | OutOfHospital -> "#20b16d", "cs-outOfHospitalToDate", "Odpuščeni iz bolnišnice - skupaj"
        | InHospital    -> "#be7a2a", "cs-inHospital", "Hospitalizirani - trenutno"
        | AllInHospital -> "#de9a5a", "cs-inHospitalToDate", "Hospitalizirani - skupaj"
        | NeedsO2       -> "#70a471", "cs-needsO2", "Potrebuje kisik"
        | Icu           -> "#bf5747", "cs-inHospitalICU", "V intenzivni enoti"
        | Critical      -> "#d99a91", "cs-critical", "Respirator / kritično stanje"
        | Deceased      -> "#666666", "cs-deceasedToDate", "Umrli - skupaj"
        | Hospital      -> "#be772a", "cs-hospital", "Hospitalizirani"
        | Home          -> "#003f5c", "cs-home", "Doma"

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
        | BySeries ->
            { state with
                breakdown=breakdown
                allSegmentations = [ Totals ]
                activeSegmentations = Set [ Totals ]
                activeSeries = Set Series.all
            }
        | BySource ->
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
                    |> List.sortBy (fun (f,cnt) -> cnt |> Option.defaultValue -1 |> ( * ) -1)
                    |> List.map (fst >> Facility)
            { state with
                breakdown=breakdown
                allSegmentations = segmentations
                activeSegmentations = Set segmentations
                activeSeries = Set [ InHospital ]
            }
    static member initial =
        {
            scaleType = Linear
            data = [||]
            error = None
            allSegmentations = [ Totals ]
            activeSegmentations = Set [ Totals ]
            allSeries =
                // exclude stuff that doesn't exist or doesn't make sense in Total
                let exclude = Set [ Home; Hospital; InCare; NeedsO2 ]
                Series.all |> List.filter (not << exclude.Contains)
            activeSeries = Set Series.all
            breakdown = BySource
        }
        |> State.switchBreakdown BySource



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
    let cmd = Cmd.OfAsync.either Data.Patients.fetch Data.Patients.url ConsumeServerData ConsumeServerError
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

let renderChartOptions (state : State) =

    let startDate = DateTime(2020,03,10)

    let zeroToNone value =
        match value with
        | None -> None
        | Some x ->
            if x = 0 then None
            else Some x

    let renderSeries series =
        let renderPoint : (Data.Patients.PatientsStats -> JsTimestamp * int option) =
            match series with
            | InCare        -> fun ps -> ps.JsDate, ps.total.inCare |> zeroToNone
            | OutOfHospital -> fun ps -> ps.JsDate, ps.total.outOfHospital.toDate |> zeroToNone
            | InHospital    -> fun ps -> ps.JsDate, ps.total.inHospital.today |> zeroToNone
            | AllInHospital -> fun ps -> ps.JsDate, ps.total.inHospital.toDate |> zeroToNone
            | NeedsO2       -> fun ps -> ps.JsDate, ps.total.needsO2.toDate |> zeroToNone
            | Icu           -> fun ps -> ps.JsDate, ps.total.icu.today |> zeroToNone
            | Critical      -> fun ps -> ps.JsDate, ps.total.critical.today |> zeroToNone
            | Deceased      -> fun ps -> ps.JsDate, ps.total.deceased.toDate |> zeroToNone
            | Hospital      -> fun ps -> ps.JsDate, failwithf "home & hospital"
            | Home          -> fun ps -> ps.JsDate, failwithf "home & totals"

        let color, className, name = Series.getSeriesInfo series

        {|
            visible = state.activeSeries |> Set.contains series
            color = color
            name = name
            className = className
            data =
                state.data
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map renderPoint
                |> Array.ofSeq
            //yAxis = 0 // axis index
            //showInLegend = true
            //fillOpacity = 0
        |}
        |> pojo


    let renderSources segmentation =
        let facility, (renderPoint: Data.Patients.PatientsStats -> JsTimestamp * int option) =
            match segmentation with
            | Totals -> "Skupaj", fun ps -> ps.JsDate, ps.total.inHospital.today |> zeroToNone
            | Facility f ->
                f, (fun ps ->
                    let value =
                        ps.facilities
                        |> Map.tryFind f
                        |> Option.bind (fun stats -> stats.inHospital.today)
                        |> zeroToNone
                    ps.JsDate, value)

        let color, name = Data.Hospitals.facilitySeriesInfo facility
        {|
            visible = true
            color = color
            name = name
            className = "cs-hospital-"+facility
            data =
                state.data
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map renderPoint
                |> Array.ofSeq
            showInLegend = true
            //yAxis = 0 // axis index
            //showInLegend = true
            //fillOpacity = 0
        |}
        |> pojo

    let allSeries = [|
        match state.breakdown with
        | BySeries ->
            for segmentation in state.allSegmentations do // |> Seq.filter (fun s -> Set.contains s state.activeSegmentations) do
                for series in state.allSeries |> Seq.filter (fun s -> Set.contains s state.activeSeries) do
                    yield renderSeries series
        | BySource ->
            for segmentation in state.allSegmentations do // |> Seq.filter (fun s -> Set.contains s state.activeSegmentations) do
                yield renderSources segmentation
    |]

    let className =
        match state.breakdown with
        | BySeries -> "covid19-patients"
        | BySource -> "covid19-hospitals"

    let baseOptions = Highcharts.basicChartOptions state.scaleType className
    {| baseOptions with
        series = allSeries
        legend = pojo
            {|
                enabled = Some true
                title = {| text=if state.breakdown=BySeries then "" else "Hospitalizirani v:" |}
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
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state
            |> Highcharts.chart
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
    | [||], None -> Html.div [ prop.text "loading" ]
    | _, Some err -> Html.div [ prop.text err ]
    | data, None ->
        Html.div [
            Utils.renderScaleSelector state.scaleType (ScaleTypeChanged >> dispatch)
            renderChartContainer state
            renderBreakdownSelectors state dispatch
        ]

let patientsChart () =
    React.elmishComponent("RegionsChart", init (), update, render)
