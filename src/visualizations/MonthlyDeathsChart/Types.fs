module MonthlyDeathsChart.Types

open Types

type MonthlyDeathsData = Data.MonthlyDeaths.DataPoint list

type DisplayType =
    | AbsoluteDeaths
    | ExcessDeaths

type State = {
    StatsData : StatsData
    MonthlyDeathsData : RemoteData<MonthlyDeathsData, string>
}

type Msg =
    | MonthlyDeathsDataReceived of Result<MonthlyDeathsData, string>

let colors = {|
    CurrentYear = "#a483c7"
    BaselineYear = "#d5d5d5"
|}
