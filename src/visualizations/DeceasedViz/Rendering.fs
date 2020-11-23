module DeceasedViz.Rendering

open System
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
    | DeceasedAcute
    | DeceasedCare
    | DeceasedOther

module Series =
    let all =
        [ DeceasedOther; DeceasedCare; DeceasedAcute; DeceasedInIcu ]

    let getSeriesInfo = function
        | DeceasedInIcu  -> true,  "#6d5b80",   "deceased-icu"
        | DeceasedAcute  -> true,  "#8c71a8",   "deceased-acute"
        | DeceasedCare   -> true,  "#a483c7",   "deceased-care"
        | DeceasedOther  -> true,  "#c59eef",   "deceased-rest"

let init() : State * Cmd<Msg> =
    let state = {
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

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let total = pts |> Array.map (fun p -> p?point?y |> Utils.optionToInt) |> Array.sum
    let fmtDate = pts.[0]?point?fmtDate

    fmtDate
    + "<br>"
    + (pts
       |> Seq.map (fun p ->
           sprintf """<span style="color:%s">●</span> %s: <b>%s</b>""" 
                p?series?color p?series?name p?point?y)
       |> String.concat "<br>")
    + sprintf """<br><br><span style="color: rgba(0,0,0,0)">●</span> %s: <b>%s</b>""" 
        (I18N.t "charts.deceased.deceased-total") 
        (total |> string)

let renderChartOptions (state : State) dispatch =
    let className = "cases-chart"
    let scaleType = ScaleType.Linear

    let subtract (a : int option) (b : int option) =
        match a, b with
        | Some aa, Some bb -> Some (bb - aa)
        | Some aa, None -> -aa |> Some
        | None, Some _ -> b
        | _ -> None

    let renderSeries series =

        let getPoint dataPoint : int option =
            match series with
            | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
            | DeceasedAcute ->
                    dataPoint.total.deceased.hospital.toDate
                    |> subtract dataPoint.total.deceased.hospital.icu.toDate
            | DeceasedCare -> dataPoint.total.deceasedCare.toDate
            | DeceasedOther ->
                    dataPoint.total.deceased.toDate
                    |> subtract dataPoint.total.deceased.hospital.toDate
                    |> subtract dataPoint.total.deceasedCare.toDate

        let getPointTotal dataPoint : int option =
            match series with
            | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
            | DeceasedAcute -> dataPoint.total.deceased.hospital.toDate
            | DeceasedCare -> dataPoint.total.deceasedCare.toDate
            | DeceasedOther -> dataPoint.total.deceased.toDate

        let visible, color, seriesId = Series.getSeriesInfo series
        {|
            ``type`` = "column"
            visible = visible
            color = color
            name = I18N.tt "charts.deceased" seriesId
            data =
                state.PatientsData
                |> Seq.map (fun dataPoint ->
                    {|
                        x = dataPoint.Date |> jsTime12h
                        y = getPoint dataPoint
                        seriesId = seriesId
                        fmtDate = I18N.tOptions "days.longerDate"
                                      {| date = dataPoint.Date |}
                        fmtTotal = getPointTotal dataPoint |> string
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
                column = pojo
                        {|
                          groupPadding = 0
                          pointPadding = 0
                          borderWidth = 0 |}
                series = {| stacking = "normal"; crisp = true
                            borderWidth = 0
                            pointPadding = 0; groupPadding = 0
                            |}
            |}

        tooltip = pojo
            {|
                shared = true
                formatter = fun () -> tooltipFormatter jsThis
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

let renderChart() =
    React.elmishComponent("CasesChart", init(), update, render)
