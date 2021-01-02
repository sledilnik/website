module ExcessDeathsChart.RelativeByAgeGroup

open Browser
open Highcharts
open Fable.Core.JsInterop
open Fable.DateFunctions

open Types

type AgeGroupsDataPoint<'N> = {
    AgeGroupFrom0to51 : 'N
    AgeGroupFrom52to71 : 'N
    AgeGroup72AndMore : 'N
}

type DataPoint<'N> = {
    Date : System.DateTime
    Deceased : AgeGroupsDataPoint<'N>
    DeceasedMale : AgeGroupsDataPoint<'N>
    DeceasedFemale : AgeGroupsDataPoint<'N>
}

let baselineStartYear, baselineEndYear = 2015, 2019

let collapseAgeGroupsDataPoint (ageGroup : Data.DailyDeaths.AgeGroupDataPoint) =

    let sumAgeGroups ageGroups =
        ageGroups
        |> List.map (fun ag -> Option.defaultValue 0 ag)
        |> List.sum

    { AgeGroupFrom0to51  = sumAgeGroups [ ageGroup.DeceasedAgeGroupFrom0to3
                                          ageGroup.DeceasedAgeGroupFrom4to18
                                          ageGroup.DeceasedAgeGroupFrom18to31
                                          ageGroup.DeceasedAgeGroupFrom32to41
                                          ageGroup.DeceasedAgeGroupFrom42to51 ]
      AgeGroupFrom52to71 = sumAgeGroups [ ageGroup.DeceasedAgeGroupFrom52to61
                                          ageGroup.DeceasedAgeGroupFrom62to71 ]
      AgeGroup72AndMore  = sumAgeGroups [ ageGroup.DeceasedAgeGroup72AndMore ] }

let collapseAgeGroupsData (data : DailyDeathsData) : DataPoint<int> list =

    data
    |> List.map (fun dp ->
        let male = collapseAgeGroupsDataPoint dp.DeceasedMale
        let female = collapseAgeGroupsDataPoint dp.DeceasedFemale
        { Date = dp.Date
          Deceased = {
            AgeGroupFrom0to51 = male.AgeGroupFrom0to51 + female.AgeGroupFrom0to51
            AgeGroupFrom52to71 = male.AgeGroupFrom52to71 + female.AgeGroupFrom52to71
            AgeGroup72AndMore = male.AgeGroup72AndMore + female.AgeGroup72AndMore
          }
          DeceasedMale = male
          DeceasedFemale = female }
    )

let averageDataPoint dataPoints =
    let countDays = List.length dataPoints |> float
    { AgeGroupFrom0to51 = (dataPoints |> List.sumBy (fun dp -> dp.AgeGroupFrom0to51) |> float) / countDays
      AgeGroupFrom52to71 = (dataPoints|> List.sumBy (fun dp -> dp.AgeGroupFrom52to71) |> float) / countDays
      AgeGroup72AndMore = (dataPoints|> List.sumBy (fun dp -> dp.AgeGroup72AndMore) |> float) / countDays }

let dataPointDifference deceased baseline =
    let difference deceased baseline = (float deceased - float baseline) / (float baseline) * 100.0
    { AgeGroupFrom0to51 = difference deceased.AgeGroupFrom0to51 baseline.AgeGroupFrom0to51
      AgeGroupFrom52to71 = difference deceased.AgeGroupFrom52to71 baseline.AgeGroupFrom52to71
      AgeGroup72AndMore = difference deceased.AgeGroup72AndMore baseline.AgeGroup72AndMore }

let calculateDataSeties sex data =

    let collapsedData =
        data
        |> collapseAgeGroupsData

    let averageByMonth =
        collapsedData
        |> List.filter (fun dp -> dp.Date.Year >= baselineStartYear && dp.Date.Year <= baselineEndYear)
        |> List.groupBy (fun dp -> dp.Date.Month)
        |> List.map (fun (month, dps) ->
            month, {| Deceased = dps |> List.map (fun dp -> dp.Deceased) |> averageDataPoint
                      DeceasedMale = dps |> List.map (fun dp -> dp.DeceasedMale) |> averageDataPoint
                      DeceasedFemale = dps |> List.map (fun dp -> dp.DeceasedFemale) |> averageDataPoint
                   |} )
        |> FSharp.Collections.Map

    let difference =
        collapsedData
        |> List.filter (fun dp -> dp.Date.Year > baselineEndYear)
        |> List.groupBy (fun dp -> dp.Date.Year, dp.Date.Month)
        |> List.map (fun ((year, month), dps) ->
            let averageBaseline = averageByMonth.Item month
            {| Date = System.DateTime(year, month, 1)
               Deceased =
                match sex with
                | Both -> dataPointDifference (dps |> List.map (fun dp -> dp.Deceased) |> averageDataPoint) averageBaseline.Deceased
                | Male -> dataPointDifference (dps |> List.map (fun dp -> dp.DeceasedMale) |> averageDataPoint) averageBaseline.DeceasedMale
                | Female -> dataPointDifference (dps |> List.map (fun dp -> dp.DeceasedFemale) |> averageDataPoint) averageBaseline.DeceasedFemale
            |}
        )

    [|
        {| ``type`` = "column"
           name = "0-51"
           color = "#e9b825"
           data =
               difference
               |> List.map (fun dp ->
                    {| x = dp.Date |> jsTime12h
                       y = dp.Deceased.AgeGroupFrom0to51
                    |} |> pojo)
               |> List.toArray
        |} |> pojo
        {| ``type`` = "column"
           name = "51-71"
           color = "#189a73"
           data =
               difference
               |> List.map (fun dp ->
                    {| x = dp.Date |> jsTime12h
                       y = dp.Deceased.AgeGroupFrom52to71
                    |} |> pojo)
               |> List.toArray
        |} |> pojo
        {| ``type`` = "column"
           name = "72+"
           color = "#b01c83"
           data =
               difference
               |> List.map (fun dp ->
                    {| x = dp.Date |> jsTime12h
                       y = dp.Deceased.AgeGroup72AndMore
                    |} |> pojo)
               |> List.toArray
        |} |> pojo
    |]

let renderChartOptions sex (data : DailyDeathsData) =
    {| chart = {| ``type`` = "column " |} |> pojo
       title = ""
       xAxis = {| ``type`` = "datetime" |}
       yAxis = {| title = {| text = None |} ; opposite = true ; labels = {| formatter = fun (x) -> x?value + " %" |} |> pojo |}
       tooltip = {| formatter = fun () -> sprintf "%s: <b>%.1f %%</b>" jsThis?key jsThis?y |} |> pojo
       responsive = ChartOptions.responsive
       series = calculateDataSeties sex data
       credits =
        {| enabled = true
           text = sprintf "%s: %s, %s"
                (I18N.t "charts.common.dataSource")
                (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
                (I18N.tOptions ("charts.common.dsMZ") {| context = localStorage.getItem ("contextCountry") |})
           href = "https://www.stat.si/StatWeb/Field/Index/17/95" |} |> pojo
    |} |> pojo
