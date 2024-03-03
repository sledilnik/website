module ExcessDeathsChart.Types

open Types

type DailyDeathsData = Data.DailyDeaths.DataPoint list

let START_YEAR = 2020

let chartText = I18N.chartText "excessDeaths"

type IOption =
    abstract member Label : string

type Sex =
    | Both
    | Male
    | Female

    static member All = [ Both ; Male ; Female ]

    interface IOption with

        member this.Label =
            match this with
            | Both -> chartText "excessByAgeGroup.sex.both"
            | Male -> chartText "excessByAgeGroup.sex.male"
            | Female -> chartText "excessByAgeGroup.sex.female"

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
    | ExcessDeathsByAgeGroup of Sex
with
    static member All = [ AbsoluteDeaths ; ExcessDeaths ;  ExcessDeathsByAgeGroup Both ]

    static member Default = AbsoluteDeaths

    member this.GetName =
        match this with
        | AbsoluteDeaths -> I18N.t "charts.excessDeaths.absolute.title"
        | ExcessDeaths -> I18N.t "charts.excessDeaths.excess.title"
        | ExcessDeathsByAgeGroup _ -> I18N.t "charts.excessDeaths.excessByAgeGroup.title"

type State = {
    DisplayType : DisplayType
    StatsData : StatsData
    DailyDeathsData : RemoteData<Data.DailyDeaths.DataPoint list, string>
    WeeklyDeathsData : RemoteData<WeeklyDeathsData, string>
}

type Msg =
    | DailyDeathsDataReceived of Result<DailyDeathsData, string>
    | DisplayTypeChanged of DisplayType
