module ExcessDeathsChart.RelativeByAgeGroup

open Browser
open Highcharts
open Fable.Core.JsInterop

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
    let difference deceased baseline = System.Math.Round((float deceased - float baseline) / (float baseline) * 100.0, 1)
    { AgeGroupFrom0to51 = difference deceased.AgeGroupFrom0to51 baseline.AgeGroupFrom0to51
      AgeGroupFrom52to71 = difference deceased.AgeGroupFrom52to71 baseline.AgeGroupFrom52to71
      AgeGroup72AndMore = difference deceased.AgeGroup72AndMore baseline.AgeGroup72AndMore }

let dataPointTotal (deceased : AgeGroupsDataPoint<int> list) =
    { AgeGroupFrom0to51 = deceased |> List.sumBy (fun dp -> dp.AgeGroupFrom0to51)
      AgeGroupFrom52to71 = deceased |> List.sumBy (fun dp -> dp.AgeGroupFrom52to71)
      AgeGroup72AndMore = deceased |> List.sumBy (fun dp -> dp.AgeGroup72AndMore) }

let ageGroupProportions (ageGroupsDataPoints : AgeGroupsDataPoint<int> list) =
    let sum =
        { AgeGroupFrom0to51 = ageGroupsDataPoints |> List.sumBy (fun dp -> dp.AgeGroupFrom0to51)
          AgeGroupFrom52to71 = ageGroupsDataPoints |> List.sumBy (fun dp -> dp.AgeGroupFrom52to71)
          AgeGroup72AndMore = ageGroupsDataPoints |> List.sumBy (fun dp -> dp.AgeGroup72AndMore) }

    let total = sum.AgeGroupFrom0to51 + sum.AgeGroupFrom52to71 + sum.AgeGroup72AndMore

    let daysInMonthFactor = float(List.length ageGroupsDataPoints) / 30.0

    { AgeGroupFrom0to51 = float sum.AgeGroupFrom0to51 / float total * daysInMonthFactor
      AgeGroupFrom52to71 = float sum.AgeGroupFrom52to71 / float total * daysInMonthFactor
      AgeGroup72AndMore = float sum.AgeGroup72AndMore / float total * daysInMonthFactor }

let calculateDataSeries sex data =

    let lastDataPoint = List.last data

    let collapsedData =
        data
        |> collapseAgeGroupsData

    let averageByMonth =
        collapsedData
        |> List.filter (fun dp -> dp.Date.Year >= baselineStartYear && dp.Date.Year <= baselineEndYear)
        |> List.groupBy (fun dp -> dp.Date.Month)
        |> List.map (fun (month, dps) ->
            month, {| DeceasedRelative = dps |> List.map (fun dp -> dp.Deceased) |> averageDataPoint
                      DeceasedMale = dps |> List.map (fun dp -> dp.DeceasedMale) |> averageDataPoint
                      DeceasedFemale = dps |> List.map (fun dp -> dp.DeceasedFemale) |> averageDataPoint
                   |} )
        |> FSharp.Collections.Map

    let averageByMonthForLastMonth =
        let filtered =
            collapsedData
            |> List.filter (fun dp -> dp.Date.Year >= baselineStartYear && dp.Date.Year <= baselineEndYear)
            |> List.filter (fun dp ->
                match dp.Date, lastDataPoint.Date with
                | dp, lastDp when dp.Month = lastDp.Month && dp.Day <= lastDp.Day -> true
                | _ -> false)
        {| DeceasedRelative = filtered |> List.map (fun dp -> dp.Deceased) |> averageDataPoint
           DeceasedMale = filtered |> List.map (fun dp -> dp.DeceasedMale) |> averageDataPoint
           DeceasedFemale = filtered |> List.map (fun dp -> dp.DeceasedFemale) |> averageDataPoint
        |}

    let difference =
        collapsedData
        |> List.filter (fun dp -> dp.Date.Year > baselineEndYear)
        |> List.groupBy (fun dp -> dp.Date.Year, dp.Date.Month)
        |> List.map (fun ((year, month), dps) ->
            let averageBaseline =
                if (year, month) = (lastDataPoint.Date.Year, lastDataPoint.Date.Month) then
                    averageByMonthForLastMonth
                else
                    averageByMonth.Item month

            let both, male, female = (
                dataPointDifference (dps |> List.map (fun dp -> dp.Deceased) |> averageDataPoint) averageBaseline.DeceasedRelative,
                dataPointDifference (dps |> List.map (fun dp -> dp.DeceasedMale) |> averageDataPoint) averageBaseline.DeceasedMale,
                dataPointDifference (dps |> List.map (fun dp -> dp.DeceasedFemale) |> averageDataPoint) averageBaseline.DeceasedFemale )

            let allValues = [
                both.AgeGroupFrom0to51
                both.AgeGroupFrom52to71
                both.AgeGroup72AndMore
                male.AgeGroupFrom0to51
                male.AgeGroupFrom52to71
                male.AgeGroup72AndMore
                female.AgeGroupFrom0to51
                female.AgeGroupFrom52to71
                female.AgeGroup72AndMore
            ]

            {| Date = System.DateTime(year, month, 1)
               DeceasedRelativeMin = List.min allValues
               DeceasedRelativeMax = List.max allValues
               DeceasedRelative =
                match sex with
                | Both -> both
                | Male -> male
                | Female -> female
               DeceasedTotal =
                match sex with
                | Both -> dataPointTotal (dps |> List.map (fun dp -> dp.Deceased))
                | Male -> dataPointTotal (dps |> List.map (fun dp -> dp.DeceasedMale))
                | Female -> dataPointTotal (dps |> List.map (fun dp -> dp.DeceasedFemale))
               DeceasedAgeGroupProportions =
                match sex with
                | Both -> dps |> List.map (fun dp -> dp.Deceased) |> ageGroupProportions
                | Male -> dps |> List.map (fun dp -> dp.DeceasedMale) |> ageGroupProportions
                | Female -> dps |> List.map (fun dp -> dp.DeceasedFemale) |> ageGroupProportions
            |}
        )

    let width = 20.0

    let series = [|
        {| ``type`` = "column"
           name = "0-51"
           color = "#e9b825"
           data =
               difference
               |> List.map (fun dp ->
                    {| x = dp.Date |> jsTime12h
                       y = dp.DeceasedRelative.AgeGroupFrom0to51
                       deceasedTotal = dp.DeceasedTotal.AgeGroupFrom0to51
                       pointWidth = width * dp.DeceasedAgeGroupProportions.AgeGroupFrom0to51
                    |} |> pojo)
               |> List.toArray
        |} |> pojo
        {| ``type`` = "column"
           name = "52-71"
           color = "#189a73"
           data =
               difference
               |> List.map (fun dp ->
                    {| x = dp.Date |> jsTime12h
                       y = dp.DeceasedRelative.AgeGroupFrom52to71
                       deceasedTotal = dp.DeceasedTotal.AgeGroupFrom52to71
                       pointWidth = width * dp.DeceasedAgeGroupProportions.AgeGroupFrom52to71
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
                       y = dp.DeceasedRelative.AgeGroup72AndMore
                       deceasedTotal = dp.DeceasedTotal.AgeGroup72AndMore
                       pointWidth = width * dp.DeceasedAgeGroupProportions.AgeGroup72AndMore
                    |} |> pojo)
               |> List.toArray
        |} |> pojo
    |]

    let deceasedRelativeMin = (difference |> List.minBy (fun dp -> dp.DeceasedRelativeMin)).DeceasedRelativeMin
    let deceasedRelativeMax = (difference |> List.maxBy (fun dp -> dp.DeceasedRelativeMax)).DeceasedRelativeMax

    series, deceasedRelativeMin, deceasedRelativeMax

let renderChartOptions sex (data : DailyDeathsData) =
    let series, deceasedRelativeMin, deceasedRelativeMax = calculateDataSeries sex data

    {| chart = {| ``type`` = "column" |} |> pojo
       title = ""
       xAxis = {| ``type`` = "datetime" |}
       yAxis = {| title = {| text = None |} ; opposite = true ; min = deceasedRelativeMin ; max = deceasedRelativeMax ; labels = {| formatter = fun (x) -> x?value + " %" |} |> pojo |}
       tooltip = {| valueSuffix = " %" ; xDateFormat = "%B %Y" ; footerFormat = chartText "excessByAgeGroup.totalDeceased" + ": <b>{point.deceasedTotal}</b>" |} |> pojo
       responsive = ChartOptions.responsive
       plotOptions = {| series = {| pointPadding = 0 ; borderWidth = 0 |} |> pojo |} |> pojo
       series = series
       credits =
        {| enabled = true
           text = sprintf "%s: %s"
                (I18N.t "charts.common.dataSource")
                (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
           href = "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-notranje-zadeve/" |} |> pojo
    |} |> pojo
