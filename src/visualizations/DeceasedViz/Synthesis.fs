module DeceasedViz.Synthesis

open Data.Patients
open DeceasedViz.Analysis
open Types
open Highcharts

type DeceasedVizState = {
    StatsData: StatsData
    PatientsData : PatientsStats []
    Page: VisualizationPage
    RangeSelectionButtonIndex: int
    Error : string option
}

type Series =
    | DeceasedInIcu
    | DeceasedAcute
    | DeceasedCare
    | DeceasedOther

type SeriesInfo = {
    SeriesType: Series
    SeriesId: string
    Color: string
}

let subtract (a : int option) (b : int option) =
    match a, b with
    | Some aa, Some bb -> Some (bb - aa)
    | Some aa, None -> -aa |> Some
    | None, Some _ -> b
    | _ -> None

let getPoint state (series: SeriesInfo) dataPoint : int option =
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

let getPointTotal state series dataPoint : int option =
    match state.Page.MetricsType with
    | HospitalsToday ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.today
        | DeceasedAcute -> dataPoint.total.deceased.hospital.today
        | DeceasedCare -> dataPoint.total.deceasedCare.today
        | DeceasedOther -> dataPoint.total.deceased.today
    | HospitalsToDate ->
        match series.SeriesType with
        | DeceasedInIcu -> dataPoint.total.deceased.hospital.icu.toDate
        | DeceasedAcute -> dataPoint.total.deceased.hospital.toDate
        | DeceasedCare -> dataPoint.total.deceasedCare.toDate
        | DeceasedOther -> dataPoint.total.deceased.toDate

let constructSeriesData state series =
    state.PatientsData
    |> Seq.map (fun dataPoint ->
        {|
            x = dataPoint.Date |> jsTime12h
            y = getPoint state series dataPoint
            seriesId = series.SeriesId
            fmtDate = I18N.tOptions "days.longerDate"
                          {| date = dataPoint.Date |}
            fmtTotal = getPointTotal state series dataPoint |> string
        |} |> pojo
    )
    |> Array.ofSeq

let pageSeries state =
    match state.Page.MetricsType with
    | HospitalMetricsType ->
        [
          { SeriesType = DeceasedInIcu
            SeriesId = "deceased-icu"; Color = "#6d5b80" }
          { SeriesType = DeceasedAcute
            SeriesId = "deceased-acute"; Color = "#8c71a8" }
          { SeriesType = DeceasedCare
            SeriesId = "deceased-care"; Color = "#a483c7" }
          { SeriesType = DeceasedOther
            SeriesId = "deceased-rest"; Color = "#c59eef" }
         ]
    | _ -> invalidOp "todo"
