module ExcessDeathsChart.Types

open Types

type MonthlyDeathsData = Data.MonthlyDeaths.DataPoint list

type DisplayType =
    | AbsoluteDeaths
    | ExcessDeaths
with
    static member available = [ AbsoluteDeaths ; ExcessDeaths ]

    static member getName = function
        | AbsoluteDeaths -> I18N.t "charts.excessDeaths.absolute.title"
        | ExcessDeaths -> I18N.t "charts.excessDeaths.excess.title"

    static member getColor = function
        | AbsoluteDeaths -> "gray"
        | ExcessDeaths -> "hotpink"

type State = {
    StatsData : StatsData
    MonthlyDeathsData : RemoteData<MonthlyDeathsData, string>
    DisplayType : DisplayType
}

type Msg =
    | MonthlyDeathsDataReceived of Result<MonthlyDeathsData, string>
    | DisplayTypeChanged of DisplayType

open Browser
open Fable.Core.JsInterop
open Highcharts

let baseOptions =
    {| title = ""
       xAxis = {| labels = {| formatter = fun x -> Utils.monthNameOfIndex x?value |} |> pojo |}
       credits =
            {| enabled = true
               text = sprintf "%s: %s, %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsSURS") {| context = localStorage.getItem ("contextCountry") |})
                    (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
               href = "https://www.stat.si/StatWeb/Field/Index/17/95" |} |> pojo
    |}
