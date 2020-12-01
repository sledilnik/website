module ExcessDeathsChart.Types

open Types

type DailyDeathsData = Data.DailyDeaths.DataPoint list

type WeeklyDeaths = {
    Year : int
    Week : int
    Deceased : int
}

type WeeklyDeathsData = WeeklyDeaths list

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
    WeeklyDeathsData : RemoteData<WeeklyDeathsData, string>
    DisplayType : DisplayType
}

type Msg =
    | DailyDeathsDataReceived of Result<DailyDeathsData, string>
    | DisplayTypeChanged of DisplayType

open Browser
open Highcharts

let baseOptions =
    {| title = ""
       credits =
            {| enabled = true
               text = sprintf "%s: %s, %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsSURS") {| context = localStorage.getItem ("contextCountry") |})
                    (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
               href = "https://www.stat.si/StatWeb/Field/Index/17/95" |} |> pojo
    |}
