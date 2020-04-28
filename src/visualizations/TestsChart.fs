[<RequireQualifiedAccess>]
module TestsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Types
open Highcharts

type DisplayType =
    | MultiChart

type State = {
    data: StatsData
    displayType: DisplayType
}

type Msg =
    | ChangeDisplayType of DisplayType

let init data : State * Cmd<Msg> =
    let state = {
        data = data
        displayType = MultiChart
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType rt ->
        { state with displayType=rt }, Cmd.none

let renderChartOptions (state : State) =
    let className = "tests-chart"
    let scaleType = ScaleType.Linear

    let xAxisPoint (dp: StatsDataPoint) = dp.Date
    let negativeTests (dp: StatsDataPoint) = dp.PerformedTests.Value - dp.PositiveTests.Value
    let percentPositive (dp: StatsDataPoint) = Math.Round(float dp.PositiveTests.Value / float dp.PerformedTests.Value * float 100.0, 2)
    
    let allYAxis = [|
        {|
            index = 0
            title = {| text = null |} 
            opposite = true
            visible = true
            max = None
        |}
        {|
            index = 1
            title = {| text = null |} 
            opposite = false
            visible = false
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
                    |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime12h, negativeTests dp)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Pozitivnih testov (na dan)"
                ``type`` = "column"
                color = "#bda506"
                yAxis = 0 
                data = state.data |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                    |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime12h, dp.PositiveTests)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Delež pozitivnih testov (%)"
                ``type`` = "line"
                color = "#665191"
                yAxis = 1
                data = state.data |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                    |> Seq.map (fun dp -> (xAxisPoint dp |> jsTime12h, percentPositive dp)) |> Seq.toArray
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

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state
    ]

let testsChart (props : {| data : StatsData |}) =
    React.elmishComponent("TestsChart", init props.data, update, render)

