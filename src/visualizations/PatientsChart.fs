[<RequireQualifiedAccess>]
module PatientsChart

open System
open System.Text
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Highcharts
open Types

open Data.Patients

type HospitalType =
    | CovidHospitals
    | CovidHospitalsICU
    | CareHospitals

    static member All =
        [ CovidHospitals
          CovidHospitalsICU
          CareHospitals ]

type Breakdown =
    | ByHospital
    | AllHospitals
    | Facility of string
    static member All state =
        seq {
            yield ByHospital
            yield AllHospitals

            for fcode in state.AllFacilities do
                yield Facility fcode
        }

    static member Default = AllHospitals

    member this.GetName =
        match this with
        | ByHospital -> I18N.t "charts.patients.byHospital"
        | AllHospitals -> I18N.t "charts.patients.allHospitals"
        | Facility facility -> Utils.Dictionaries.GetFacilityName(facility)

and State =
    { PatientsData: PatientsStats []
      Error: string option
      AllFacilities: string list
      HTypeToDisplay: HospitalType
      Breakdown: Breakdown
      RangeSelectionButtonIndex: int }
    static member initial hTypeToDisplay =
        { PatientsData = [||]
          Error = None
          AllFacilities = []
          HTypeToDisplay = hTypeToDisplay
          Breakdown = Breakdown.Default
          RangeSelectionButtonIndex = 0 }

type Series =
    | InHospital
    | Acute
    | Icu
    | IcuOther
    | NivVentilator
    | InvVentilator
    | Care
    | InHospitalIn
    | InHospitalOut
    | InHospitalDeceased
    | IcuIn
    | IcuOut
    | IcuDeceased
    | CareIn
    | CareOut
    | CareDeceased

module Series =
    let structure hTypeToDisplay =
        match hTypeToDisplay with
        | CareHospitals -> [ Care; CareIn; CareOut; CareDeceased ]
        | CovidHospitals ->
            [ Acute
              Icu
              InHospitalIn
              InHospitalOut
              InHospitalDeceased ]
        | CovidHospitalsICU ->
            [ IcuOther
              NivVentilator
              InvVentilator
              IcuIn
              IcuOut
              IcuDeceased ]

    let byHospital = [ InHospital ]

    let getSeriesInfo =
        function
        | InHospital -> "#de9a5a", "hospitalized", 0
        | Acute -> "#de9a5a", "acute", 0
        | Icu -> "#de2d26", "icu", 0
        | IcuOther -> "#fb6a4a", "icu-other", 0
        | NivVentilator -> "#de2d26", "niVentilator", 0
        | InvVentilator -> "#a50f15", "ventilator", 0
        | Care -> "#dba51d", "care", 0
        | IcuIn -> "#fb6a4a", "icu-admitted", 1
        | IcuOut -> "#de9a5a", "icu-discharged", 1
        | IcuDeceased -> "#8c71a8", "icu-deceased", 1
        | InHospitalIn -> "#d5c768", "admitted", 1
        | InHospitalOut -> "#8cd4b2", "discharged", 1
        | InHospitalDeceased -> "#8c71a8", "deceased", 1
        | CareIn -> "#d5c768", "admitted", 1
        | CareOut -> "#8cd4b2", "discharged", 1
        | CareDeceased -> "#8c71a8", "deceased", 1



type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | SwitchBreakdown of Breakdown
    | RangeSelectionChanged of int

let init (hTypeToDisplay: HospitalType): State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumePatientsData ConsumeServerError

    State.initial hTypeToDisplay, cmd

let hasToDateValue (state: State) (ps: FacilityPatientStats) =
    match state.HTypeToDisplay with
    | CareHospitals -> ps.care.toDate.IsSome
    | CovidHospitals -> ps.inHospital.toDate.IsSome
    | CovidHospitalsICU -> ps.icu.toDate.IsSome

let getDailyValue (state: State) (ps: FacilityPatientStats) =
    match state.HTypeToDisplay with
    | CareHospitals -> ps.care.today
    | CovidHospitals -> ps.inHospital.today
    | CovidHospitalsICU -> ps.icu.today

let getFacilitiesList (state: State) (data: PatientsStats array) =
    data.[data.Length - 1].facilities
    |> Map.toSeq
    |> Seq.filter (fun (_, ps) -> hasToDateValue state ps)
    |> Seq.map (fun (facility, ps) -> facility, getDailyValue state ps)
    |> Seq.fold (fun hospitals (hospital, cnt) -> hospitals |> Map.add hospital cnt) Map.empty // all
    |> Map.toList
    |> List.sortBy (fun (_, cnt) -> cnt |> Option.defaultValue -1 |> (*) -1)
    |> List.map (fst)

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) ->
        { state with
              PatientsData = data
              AllFacilities = getFacilitiesList state data },
        Cmd.none
    | ConsumePatientsData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | SwitchBreakdown breakdown -> { state with Breakdown = breakdown }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none

let renderByHospitalChart (state: State) dispatch =

    let startDate = DateTime(2020, 3, 10)

    let renderSources fcode =
        let renderPoint ps: (JsTimestamp * int option) =
            let value =
                ps.facilities
                |> Map.tryFind fcode
                |> Option.bind (fun ps -> getDailyValue state ps)

            ps.JsDate12h, value

        {| visible = true
           name = Utils.Dictionaries.GetFacilityName(fcode)
           color = Utils.Dictionaries.GetFacilityColor(fcode)
           dashStyle = Solid |> DashStyle.toString
           yAxis = 0
           data =
               state.PatientsData
               |> Seq.skipWhile (fun dp -> dp.Date < startDate)
               |> Seq.map renderPoint
               |> Array.ofSeq
           showInLegend = true |}
        |> pojo

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions
            ScaleType.Linear
            "covid19-patients-by-hospital"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick

    {| baseOptions with
           series =
               [| for fcode in state.AllFacilities do
                   yield renderSources fcode |]
           yAxis =
               [| {| index = 0
                     height = "100%"
                     top = "0%"
                     offset = 0
                     title = {| text = null |}
                     ``type`` = "linear"
                     opposite = true
                     gridZIndex = -1
                     visible = true
                     plotLines = [| {| value = 0; color = "black" |} |] |} |]
           tooltip =
               pojo
                   {| shared = true
                      formatter = None
                      xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>" |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |} |}

    |> pojo


let renderStructureChart (state: State) dispatch =

    let startDate = DateTime(2020, 3, 10)

    let tooltipFormatter jsThis =
        let points: obj [] = jsThis?points

        let mutable curGroup = 0
        let mutable sum = 0.

        match points with
        | [||] -> ""
        | _ ->
            let s = StringBuilder()
            let date = points.[0]?point?date

            s.AppendFormat("<b>{0}</b><br/>", date.ToString())
            |> ignore

            s.Append "<table>" |> ignore

            points
            |> Array.iter
                (fun dp ->
                    let name = dp?series?name
                    let color = dp?series?color
                    let group = dp?point?group
                    let value: float = dp?point?y

                    if group > curGroup then
                        s.Append "<tr>" |> ignore

                        let sumStr =
                            sprintf
                                "<td></td><td>%s</td><td style='text-align: right; padding-left: 10px'><b>%s</b></td>"
                                "SKUPAJ"
                                (I18N.NumberFormat.formatNumber (abs sum))

                        s.Append sumStr |> ignore
                        s.Append "</tr><tr></tr>" |> ignore

                        curGroup <- group
                        sum <- 0.
                    else
                        sum <- sum + value

                    s.Append "<tr>" |> ignore

                    let tooltip =
                        sprintf
                            "<td><span style='color:%s'>●</span></td><td>%s</td><td style='text-align: right; padding-left: 10px'><b>%s</b></td>"
                            color
                            name
                            (I18N.NumberFormat.formatNumber (abs value))

                    s.Append tooltip |> ignore
                    s.Append "</tr>" |> ignore)

            s.Append "</table>" |> ignore
            s.ToString()

    let psData: (DateTime * FacilityPatientStats) [] =
        match state.Breakdown with
        | Facility fcode ->
            state.PatientsData
            |> Seq.skipWhile (fun dp -> dp.Date < startDate)
            |> Seq.map (fun ps -> (ps.Date, ps.facilities |> Map.find fcode))
            |> Seq.toArray
        | _ ->
            state.PatientsData
            |> Seq.skipWhile (fun dp -> dp.Date < startDate)
            |> Seq.map (fun ps -> (ps.Date, ps.total.ToFacilityStats))
            |> Seq.toArray

    let renderBarSeries series =
        let subtract (a: int option) (b: int option) =
            match a, b with
            | Some aa, Some bb -> Some(bb - aa)
            | Some aa, None -> -aa |> Some
            | None, Some _ -> b
            | _ -> None

        let negative (a: int option) =
            match a with
            | Some aa -> -aa |> Some
            | None -> None

        let getPoint (ps: FacilityPatientStats): int option =
            match series with
            | InHospital -> ps.inHospital.today
            | Acute -> ps.inHospital.today |> subtract ps.icu.today
            | Icu -> ps.icu.today
            | IcuOther ->
                ps.icu.today
                |> subtract ps.niv.today
                |> subtract ps.critical.today
            | NivVentilator -> ps.niv.today
            | InvVentilator -> ps.critical.today
            | Care -> ps.care.today
            | IcuIn -> ps.icu.``in``
            | IcuOut -> negative ps.icu.out
            | IcuDeceased -> negative ps.deceased.icu.today
            | InHospitalIn -> ps.inHospital.``in``
            | InHospitalOut -> negative ps.inHospital.out
            | InHospitalDeceased -> negative ps.deceased.today
            | CareIn -> ps.care.``in``
            | CareOut -> negative ps.care.out
            | CareDeceased -> negative ps.deceasedCare.today

        let color, seriesId, seriesIdx = Series.getSeriesInfo series

        {| color = color
           name = I18N.tt "charts.patients" seriesId
           yAxis = seriesIdx
           data =
               psData
               |> Seq.map
                   (fun (date, ps) ->
                       {| x = date |> jsTime12h
                          y = getPoint ps
                          group = seriesIdx
                          date = I18N.tOptions "days.longerDate" {| date = date |} |})
               |> Seq.toArray |}
        |> pojo

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let className = "covid19-patients-structure"

    let baseOptions =
        basicChartOptions ScaleType.Linear className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           chart =
               pojo
                   {| animation = false
                      ``type`` = "column"
                      zoomType = "x"
                      className = className
                      events = pojo {| load = onLoadEvent (className) |} |}
           yAxis =
               [| {| index = 0
                     height = "60%"
                     top = "0%"
                     offset = 0
                     title = {| text = null |}
                     ``type`` = "linear"
                     opposite = true
                     gridZIndex = -1
                     visible = true |}
                  {| index = 1
                     height = "35%"
                     top = "65%"
                     offset = 0
                     title = {| text = null |}
                     ``type`` = "linear"
                     opposite = true
                     gridZIndex = -1
                     visible = true |} |]
           plotOptions =
               pojo
                   {| column =
                          pojo
                              {| dataGrouping = pojo {| enabled = false |}
                                 stacking = "normal"
                                 crisp = false
                                 borderWidth = 0
                                 pointPadding = 0
                                 groupPadding = 0 |} |}
           series =
               [| for series in Series.structure state.HTypeToDisplay do
                   yield renderBarSeries series |]
           tooltip =
               pojo
                   {| shared = true
                      split = false
                      useHTML = true
                      formatter = (fun () -> tooltipFormatter jsThis) |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           responsive =
               pojo
                   {| rules =
                          [| {| condition = {| maxWidth = 768 |}
                                chartOptions =
                                    {| yAxis =
                                           [| {| labels = pojo {| enabled = false |} |}
                                              {| labels = pojo {| enabled = false |} |} |] |} |} |] |} |}


    |> pojo


let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ match state.Breakdown with
                               | ByHospital ->
                                   renderByHospitalChart state dispatch
                                   |> chartFromWindow
                               | _ ->
                                   renderStructureChart state dispatch
                                   |> chartFromWindow ] ]

let renderBreakdownSelector state breakdown dispatch =
    Html.div [ prop.onClick (fun _ -> SwitchBreakdown breakdown |> dispatch)
               Utils.classes [ (true, "btn btn-sm metric-selector")
                               (state.Breakdown = breakdown, "metric-selector--selected") ]
               prop.text breakdown.GetName ]

let renderBreakdownSelectors state dispatch =
    Html.div [ prop.className "metrics-selectors"
               prop.children (
                   Breakdown.All state
                   |> Seq.map (fun breakdown -> renderBreakdownSelector state breakdown dispatch)
               ) ]

let render (state: State) dispatch =
    match state.PatientsData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [ renderChartContainer state dispatch
                   renderBreakdownSelectors state dispatch ]

let patientsChart (props: {| hTypeToDisplay: HospitalType |}) =
    React.elmishComponent ("PatientsChart", init props.hTypeToDisplay, update, render)
