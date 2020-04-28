[<RequireQualifiedAccess>]
module CasesChart

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

type Series =
    | Recovered
    | Active
    | InHospital
    | Icu
    | Critical
    | Deceased

module Series =
    let all =
        [ Recovered; Active; InHospital; Icu; Critical; Deceased; ]

    let getSeriesInfo = function
        | Recovered     -> "#8cd4b2",   "cs-recovered",     "Preboleli"
        | Active        -> "#bda506",   "cs-active",        "Aktivni"
        | InHospital    -> "#be7a2a",   "cs-inHospital",    "Hospitalizirani"
        | Icu           -> "#bf5747",   "cs-inHospitalICU", "V intenzivni enoti"
        | Critical      -> "#d99a91",   "cs-critical",      "Na respiratorju"
        | Deceased      -> "#666666",   "cs-deceased",      "Umrli"

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
    let className = "cases-chart"
    let scaleType = ScaleType.Linear
    
    let startDate = DateTime(2020,3,4)
    let subtract a b = b - a

    let renderSeries series =

        let renderPoint : (StatsDataPoint -> JsTimestamp * int option) =
            match series with
            | Recovered     -> fun dp -> dp.Date |> jsTime12h, dp.Cases.ClosedToDate  // todo   
            | Active        -> fun dp -> dp.Date |> jsTime12h, dp.Cases.ActiveToDate.Value |> subtract dp.StatePerTreatment.InHospital.Value |> Some // todo rename  
            | InHospital    -> fun dp -> dp.Date |> jsTime12h, dp.StatePerTreatment.InHospital.Value |> subtract dp.StatePerTreatment.InICU.Value |> Some  
            | Icu           -> fun dp -> dp.Date |> jsTime12h, dp.StatePerTreatment.InICU.Value |> subtract dp.StatePerTreatment.Critical.Value |> Some
            | Critical      -> fun dp -> dp.Date |> jsTime12h, dp.StatePerTreatment.Critical
            | Deceased      -> fun dp -> dp.Date |> jsTime12h, dp.StatePerTreatment.Deceased

        let color, className, name = Series.getSeriesInfo series

        {|
            ``type`` = "area"
            color = color
            name = name
            //className = className
            data =
                state.data
                |> Seq.skipWhile (fun dp -> dp.Date < startDate)
                |> Seq.map renderPoint
                |> Array.ofSeq
        |}
        |> pojo

    let allSeries = [|
        for series in Series.all do
            yield renderSeries series
    |]
    
    let baseOptions = Highcharts.basicChartOptions scaleType className
    {| baseOptions with
        series = allSeries
        plotOptions = pojo 
            {| 
                series = {| stacking = "normal"; marker = pojo {| symbol = "circle"; radius = 3 |} |} 
            |}        

        legend = pojo
            {|
                enabled = true
                title = {| text = null |}
                align = "left"
                verticalAlign = "top"
                x = 10
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

let render (state: State) dispatch =
    Html.div [
        renderChartContainer state
    ]

let casesChart (props : {| data : StatsData |}) =
    React.elmishComponent("CasesChart", init props.data, update, render)

