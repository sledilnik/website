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
