[<RequireQualifiedAccess>]
module VaccineEffectChart

open System
open System.Text
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser
open Fable.Core.JsInterop

open Highcharts
open Types

let chartText = I18N.chartText "vaccineEffect"

type DisplayType =
    | ConfirmedCases
    | HospitalizedCases
    | IcuCases
    static member All =
        [ ConfirmedCases
          HospitalizedCases
          IcuCases ]

    static member Default = IcuCases

    member this.GetName =
        match this with
        | ConfirmedCases -> chartText "confirmedCases"
        | HospitalizedCases -> chartText "hospitalizedCases"
        | IcuCases -> chartText "icuCases"

type ChartType =
    | Absolute
    | Absolute100k
    | Relative100k
    static member All = [ Absolute; Absolute100k; Relative100k ]

    static member Default = Absolute100k

    member this.GetName =
        match this with
        | Absolute -> chartText "absolute"
        | Absolute100k -> chartText "absolute100k"
        | Relative100k -> chartText "relative100k"

type DataPoint =
    { Date: DateTime
      DateTo: DateTime
      CasesProtectedWithVaccine: float option
      CasesPartiallyVaccinated: float option
      CasesRecovered: float option
      CasesOther: float option }

type State =
    { DisplayType: DisplayType
      ChartType: ChartType
      Data: StatsData
      WeeklyData: WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg =
    | DisplayTypeChanged of DisplayType
    | ChartTypeChanged of ChartType
    | RangeSelectionChanged of int


let init data weeklyData : State * Cmd<Msg> =
    let state =
        { DisplayType = DisplayType.Default
          ChartType = ChartType.Default
          Data = data
          WeeklyData = weeklyData
          RangeSelectionButtonIndex = 3 }

    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DisplayTypeChanged displayType -> { state with DisplayType = displayType }, Cmd.none
    | ChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none

let tooltipFormatter jsThis =
    let points: obj [] = jsThis?points

    match points with
    | [||] -> ""
    | _ ->
        // points.[0].point.y

        let totalCases =
            points |> Array.sumBy (fun point -> point?point?y)

        let s = StringBuilder()

        let fmtHeader: string = points.[0]?point?fmtHeader

        s.AppendFormat("<b>{0}</b><br/>", fmtHeader)
        |> ignore

        s.Append "<table>" |> ignore

        points
        |> Array.iter
            (fun dp ->
                let label = dp?series?name
                let color = dp?series?color
                let value: float = dp?point?y

                match value with
                | 0. -> ()
                | _ ->
                    let format =
                        "<td style='color: {0}'>●</td>"
                        + "<td style='text-align: left; padding-left: 6px'>{1}:</td>"
                        + "<td style='text-align: right; padding-left: 6px'>"
                        + "<b>{2}</b></td>"
                        + "<td style='text-align: right; padding-left: 10px'>"
                        + "{3}</td>"

                    let percentage =
                        value * 100. / totalCases
                        |> Utils.percentWith1DecimalFormatter

                    s.Append "<tr>" |> ignore

                    let tooltipStr =
                        String.Format(format, color, label, I18N.NumberFormat.formatNumber (value), percentage)

                    s.Append tooltipStr |> ignore
                    s.Append "</tr>" |> ignore)

        s.Append "</table>" |> ignore
        s.ToString()


let renderChartOptions state dispatch =

    let protectedWithVaccineMap =
        state.Data
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = // protected 14 days after 2nd dose
                    if i >= 14 then
                        state.Data.[i - 14]
                            .Vaccination
                            .Administered2nd
                            .ToDate
                    else
                        None

                dp.Date, protectedWithVaccine)
        |> Map.ofSeq

    let protectedWithVaccineOnDay date =
        match protectedWithVaccineMap.TryFind(date) with
        | Some v -> v
        | None -> None

    let partiallyVaccinatedMap =
        state.Data
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let protectedWithVaccine = // protected 14 days after 2nd dose
                    if i >= 14 then
                        state.Data.[i - 14]
                            .Vaccination
                            .Administered2nd
                            .ToDate
                    else
                        None

                dp.Date,
                dp.Vaccination.Administered.ToDate
                |> Utils.subtractIntOption protectedWithVaccine)
        |> Map.ofSeq

    let partiallyVaccinatedOnDay date =
        match partiallyVaccinatedMap.TryFind(date) with
        | Some v -> v
        | None -> None

    let recoveredMap =
        state.Data
        |> Seq.toArray
        |> Seq.mapi (fun i dp -> dp.Date, dp.Cases.RecoveredToDate)
        |> Map.ofSeq

    let recoveredOnDay date =
        match recoveredMap.TryFind(date) with
        | Some v -> v
        | None -> None


    let intOptionToFloat opt =
        match opt with
        | Some i -> Some((float) i)
        | None -> None

    let addFloatOption a b =
        match a, b with
        | Some a, Some b -> Some(a + b)
        | Some a, None -> Some(a)
        | None, Some b -> Some(b)
        | None, None -> None

    let get100k value population =
        match value with
        | Some v ->
            Some(
                v * 100000.
                / (float) (population |> Utils.optionToInt)
            )
        | None -> None

    let checkAndProcess100k (dp: DataPoint) : DataPoint =
        match state.ChartType with
        | Absolute -> dp
        | Absolute100k
        | Relative100k ->
            let protectedWithVaccine = protectedWithVaccineOnDay dp.DateTo
            let partiallyVaccinated = partiallyVaccinatedOnDay dp.DateTo
            let recovered = recoveredOnDay dp.DateTo

            let otherPopulation =
                if state.DisplayType = IcuCases then
                    Utils.Dictionaries.regions.["si"].Population
                    |> Utils.subtractIntOption protectedWithVaccine
                    |> Utils.subtractIntOption partiallyVaccinated
                    |> Utils.subtractIntOption recovered
                else
                    Utils.Dictionaries.regions.["si"].Population
                    |> Utils.subtractIntOption protectedWithVaccine

            { dp with
                  CasesProtectedWithVaccine = get100k dp.CasesProtectedWithVaccine protectedWithVaccine
                  CasesPartiallyVaccinated = get100k dp.CasesPartiallyVaccinated partiallyVaccinated
                  CasesRecovered = get100k dp.CasesRecovered recovered
                  CasesOther = get100k dp.CasesOther otherPopulation }


    let dailyConfirmedData =
        state.Data
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                let confirmedProtectedWithVaccine =
                    match dp.Cases.VaccinatedConfirmedToDate with
                    | Some v ->
                        if i >= 1 then
                            dp.Cases.VaccinatedConfirmedToDate
                            |> Utils.subtractIntOption state.Data.[i - 1].Cases.VaccinatedConfirmedToDate
                        else
                            dp.Cases.VaccinatedConfirmedToDate
                    | _ -> None

                let confirmedOther =
                    match dp.Cases.ConfirmedToDate with
                    | Some v ->
                        if i >= 1 then
                            dp.Cases.ConfirmedToDate
                            |> Utils.subtractIntOption state.Data.[i - 1].Cases.ConfirmedToDate
                            |> Utils.subtractIntOption confirmedProtectedWithVaccine
                        else
                            dp.Cases.ConfirmedToDate
                            |> Utils.subtractIntOption confirmedProtectedWithVaccine
                    | _ -> None

                { Date = dp.Date
                  DateTo = dp.Date
                  CasesProtectedWithVaccine = confirmedProtectedWithVaccine |> intOptionToFloat
                  CasesPartiallyVaccinated = None
                  CasesRecovered = None
                  CasesOther = confirmedOther |> intOptionToFloat })

    let weeklyHospitalizedData =
        state.WeeklyData
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                { Date = dp.Date
                  DateTo = dp.DateTo
                  CasesProtectedWithVaccine = dp.HospitalizedVaccinated |> intOptionToFloat
                  CasesPartiallyVaccinated = None
                  CasesRecovered = None
                  CasesOther = dp.HospitalizedOther |> intOptionToFloat })

    let weeklyIcuData =
        state.WeeklyData
        |> Seq.toArray
        |> Seq.mapi
            (fun i dp ->
                { Date = dp.Date
                  DateTo = dp.DateTo
                  CasesProtectedWithVaccine = dp.IcuVaccinated |> intOptionToFloat
                  CasesPartiallyVaccinated = dp.IcuVaccinatedPartially |> intOptionToFloat
                  CasesRecovered = dp.IcuRecovered |> intOptionToFloat
                  CasesOther = dp.IcuOther |> intOptionToFloat })

    let label, allSeries, lastWeek =
        let color, data, lastWeek =
            match state.DisplayType with
            | ConfirmedCases ->
                let daily =
                    dailyConfirmedData
                    |> Seq.skipWhile (fun dp -> dp.CasesProtectedWithVaccine.IsNone) // head
                    |> Seq.rev
                    |> Seq.skipWhile (fun dp -> dp.CasesProtectedWithVaccine.IsNone) // tail
                    |> Seq.rev
                    |> Seq.map checkAndProcess100k

                let emptyRec =
                    { Date = DateTime.MaxValue
                      DateTo = DateTime.MinValue
                      CasesProtectedWithVaccine = None
                      CasesPartiallyVaccinated = None
                      CasesRecovered = None
                      CasesOther = None }

                let mutable sumRec = emptyRec

                let weeklyConfirmedData =
                    seq {
                        sumRec <- emptyRec

                        for dp in daily do
                            sumRec <-
                                { sumRec with
                                      Date =
                                          if dp.Date.CompareTo(sumRec.Date) < 0 then
                                              dp.Date
                                          else
                                              sumRec.Date
                                      DateTo =
                                          if dp.DateTo.CompareTo(sumRec.Date) > 0 then
                                              dp.DateTo
                                          else
                                              sumRec.Date
                                      CasesProtectedWithVaccine =
                                          sumRec.CasesProtectedWithVaccine
                                          |> addFloatOption dp.CasesProtectedWithVaccine
                                      CasesOther = sumRec.CasesOther |> addFloatOption dp.CasesOther }

                            if dp.Date.DayOfWeek = DayOfWeek.Sunday then
                                yield sumRec
                                sumRec <- emptyRec

                        if sumRec.Date <> DateTime.MaxValue then
                            yield sumRec // flush
                    }

                "#d5c768", weeklyConfirmedData, weeklyConfirmedData |> Seq.tryLast


            | HospitalizedCases ->
                "#de9a5a",
                weeklyHospitalizedData
                |> Seq.skipWhile (fun dp -> dp.CasesProtectedWithVaccine.IsNone)
                |> Seq.rev
                |> Seq.skipWhile (fun dp -> dp.CasesOther.IsNone) // skip empty weeks at tail
                |> Seq.rev
                |> Seq.map checkAndProcess100k,
                None

            | IcuCases ->
                "#fb6a4a",
                weeklyIcuData
                |> Seq.skipWhile (fun dp -> dp.CasesOther.IsNone)
                |> Seq.map checkAndProcess100k,
                None

        let startDate =
            data |> Seq.map (fun dp -> dp.Date) |> Seq.min // TODO: can we get it from raneg selector?

        let endDate =
            data |> Seq.map (fun dp -> dp.DateTo) |> Seq.max // TODO: can we get it from raneg selector?

        let totalProtected =
            data
            |> Seq.map (fun dp -> protectedWithVaccineOnDay (dp.DateTo))
            |> Seq.last
            |> Option.defaultValue 0

        let otherC =
            data
            |> Seq.sumBy
                (fun dp ->
                    (dp.CasesOther |> Option.defaultValue 0.)
                    + (dp.CasesPartiallyVaccinated
                       |> Option.defaultValue 0.)
                    + (dp.CasesRecovered |> Option.defaultValue 0.))

        let protectedC =
            data
            |> Seq.sumBy
                (fun dp ->
                    dp.CasesProtectedWithVaccine
                    |> Option.defaultValue 0.)

        let multiple =
            Utils.roundTo1Decimal (otherC / protectedC)

        let label =
            match state.ChartType with
            | Absolute ->
                let txtId =
                    match state.DisplayType with
                    | ConfirmedCases -> "charts.vaccineEffect.confirmedCasesAbs"
                    | HospitalizedCases -> "charts.vaccineEffect.hospitalizedCasesAbs"
                    | IcuCases -> "charts.vaccineEffect.icuCasesAbs"

                I18N.tOptions
                    txtId
                    {| startDate = startDate
                       endDate = endDate
                       protectedToDatePct =
                           Utils.percentWith3DecimalFormatter (protectedC * 100. / (float) totalProtected)
                       totalProtected = I18N.NumberFormat.formatNumber totalProtected
                       protectedC = I18N.NumberFormat.formatNumber protectedC
                       protectedPct = Utils.percentWith1DecimalFormatter (protectedC * 100. / (protectedC + otherC))
                       otherC = I18N.NumberFormat.formatNumber otherC
                       otherPct = Utils.percentWith1DecimalFormatter (otherC * 100. / (protectedC + otherC)) |}
            | _ ->
                let txtId =
                    match state.DisplayType with
                    | ConfirmedCases -> "charts.vaccineEffect.confirmedCasesRatio"
                    | HospitalizedCases -> "charts.vaccineEffect.hospitalizedCasesRatio"
                    | IcuCases -> "charts.vaccineEffect.icuCasesRatio"

                I18N.tOptions txtId {| multiple = I18N.NumberFormat.formatNumber multiple |}

        let otherLabel =
            if state.DisplayType = IcuCases then
                "casesOtherShort"
            else
                "casesOther"

        label,
        [ yield
            pojo
                {| name = chartText otherLabel
                   ``type`` = "column"
                   color = color
                   data =
                       data
                       |> Seq.map
                           (fun dp ->
                               {| x = jsDatesMiddle dp.Date dp.DateTo
                                  y = Utils.roundTo1Decimal (dp.CasesOther |> Option.defaultValue 0.)
                                  fmtHeader =
                                      I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |})
                       |> Seq.toArray |}
          if state.DisplayType = IcuCases then
              yield
                  pojo
                      {| name = chartText "casesRecovered"
                         ``type`` = "column"
                         color = "#8cd4b2"
                         data =
                             data
                             |> Seq.map
                                 (fun dp ->
                                     {| x = jsDatesMiddle dp.Date dp.DateTo
                                        y = Utils.roundTo1Decimal (dp.CasesRecovered |> Option.defaultValue 0.)
                                        fmtHeader =
                                            I18N.tOptions
                                                "days.weekYearFromToDate"
                                                {| date = dp.Date; dateTo = dp.DateTo |} |})
                             |> Seq.toArray |}

              yield
                  pojo
                      {| name = chartText "casesPartiallyVaccinated"
                         ``type`` = "column"
                         color = "#189a73"
                         data =
                             data
                             |> Seq.map
                                 (fun dp ->
                                     {| x = jsDatesMiddle dp.Date dp.DateTo
                                        y =
                                            Utils.roundTo1Decimal (
                                                dp.CasesPartiallyVaccinated
                                                |> Option.defaultValue 0.
                                            )
                                        fmtHeader =
                                            I18N.tOptions
                                                "days.weekYearFromToDate"
                                                {| date = dp.Date; dateTo = dp.DateTo |} |})
                             |> Seq.toArray |}
          yield
              pojo
                  {| name = chartText "casesProtected"
                     ``type`` = "column"
                     color = "#0e5842"
                     data =
                         data
                         |> Seq.map
                             (fun dp ->
                                 {| x = jsDatesMiddle dp.Date dp.DateTo
                                    y =
                                        Utils.roundTo1Decimal (
                                            dp.CasesProtectedWithVaccine
                                            |> Option.defaultValue 0.
                                        )
                                    fmtHeader =
                                        I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |})
                         |> Seq.toArray |} ],
        lastWeek


    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccine-effect" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    label,
    {| baseOptions with
           series = List.toArray allSeries
           xAxis =
               baseOptions.xAxis
               |> Array.map (fun xAxis ->
                   match lastWeek with
                   | Some lw ->
                      {| xAxis with
                          tickInterval = 86400000 * 7
                          plotBands =
                                [|
                                   {| from=jsTime <| lw.Date
                                      ``to``=jsTime <| lw.DateTo.AddDays(7.) // ugly hack to fully color incomplete week
                                      color="#ffffe0"
                                    |}
                                |] |} |> pojo
                   | None -> pojo xAxis )
           yAxis =
               baseOptions.yAxis
               |> Array.map
                   (fun yAxis ->
                       {| yAxis with
                              min = None
                              labels =
                                  match state.ChartType with
                                  | Relative100k -> pojo {| format = "{value} %" |}
                                  | _ -> pojo {| format = "{value}" |}
                              reversedStacks = true |})
           credits =
               match state.DisplayType with
               | IcuCases -> chartCreditsHospitals
               | _ -> chartCreditsNIJZ
           plotOptions =
               pojo
                   {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                      series =
                          {| stacking =
                                 match state.ChartType with
                                 | Absolute
                                 | Absolute100k -> "normal"
                                 | Relative100k -> "percent"
                             crisp = false
                             borderWidth = 0
                             pointPadding = 0
                             groupPadding = 0 |} |}
           legend =
               pojo
                   {| enabled = true
                      reversed = true
                      layout = "horizontal" |}
           tooltip =
               pojo
                   {| formatter = fun () -> tooltipFormatter jsThis
                      shared = true
                      split = false
                      useHTML = true |}
           responsive =
               pojo
                   {| rules =
                          [| {| condition = {| maxWidth = 768 |}
                                chartOptions = {| yAxis = [| {| labels = {| enabled = false |} |} |] |} |} |] |} |}



let renderChartContainer state dispatch =
    let label, chart = renderChartOptions state dispatch

    Html.div [ Html.div [ prop.style [ style.height 480 ]
                          prop.className "highcharts-wrapper"
                          prop.children [ chart |> chartFromWindow ] ]
               Html.div [ prop.className "disclaimer"
                          prop.children [ Utils.Markdown.render (label + chartText "disclaimer") ] ] ]

let renderChartTypeSelector (activeChartType: ChartType) dispatch =
    let renderSelector (chartType: ChartType) =
        let active = chartType = activeChartType

        Html.div [ prop.text chartType.GetName
                   prop.onClick (fun _ -> dispatch chartType)
                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (active, "selected") ] ]

    Html.div [ prop.className "chart-display-property-selector"
               ChartType.All
               |> List.map renderSelector
               |> prop.children ]

let renderDisplaySelectors (activeDisplayType: DisplayType) dispatch =
    let renderDisplayTypeSelector (displayTypeToRender: DisplayType) =
        let active = displayTypeToRender = activeDisplayType

        Html.div [ prop.onClick (fun _ -> dispatch displayTypeToRender)
                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (active, "selected") ]
                   prop.text displayTypeToRender.GetName ]

    Html.div [ prop.className "chart-display-property-selector"
               DisplayType.All
               |> List.map renderDisplayTypeSelector
               |> prop.children ]

let render state dispatch =
    Html.div [ Utils.renderChartTopControls [ renderDisplaySelectors state.DisplayType (DisplayTypeChanged >> dispatch)
                                              renderChartTypeSelector state.ChartType (ChartTypeChanged >> dispatch) ]
               renderChartContainer state dispatch ]

let vaccineEffectChart
    (props: {| data: StatsData
               weeklyData: WeeklyStatsData |})
    =
    React.elmishComponent ("VaccineEffectChart", init props.data props.weeklyData, update, render)
