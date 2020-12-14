module ExcessDeathsChart.Types

open Types

let CURRENT_YEAR = 2020

type DailyDeathsData = Data.DailyDeaths.DataPoint list

type WeeklyDeaths = {
    Year : int
    Week : int
    WeekStartDate : System.DateTime
    WeekEndDate : System.DateTime
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

type State = {
    StatsData : StatsData
    WeeklyDeathsData : RemoteData<WeeklyDeathsData, string>
    DisplayType : DisplayType
}

type Msg =
    | DailyDeathsDataReceived of Result<DailyDeathsData, string>
    | DisplayTypeChanged of DisplayType
