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
                name = "Nujna medicinska pomoč (skupaj)"
                ``type`` = "line"
                color = "#70a471"
                dashStyle = Dot |> DashStyle.toString
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.examinations.medicalEmergency)) |> Seq.toArray
            |}

        yield pojo
            {|
                name = "Sum COVID-19 (pregled)"
                ``type`` = "line"
                color = "#a05195"
                dashStyle = Dot |> DashStyle.toString
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.examinations.suspectedCovid)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Sum COVID-19 (telefonsko)"
                ``type`` = "line"
                color = "#d45087"
                dashStyle = Dot |> DashStyle.toString
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.phoneTriage.suspectedCovid)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Napotitev v samoizolacijo"
                ``type`` = "line"
                color = "#665191"
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.sentTo.selfIsolation)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Test (opravljenih)"
                ``type`` = "line"
                color = "#19aebd"
                data = state.hcData 
                    |> Seq.map (fun dp -> (dp.Date |> jsTime12h, dp.all.tests.performed)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = "Test (pozitivnih*)"
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

        // need to hide negative label for addContainmentMeasuresFlags
        yAxis = baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = false |})

        legend = pojo {| enabled = true ; layout = "horizontal" |}
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

            Html.div [
                prop.className "disclaimer"
                prop.children [
                    Html.text "Opomba: omejitve prikazanih podatkov so razložene v "
                    Html.a
                        [ prop.className "faq-link"
                          prop.target "_blank"
                          prop.href "/FAQ/#hcenters-chart"
                          prop.text "FAQ" ]
                ]
            ]
        ]


let hCentersChart () =
    React.elmishComponent("HCentersChart", init (), update, render)
