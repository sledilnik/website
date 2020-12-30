module ExcessDeathsChart.Types

open Types

type DailyDeathsData = Data.DailyDeaths.DataPoint list

let START_YEAR = 2020

// type Sex =
//     | Male
//     | Female

// type AgeGroup =
//     | AgeGroupFrom0to3
//     | AgeGroupFrom4to18
//     | AgeGroupFrom1tto31
//     | AgeGroupFrom3tto41
//     | AgeGroupFrom4tto51
//     | AgeGroupFrom5tto61
//     | AgeGroupFrom6tto71
//     | AgeGroupOver72

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
