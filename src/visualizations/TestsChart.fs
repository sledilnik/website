[<RequireQualifiedAccess>]
module TestsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Types
open Highcharts


type DisplayType =
    | Total
    | Regular
    | SurveyApr2020
with
    static member all = [ Total; Regular; SurveyApr2020 ]
    static member getName = function
        | Total -> "Skupaj"
        | Regular -> "Redno"
        | SurveyApr2020 -> "Raziskava"

type State = {
    data: StatsData
    displayType: DisplayType
}

type Msg =
    | ChangeDisplayType of DisplayType

let init data : State * Cmd<Msg> =
    let state = {
        data = data
        displayType = Total
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType dt ->
        { state with displayType = dt }, Cmd.none

let renderChartOptions (state : State) =
    let className = "tests-chart"
    let scaleType = ScaleType.Linear

    let negativeTests (dp: StatsDataPoint) = dp.PerformedTests.Value - dp.PositiveTests.Value
    let percentPositive (dp: StatsDataPoint) = Math.Round(float dp.PositiveTests.Value / float dp.PerformedTests.Value * float 100.0, 2)
    
    let allYAxis = [|
        {|
            index = 0
            title = {| text = null |} 
            labels = pojo {| format = "{value}" |}
            opposite = true
            visible = true
            max = None
        |}
        {|
            index = 1
            title = {| text = null |} 
            labels = pojo {| format = "{value}%" |}
            opposite = false
            visible = true
            max = Some 15
        |}
    |]

    let startDate = DateTime(2020,3,4)
    let allSeries = [
        yield pojo
            {|
                name = "Negativnih testov (na dan)"
                ``type`` = "column"
                color = "#19aebd"
                yAxis = 0
                data = state.data |> Seq.skipWhile (fun dp -> dp.Date < startDate) 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, negativeTests dp)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Pozitivnih testov (na dan)"
                ``type`` = "column"
                color = "#bda506"
                yAxis = 0 
                data = state.data |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.PositiveTests)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Delež pozitivnih testov (%)"
                ``type`` = "line"
                color = "#665191"
                yAxis = 1
                data = state.data |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, percentPositive dp)) |> Seq.toArray
            |}
    ]
    
    let baseOptions = Highcharts.basicChartOptions scaleType className
    {| baseOptions with
        yAxis = allYAxis
        series = List.toArray allSeries
        plotOptions = pojo 
            {| 
                series = {| stacking = "normal" |} 
            |}        

        legend = pojo
            {|
                enabled = true
                title = {| text = null |}
                align = "left"
                verticalAlign = "top"
                x = 60
                y = 30
                borderColor = "#ddd"
                borderWidth = 1
                layout = "vertical"
                floating = true
                backgroundColor = "#FFF"
            |}

        responsive = pojo 
            {|
                rules = 
                    [| {|
                        condition = {| maxWidth = 500 |}
                        chartOptions = 
                            {| 
                                legend = {| enabled = false |}
                                yAxis = [| 
                                    {| labels = {| enabled = false |} |} 
                                    {| labels = {| enabled = false |} |} 
                                |]
                            |}
                    |} |]
            |}
 
    |}

let renderChartContainer (state : State) =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state
            |> Highcharts.chart
        ]
    ]

let renderSelector state (dt: DisplayType) dispatch =
    Html.div [
        let isActive = state.displayType = dt
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        prop.className [ true, "btn btn-sm metric-selector"; isActive, "metric-selector--selected" ]
        prop.text (DisplayType.getName dt) ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.all
            |> List.map (fun dt -> renderSelector state dt dispatch) ) ]

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state
        renderDisplaySelectors state dispatch
    ]

let testsChart (props : {| data : StatsData |}) =
    React.elmishComponent("TestsChart", init props.data, update, render)

