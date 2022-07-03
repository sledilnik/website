[<RequireQualifiedAccess>]
module VaccineEffectAgeChart

open System
open System.Text
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser
open Fable.Core.JsInterop

open Highcharts
open Types

open Data.Vaccinations

let chartText = I18N.chartText "vaccineEffectAge"

type DisplayType =
    | ToDate
    | AllAges
    | AgeBelow65
    | AgeAbove65
    static member All =
        [ ToDate
          AllAges
          AgeBelow65
          AgeAbove65 ]

    static member Default = ToDate

    member this.GetName =
        match this with
        | ToDate -> chartText "toDate"
        | AllAges -> chartText "ageAll"
        | AgeBelow65 -> chartText "ageBelow65"
        | AgeAbove65 -> chartText "ageAbove65"

    member this.GetIndex =
        match this with
        | ToDate
        | AllAges -> 0
        | AgeBelow65 -> 1
        | AgeAbove65 -> 2


type ChartType =
    | Absolute
    | Absolute100k
    static member All = [ Absolute; Absolute100k ]

    static member Default = Absolute100k

    member this.GetName =
        match this with
        | Absolute -> chartText "absolute"
        | Absolute100k -> chartText "absolute100k"

type HospitalizedData =
    { Date: DateTime
      DateTo: DateTime
      VaccinatedIn: float array
      OtherIn: float array
      VaccinatedIn100k: float array
      OtherIn100k: float array }
    static member get_Zero() =
        { Date = DateTime.MaxValue
          DateTo = DateTime.MinValue
          VaccinatedIn = [| 0.; 0.; 0. |]
          OtherIn = [| 0.; 0.; 0. |]
          VaccinatedIn100k = [| 0.; 0.; 0. |]
          OtherIn100k = [| 0.; 0.; 0. |] }

    static member (+)(h1: HospitalizedData, h2: HospitalizedData) =
        { Date =
            if DateTime.Compare(h1.Date, h2.Date) < 1 then
                h1.Date
            else
                h2.Date
          DateTo =
            if DateTime.Compare(h1.DateTo, h2.DateTo) < 1 then
                h2.DateTo
            else
                h1.DateTo
          VaccinatedIn =
            h1.VaccinatedIn
            |> Array.mapi (fun i v -> v + h2.VaccinatedIn.[i])
          OtherIn =
            h1.OtherIn
            |> Array.mapi (fun i v -> v + h2.OtherIn.[i])
          VaccinatedIn100k =
            h1.VaccinatedIn100k
            |> Array.mapi (fun i v -> v + h2.VaccinatedIn100k.[i])
          OtherIn100k =
            h1.OtherIn100k
            |> Array.mapi (fun i v -> v + h2.OtherIn100k.[i]) }

type State =
    { Data: WeeklyEpisariData
      VaccinationData: VaccinationStats array
      Error: string option
      DisplayType: DisplayType
      ChartType: ChartType
      RangeSelectionButtonIndex: int }

type Msg =
    | ConsumeVaccinationData of Result<VaccinationStats array, string>
    | ConsumeServerError of exn
    | DisplayTypeChanged of DisplayType
    | ChartTypeChanged of ChartType
    | RangeSelectionChanged of int


let init data : State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeVaccinationData ConsumeServerError

    let state =
        { Data = data
          VaccinationData = [||]
          Error = None
          DisplayType = DisplayType.Default
          ChartType = ChartType.Default
          RangeSelectionButtonIndex = 3 }

    state, cmd

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeVaccinationData (Ok data) -> { state with VaccinationData = data }, Cmd.none
    | ConsumeVaccinationData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | DisplayTypeChanged displayType -> { state with DisplayType = displayType }, Cmd.none
    | ChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none
    | RangeSelectionChanged buttonIndex -> { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none


let tooltipFormatter state jsThis =
    let points: obj [] = jsThis?points

    match points with
    | [||] -> ""
    | _ ->
        let total =
            points |> Array.sumBy (fun point -> point?point?y)

        let s = StringBuilder()

        let header: string =
            match state.DisplayType with
            | ToDate -> jsThis?x
            | _ -> points.[0]?point?fmtHeader

        s.AppendFormat("<b>{0}</b><br/>", header)
        |> ignore

        s.Append "<table>" |> ignore

        points
        |> Array.iter (fun dp ->
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
                    value * 100. / total
                    |> Utils.percentWith1DecimalFormatter

                s.Append "<tr>" |> ignore

                let tooltipStr =
                    String.Format(
                        format,
                        color,
                        label,
                        I18N.NumberFormat.formatNumber (Utils.roundTo1Decimal value),
                        percentage
                    )

                s.Append tooltipStr |> ignore
                s.Append "</tr>" |> ignore)

        match state.ChartType with
        | Absolute -> // add total
            let format =
                "<td></td>"
                + "<td style='text-align: left; padding-left: 6px'><b>{0}:</b></td>"
                + "<td style='text-align: right; padding-left: 6px'><b>{1}</b></td>"
                + "<td></td>"

            s.Append "<tr>" |> ignore

            let totalTooltip =
                String.Format(format, I18N.t "charts.common.total", I18N.NumberFormat.formatNumber (total))

            s.Append totalTooltip |> ignore
            s.Append "</tr>" |> ignore
        | Absolute100k -> // add ratios
            if points.Length = 2 then // only if both categories selected
                let vaccinated: float = points.[0]?point?y
                let other: float = points.[1]?point?y

                s.Append "<tr><td></td><td></td><td></td><td></td></tr>"
                |> ignore

                let format =
                    "<tr><td></td>"
                    + "<td style='text-align: left; padding-left: 6px'><b>{0}:</b></td>"
                    + "<td style='text-align: right; padding-left: 6px'><b>{1}</b></td>"
                    + "<td></td></tr>"

                let riskRatio =
                    String.Format(
                        format,
                        chartText "riskRatio",
                        I18N.NumberFormat.formatNumber (Utils.roundTo1Decimal (other / vaccinated))
                    )

                s.Append riskRatio |> ignore

                let vaccineEfficiency =
                    String.Format(
                        format,
                        chartText "vaccineEfficiency",
                        Utils.percentWith1DecimalFormatter (100. * (1. - (vaccinated / other)))
                    )

                s.Append vaccineEfficiency |> ignore

        s.Append "</table>" |> ignore
        s.ToString()

let averageOnWeek (dataMap: Map<DateTime, int>) (date: DateTime) (dateTo: DateTime) =
    match dataMap.TryFind(date), dataMap.TryFind(date) with
    | Some v1, Some v2 -> Math.Round((((v1 + v2) |> float) / 2.), 0) |> int
    | Some v1, None -> v1
    | None, Some v2 -> v2
    | None, None -> 0

let get100k value population = value * 100000. / (float) population

let getSummaryData (state: State) (dp: WeeklyEpisariDataPoint) : HospitalizedData =

    let protectedWithVaccineMap =
        state.VaccinationData
        |> Seq.toArray
        |> Seq.mapi (fun i dp ->
            let protectedWithVaccine = // protected 14 days after 2nd dose
                if i >= 15 then
                    state.VaccinationData.[i - 15]
                        .administered2nd
                        .toDate
                    |> Option.defaultValue 0
                else
                    0

            dp.Date, protectedWithVaccine)
        |> Map.ofSeq

    let protectedWithVaccineAbove65Map =
        state.VaccinationData
        |> Seq.toArray
        |> Seq.mapi (fun i dp ->
            let protectedWithVaccine = // protected 14 days after 2nd dose
                if i >= 15 then
                    state.VaccinationData.[i - 15].administeredPerAge
                    |> List.sumBy (fun ag ->
                        match ag.ageFrom, ag.ageTo with
                        | Some f, _ ->
                            if f >= 65 then
                                (ag.administered2nd |> Option.defaultValue 0)
                            else
                                0
                        | _, _ -> 0)
                else
                    0

            dp.Date, protectedWithVaccine)
        |> Map.ofSeq

    let vaccinatedIn =
        dp.CovidInVaccinated
        |> Option.defaultValue 0
        |> float

    let otherIn =
        ((dp.CovidIn |> Option.defaultValue 0) |> float)
        - vaccinatedIn

    let protectedWithVaccine =
        averageOnWeek protectedWithVaccineMap dp.Date dp.DateTo

    let otherPopulation =
        (Utils.Dictionaries.regions.["si"].Population
         |> Option.defaultValue 0)
        - protectedWithVaccine

    let vaccinatedInAbove65 =
        (dp.PerAge
         |> List.sumBy (fun ag ->
             match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
             | Some f, None ->
                 if f = 65 then
                     (ag.VaccinatedIn |> Option.defaultValue 0)
                 else
                     0
             | _, _ -> 0))
        |> float

    let otherInAbove65 =
        ((dp.PerAge
          |> List.sumBy (fun ag ->
              match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
              | Some f, _ ->
                  if f >= 65 then
                      (ag.CovidIn |> Option.defaultValue 0)
                  else
                      0
              | _, _ -> 0))
         |> float)
        - vaccinatedInAbove65

    let protectedWithVaccineAbove65 =
        averageOnWeek protectedWithVaccineAbove65Map dp.Date dp.DateTo

    let otherAbove65Population =
        (Utils.AgePopulationStats.agePopulationStats
         |> Map.toList
         |> List.sumBy (fun (key, ag) ->
             match ag.GroupKey.AgeFrom, ag.GroupKey.AgeTo with
             | Some f, _ -> if f >= 65 then ag.Population else 0
             | _, _ -> 0))
        - protectedWithVaccineAbove65

    { Date = dp.Date
      DateTo = dp.DateTo
      VaccinatedIn =
        [| vaccinatedIn
           vaccinatedIn - vaccinatedInAbove65
           vaccinatedInAbove65 |]
      OtherIn =
        [| otherIn
           otherIn - otherInAbove65
           otherInAbove65 |]
      VaccinatedIn100k =
        [| get100k vaccinatedIn protectedWithVaccine
           get100k (vaccinatedIn - vaccinatedInAbove65) (protectedWithVaccine - protectedWithVaccineAbove65)
           get100k vaccinatedInAbove65 protectedWithVaccineAbove65 |]
      OtherIn100k =
        [| get100k otherIn otherPopulation
           get100k (otherIn - otherInAbove65) (otherPopulation - otherAbove65Population)
           get100k otherInAbove65 otherAbove65Population |] }


let renderChartOptions (state: State) dispatch =

    let summaryData =
        state.Data
        |> Array.skipWhile (fun dp -> dp.CovidInVaccinated.IsNone)
        |> Array.map (fun dp -> getSummaryData state dp)
        |> Array.sum

    let allSeries =
        [| yield
            pojo
                {| name = chartText "vaccinatedIn"
                   ``type`` = "column"
                   color = "#0e5842"
                   data =
                    match state.ChartType with
                    | Absolute -> summaryData.VaccinatedIn
                    | Absolute100k -> summaryData.VaccinatedIn100k
                    |> Array.map (fun dp -> {| y = dp |}) |}
           yield
               pojo
                   {| name = chartText "otherIn"
                      ``type`` = "column"
                      color = "#de9a5a"
                      data =
                       match state.ChartType with
                       | Absolute -> summaryData.OtherIn
                       | Absolute100k -> summaryData.OtherIn100k
                       |> Array.map (fun dp -> {| y = dp |}) |} |]

    let label =
        I18N.tOptions
            "charts.vaccineEffectAge.hospitalizedIn"
            {| startDate = summaryData.Date
               endDate = summaryData.DateTo |}

    pojo
        {| optionsWithOnLoadEvent "covid19-vaccine-effect-summary" with
            chart = pojo {| ``type`` = "column" |}
            title =
                pojo
                    {| text = label
                       style = pojo {| fontSize = "12px" |} |}
            xAxis =
                [| {| ``type`` = "category"
                      categories =
                       [| chartText "ageAll"
                          chartText "ageBelow65"
                          chartText "ageAbove65" |] |} |]
            yAxis =
                [| {| opposite = true
                      title = {| text = None |} |} |]
            series = allSeries
            plotOptions =
                pojo
                    {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                       series =
                        pojo
                            {| stacking = None
                               crisp = false
                               borderWidth = 0
                               pointPadding = 0
                               groupPadding = 0.1 |} |}
            legend =
                pojo
                    {| enabled = true
                       layout = "horizontal" |}
            tooltip =
                pojo
                    {| formatter = fun () -> tooltipFormatter state jsThis
                       shared = true
                       split = false
                       useHTML = true |}
            credits = chartCreditsNIJZ
            responsive =
                pojo
                    {| rules =
                        [| {| condition = {| maxWidth = 768 |}
                              chartOptions = {| yAxis = [| {| labels = pojo {| enabled = false |} |} |] |} |} |] |}
            navigator = pojo {| enabled = false |}
            scrollbar = pojo {| enabled = false |}
            rangeSelector = pojo {| enabled = false |} |}


let renderWeeklyChart state dispatch =

    let getOtherInData state dp =
        {| x = jsDatesMiddle dp.Date dp.DateTo
           y =
            match state.ChartType with
            | Absolute -> dp.OtherIn.[state.DisplayType.GetIndex]
            | Absolute100k -> dp.OtherIn100k.[state.DisplayType.GetIndex]
           fmtHeader = I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |}

    let getVaccinatedInData state dp =
        {| x = jsDatesMiddle dp.Date dp.DateTo
           y =
            match state.ChartType with
            | Absolute -> dp.VaccinatedIn.[state.DisplayType.GetIndex]
            | Absolute100k -> dp.VaccinatedIn100k.[state.DisplayType.GetIndex]
           fmtHeader = I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |}

    let allSeries =
        [| yield
            pojo
                {| name = chartText "vaccinatedIn"
                   ``type`` = "column"
                   color = "#0e5842"
                   data =
                    state.Data
                    |> Array.skipWhile (fun dp -> dp.CovidInVaccinated.IsNone)
                    |> Array.map (fun dp -> getSummaryData state dp)
                    |> Array.map (fun dp -> getVaccinatedInData state dp)
                    |> Seq.toArray |}

           yield
               pojo
                   {| name = chartText "otherIn"
                      ``type`` = "column"
                      color = "#de9a5a"
                      data =
                       state.Data
                       |> Array.skipWhile (fun dp -> dp.CovidInVaccinated.IsNone)
                       |> Array.map (fun dp -> getSummaryData state dp)
                       |> Array.map (fun dp -> getOtherInData state dp)
                       |> Seq.toArray |} |]

    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccine-effect-age" state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    pojo
        {| baseOptions with
            series = allSeries
            credits = chartCreditsNIJZ
            plotOptions =
                pojo
                    {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                       series =
                        pojo
                            {| stacking = None
                               crisp = false
                               borderWidth = 0
                               pointPadding = 0
                               groupPadding = 0 |} |}
            legend =
                pojo
                    {| enabled = true
                       layout = "horizontal" |}
            tooltip =
                pojo
                    {| formatter = fun () -> tooltipFormatter state jsThis
                       shared = true
                       split = false
                       useHTML = true |}
            responsive =
                pojo
                    {| rules =
                        [| {| condition = {| maxWidth = 768 |}
                              chartOptions = {| yAxis = [| {| labels = pojo {| enabled = false |} |} |] |} |} |] |} |}


let renderChartContainer state dispatch =
    Html.div [ Html.div [ prop.style [ style.height 480 ]
                          prop.className "highcharts-wrapper"
                          prop.children [ match state.DisplayType with
                                          | ToDate ->
                                              React.keyedFragment (
                                                  1,
                                                  [ renderChartOptions state dispatch
                                                    |> Highcharts.chart ]
                                              )
                                          | _ ->
                                              React.keyedFragment (
                                                  2,
                                                  [ renderWeeklyChart state dispatch
                                                    |> Highcharts.chartFromWindow ]
                                              ) ] ]
               Html.div [ prop.className "disclaimer"
                          prop.children [ Utils.Markdown.render (chartText "disclaimer") ] ] ]

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

let renderDisplaySelectors state dispatch =
    let renderSelector (displayType: DisplayType) dispatch =
        Html.div [ let isActive = state.DisplayType = displayType
                   prop.onClick (fun _ -> DisplayTypeChanged displayType |> dispatch)

                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (isActive, "selected") ]

                   prop.text displayType.GetName ]

    Html.div [ prop.className "chart-display-property-selector"
               prop.children (
                   DisplayType.All
                   |> Seq.map (fun dt -> renderSelector dt dispatch)
               ) ]

let render state dispatch =
    match state.VaccinationData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [ Utils.renderChartTopControls [ renderDisplaySelectors state dispatch
                                                  renderChartTypeSelector state.ChartType (ChartTypeChanged >> dispatch) ]
                   renderChartContainer state dispatch ]

let vaccineEffectAgeChart (props: {| data: WeeklyEpisariData |}) =
    React.elmishComponent ("VaccineEffectAgeChart", init props.data, update, render)
