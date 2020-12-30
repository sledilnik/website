module DeceasedViz.Synthesis

open Data.Patients
open DeceasedViz.Analysis
open DataAnalysis.AgeGroupsTimeline
open Types
open Highcharts

type DeceasedVizState = {
    StatsData: StatsData
    PatientsData : PatientsStats []
    Page: VisualizationPage
    RangeSelectionButtonIndex: int
    Error : string option
}

type SeriesType =
    | DeceasedInIcu
    | DeceasedAcute
    | DeceasedCare
    | DeceasedOther
    | DeceasedAgeGroup of int

let (|HospitalSeriesType|AgeGroupSeriesType|) (seriesType: SeriesType) =
    match seriesType with
    | DeceasedInIcu -> HospitalSeriesType
    | DeceasedAcute -> HospitalSeriesType
    | DeceasedCare -> HospitalSeriesType
    | DeceasedOther -> HospitalSeriesType
    | DeceasedAgeGroup _ -> AgeGroupSeriesType

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
    match state.Page.MetricsType with
    | HospitalsToday ->
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
    | HospitalsToDate ->
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
    | _ -> invalidOp "bug"

let getHospitalsPointTotalValue state series dataPoint : int option =
    match state.Page.MetricsType with
    | HospitalsToday ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.today
        | DeceasedAcute -> dataPoint.total.deceased.hospital.today
        | DeceasedCare -> dataPoint.total.deceasedCare.today
        | DeceasedOther -> dataPoint.total.deceased.today
        | _ -> invalidOp "bug"
    | HospitalsToDate ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
        | DeceasedAcute -> dataPoint.total.deceased.hospital.toDate
        | DeceasedCare -> dataPoint.total.deceasedCare.toDate
        | DeceasedOther -> dataPoint.total.deceased.toDate
        | _ -> invalidOp "bug"
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
    | AgeGroupSeriesType -> invalidOp "todo"

let renderAllHospitalSeriesData state =
    let hospitalSeries =
        [|
          { SeriesType = DeceasedInIcu
            SeriesId = "deceased-icu"; Color = "#6d5b80" }
          { SeriesType = DeceasedAcute
            SeriesId = "deceased-acute"; Color = "#8c71a8" }
          { SeriesType = DeceasedCare
            SeriesId = "deceased-care"; Color = "#a483c7" }
          { SeriesType = DeceasedOther
            SeriesId = "deceased-rest"; Color = "#c59eef" }
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

let renderAllAgeGroupsSeriesData state =
    let calculationFormula =
        match state.Page.MetricsType with
        | ByAgeToDate -> Total
        | ByAgeToday -> Daily
        | _ -> invalidOp "not supported"

    getAgeGroupTimelineAllSeriesData
        state.StatsData calculationFormula
        (fun dataPoint -> dataPoint.DeceasedPerAgeToDate)

let renderAllSeriesData state =
    match state.Page.MetricsType with
    | HospitalMetricsType -> renderAllHospitalSeriesData state
    | AgeGroupsMetricsType -> renderAllAgeGroupsSeriesData state
    | _ -> invalidOp "todo"
