
[<RequireQualifiedAccess>]
module PatientsChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Recharts

open Data.Patients

type Segmentation =
    | Totals
    | Hospital of string

type SegmentationCfg = {
    target: Segmentation
    visible: bool
    color: string
}

let segmentation = [{
    target = Totals
    visible = true
    color = "#808080"
}]

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
        | InCare -> "#ffa600"
        | OutOfHospital -> "#159ab0"
        | InHospital -> "#70a471"
        | NeedsO2 -> "#70a471"
        | Icu -> "#8080A0"
        | Critical -> "#802020"
        | Deceased -> "#000000"
        | Hospital -> "#0a6b85"
        | Home -> "#003f5c"

    let getName = function
        | InCare -> "Oskrbovani"
        | OutOfHospital -> "Iz bol. oskrbe (vsi)"
        | InHospital -> "V bol. oskrbi"
        | NeedsO2 -> "Potrebuje kisik"
        | Icu -> "Intenzivna nega"
        | Critical -> "Kritično stanje (ocena)"
        | Deceased -> "Umrli (vsi)"
        | Hospital -> "Hospitalizirani"
        | Home -> "Doma"

type State = {
    scaleType : ScaleType
    data : PatientsStats []
    error: string option
    allSegmentations: Segmentation list
    activeSegmentations: Set<Segmentation>
    allSeries: Series list
    activeSeries: Set<Series>
  } with
    static member initial = {
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
    }

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

let init () : State * Cmd<Msg> =
    State.initial, Cmd.OfAsync.either Data.Patients.fetch Data.Patients.url ConsumeServerData ConsumeServerError

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeServerData (Ok data) ->
        { state with data = data }, Cmd.none
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

    let renderSeries segmentation series =
        let dataKey : Data.Patients.PatientsStats -> int =
            let orZero = Option.defaultValue 0
            match segmentation with
            | Totals ->
                match series with
                | InCare -> fun ps -> ps.total.inCare |> orZero
                | OutOfHospital -> fun ps -> ps.total.outOfHospital.toDate |> orZero
                | InHospital -> fun ps -> ps.total.inHospital.today |> orZero
                | NeedsO2 -> fun ps -> ps.total.needsO2.toDate |> orZero
                | Icu -> fun ps -> ps.total.icu.today |> orZero
                | Critical -> fun ps -> ps.total.critical.today |> orZero
                | Deceased -> fun ps -> ps.total.deceased.toDate |> orZero
                | Hospital ->fun ps -> failwithf "home & hospital"
                | Home -> fun ps -> failwithf "home & totals"
            | _ -> failwithf "not implemented"

        Recharts.line [
            line.name (series |> Series.getName)
            line.monotone
            line.isAnimationActive false
            line.stroke (series |> Series.getColor)
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let children =
        let formatDate (d: Data.Patients.PatientsStats) = sprintf "%d.%d" d.day d.month
        seq {
            yield Recharts.xAxis [ xAxis.dataKey formatDate ]

            //let yAxisPropsDefaut = [ yAxis.label {| value = "Število pacientov" ; angle = -90 ; position = "insideLeft" |} ]
            let yAxisPropsDefaut = [ ]

            match state.scaleType with
            | Log ->
                yield Recharts.yAxis (yAxisPropsDefaut @ [yAxis.scale ScaleType.Log ; yAxis.domain (domain.auto, domain.auto) ])
            | _ ->
                yield Recharts.yAxis yAxisPropsDefaut

            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            for segmentation in state.allSegmentations |> Seq.filter (fun s -> Set.contains s state.activeSegmentations) do
                for series in state.allSeries |> Seq.filter (fun s -> Set.contains s state.activeSeries) do
                    yield renderSeries segmentation series
        }

    Recharts.lineChart [
        lineChart.data state.data
        lineChart.children (Seq.toList children)
    ]

let renderChartContainer state =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 500
        responsiveContainer.chart (renderChart state)
    ]

let render (state : State) dispatch =
    match state.data, state.error with
    | [||], None -> Html.div [ prop.text "loading" ]
    | _, Some err -> Html.div [ prop.text err ]
    | data, None ->
        Html.div [
            //Utils.renderScaleSelector state.scaleType (ScaleTypeChanged >> dispatch)
            renderChartContainer state
            //renderMetricsSelectors state.Metrics dispatch
        ]

let patientsChart () =
    React.elmishComponent("RegionsChart", init (), update, render)
