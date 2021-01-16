module DeceasedViz.Synthesis

open Data.Patients
open DeceasedViz.Analysis
open DataAnalysis.AgeGroupsTimeline
open Types
open Highcharts


type DeceasedVizState = {
    StatsData: StatsData
    PatientsData : PatientsStats []
    MetricType : MetricType
    ChartType : ChartType
    Page: PageType
    RangeSelectionButtonIndex: int
    Error : string option
}

type SeriesType =
    | DeceasedInIcu
    | DeceasedAcute
    | DeceasedCare
    | DeceasedOther
    | DeceasedAgeGroup of int
    | DeceasedTypeRhOccupant
    | DeceasedTypeOther

let (|HospitalSeriesType|AgeGroupSeriesType|PersonTypeSeriesType|) (seriesType: SeriesType) =
    match seriesType with
    | DeceasedInIcu -> HospitalSeriesType
    | DeceasedAcute -> HospitalSeriesType
    | DeceasedCare -> HospitalSeriesType
    | DeceasedOther -> HospitalSeriesType
    | DeceasedAgeGroup _ -> AgeGroupSeriesType
    | DeceasedTypeRhOccupant -> PersonTypeSeriesType 
    | DeceasedTypeOther -> PersonTypeSeriesType

type SeriesInfo = {
    SeriesType: SeriesType
    SeriesId: string
    Color: string
}

let subtract (a : int option) (b : int option) =
    match a, b with
    | Some aa, Some bb -> Some (bb - aa)
    | Some aa, None -> -aa |> Some
    | None, Some _ -> b
    | _ -> None

let getHospitalsPointValue state (series: SeriesInfo) dataPoint : int option =
    match state.MetricType with
    | Today ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.today
        | DeceasedAcute ->
                dataPoint.total.deceased.hospital.today
                |> subtract dataPoint.total.deceased.hospital.icu.today
        | DeceasedCare -> dataPoint.total.deceasedCare.today
        | DeceasedOther ->
                dataPoint.total.deceased.today
                |> subtract dataPoint.total.deceased.hospital.today
                |> subtract dataPoint.total.deceasedCare.today
        | _ -> invalidOp "bug"
    | ToDate ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
        | DeceasedAcute ->
                dataPoint.total.deceased.hospital.toDate
                |> subtract dataPoint.total.deceased.hospital.icu.toDate
        | DeceasedCare -> dataPoint.total.deceasedCare.toDate
        | DeceasedOther ->
                dataPoint.total.deceased.toDate
                |> subtract dataPoint.total.deceased.hospital.toDate
                |> subtract dataPoint.total.deceasedCare.toDate
        | _ -> invalidOp "bug"

let getHospitalsPointTotalValue state series dataPoint : int option =
    match state.MetricType with
    | Today ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.today
        | DeceasedAcute -> dataPoint.total.deceased.hospital.today
        | DeceasedCare -> dataPoint.total.deceasedCare.today
        | DeceasedOther -> dataPoint.total.deceased.today
        | _ -> invalidOp "bug"
    | ToDate ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
        | DeceasedAcute -> dataPoint.total.deceased.hospital.toDate
        | DeceasedCare -> dataPoint.total.deceasedCare.toDate
        | DeceasedOther -> dataPoint.total.deceased.toDate
        | _ -> invalidOp "bug"

let constructHospitalsSeriesData state series =
    match series.SeriesType with
    | HospitalSeriesType ->
        state.PatientsData
        |> Seq.map (fun dataPoint ->
            {|
                x = dataPoint.Date |> jsTime12h
                y = getHospitalsPointValue state series dataPoint
                seriesId = series.SeriesId
                date = I18N.tOptions "days.longerDate"
                              {| date = dataPoint.Date |}
                fmtTotal = getHospitalsPointTotalValue state series dataPoint
                           |> string
            |} |> pojo
        )
        |> Array.ofSeq
    | _ -> invalidOp "todo"

let renderAllHospitalSeriesData state =
    let hospitalSeries =
        [|
          { SeriesType = DeceasedInIcu
            SeriesId = "deceased-icu"; Color = "#c59eef" }
          { SeriesType = DeceasedAcute
            SeriesId = "deceased-acute"; Color = "#a483c7" }
          { SeriesType = DeceasedCare
            SeriesId = "deceased-care"; Color = "#8c71a8" }
          { SeriesType = DeceasedOther
            SeriesId = "deceased-rest"; Color = "#6d5b80" }
         |]

    let renderSeriesData series =
        {|
            ``type`` = "column"
            visible = true
            color = series.Color
            name = I18N.tt "charts.deceased" series.SeriesId
            data = constructHospitalsSeriesData state series
            animation = false
        |}
        |> pojo

    hospitalSeries |> Array.map renderSeriesData

let constructPersonTypeSeriesData state series =

    let getPersonTypeSeries state series =
        let data =
            state.StatsData
            |> Seq.map (fun dataPoint ->
                {| date = dataPoint.Date 
                   value =
                        match series.SeriesType with
                        | DeceasedTypeRhOccupant -> dataPoint.DeceasedPerType.RhOccupant
                        | DeceasedTypeOther -> dataPoint.DeceasedPerType.Other
                        | _ -> invalidOp "bug"
                   |} )
        match state.MetricType with
        | Today ->
            data 
            |> Seq.pairwise
            |> Seq.map
                   (fun (prevDay, currDay) ->
                        {| date = currDay.date
                           value =  match currDay.value with
                                    | Some value -> currDay.value |> subtract prevDay.value 
                                    | None -> None |} )
        | ToDate -> data

    match series.SeriesType with
    | PersonTypeSeriesType ->
        getPersonTypeSeries state series
        |> Seq.map (fun dp ->
            {|
                x = dp.date |> jsTime12h
                y = dp.value
                seriesId = series.SeriesId
                date = I18N.tOptions "days.longerDate" {| date = dp.date |}
                fmtTotal = dp.value |> string
            |} |> pojo
        )
        |> Array.ofSeq
    | _ -> invalidOp "todo"

let renderAllPersonTypeSeriesData state =
    let ptSeries =
        [|
          { SeriesType = DeceasedTypeOther
            SeriesId = "deceased-other"; Color = "#c59eef" }
          { SeriesType = DeceasedTypeRhOccupant
            SeriesId = "deceased-rhoccupant"; Color = "#8c71a8" }
         |]

    let renderSeriesData series =
        {|
            ``type`` = "column"
            visible = true
            color = series.Color
            name = I18N.tt "charts.deceased" series.SeriesId
            data = constructPersonTypeSeriesData state series
            animation = false
        |}
        |> pojo

    ptSeries |> Array.map renderSeriesData

let renderAllAgeGroupsSeriesData state =
    let calculationFormula =
        match state.MetricType with
        | ToDate -> Total
        | Today -> Daily

    getAgeGroupTimelineAllSeriesData
        state.StatsData calculationFormula
        (fun dataPoint -> dataPoint.DeceasedPerAgeToDate)

let renderAllSeriesData state =
    match state.Page with
    | HospitalsPage -> renderAllHospitalSeriesData state
    | AgeGroupsPage -> renderAllAgeGroupsSeriesData state
    | PersonTypePage -> renderAllPersonTypeSeriesData state
