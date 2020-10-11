module SchoolsViz.Synthesis

open System
open Highcharts

type Date = { Year: int; Month: int; Day: int }
    with static member from(year: int, month: int, day: int) =
            { Year = year; Month = month; Day = day }

         member this.ToDateTime() = DateTime(this.Year, this.Month, this.Day)
         member this.ToJs() = this.ToDateTime() |> jsTime12h

type ChartSeriesEntry = Date * float

type ChartSeries = {
    Title: string
    Color: string
    Entries: ChartSeriesEntry[]
}

let allSeries: ChartSeries[] =
    [|
        { Title = "učenci"
          Color = "#FFEEBA"
          Entries = [|
              Date.from(2020, 10, 1), 1.
              Date.from(2020, 10, 2), 10.
              Date.from(2020, 10, 3), 64.
              Date.from(2020, 10, 4), 23.
              Date.from(2020, 10, 5), 134.
              Date.from(2020, 10, 6), 3.
          |] };
        { Title = "zaposleni"
          Color = "#B01C83"
          Entries = [|
              Date.from(2020, 10, 1), 5.
              Date.from(2020, 10, 2), 2.
              Date.from(2020, 10, 3), 4.
              Date.from(2020, 10, 4), 5.
              Date.from(2020, 10, 5), 1.
              Date.from(2020, 10, 6), 12.
          |] }
    |]

