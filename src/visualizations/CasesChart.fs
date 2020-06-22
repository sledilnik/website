[<RequireQualifiedAccess>]
module CasesChart

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
    displayType: DisplayType
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

type Series =
    | Deceased
    | Recovered
    | Active
    | InHospital
    | Icu
    | Critical

module Series =
    let all =
        [ Deceased; Recovered; Active; InHospital; Icu; Critical; ]
    let active =
        [ Active; InHospital; Icu; Critical; ]
    let inHospital =
        [ InHospital; Icu; Critical; ]
    let closed =
        [ Deceased; Recovered;  ]

    let getSeriesInfo = function
        | Deceased      -> "#666666",   "deceased"
        | Recovered     -> "#8cd4b2",   "recovered"
        | Active        -> "#d5c768",   "active"
        | InHospital    -> "#de9a5a",   "hospitalized"
        | Icu           -> "#d99a91",   "icu"
        | Critical      -> "#bf5747",   "ventilator"

let init data : State * Cmd<Msg> =
    let state = {
        data = data
        displayType = MultiChart
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
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
            fmtStr <- fmtStr + sprintf """<br>%s<span style="color:%s">●</span> %s: <b>%s</b>"""
                fmtUnder
                p?series?color
                p?series?name
                p?point?fmtTotal
            match p?point?seriesId with
            | "active" | "hospitalized" | "icu"  -> fmtUnder <- fmtUnder + "↳ "
            | _ -> ()

    fmtStr

let renderChartOptions (state : State) dispatch =
    let className = "cases-chart"
    let scaleType = ScaleType.Linear

    let subtract a b = b - a

    let renderSeries series =

        let getPoint : (StatsDataPoint -> int option) =
            match series with
            | Recovered     -> fun dp -> dp.Cases.RecoveredToDate
            | Deceased      -> fun dp -> dp.StatePerTreatment.DeceasedToDate
            | Active        -> fun dp -> dp.Cases.Active.Value |> subtract dp.StatePerTreatment.InHospital.Value |> Some
            | InHospital    -> fun dp -> dp.StatePerTreatment.InHospital.Value |> subtract dp.StatePerTreatment.InICU.Value |> Some
            | Icu           -> fun dp -> dp.StatePerTreatment.InICU.Value |> subtract dp.StatePerTreatment.Critical.Value |> Some
            | Critical      -> fun dp -> dp.StatePerTreatment.Critical

        let getPointTotal : (StatsDataPoint -> int option) =
            match series with
            | Recovered     -> fun dp -> dp.Cases.RecoveredToDate
            | Deceased      -> fun dp -> dp.StatePerTreatment.DeceasedToDate
            | Active        -> fun dp -> dp.Cases.Active
            | InHospital    -> fun dp -> dp.StatePerTreatment.InHospital
            | Icu           -> fun dp -> dp.StatePerTreatment.InICU
            | Critical      -> fun dp -> dp.StatePerTreatment.Critical

        let color, seriesid = Series.getSeriesInfo series
        {|
            ``type`` = "column"
            color = color
            name = I18N.tt "charts.cases" seriesid
            data =
                state.data
                |> Seq.filter (fun dp -> dp.Cases.Active.IsSome)
                |> Seq.map (fun dp ->
                    {|
                        x = dp.Date |> jsTime12h
                        y = getPoint dp
                        seriesId = seriesid
                        fmtDate = I18N.tOptions "days.longerDate" {| date = dp.Date |}
                        fmtTotal = getPointTotal dp |> string
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
        Highcharts.basicChartOptions
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
            |> Highcharts.chartFromWindow
        ]
    ]

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state dispatch
    ]

let casesChart (props : {| data : StatsData |}) =
    React.elmishComponent("CasesChart", init props.data, update, render)
