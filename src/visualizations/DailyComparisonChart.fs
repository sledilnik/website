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
    data: StatsData
    displayType: DisplayType
}

type Msg =
    | ChangeDisplayType of DisplayType

let init data : State * Cmd<Msg> =
    let state = {
        data = data
        displayType = New
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType dt ->
        { state with displayType = dt }, Cmd.none

type DataPoint =
    { x : string
      y : int
      date : DateTime
    }

let renderChartOptions (state : State) dispatch =
    let startDate = DateTime.Today.AddDays(-28.0)

    // TODO: ugly hack to get DayOfWeek to integer in F#
    let getDOW (date: DateTime) =
        match date.DayOfWeek with
        | DayOfWeek.Sunday -> I18N.dow 0
        | DayOfWeek.Monday -> I18N.dow 1
        | DayOfWeek.Tuesday -> I18N.dow 2
        | DayOfWeek.Wednesday -> I18N.dow 3
        | DayOfWeek.Thursday -> I18N.dow 4
        | DayOfWeek.Friday -> I18N.dow 5
        | DayOfWeek.Saturday -> I18N.dow 6
        | _ -> ""

    let getValue dp =
        match state.displayType with
        | New -> dp.Cases.ConfirmedToday |> Option.defaultValue 0
        | Active -> dp.Cases.Active |> Option.defaultValue 0
        | Tests -> dp.Tests.Performed.Today |> Option.defaultValue 0

    let fourWeeks = 
        state.data 
        |> Seq.skipWhile (fun dp -> dp.Date < startDate)
        |> Seq.map (fun dp -> (dp.Date, getValue dp)) 
        |> Seq.toArray

    let allSeries = [
        for i in 0..3 do    
            let idx = i * 7
            let len = min 7 (fourWeeks.Length - idx)

            let desaturate (rgb:string) (sat:float) = 
                let argb = Int32.Parse (rgb.Replace("#", ""), Globalization.NumberStyles.HexNumber)
                let r = (argb &&& 0x00FF0000) >>> 16
                let g = (argb &&& 0x0000FF00) >>> 8
                let b = (argb &&& 0x000000FF)
                let avg = float(r + g + b) / 3.0
                let newR = int (Math.Round (float(r) * sat + avg * (1.0 - sat)))
                let newG = int (Math.Round (float(g) * sat + avg * (1.0 - sat)))
                let newB = int (Math.Round (float(b) * sat + avg * (1.0 - sat)))
                sprintf "#%02x%02x%02x" newR newG newB

            let color =
                match state.displayType with
                | New -> desaturate "#bda506" (0.25 + float i / 4.0)
                | Active -> desaturate "#dba51d" (0.25 + float i / 4.0)
                | Tests -> desaturate "#19aebd" (0.25 + float i / 4.0)

            yield pojo
                {|
                    ``type`` = "column"
                    color = color
                    data = 
                        Array.sub fourWeeks idx len 
                        |> Array.map (fun (date, value) ->
                            pojo {|
                                 //x = date.DayOfWeek
                                 y = value
                                 date = I18N.tOptions "days.date" {| date = date |}
                            |}
                        )
                |}
    ]

    let tooltipFormatter state jsThis =
        let category = jsThis?x
        let pts: obj[] = jsThis?points
        let fmtDate = pts.[0]?fmtDate

        let mutable fmtStr = sprintf "<b>%s</b><br>%s<br>" (DisplayType.getName state.displayType) category
        let mutable fmtLine = ""
        fmtStr <- fmtStr + "<table>"
        for p in pts do
            match p?point?date with
            | "null" -> ()
            | _ ->
                fmtLine <- sprintf "<tr><td><span style='color:%s'>●</span></td><td>%s</td><td style='text-align: right; padding-left: 10px'>%d</td></tr>"
                    p?series?color
                    p?point?date
                    p?point?y
                fmtStr <- fmtStr + fmtLine
        fmtStr <- fmtStr + "</table>"
        fmtStr

    {| optionsWithOnLoadEvent "covid19-daily-comparison" with
        chart = pojo {| ``type`` = "column" |}
        title = pojo {| text = None |}
        xAxis = [| 
            {|
                ``type`` = "category"
                categories = [| I18N.dow 0; I18N.dow 1; I18N.dow 2; I18N.dow 3; I18N.dow 4; I18N.dow 5; I18N.dow 6; |]
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
        let isActive = state.displayType = dt
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
