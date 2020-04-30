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
        | Active        -> "#bda506",   "cs-active",        "Aktivni"
        | InHospital    -> "#be7a2a",   "cs-inHospital",    "Hospitalizirani"
        | Icu           -> "#bf5747",   "cs-inHospitalICU", "V intenzivni enoti"
        | Critical      -> "#d99a91",   "cs-critical",      "Na respiratorju"

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
    sprintf """%s<br><span style="color:%s">⬤</span> %s: <b>%s</b><br>
                 <br><span style="color:%s">⬤</span> %s: <b>%s</b><br>
                 <br><span style="color:%s">⬤</span> %s: <b>%s</b><br>
                 <br>↳ <span style="color:%s">⬤</span> %s: <b>%s</b><br>
                 <br>↳ <span style="color:%s">⬤</span> %s: <b>%s</b><br>
                 <br>↳ <span style="color:%s">⬤</span> %s: <b>%s</b><br>"""
        pts.[0]?point?fmtDate
        pts.[0]?series?color 
        pts.[0]?series?name 
        pts.[0]?point?fmtTotal
        pts.[1]?series?color 
        pts.[1]?series?name 
        pts.[1]?point?fmtTotal
        pts.[2]?series?color 
        pts.[2]?series?name 
        pts.[2]?point?fmtTotal
        pts.[3]?series?color 
        pts.[3]?series?name 
        pts.[3]?point?fmtTotal
        pts.[4]?series?color 
        pts.[4]?series?name 
        pts.[4]?point?fmtTotal
        pts.[5]?series?color 
        pts.[5]?series?name 
        pts.[5]?point?fmtTotal

let renderChartOptions (state : State) =
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
                series = {| stacking = "normal"; groupPadding = 0 |}
            |}        
            
        tooltip = pojo
            {|
                shared = true
                formatter = fun () -> legendFormatter jsThis
            |}

(*        tooltip = pojo 
           {| shared = true
              formatter = fun () ->
                // Available data are: https://api.highcharts.com/highcharts/tooltip.formatter
                // this.percentage (not shared) / this.points[i].percentage (shared): Stacked series and pies only. The point's percentage of the total.
                // this.point (not shared) / this.points[i].point (shared): The point object. The point name, if defined, is available through this.point.name.
                // this.points: In a shared tooltip, this is an array containing all other properties for each point.
                // this.series (not shared) / this.points[i].series (shared): The series object. The series name is available through this.series.name.
                // this.total (not shared) / this.points[i].total (shared): Stacked series only. The total value at this point's x value.
                // this.x: The x value. This property is the same regardless of the tooltip being shared or not.
                // this.y (not shared) / this.points[i].y (shared): The y value.           |}
                let dt = DateTime(jsThis?x)
                dt.ToShortDateString
                
            |}            
*)
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

