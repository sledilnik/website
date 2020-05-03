module Highcharts

open System
open Fable.Core
open Fable.React
open Browser

open Types

[<Import("renderChart", from="./_highcharts")>]
let chart: obj -> ReactElement = jsNative

[<Import("renderMap", from="./_highcharts")>]
let map: obj -> ReactElement = jsNative

[<AutoOpen>]
module Helpers =
    // Plain-Old-Javascript-Object (i.e. box)
    let inline pojo o = JsInterop.toPlainJsObj o

    // plain old javascript object
    [<Emit """Array.prototype.slice.call($0)""">]
    let poja (a: 'T[]) : obj = jsNative

    type JsTimestamp = float
    [<Emit("$0.getTime()")>]
    let jsTime (x: DateTime): JsTimestamp = jsNative

    let jsNoon : JsTimestamp = 43200000.0
    let jsTime12h = jsTime >> ( + ) jsNoon

type DashStyle =
    | Solid
    | ShortDash
    | ShortDot
    | ShortDashDot
    | ShortDashDotDot
    | Dot
    | Dash
    | LongDash
    | DashDot
    | LongDashDot
    | LongDashDotDot
  with
    static member toString = function
        | Solid -> "Solid"
        | ShortDash -> "ShortDash"
        | ShortDot -> "ShortDot"
        | ShortDashDot -> "ShortDashDot"
        | ShortDashDotDot -> "ShortDashDotDot"
        | Dot -> "Dot"
        | Dash -> "Dash"
        | LongDash -> "LongDash"
        | DashDot -> "DashDot"
        | LongDashDot -> "LongDashDot"
        | LongDashDotDot -> "LongDashDotDot"


let shadedWeekendPlotBands =
    let saturday = DateTime(2020,02,22)
    let nWeeks = (DateTime.Today-saturday).TotalDays / 7.0 |> int
    let oneDay = 86400000.0
    let origin = jsTime saturday // - oneDay / 2.0
    [|
        for i in 0..nWeeks+2 do
            //yield {| value=origin + oneDay * 7.0 * float i; label=None; color=Some "rgba(0,0,0,0.05)"; width=Some 5 |}
            //yield {| value=origin + oneDay * 7.0 * float (i+1); label=None; color=Some "rgba(0,0,0,0.05)"; width=Some 5 |}
            yield
                {|
                    ``from`` = origin + oneDay * 7.0 * float i
                    ``to`` = origin + oneDay * 7.0 * float i + oneDay * 2.0
                    color = "rgb(0,0,0,0.025)"
                    label = None
                |}
    |]

let addContainmentMeasuresFlags
    (startDate: JsTimestamp)
    (endDate: JsTimestamp option) =
    let events = [|
    // day, mo, color,    title,       tooltip text
        4,  3, "#FFFFFF", "1. primer", "Prvi potrjen primer:<br/>turist iz Maroka"
        6,  3, "#FFe6e6", "DSO",       "Prepoved obiskov v domovih starejših občanov,<br/>potrjena okužba zdravnika v Metliki"
        8,  3, "#FFFFFF", "Točke",     "16 vstopnih točk za testiranje"
        10, 3, "#FFe6e6", "Meje",      "Zapora nekaterih mejnih prehodov z Italijo,<br/>poostren nadzor za osebna vozila"
        13, 3, "#FFFFFF", "Vlada",     "Sprejeta nova vlada"
        14, 3, "#FFe6e6", "Prevozi",   "Ukinitev javnih prevozov"
        16, 3, "#FFe6e6", "Šole",      "Zaprtje šol, restavracij"
        20, 3, "#FFe6e6", "Zbiranje",  "Prepoved zbiranja na javnih mestih"
        30, 3, "#FFe6e6", "Občine",    "Prepoved gibanja izven meja občin, obvezno razkuževanje <br/>večstanovanjskih zgradb, trgovine za ranljive skupine do 10h"
        4,  4, "#e6f0ff", "Trgovine",  "Trgovine od 8-10 ure in zadnjo uro izključno <br/>za ranljive skupine (invalidi, upokojenci, nosečnice)"
        12, 4, "#FFe6e6", "Karantena", "Obvezna 7 dnevna karantena pri prihodu iz tujine"
        18, 4, "#ebfaeb", "Vikendi",   "Sproščanje: gibanje med občinami (vikendi...), <br/>pojasnila glede splošne prepovedi gibanja"
        20, 4, "#ebfaeb", "Servisi",   "Sproščanje: gradnja, servisi, šport na prostem,<br/>dovoljeni nekateri linijski prevozi"
        21, 4, "#FFFFFF", "Raziskava", "Začetek nacionalne raziskave 3000 naključno izbranih oseb"
        29, 4, "#ebfaeb", "Muzeji",    "Sproščanje: knjižnice, galerije, muzeji, nep.posredovanje, dimnikarji"
        30, 4, "#ebfaeb", "Občine",    "Sproščanje: gibanje izven meja svoje občine"
        4,  5, "#ebfaeb", "Tržnice",   "Sproščanje: tržnice, strežba na terasah, trgovine do 400m2, frizerski in kozmetični saloni"
    |]
    {|
        ``type`` = "flags"
        shape = "flag"
        showInLegend = false
        color = "#444"
        data =
            events |> Array.choose (fun (d,m,color,title,text) ->
                let ts = DateTime(2020,m,d) |> jsTime
                let showMeasure =
                    match startDate, endDate with
                    | startDate, None -> ts >= startDate
                    | startDate, Some endDate ->
                        ts >= startDate && ts <= endDate

                if showMeasure then
                    Some {| x=ts; fillColor=color; title=title; text=text |}
                else None
            )
    |}

(* Trigger document event for iframe resizing *)
let onLoadEvent (name : String) =
    let res (e : Event) =
        let event = document.createEvent("Event")
        event.initEvent("chartLoaded", true, true)
        document.dispatchEvent(event)
    res

let optionsWithOnLoadEvent (className : string) =
    {| chart = pojo
        {| events = pojo
            {| load = onLoadEvent(className) |}
        |}
    |}

let basicChartOptions (scaleType:ScaleType) (className:string)=
    {|
        chart = pojo
            {|
                //height = "100%"
                ``type`` = "line"
                zoomType = "x"
                //styledMode = false // <- set this to 'true' for CSS styling
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        title = pojo {| text = None |}
        xAxis = [|
            {|
                index=0; crosshair=true; ``type``="datetime"
                gridLineWidth=1 //; isX=true
                gridZIndex = -1
                tickInterval=86400000
                //labels = pojo {| align = "center"; y = 30; reserveSpace = false |} // style = pojo {| marginBottom = "-30px" |}
                labels = pojo {| align = "center"; y = 30; reserveSpace = true; distance = -20; |} // style = pojo {| marginBottom = "-30px" |}
                //labels = {| rotation= -45 |}
                plotLines=[|
                    {| value=jsTime <| DateTime(2020,3,13); label=Some {| text="nov režim testiranja, izolacija"; rotation=270; align="right"; x=12 |} |}
                    {| value=jsTime <| DateTime(2020,3,20); label=Some {| text="nov režim testiranja"; rotation=270; align="right"; x=12 |} |}
                    {| value=jsTime <| DateTime(2020,4,8);  label=Some {| text="nov režim testiranja"; rotation=270; align="right"; x=12 |} |}
                    {| value=jsTime <| DateTime(2020,4,15); label=Some {| text="nov režim testiranja"; rotation=270; align="right"; x=12 |} |}
                    {| value=jsTime <| DateTime(2020,4,21); label=Some {| text="nov režim testiranja, raziskava"; rotation=270; align="right"; x=12 |} |}
                |]
                plotBands=[|
                    {| ``from``=jsTime <| DateTime(2020,2,29);
                       ``to``=jsTime <| DateTime(2020,3,13);
                       color="transparent"
                       label=Some {| align="center"; text="Faza 1" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,3,13);
                       ``to``=jsTime <| DateTime(2020,3,20);
                       color="transparent"
                       label=Some {| align="center"; text="Faza 2" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,3,20);
                       ``to``=jsTime <| DateTime(2020,4,8);
                       color="transparent"
                       label=Some {| align="center"; text="Faza 3" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,4,8);
                       ``to``=jsTime <| DateTime(2020,4,15);
                       color="transparent"
                       label=Some {| align="center"; text="Faza 4" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,4,15);
                       ``to``=jsTime <| DateTime(2020,4,21);
                       color="transparent"
                       label=Some {| align="center"; text="Faza 5" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,4,21);
                       ``to``=jsTime <| DateTime.Today;
                       color="transparent"
                       label=Some {| align="center"; text="Faza 6" |}
                    |}
                    yield! shadedWeekendPlotBands
                |]
            |}
        |]
        yAxis = [|
            {|
                index = 0
                ``type`` = if scaleType=Linear then "linear" else "logarithmic"
                min = if scaleType=Linear then None else Some 1.0
                //floor = if scaleType=Linear then None else Some 1.0
                opposite = true // right side
                title = {| text = null |} // "oseb" |}
                showFirstLabel = None
                tickInterval = if scaleType=Linear then None else Some 0.25
                gridZIndex = -1
                plotLines = [| {| value = 0; color = "black" |} |]
            |}
        |]
        legend =
            {|
                enabled = false
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                //labelFormatter = string //fun series -> series.name
                layout = "vertical"
                //backgroundColor = None :> string option
            |}
        responsive = pojo
            {|
                rules =
                    [| {|
                        condition = {| maxWidth = 500 |}
                        chartOptions =
                            {|
                                legend = {| enabled = false |}
                                yAxis = [| {| labels = {| enabled = false |} |} |]
                            |}
                    |} |]
            |}


        plotOptions = pojo
            {|
                line = pojo
                    {|
                        //dataLabels = pojo {| enabled = true |}
                        marker = pojo {| symbol = "circle"; radius = 3 |}
                        //enableMouseTracking = false
                    |}
            |}
        tooltip = pojo
            {|
                //xDateFormat = @"%A, %e. %b"
                shared = true
                dateTimeLabelFormats = pojo
                    {|
                        // our data is sampled (offset) to noon: 12:00
                        // but here we force to always format dates without any time
                        // - https://api.highcharts.com/highcharts/tooltip.dateTimeLabelFormats
                        day = @"%A, %e. %B %Y"
                        hour = @"%A, %e. %B %Y"
                        minute = @"%A, %e. %B %Y"
                        second = @"%A, %e. %B %Y"
                    |}
            |}
        credits = pojo {| enabled = false |}
    |}
