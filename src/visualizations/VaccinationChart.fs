[<RequireQualifiedAccess>]
module VaccinationChart

open System
open System.Text
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser
open Fable.Core.JsInterop

open Types
open Highcharts

open Data.Vaccinations

let chartText = I18N.chartText "vaccination"

type ScaleType = Absolute | Relative

type DisplayType =
    | Used
    | ByManufacturer
    | Unused
    | ByWeek
    | ByAge1st
    | ByAgeAll
    static member All = [ Used ; ByManufacturer; Unused; ByWeek; ByAgeAll; ByAge1st; ]
    static member Default = Used
    static member GetName =
        function
        | Used -> chartText "used"
        | ByManufacturer -> chartText "byManufacturer"
        | Unused -> chartText "unused"
        | ByWeek -> chartText "byWeek"
        | ByAge1st -> chartText "byAge1st"
        | ByAgeAll -> chartText "byAgeAll"
    static member ShowScaleType =
        function
        | ByAgeAll | ByAge1st -> true
        | _ -> false

let AllVaccinationTypes = [
    "janssen",     "#019cdc"
    "az",          "#ffa600"
    "moderna",     "#f95d6a"
    "pfizer",      "#73ccd5"
]

type State =
    { VaccinationData: VaccinationStats array
      Error: string option
      DisplayType: DisplayType
      ScaleType: ScaleType
      RangeSelectionButtonIndex: int }


type Msg =
    | ConsumeVaccinationData of Result<VaccinationStats array, string>
    | ConsumeServerError of exn
    | DisplayTypeChanged of DisplayType
    | ScaleTypeChanged of ScaleType
    | RangeSelectionChanged of int

let init: State * Cmd<Msg> =
    let cmd =
        Cmd.OfAsync.either getOrFetch () ConsumeVaccinationData ConsumeServerError

    let state =
        { VaccinationData = [||]
          Error = None
          DisplayType = DisplayType.Default
          ScaleType = ScaleType.Relative
          RangeSelectionButtonIndex = 0 }

    state, cmd

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumeVaccinationData (Ok data) ->
        { state with VaccinationData = data }, Cmd.none
    | ConsumeVaccinationData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | DisplayTypeChanged dt ->
        { state with DisplayType = dt }, Cmd.none
    | ScaleTypeChanged st ->
        { state with ScaleType = st }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let defaultTooltip hdrFormat formatter =
    {|
        split = false
        shared = true
        useHTML = true
        formatter = formatter
        headerFormat = hdrFormat
        xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>"
    |} |> pojo

let defaultSeriesOptions stackType =
    {|
        stacking = stackType
        crisp = false
        borderWidth = 0
        pointPadding = 0
        groupPadding = 0
    |}

let subtractWeekly curr prev =
    match curr, prev with
    | Some c, Some p  -> Some (c - p)
    | _ -> None

let calcUnusedDoses delivered used =
    match delivered, used with
    | Some d, Some u  -> Some (d - u)
    | _ -> None




// Highcharts will sum columns together when there aren't enough pixels to draw them individually
// As data in some of the vaccination charts is cumulative already, the aggregation method must be "high"
// instead of the default "sum"
// Docs: https://api.highcharts.com/highstock/series.column.dataGrouping.approximation
// This fixes https://github.com/sledilnik/website/issues/927
let dataGroupingConfigurationForCumulativeData = pojo {| approximation = "high" |}


let renderVaccinationChart state dispatch =

    let allSeries =
        match state.DisplayType with
        | Used ->
            [
              yield
                pojo
                    {| name = chartText "administered"
                       ``type`` = "column"
                       color = "#189a73"
                       dataGrouping = dataGroupingConfigurationForCumulativeData
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.administered.toDate)) |}
              yield
                pojo
                    {| name = chartText "administered2nd"
                       ``type`` = "column"
                       color = "#0e5842"
                       dataGrouping = dataGroupingConfigurationForCumulativeData
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.administered2nd.toDate)) |}
              yield
                pojo
                    {| name = chartText "deliveredDoses"
                       ``type`` = "line"
                       color = "#73ccd5"
                       dataGrouping = dataGroupingConfigurationForCumulativeData
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.deliveredToDate)) |}
              yield
                pojo
                    {| name = chartText "usedDoses"
                       ``type`` = "line"
                       color = "#20b16d"
                       dataGrouping = dataGroupingConfigurationForCumulativeData
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, dp.usedToDate)) |}
              yield
                pojo
                    {| name = chartText "unusedDoses"
                       ``type`` = "line"
                       color = "#ffa600"
                       dataGrouping = dataGroupingConfigurationForCumulativeData
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> (dp.JsDate12h, calcUnusedDoses dp.deliveredToDate dp.usedToDate)) |}
            ]
        | _ -> []

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccination"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = List.toArray allSeries
        yAxis =
            baseOptions.yAxis
            |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
        plotOptions =
            pojo
               {| line = pojo {| dataLabels = pojo {| enabled = false |}; marker = pojo {| enabled = false |} |}
                  series = defaultSeriesOptions None |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
        tooltip = defaultTooltip "{point.key}<br>" None
    |}


let renderStackedChart state dispatch =

    let tooltipFormatter jsThis =
        let points: obj[] = jsThis?points

        match points with
        | [||] -> ""
        | _ ->
            let total = points |> Array.sumBy(fun point -> float point?point?y)

            let s = StringBuilder()

            let date = points.[0]?point?date
            s.AppendFormat ("<b>{0}</b><br/>", date.ToString()) |> ignore

            s.Append "<table>" |> ignore

            points
            |> Array.iter
                   (fun dp ->
                        match dp?point?y with
                        | 0 -> ()
                        | value ->
                            let format =
                                "<td style='color: {0}'>●</td>"+
                                "<td style='text-align: left; padding-left: 6px'>{1}:</td>"+
                                "<td style='text-align: right; padding-left: 6px'><b>{2}</b></td>"

                            s.Append "<tr>" |> ignore
                            let dpTooltip =
                                String.Format
                                    (format,
                                     dp?series?color,
                                     dp?series?name,
                                     I18N.NumberFormat.formatNumber(value))
                            s.Append dpTooltip |> ignore
                            s.Append "</tr>" |> ignore
                    )
            let format =
                "<td></td>"+
                "<td style='text-align: left; padding-left: 6px'><b>{0}:</b></td>"+
                "<td style='text-align: right; padding-left: 6px'><b>{1}</b></td>"

            s.Append "<tr>" |> ignore
            let totalTooltip =
                String.Format
                    (format,
                     I18N.t "charts.common.total",
                     I18N.NumberFormat.formatNumber(total))
            s.Append totalTooltip |> ignore
            s.Append "</tr>" |> ignore

            s.Append "</table>" |> ignore
            s.ToString()

    let getValue dp vType =
        match state.DisplayType with
        | Unused ->
            calcUnusedDoses (dp.deliveredByManufacturer.TryFind(vType)) (dp.usedByManufacturer.TryFind(vType))
        | _ ->
            dp.deliveredByManufacturer.TryFind(vType)

    let allSeries = seq {
        for vType, vColor in AllVaccinationTypes do
            yield
                pojo
                    {| name = chartText vType
                       ``type`` = "column"
                       color = vColor
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp ->
                                            {|
                                                x = dp.JsDate12h
                                                y = getValue dp vType
                                                date = I18N.tOptions "days.longerDate" {| date = dp.Date |}
                                            |} |> pojo ) |}
    }

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccination-stacked"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = Seq.toArray allSeries
        yAxis =
            baseOptions.yAxis
            |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
        plotOptions =
            pojo
               {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                  series = defaultSeriesOptions "normal" |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
        tooltip = defaultTooltip "" (fun () -> tooltipFormatter jsThis)
    |}


let renderAgeChart state dispatch =

    let tooltipFormatter jsThis =
        let points: obj[] = jsThis?points

        match points with
        | [||] -> ""
        | _ ->
            let totalVaccinated = points |> Array.sumBy(fun point -> float point?point?vaccinated)
            let totalPopulation = points |> Array.sumBy(fun point -> float point?point?population)

            let s = StringBuilder()

            let date = points.[0]?point?date
            s.AppendFormat ("<b>{0}</b><br/>", date.ToString()) |> ignore

            s.Append "<table>" |> ignore

            points
            |> Array.sortByDescending
                (fun ag ->
                    match state.ScaleType with
                    | Absolute -> 0.
                    | Relative -> ((float)ag?point?vaccinated / (float)ag?point?population))
            |> Array.iter
                   (fun ageGroup ->
                        let ageGroupLabel = ageGroup?series?name
                        let ageGroupColor = ageGroup?series?color
                        let dataPoint = ageGroup?point

                        let dataValue: int = dataPoint?vaccinated
                        let population: int = dataPoint?population

                        match dataValue with
                        | 0 -> ()
                        | _ ->
                            let format =
                                "<td style='color: {0}'>●</td>"+
                                "<td style='text-align: center; padding-left: 6px'>{1}:</td>"+
                                "<td style='text-align: right; padding-left: 6px'>{2}</td>" +
                                "<td style='text-align: right; padding-left: 10px'><b>{3}</b></td>"

                            let percentage = float dataValue * 100. / float population |> Utils.percentWith1DecimalFormatter

                            s.Append "<tr>" |> ignore
                            let ageGroupTooltip =
                                String.Format
                                    (format,
                                     ageGroupColor,
                                     ageGroupLabel,
                                     I18N.NumberFormat.formatNumber(dataValue),
                                     percentage)
                            s.Append ageGroupTooltip |> ignore
                            s.Append "</tr>" |> ignore
                    )
            let format =
                "<td></td>"+
                "<td style='text-align: center; padding-left: 6px'><b>{0}:</b></td>"+
                "<td style='text-align: right; padding-left: 6px'>{1}</td>" +
                "<td style='text-align: right; padding-left: 10px'><b>{2}</b></td>"

            let percentage = float totalVaccinated * 100. / float totalPopulation |> Utils.percentWith1DecimalFormatter
            s.Append "<tr>" |> ignore
            let ageGroupTooltip =
                String.Format
                    (format,
                     I18N.t "charts.common.total",
                     I18N.NumberFormat.formatNumber(totalVaccinated),
                     percentage)
            s.Append ageGroupTooltip |> ignore
            s.Append "</tr>" |> ignore

            s.Append "</table>" |> ignore
            s.ToString()

    let getAgeGroupRec dp ageGroup population =
        let aG =
            dp.administeredPerAge
            |> List.find (fun aG -> aG.ageFrom = ageGroup.AgeFrom && aG.ageTo = ageGroup.AgeTo)
        let value =
            match state.DisplayType with
            | ByAge1st -> aG.administered
            | _ -> aG.administered2nd
        let y =
            match state.ScaleType with
            | Absolute ->
                match value with
                | Some v -> Some ((float)v)
                | _ -> None
            | Relative ->
                match value with
                | Some v -> Some ((float)v / (float)population * 100.)
                | _ -> None
        {|
            x = dp.JsDate12h
            y = y
            vaccinated = value
            population = population
            date = I18N.tOptions "days.longerDate" {| date = dp.Date |}
        |} |> pojo

    let allAgeGroups =
        state.VaccinationData
        |> Array.tryLast
        |> Option.map (fun dp -> dp.administeredPerAge)
        |> Option.defaultValue List.empty
        |> List.mapi (fun idx aG -> { AgeFrom = aG.ageFrom; AgeTo = aG.ageTo }, idx)

    let ageGroupColors =
            [| "#FFDA6B";"#E9B825";"#AEEFDB";"#80DABF";"#52C4A2";"#43B895";"#33AB87";"#2DA782"
               "#26A37D";"#189A73";"#F4B2E0";"#E586C8";"#D559B0";"#C33B9A";"#B01C83";"#9e1975" |]

    let ageGroupPopulation =
            [| 372727; 141046; 113475; 134316; 151422; 160837; 150023; 151029
               150947; 144449; 135564; 101248;  77562;  60447;  37062;  17972 |]

    let allSeries = seq {
        for ageGroup, idx in allAgeGroups do
            yield
                pojo
                    {| name = ageGroup.Label
                       ``type`` =
                            match state.ScaleType with
                            | Absolute -> "column"
                            | Relative -> "line"
                       color = ageGroupColors.[idx]
                       data =
                           state.VaccinationData
                           |> Array.map (fun dp -> getAgeGroupRec dp ageGroup ageGroupPopulation.[idx]) |}
    }

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let stackType =
        match state.ScaleType with
        | Absolute -> Some "normal"
        | Relative -> None
    let baseOptions =
        basicChartOptions Linear "covid19-vaccination-stacked"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = Seq.toArray allSeries
        yAxis =
            baseOptions.yAxis
            |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
        plotOptions =
            pojo
               {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                  series = defaultSeriesOptions stackType |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
        tooltip = defaultTooltip "" (fun () -> tooltipFormatter jsThis)
    |}

let renderWeeklyChart state dispatch =

    let valueToWeeklyDataPoint (date: DateTime) (value : int option) =
        let fromDate = date.AddDays(-7.)
        {|
            x = date |> jsTime
            y = value
            fmtHeader =
              I18N.tOptions "days.weekYearFromToDate" {| date = fromDate; dateTo = date |}
        |}

    let toWeeklyData (dataArray : VaccinationStats array) =
        dataArray
        |> Array.skipWhile (fun dp -> dp.Date.DayOfWeek <> DayOfWeek.Sunday)
        |> Array.mapi (fun i e -> if i % 7 = 0 then Some(e) else None)
        |> Array.choose id
        |> Array.pairwise

    let allSeries = seq {
        yield
            pojo
                {| name = chartText "administered"
                   ``type`` = "column"
                   color = "#189a73"
                   data =
                       state.VaccinationData
                       |> toWeeklyData
                       |> Array.map (
                            fun (prevW, currW) ->
                                valueToWeeklyDataPoint
                                    currW.Date (subtractWeekly currW.administered.toDate prevW.administered.toDate)) |}
        yield
            pojo
                {| name = chartText "administered2nd"
                   ``type`` = "column"
                   color = "#0e5842"
                   data =
                       state.VaccinationData
                       |> toWeeklyData
                       |> Array.map (
                            fun (prevW, currW) ->
                                valueToWeeklyDataPoint
                                    currW.Date (subtractWeekly currW.administered2nd.toDate prevW.administered2nd.toDate)) |}
        yield
            pojo
                {| name = chartText "deliveredDoses"
                   ``type`` = "line"
                   color = "#73ccd5"
                   data =
                       state.VaccinationData
                       |> toWeeklyData
                       |> Array.map (
                            fun (prevW, currW) ->
                                valueToWeeklyDataPoint
                                    currW.Date (subtractWeekly currW.deliveredToDate prevW.deliveredToDate)) |}
        yield
            pojo
                {| name = chartText "usedDoses"
                   ``type`` = "line"
                   color = "#20b16d"
                   data =
                       state.VaccinationData
                       |> toWeeklyData
                       |> Array.map (
                            fun (prevW, currW) ->
                                valueToWeeklyDataPoint
                                    currW.Date (subtractWeekly currW.usedToDate prevW.usedToDate)) |}
        yield
            pojo
                {| name = chartText "unusedDoses"
                   ``type`` = "line"
                   color = "#ffa600"
                   data =
                       state.VaccinationData
                       |> toWeeklyData
                       |> Array.map (
                            fun (prevW, currW) ->
                                valueToWeeklyDataPoint
                                    currW.Date (calcUnusedDoses currW.deliveredToDate currW.usedToDate)) |}
    }

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions Linear "covid19-vaccination-weekly"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick
    {| baseOptions with
        series = Seq.toArray allSeries
        yAxis =
            baseOptions.yAxis
            |> Array.map (fun ax -> {| ax with showFirstLabel = false |})
        plotOptions =
            pojo
               {| column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                  series = defaultSeriesOptions None |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
        tooltip = defaultTooltip "{point.fmtHeader}<br>" None
    |}


let renderChartContainer (state: State) dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [
                    match state.DisplayType with
                    | ByWeek ->
                        renderWeeklyChart state dispatch |> Highcharts.chartFromWindow
                    | Used ->
                        renderVaccinationChart state dispatch |> Highcharts.chartFromWindow
                    | Unused | ByManufacturer ->
                        renderStackedChart state dispatch |> Highcharts.chartFromWindow
                    | ByAgeAll | ByAge1st ->
                        renderAgeChart state dispatch |> Highcharts.chartFromWindow ] ]

let renderScaleTypeSelectors state dispatch =
    let renderScaleTypeSelector
        (scaleType : ScaleType)
        (activeScaleType : ScaleType)
        (label : string) =
        let defaultProps =
            [ prop.text label
              Utils.classes [
                  (true, "chart-display-property-selector__item")
                  (scaleType = activeScaleType, "selected") ] ]
        if scaleType = activeScaleType then
            Html.div defaultProps
        else
            Html.div
                ((prop.onClick (fun _ -> dispatch scaleType)) :: defaultProps)

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            renderScaleTypeSelector
                Absolute state.ScaleType (I18N.t "charts.common.absolute")
            renderScaleTypeSelector
                Relative state.ScaleType (I18N.t "charts.common.populationShare")
        ]
    ]

let renderDisplaySelectors state dispatch =
    let renderSelector (dt: DisplayType) dispatch =
        Html.div [ let isActive = state.DisplayType = dt
                   prop.onClick (fun _ -> DisplayTypeChanged dt |> dispatch)
                   Utils.classes [ (true, "chart-display-property-selector__item")
                                   (isActive, "selected") ]
                   prop.text (DisplayType.GetName dt) ]

    Html.div [ prop.className "chart-display-property-selector"
               prop.children
                   (DisplayType.All
                    |> Seq.map (fun dt -> renderSelector dt dispatch)) ]


let render (state: State) dispatch =
    match state.VaccinationData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            Utils.renderChartTopControls [
                renderDisplaySelectors state dispatch
                if DisplayType.ShowScaleType state.DisplayType then
                    renderScaleTypeSelectors state (ScaleTypeChanged >> dispatch) ]
            renderChartContainer state dispatch ]

let vaccinationChart () =
    React.elmishComponent ("VaccinationChart", init, update, render)
