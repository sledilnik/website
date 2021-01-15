[<RequireQualifiedAccess>]
module DailyComparisonChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Types
open Highcharts

open Data.Patients
open Data.LabTests

type DisplayType =
    | Active
    | New
    | TestsPCR
    | PositivePctPCR
    | TestsHAT
    | PositivePctHAT
    | HospitalAdmitted
    | HospitalDischarged
    | ICUAdmitted
    | Deceased
    | Vaccinated
    static member UseStatsData dType = [ Active; New; Vaccinated ] |> List.contains dType

    static member UseLabTestsData dType =[ TestsPCR; PositivePctPCR; TestsHAT; PositivePctHAT ] |> List.contains dType

    static member All =
        [ New
          Active
          // DISABLED Vaccinated 
          TestsPCR
          PositivePctPCR
          TestsHAT
          PositivePctHAT
          HospitalAdmitted
          HospitalDischarged
          ICUAdmitted
          Deceased ]

    static member Default = New

    member this.GetName =
        match this with
        | New -> I18N.t "charts.dailyComparison.new"
        | Active -> I18N.t "charts.dailyComparison.active"
        | TestsPCR -> I18N.t "charts.dailyComparison.tests"
        | PositivePctPCR -> I18N.t "charts.dailyComparison.positivePct"
        | TestsHAT -> I18N.t "charts.dailyComparison.testsHAT"
        | PositivePctHAT -> I18N.t "charts.dailyComparison.positivePctHAT"
        | HospitalAdmitted -> I18N.t "charts.dailyComparison.hospitalAdmitted"
        | HospitalDischarged -> I18N.t "charts.dailyComparison.hospitalDischarged"
        | ICUAdmitted -> I18N.t "charts.dailyComparison.icuAdmitted"
        | Deceased -> I18N.t "charts.dailyComparison.deceased"
        | Vaccinated -> I18N.t "charts.dailyComparison.vaccinated"

    member this.GetColor =
        match this with
        | New -> "#bda506"
        | Active -> "#dba51d"
        | TestsPCR -> "#19aebd"
        | PositivePctPCR -> "#665191"
        | TestsHAT -> "#19aebd"
        | PositivePctHAT -> "#665191"
        | HospitalAdmitted -> "#be7A2a"
        | HospitalDischarged -> "#20b16d"
        | ICUAdmitted -> "#d96756"
        | Deceased -> "#6d5b80"
        | Vaccinated -> "#189a73"

type State =
    { StatsData: StatsData
      PatientsData: PatientsStats []
      LabTestsData: LabTestsStats []
      Error: string option
      DisplayType: DisplayType }

type Msg =
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeLabTestsData of Result<LabTestsStats [], string>
    | ConsumeServerError of exn
    | ChangeDisplayType of DisplayType

let init data: State * Cmd<Msg> =
    let cmdP =
        Cmd.OfAsync.either Data.Patients.getOrFetch () ConsumePatientsData ConsumeServerError

    let cmdL =
        Cmd.OfAsync.either Data.LabTests.getOrFetch () ConsumeLabTestsData ConsumeServerError

    let state =
        { StatsData = data
          PatientsData = [||]
          LabTestsData = [||]
          Error = None
          DisplayType = DisplayType.Default }

    state, (cmdP @ cmdL)

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumePatientsData (Ok data) -> { state with PatientsData = data }, Cmd.none
    | ConsumePatientsData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeLabTestsData (Ok data) -> { state with LabTestsData = data }, Cmd.none
    | ConsumeLabTestsData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | ChangeDisplayType dt -> { state with DisplayType = dt }, Cmd.none

let renderChartOptions (state: State) dispatch =

    let weeksShown = 6

    let percentageFormatter value =
        let valueF = float value / 100.0
        Utils.percentWith1DecimalFormatter (valueF)

    let tooltipFormatter state jsThis =
        let category = jsThis?x
        let pts: obj [] = jsThis?points

        let mutable fmtStr =
            sprintf "<b>%s</b><br>%s<br>" (state.DisplayType.GetName) category

        let mutable fmtLine = ""
        fmtStr <- fmtStr + "<table>"

        for p in pts do
            let yStr =
                match state.DisplayType with
                | PositivePctPCR | PositivePctHAT -> percentageFormatter p?point?y
                | _ -> I18N.NumberFormat.formatNumber (p?point?y: float)

            fmtLine <-
                sprintf
                    "<tr><td><span style='color:%s'>●</span></td><td>%s</td><td style='text-align: right; padding-left: 10px'><b>%s</b></td><td style='text-align: right; padding-left: 10px'>%s</td></tr>"
                    p?series?color
                    p?point?date
                    yStr
                    p?point?diff

            fmtStr <- fmtStr + fmtLine

        fmtStr <- fmtStr + "</table>"
        fmtStr

    let getStatsValue dp =
        match state.DisplayType with
        | New -> dp.Cases.ConfirmedToday
        | Active -> dp.Cases.Active
        | Vaccinated -> dp.Vaccination.Administered.Today
        | _ -> None

    let getTestsValue (dp: LabTestsStats) =
        match state.DisplayType with
        | TestsPCR -> dp.total.performed.today
        | PositivePctPCR ->
            match dp.total.positive.today, dp.total.performed.today with
            | Some p, Some t -> Some((float p / float t * 100.0 * 100.0) |> int)
            | _ -> None
        | TestsHAT -> dp.data.["hagt"].performed.today
        | PositivePctHAT ->
            match dp.data.["hagt"].positive.today, dp.data.["hagt"].performed.today with
            | Some p, Some t -> Some((float p / float t * 100.0 * 100.0) |> int)
            | _ -> None
        | _ -> None

    let getPatientsValue (dp: PatientsStats) =
        match state.DisplayType with
        | HospitalAdmitted -> dp.total.inHospital.``in``
        | HospitalDischarged -> dp.total.inHospital.out
        | ICUAdmitted -> dp.total.icu.``in``
        | Deceased -> dp.total.deceased.today
        | _ -> None

    let dataShown =
        if DisplayType.UseStatsData state.DisplayType then
            state.StatsData
            |> Seq.skipWhile
                (fun dp ->
                    dp.Date
                    <= DateTime.Today.AddDays(float (-weeksShown * 7 - 1))) // last day empty
            |> Seq.skipWhile (fun dp -> dp.Date.DayOfWeek <> DayOfWeek.Monday)
            |> Seq.map (fun dp -> (dp.Date, getStatsValue dp))
            |> Seq.toArray
        else if DisplayType.UseLabTestsData state.DisplayType then
            state.LabTestsData
            |> Seq.skipWhile
                (fun dp ->
                    dp.Date
                    <= DateTime.Today.AddDays(float (-weeksShown * 7 - 1))) // last day empty
            |> Seq.skipWhile (fun dp -> dp.Date.DayOfWeek <> DayOfWeek.Monday)
            |> Seq.map (fun dp -> (dp.Date, getTestsValue dp))
            |> Seq.toArray
        else
            state.PatientsData
            |> Seq.skipWhile
                (fun dp ->
                    dp.Date
                    <= DateTime.Today.AddDays(float (-(weeksShown * 7))))
            |> Seq.skipWhile (fun dp -> dp.Date.DayOfWeek <> DayOfWeek.Monday)
            |> Seq.map (fun dp -> (dp.Date, getPatientsValue dp))
            |> Seq.toArray

    let allSeries =
        [ for weekIdx in 0 .. weeksShown - 1 do
            let idx = weekIdx * 7
            let len = min 7 (dataShown.Length - idx)

            let desaturateColor (rgb: string) (sat: float) =
                let argb =
                    Int32.Parse(rgb.Replace("#", ""), Globalization.NumberStyles.HexNumber)

                let r = (argb &&& 0x00FF0000) >>> 16
                let g = (argb &&& 0x0000FF00) >>> 8
                let b = (argb &&& 0x000000FF)
                let avg = (float (r + g + b) / 3.0) * 1.6

                let newR =
                    int (Math.Round(float (r) * sat + avg * (1.0 - sat)))

                let newG =
                    int (Math.Round(float (g) * sat + avg * (1.0 - sat)))

                let newB =
                    int (Math.Round(float (b) * sat + avg * (1.0 - sat)))

                sprintf "#%02x%02x%02x" newR newG newB

            let getSeriesColor (dt: DisplayType) series =
                desaturateColor (dt.GetColor) (float (series) / float (weeksShown))

            let percent a b =
                match a, b with
                | Some v, Some p ->
                    if p = 0 then
                        if v = 0 then "" else ">500%"
                    else
                        sprintf "%+0.1f %%" (float (v) / float (p) * 100.0 - 100.0)
                // Utils.percentWith1DecimalSignFormatter((float(v) / float(p) * 100.) - 100.)
                | _, _ -> ""

            yield
                pojo
                    {| ``type`` = "column"
                       color = getSeriesColor state.DisplayType weekIdx
                       data =
                           dataShown
                           |> Array.skip idx
                           |> Array.take len
                           |> Array.mapi
                               (fun i (date, value) ->
                                   {| y = value
                                      date = I18N.tOptions "days.date" {| date = date |}
                                      diff =
                                          if weekIdx > 0 then
                                              let _, prev = dataShown.[(weekIdx - 1) * 7 + i]
                                              percent value prev
                                          else
                                              ""
                                      dataLabels =
                                          if weekIdx = weeksShown - 1 then
                                              match state.DisplayType with
                                              | PositivePctPCR | PositivePctHAT ->
                                                  pojo
                                                      {| enabled = true
                                                         formatter = fun () -> percentageFormatter jsThis?y |}
                                              | _ -> 
                                                  pojo {| enabled = true |}
                                          else
                                              pojo {|  |} |}
                                   |> pojo) |} ]

    {| optionsWithOnLoadEvent "covid19-daily-comparison" with
           chart = pojo {| ``type`` = "column" |}
           title = pojo {| text = None |}
           xAxis =
               [| {| ``type`` = "category"
                     categories =
                         [| I18N.dow 1
                            I18N.dow 2
                            I18N.dow 3
                            I18N.dow 4
                            I18N.dow 5
                            I18N.dow 6
                            I18N.dow 0 |] |} |]
           yAxis =
               [| {| opposite = true
                     title = {| text = null |}
                     labels =
                         match state.DisplayType with
                         | PositivePctPCR | PositivePctHAT ->
                             pojo {| formatter = fun () -> percentageFormatter jsThis?value |}
                         | _ -> 
                             pojo {| formatter = None |} |} |]
           series = List.toArray allSeries
           plotOptions = pojo {| series = {| groupPadding = 0.05 |} |}
           legend = pojo {| enabled = false |}
           tooltip =
               pojo
                   {| formatter = fun () -> tooltipFormatter state jsThis
                      shared = true
                      useHTML = true |}
           credits =
               pojo
                   {| enabled = true
                      text =
                          sprintf
                              "%s: %s, %s"
                              (I18N.t "charts.common.dataSource")
                              (I18N.t "charts.common.dsNIJZ")
                              (I18N.t "charts.common.dsMZ")
                      href = "https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19" |}
           responsive =
               pojo
                   {| rules =
                          [| {| condition = {| maxWidth = 768 |}
                                chartOptions = {| yAxis = [| {| labels = pojo {| enabled = false |} |} |] |} |} |] |}
           navigator = pojo {| enabled = false |}
           scrollbar = pojo {| enabled = false |}
           rangeSelector = pojo {| enabled = false |} |}

let renderChartContainer (state: State) dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderChartOptions state dispatch
                               |> Highcharts.chart ] ]

let renderSelector state (dt: DisplayType) dispatch =
    Html.div [ let isActive = state.DisplayType = dt
               prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)

               Utils.classes [ (true, "btn btn-sm metric-selector")
                               (isActive, "metric-selector--selected") ]

               prop.text dt.GetName ]

let renderDisplaySelectors state dispatch =
    Html.div [ prop.className "metrics-selectors"
               prop.children (
                   DisplayType.All
                   |> List.map (fun dt -> renderSelector state dt dispatch)
               ) ]

let render (state: State) dispatch =
    match state.PatientsData, state.LabTestsData, state.Error with
    | [||], _, None -> Html.div [ Utils.renderLoading ]
    | _, [||], None -> Html.div [ Utils.renderLoading ]
    | _, _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, _, None ->
        Html.div [ renderChartContainer state dispatch
                   renderDisplaySelectors state dispatch ]

let dailyComparisonChart (props: {| data: StatsData |}) =
    React.elmishComponent ("DailyComparisonChart", init props.data, update, render)
