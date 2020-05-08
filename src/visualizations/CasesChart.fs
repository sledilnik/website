[<RequireQualifiedAccess>]
module CasesChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop

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
        | Deceased      -> "#666666",   "cs-deceased",      "Umrli"
        | Recovered     -> "#8cd4b2",   "cs-recovered",     "Preboleli"
        | Active        -> "#d5c768",   "cs-active",        "Aktivni"
        | InHospital    -> "#be7a2a",   "cs-inHospital",    "Hospitalizirani"
        | Icu           -> "#d99a91",   "cs-inHospitalICU", "V intenzivni enoti"
        | Critical      -> "#bf5747",   "cs-critical",      "Na respiratorju"

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

let legendFormatter jsThis =
    let pts: obj[] = jsThis?points
    let fmtDate = pts.[0]?point?fmtDate

    let mutable fmtUnder = ""
    let mutable fmtStr = sprintf "<b>%s</b>" fmtDate
    for p in pts do 
        match p?point?fmtTotal with
        | "null" -> ()
        | _ -> 
            fmtStr <- fmtStr + sprintf """<br>%s<span style="color:%s">⬤</span> %s: <b>%s</b>""" 
                fmtUnder
                p?series?color
                p?series?name
                p?point?fmtTotal             
            match p?series?name with
            | "Aktivni" | "Hospitalizirani" | "V intenzivni enoti"  -> fmtUnder <- fmtUnder + "↳ "
            | _ -> ()

    fmtStr

type TwoW = { Key: DateTime; Active: int; Recovered: int }

let renderChartOptions (state : State) =
    let className = "cases-chart"
    let scaleType = ScaleType.Linear
    
    let subtract a b = b - a

    let twoWeeks =
        [
            DateTime(2020, 3, 4), 1, 0
            DateTime(2020, 3, 5), 6, 0
            DateTime(2020, 3, 6), 10, 0
            DateTime(2020, 3, 7), 16, 0
            DateTime(2020, 3, 8), 20, 0
            DateTime(2020, 3, 9), 31, 0
            DateTime(2020, 3, 10), 49, 0
            DateTime(2020, 3, 11), 82, 0
            DateTime(2020, 3, 12), 131, 0
            DateTime(2020, 3, 13), 179, 0
            DateTime(2020, 3, 14), 214, 0
            DateTime(2020, 3, 15), 249, 0
            DateTime(2020, 3, 16), 276, 0
            DateTime(2020, 3, 17), 286, 0
            DateTime(2020, 3, 18), 318, 0
            DateTime(2020, 3, 19), 336, 5
            DateTime(2020, 3, 20), 358, 9
            DateTime(2020, 3, 21), 390, 15
            DateTime(2020, 3, 22), 421, 19
            DateTime(2020, 3, 23), 446, 29
            DateTime(2020, 3, 24), 476, 46
            DateTime(2020, 3, 25), 489, 78
            DateTime(2020, 3, 26), 501, 126
            DateTime(2020, 3, 27), 505, 172
            DateTime(2020, 3, 28), 515, 206
            DateTime(2020, 3, 29), 506, 239
            DateTime(2020, 3, 30), 525, 266
            DateTime(2020, 3, 31), 554, 274
            DateTime(2020, 4, 1), 578, 304
            DateTime(2020, 4, 2), 592, 326
            DateTime(2020, 4, 3), 609, 348
            DateTime(2020, 4, 4), 591, 384
            DateTime(2020, 4, 5), 579, 413
            DateTime(2020, 4, 6), 578, 447
            DateTime(2020, 4, 7), 566, 489
            DateTime(2020, 4, 8), 553, 531
            DateTime(2020, 4, 9), 528, 589
            DateTime(2020, 4, 10), 504, 639
            DateTime(2020, 4, 11), 475, 680
            DateTime(2020, 4, 12), 456, 703
            DateTime(2020, 4, 13), 418, 747
            DateTime(2020, 4, 14), 406, 785
            DateTime(2020, 4, 15), 371, 836
            DateTime(2020, 4, 16), 370, 873
            DateTime(2020, 4, 17), 340, 911
            DateTime(2020, 4, 18), 333, 927
            DateTime(2020, 4, 19), 315, 946
            DateTime(2020, 4, 20), 289, 978
            DateTime(2020, 4, 21), 262, 1014
            DateTime(2020, 4, 22), 242, 1045
            DateTime(2020, 4, 23), 213, 1081
            DateTime(2020, 4, 24), 200, 1108
            DateTime(2020, 4, 25), 191, 1123
            DateTime(2020, 4, 26), 190, 1130
            DateTime(2020, 4, 27), 189, 1136
            DateTime(2020, 4, 28), 171, 1161
            DateTime(2020, 4, 29), 161, 1179
            DateTime(2020, 4, 30), 130, 1213
            DateTime(2020, 5, 1), 122, 1224
            DateTime(2020, 5, 2), 109, 1236
            DateTime(2020, 5, 3), 104, 1239
            DateTime(2020, 5, 4), 101, 1247
            DateTime(2020, 5, 5), 95, 1255
            DateTime(2020, 5, 6), 83, 1267
        ]
        |> List.map (fun (date,  active,  recovered) ->
            date, { Key = date;  Active = active;  Recovered = recovered })
        |> Map.ofList

    let renderSeries series =

        let getPoint : (StatsDataPoint -> int option) =
            match series with
            | Recovered     -> fun dp -> Some twoWeeks.[dp.Date].Recovered    
            | Deceased      -> fun dp -> dp.StatePerTreatment.DeceasedToDate
            | Active        -> fun dp -> twoWeeks.[dp.Date].Active |> subtract dp.StatePerTreatment.InHospital.Value |> Some
            | InHospital    -> fun dp -> dp.StatePerTreatment.InHospital.Value |> subtract dp.StatePerTreatment.InICU.Value |> Some 
            | Icu           -> fun dp -> dp.StatePerTreatment.InICU.Value |> subtract dp.StatePerTreatment.Critical.Value |> Some 
            | Critical      -> fun dp -> dp.StatePerTreatment.Critical

        let getPointTotal : (StatsDataPoint -> int option) =
            match series with
            | Recovered     -> fun dp -> Some twoWeeks.[dp.Date].Recovered    
            | Deceased      -> fun dp -> dp.StatePerTreatment.DeceasedToDate
            | Active        -> fun dp -> Some twoWeeks.[dp.Date].Active
            | InHospital    -> fun dp -> dp.StatePerTreatment.InHospital
            | Icu           -> fun dp -> dp.StatePerTreatment.InICU
            | Critical      -> fun dp -> dp.StatePerTreatment.Critical

        let color, className, name = Series.getSeriesInfo series
        {|
            ``type`` = "column"
            color = color
            name = name
            data =
                state.data
                |> Seq.filter (fun dp -> dp.Cases.Active.IsSome)
                |> Seq.map (fun dp ->  
                    {|
                        x = dp.Date |> jsTime12h
                        y = getPoint dp
                        fmtDate = dp.Date.ToString "d. M. yyyy"
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
    
    let baseOptions = Highcharts.basicChartOptions scaleType className
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

