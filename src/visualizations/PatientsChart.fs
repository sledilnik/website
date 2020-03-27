
[<RequireQualifiedAccess>]
module PatientsChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Recharts

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
    | NeedsO2
    | Icu
    | Critical
    | Deceased
    | Hospital
    | Home

module Series =
    let all =
        [ InCare; InHospital; OutOfHospital; NeedsO2; Icu; Critical; Deceased; Hospital; Home; ]

    let getColor = function
        | InCare        -> "#ffa600"
        | OutOfHospital -> "#8CCDAA"
        | InHospital    -> "#D09D93"
        | NeedsO2       -> "#70a471"
        | Icu           -> "#B35D4C"
        | Critical      -> "#2B6A7A"
        | Deceased      -> "#000000"
        | Hospital      -> "#0a6b85"
        | Home          -> "#003f5c"

    let getName = function
        | InCare -> "Oskrbovani"
        | OutOfHospital -> "Odpuščeni iz bolnišnice - skupaj"
        | InHospital -> "Hospitalizirani"
        | NeedsO2 -> "Potrebuje kisik"
        | Icu -> "Intenzivna nega"
        | Critical -> "Kritično stanje - ocena"
        | Deceased -> "Umrli - skupaj"
        | Hospital -> "Hospitalizirani"
        | Home -> "Doma"

/// return (seriesName * color) based on facility name
let facilityLine = function
    | "sbce"  -> "SB Celje", "#70a471"
    | "ukclj" -> "UKC Ljubljana", "#10829a"
    | "ukcmb" -> "UKC Maribor", "#003f5c"
    | "ukg"   -> "UK Golnik", "#7B7226"
    | other -> other, "#000"

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
                    seq { // take few samples
                        data.[data.Length-1]
                        data.[data.Length-2]
                        data.[data.Length/2]
                    }
                    |> Seq.collect (fun stats -> stats.facilities |> Map.toSeq |> Seq.map fst) // hospital name
                    |> Seq.fold (fun hospitals hospital -> hospitals |> Set.add hospital) Set.empty // all
                    |> Set.toList
                    |> List.sort
                    |> List.map Facility
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
    State.initial, Cmd.OfAsync.either Data.Patients.fetch Data.Patients.url ConsumeServerData ConsumeServerError

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

let renderChart (state : State) =

    let renderLineLabel (input: ILabelProperties) =
        Html.text [
            prop.x(input.x)
            prop.y(input.y)
            prop.fill color.black
            prop.textAnchor.middle
            prop.dy(-10)
            prop.fontSize 10
            prop.text input.value
        ]

    let zeroToNone value =
        match value with
        | None -> None
        | Some x ->
            if x = 0 then None
            else Some x

    let renderSeries series =
        let dash, dataKey : (int[] * (Data.Patients.PatientsStats -> int option)) =
            match series with
            | InCare        -> [|1;1|], fun ps -> ps.total.inCare |> zeroToNone
            | OutOfHospital -> [|4;1|], fun ps -> ps.total.outOfHospital.toDate |> zeroToNone
            | InHospital    -> [||], fun ps -> ps.total.inHospital.today |> zeroToNone
            | NeedsO2       -> [|1;1|], fun ps -> ps.total.needsO2.toDate |> zeroToNone
            | Icu           -> [||], fun ps -> ps.total.icu.today |> zeroToNone
            | Critical      -> [|1;1|], fun ps -> ps.total.critical.today |> zeroToNone
            | Deceased      -> [|4;1|], fun ps -> ps.total.deceased.toDate |> zeroToNone
            | Hospital      -> [||], fun ps -> failwithf "home & hospital"
            | Home          -> [||], fun ps -> failwithf "home & totals"

        Recharts.line [
            line.name (series |> Series.getName)
            line.monotone
            line.isAnimationActive false
            line.stroke (series |> Series.getColor)
            line.strokeWidth 2
            line.strokeDasharray dash
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let renderSources segmentation =
        let facility, dataKey =
            match segmentation with
            | Totals -> "Skupaj", fun ps -> ps.total.inHospital.today |> zeroToNone
            | Facility f ->
                f, fun ps ->
                    ps.facilities
                    |> Map.tryFind f
                    |> Option.bind (fun stats -> stats.inHospital.today)
                    |> zeroToNone

        let name, color = facilityLine facility

        Recharts.line [
            line.name name
            line.monotone
            line.isAnimationActive false
            line.stroke color
            line.strokeWidth 2
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let children =
        let formatDate (d: Data.Patients.PatientsStats) = sprintf "%d.%d." d.day d.month
        seq {
            yield Recharts.xAxis [ xAxis.dataKey formatDate; xAxis.padding (0,10,0,0); xAxis.interval 0 ]

            yield Recharts.legend [ line.legendType.circle; ]

            let yAxisPropsDefaut = [ yAxis.padding (16,0,0,0) ]
            match state.scaleType with
            | Log ->
                yield Recharts.yAxis (yAxisPropsDefaut @ [
                    yAxis.scale ScaleType.Log
                    //yAxis.domain (domain.auto, domain.calculate (fun (max:float) -> max*1.1))
                    yAxis.domain (domain.auto, domain.auto)
                    yAxis.allowDataOverflow false ])
            | _ ->
                yield Recharts.yAxis yAxisPropsDefaut

            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            match state.breakdown with
            | BySeries ->
                for segmentation in state.allSegmentations do // |> Seq.filter (fun s -> Set.contains s state.activeSegmentations) do
                    for series in state.allSeries |> Seq.filter (fun s -> Set.contains s state.activeSeries) do
                        yield renderSeries series
            | BySource ->
                for segmentation in state.allSegmentations do // |> Seq.filter (fun s -> Set.contains s state.activeSegmentations) do
                    yield renderSources segmentation
        }

    Recharts.lineChart [
        lineChart.data state.data
        lineChart.children (Seq.toList children)
    ]

let renderChartContainer state =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 450
        responsiveContainer.chart (renderChart state)
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
