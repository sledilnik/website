[<RequireQualifiedAccess>]
module DailyComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Types
open Highcharts

type DisplayType =
    | Active
    | New
    | Tests
with
    static member all = [ New; Active; Tests;  ]
    static member getName = function
        | New -> I18N.t "charts.dailyComparison.new"
        | Active -> I18N.t "charts.dailyComparison.active"
        | Tests -> I18N.t "charts.dailyComparison.tests"

type State = {
    Data: StatsData
    DisplayType: DisplayType
}

type Msg =
    | ChangeDisplayType of DisplayType

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        DisplayType = New
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType dt ->
        { state with DisplayType = dt }, Cmd.none

let renderChartOptions (state : State) dispatch =

    let weeksShown = 6

    let getValue dp =
        match state.DisplayType with
        | New -> dp.Cases.ConfirmedToday
        | Active -> dp.Cases.Active
        | Tests -> dp.Tests.Performed.Today

    let fourWeeks = 
        state.Data 
        |> Seq.skipWhile (fun dp -> dp.Date < DateTime.Today.AddDays(float (- weeksShown * 7)))
        |> Seq.skipWhile (fun dp -> dp.Date.DayOfWeek <> DayOfWeek.Monday)
        |> Seq.map (fun dp -> (dp.Date, getValue dp)) 
        |> Seq.toArray

    let allSeries = [
        for weekIdx in 0 .. weeksShown-1 do    
            let idx = weekIdx * 7
            let len = min 7 (fourWeeks.Length - idx)

            let desaturateColor (rgb:string) (sat:float) = 
                let argb = Int32.Parse (rgb.Replace("#", ""), Globalization.NumberStyles.HexNumber)
                let r = (argb &&& 0x00FF0000) >>> 16
                let g = (argb &&& 0x0000FF00) >>> 8
                let b = (argb &&& 0x000000FF)
                let avg = float(r + g + b) / 3.0
                let newR = int (Math.Round (float(r) * sat + avg * (1.0 - sat)))
                let newG = int (Math.Round (float(g) * sat + avg * (1.0 - sat)))
                let newB = int (Math.Round (float(b) * sat + avg * (1.0 - sat)))
                sprintf "#%02x%02x%02x" newR newG newB

            let getSeriesColor dt series =
                match dt with
                | New -> desaturateColor "#bda506" (0.1 + float series / float (weeksShown+1))
                | Active -> desaturateColor "#dba51d" (0.1 + float series / float (weeksShown+1))
                | Tests -> desaturateColor "#19aebd" (0.1 + float series / float (weeksShown+1))

            let percent a b =
                match a, b with
                | Some v, Some p -> sprintf "%+0.0f%%" (float(v) / float(p) * 100.0 - 100.0)
                | _, _ -> ""

            yield pojo
                {|
                    ``type`` = "column"
                    color = getSeriesColor state.DisplayType weekIdx
                    data = 
                        fourWeeks
                        |> Array.skip idx
                        |> Array.take len 
                        |> Array.mapi (fun i (date, value) -> 
                            {|
                                y = value
                                date = I18N.tOptions "days.date" {| date = date |}
                                diff = 
                                    if weekIdx > 0
                                    then 
                                        let _ , prev = fourWeeks.[(weekIdx-1) * 7 + i]
                                        percent value prev 
                                    else ""
                                dataLabels = 
                                    if weekIdx = weeksShown-1 then pojo {| enabled = true |}
                                    else pojo {||}
                            |} |> pojo
                        )
                |}
    ]

    let tooltipFormatter state jsThis =
        let category = jsThis?x
        let pts: obj[] = jsThis?points

        let mutable fmtStr = sprintf "<b>%s</b><br>%s<br>" (DisplayType.getName state.DisplayType) category
        let mutable fmtLine = ""
        fmtStr <- fmtStr + "<table>"
        for p in pts do
            fmtLine <- sprintf "<tr><td><span style='color:%s'>●</span></td><td>%s</td><td style='text-align: right; padding-left: 10px'><b>%d</b></td><td style='text-align: right; padding-left: 10px'>%s</td></tr>"
                p?series?color
                p?point?date
                p?point?y
                p?point?diff
            fmtStr <- fmtStr + fmtLine
        fmtStr <- fmtStr + "</table>"
        fmtStr

    {| optionsWithOnLoadEvent "covid19-daily-comparison" with
        chart = pojo {| ``type`` = "column" |}
        title = pojo {| text = None |}
        xAxis = [| 
            {|
                ``type`` = "category"
                categories = [| I18N.dow 1; I18N.dow 2; I18N.dow 3; I18N.dow 4; I18N.dow 5; I18N.dow 6; I18N.dow 0; |]
            |} 
        |]
        yAxis = [|
            {|
                opposite = true
                title = {| text = null |}
            |}
        |]

        series = List.toArray allSeries

        legend = pojo {| enabled = false |}

        tooltip = pojo
            {|
                formatter = fun () -> tooltipFormatter state jsThis
                shared = true
                useHTML = true
            |}

        credits = pojo
            {|
                enabled = true
                text =
                    sprintf "%s: %s, %s"
                        (I18N.t "charts.common.dataSource")
                        (I18N.t "charts.common.dsNIJZ")
                        (I18N.t "charts.common.dsMZ")
                href = "https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19"
            |}

        navigator = pojo {| enabled = false |}
        scrollbar = pojo {| enabled = false |}
        rangeSelector = pojo {| enabled = false |}
    |}

let renderChartContainer (state : State) dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> Highcharts.chart
        ]
    ]

let renderSelector state (dt: DisplayType) dispatch =
    Html.div [
        let isActive = state.DisplayType = dt
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (isActive, "metric-selector--selected")]
        prop.text (DisplayType.getName dt) ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.all
            |> List.map (fun dt -> renderSelector state dt dispatch) ) ]

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state dispatch
        renderDisplaySelectors state dispatch
    ]

let dailyComparisonChart (props : {| data : StatsData |}) =
    React.elmishComponent("DailyComparisonChart", init props.data, update, render)
