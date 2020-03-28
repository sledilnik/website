module Highcharts

open System
open Fable.Core
open Fable.React

open Types

[<Import("renderChart", from="./_highcharts")>]
let chart: obj -> ReactElement = jsNative

[<AutoOpen>]
module Helpers =
    // plain old javascript object
    let inline pojo o = JsInterop.toPlainJsObj o

    // plain old javascript object
    [<Emit """Array.prototype.slice.call($0)""">]
    let poja (a: 'T[]) : obj = jsNative

    type JsTimestamp = float
    [<Emit("$0.getTime()")>]
    let jsTime (x: DateTime): JsTimestamp = jsNative


let basicChartOptions (scaleType:ScaleType) =
    {|
        chart = pojo
            {|
                //height = "100%"
                ``type`` = "spline"
                zoomType = "x"
            |}
        title = null //{| text = "Graf" |}
        xAxis = [|
            {|
                index=0; crosshair=true; ``type``="datetime"
                gridLineWidth=1 //; isX=true
                tickInterval=86400000
                //labels = {| rotation= -45 |}
                plotLines=[|
                    {| value=jsTime <| DateTime(2020,3,13); label={| text="nov režim testiranja, izolacija"; rotation=270; align="right"; x=12 |} |}
                    {| value=jsTime <| DateTime(2020,3,20); label={| text="nov režim testiranja"; rotation=270; align="right"; x=12 |} |}
                |]
                plotBands=[|
                    {| ``from``=jsTime <| DateTime(2020,2,29);
                       ``to``=jsTime <| DateTime(2020,3,13);
                       color="transparent"
                       label={| align="center"; text="Faza 1" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,3,13);
                       ``to``=jsTime <| DateTime(2020,3,20);
                       color="transparent"
                       label={| align="center"; text=" Faza 2" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,3,20);
                       ``to``=jsTime <| DateTime.Today
                       color="transparent"
                       label={| align="center"; text="Faza 3" |}
                    |}
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
                //showFirstLabel = false
                tickInterval = if scaleType=Linear then None else Some 0.25
            |}
        |]
        legend = pojo
            {|
                enabled = false
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                //labelFormatter = string //fun series -> series.name
                layout = "vertical"
            |}
        plotOptions = pojo
            {|
                spline = pojo
                    {|
                        dataLabels = pojo {| enabled = true |}
                        //enableMouseTracking = false
                    |}
            |}
    |}
