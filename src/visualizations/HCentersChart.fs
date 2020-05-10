[<RequireQualifiedAccess>]
module HCentersChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Types
open Data.HCenters
open Highcharts


type State = {
    hcData : HcStats []
    error: string option
  } with
    static member initial =
        {
            hcData = [||]
            error = None
        }

type Msg =
    | ConsumeHcData of Result<HcStats [], string>
    | ConsumeServerError of exn

let init () : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either Data.HCenters.getOrFetch () ConsumeHcData ConsumeServerError
    State.initial, (cmd)

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeHcData (Ok data) ->
        { state with hcData = data }, Cmd.none
    | ConsumeHcData (Error err) ->
        { state with error = Some err }, Cmd.none

    | ConsumeServerError ex ->
        { state with error = Some ex.Message }, Cmd.none

let renderChartOptions (state : State) =
    let className = "hcenters-chart"
    let scaleType = ScaleType.Linear
    let startDate = DateTime(2020,3,18)
    let mutable startTime = startDate |> jsTime


    let allSeries = [
        yield pojo
            {|
                name = "Sumov (pregled)"
                ``type`` = "line"
                color = "#10829a"
                dashStyle = Dot |> DashStyle.toString
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.examinations.suspectedCovid)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Sumov (telefonsko)"
                ``type`` = "line"
                color = "#024a66"
                dashStyle = Dot |> DashStyle.toString
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.phoneTriage.suspectedCovid)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Napotitev v samo-izolacijo"
                ``type`` = "line"
                color = "#665191"
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.sentTo.selfIsolation)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Testov (opravljenih)"
                ``type`` = "line"
                color = "#19aebd"
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.tests.performed)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Testov (pozitivnih)"
                ``type`` = "line"
                color = "#d5c768"
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.tests.positive)) |> Seq.toArray
            |}
        yield addContainmentMeasuresFlags startTime None |> pojo

    ]
    
    let baseOptions = Highcharts.basicChartOptions scaleType className
    {| baseOptions with
        series = List.toArray allSeries

        legend = pojo
            {|
                enabled = true
                title = {| text = null |}
                align = "right"
                verticalAlign = "top"
                x = -80
                y = 30
                borderColor = "#ddd"
                borderWidth = 1
                layout = "vertical"
                floating = true
                backgroundColor = "#FFF"
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

let render (state : State) dispatch =
    match state.hcData, state.error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state
        ]

let hCentersChart () =
    React.elmishComponent("HCentersChart", init (), update, render)
