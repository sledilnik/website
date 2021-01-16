module RegionsChartViz.Rendering

open RegionsChartViz.Analysis
open RegionsChartViz.Synthesis

open Browser.Types
open Elmish
open Fable.Core
open Highcharts
open JsInterop
open Feliz
open Feliz.ElmishComponents

open System.Text
open Types

type RegionInfo = {
    Color: string
}

let regionsInfo = dict[
    "ce", { Color = "#665191" }
    "kk", { Color = "#d45087" }
    "kp", { Color = "#70a471" }
    "kr", { Color = "#ffa600" }
    "lj", { Color = "#457844" }
    "ng", { Color = "#dba51d" }
    "nm", { Color = "#afa53f" }
    "mb", { Color = "#f95d6a" }
    "ms", { Color = "#024a66" }
    "po", { Color = "#a05195" }
    "sg", { Color = "#777c29" }
    "za", { Color = "#10829a" }
]

type Msg =
    | ToggleAllRegions of bool
    | ToggleRegionVisible of string
    | MetricTypeChanged of MetricType
    | ScaleTypeChanged of ScaleType
    | RangeSelectionChanged of int

let init (config: RegionsChartConfig) (data : RegionsData)
    : RegionsChartState * Cmd<Msg> =
    let lastDataPoint = List.last data

    let regionsWithoutExcluded =
        lastDataPoint.Regions
        |> List.filter (fun region ->
            Set.contains region.Name Utils.Dictionaries.excludedRegions |> not)

    let regionsSorted =
        regionsWithoutExcluded
        |> List.sortByDescending (fun region -> region.ActiveCases)

    let regConfig = 
        regionsSorted
        |> List.map (fun region ->
            let regionKey = region.Name
            let color = regionsInfo.[regionKey].Color
            { Key = regionKey
              Color = color
              Visible = true } )

    let regionsConfig =
        match config.RelativeTo with
        | Pop100k ->
            [ { Key = "si"; Color = "#696969"; Visible = true } ]
            |> List.append regConfig
        | _ ->
            regConfig
             
    { ScaleType = Linear; MetricType = MetricType.Default
      ChartConfig = config
      RegionsData = data
      RegionsSorted = regionsSorted
      RegionsConfig = regionsConfig
      RangeSelectionButtonIndex = 0
      ShowAll = true },
    Cmd.none

let update (msg: Msg) (state: RegionsChartState)
    : RegionsChartState * Cmd<Msg> =
    match msg with
    | ToggleAllRegions visibleOrHidden ->
        let newRegionsConfig =
            state.RegionsConfig
            |> List.map (fun region ->
                { Key = region.Key
                  Color = region.Color
                  Visible = visibleOrHidden } )
        { state with RegionsConfig = newRegionsConfig; ShowAll = not state.ShowAll }, Cmd.none
    | ToggleRegionVisible regionKey ->
        let newRegionsConfig =
            state.RegionsConfig
            |> List.map (fun m ->
                if m.Key = regionKey
                then { m with Visible = not m.Visible }
                else m)
        { state with RegionsConfig = newRegionsConfig }, Cmd.none
    | MetricTypeChanged newMetricType ->
        { state with MetricType = newMetricType }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let tooltipValueFormatter (state: RegionsChartState) value =
    match state.ChartConfig.RelativeTo with
    // todo igor: format to int for absolute values
    | Absolute -> Utils.formatToInt value
    | Pop100k -> Utils.formatTo1DecimalWithTrailingZero value

let tooltipFormatter (state: RegionsChartState) _ jsThis =
    let points: obj[] = jsThis?points

    match points with
    | [||] -> ""
    | _ ->
        let s = StringBuilder()
        let date = points.[0]?point?date
        s.AppendFormat ("<b>{0}</b><br/>", date.ToString()) |> ignore
        s.Append "<table>" |> ignore

        points
        |> Array.sortByDescending
               (fun region ->
                    let dataValue: float = region?point?y
                    dataValue)
        |> Array.iter
               (fun region ->
                    let regionName = region?series?name
                    let regionColor = region?series?color

                    let dataValue: float = region?point?y

                    s.Append "<tr>" |> ignore
                    let regionTooltip =
                        sprintf
                            "<td><span style='color:%s'>●</span></td><td>%s</td><td style='text-align: right; padding-left: 10px'>%s</td>"
                            regionColor
                            regionName
                            (I18N.NumberFormat.formatNumber(dataValue, {| maximumFractionDigits=1 |}))
                    s.Append regionTooltip |> ignore
                    s.Append "</tr>" |> ignore
                )

        s.Append "</table>" |> ignore
        s.ToString()

let renderChartOptions (state : RegionsChartState) dispatch =

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions
            state.ScaleType "covid19-regions"
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    let getLastSunday (d : System.DateTime) =
        let mutable date = d
        while date.DayOfWeek <> System.DayOfWeek.Sunday do
          date <- date.AddDays -1.0
        date

    let lastDataPoint = state.RegionsData |> List.last
    let previousSunday = getLastSunday (lastDataPoint.Date.AddDays(-7.))

    let xAxis =
            baseOptions.xAxis
            |> Array.map(fun xAxis -> 
               {| xAxis with
                      gridZIndex = 1
                      plotBands =
                        match state.MetricType with
                        | MetricType.Deceased ->
                            [|
                               {| from=jsTime <| previousSunday
                                  ``to``=jsTime <| lastDataPoint.Date
                                  color="#ffffe0"
                                |}
                            |]
                        | _ -> [| |]
                 |})

    let chartTextGroup = state.ChartConfig.ChartTextsGroup

    let threshold100k value =
        (value * 100000.) 
        / (Utils.Dictionaries.regions.["si"].Population |> Utils.optionToInt |> float)
    let redThreshold = threshold100k 1350.
    let orangeThreshold = threshold100k 1000.
    let yellowThreshold = threshold100k 600.
    let greenThreshold = threshold100k 300.
    let yAxis =
            baseOptions.yAxis
            |> Array.map
                   (fun yAxis ->
                {| yAxis with
                       min = if state.ScaleType = Linear then 0. else 0.1
                       gridZIndex = 1
                       plotLines =
                           match state.ChartConfig.RelativeTo, state.MetricType with
                           | Pop100k, NewCases7Days -> [|
                               {| value=redThreshold
                                  color="red"
                                  width=1
                                  dashStyle="longdashdot"
                                  zIndex=2
                               |}
                               {| value=orangeThreshold
                                  color="orange"
                                  width=1
                                  dashStyle="longdashdot"
                                  zIndex=2
                               |}
                               {| value=yellowThreshold
                                  color="#d8d800"
                                  width=1
                                  dashStyle="longdashdot"
                                  zIndex=2
                               |}
                               {| value=greenThreshold
                                  color="green"
                                  width=1
                                  dashStyle="longdashdot"
                                  zIndex=2
                               |}
                            |]
                           | _ -> [| |]
                       plotBands =
                           match state.ChartConfig.RelativeTo, state.MetricType with
                           | Pop100k, NewCases7Days -> [|
                               {| from=redThreshold; ``to``=1000000.
                                  color="#e0e0e0"
                               |}
                               {| from=orangeThreshold; ``to``=redThreshold
                                  color="#ffd8d8"
                               |}
                               {| from=yellowThreshold; ``to``=orangeThreshold
                                  color="#fff1d8"
                               |}
                               {| from=greenThreshold; ``to``=yellowThreshold
                                  color="#ffffd8"
                               |}
                               {| from=0.0001; ``to``=greenThreshold
                                  color="#ebffeb"
                               |}
                            |]
                           | _ -> [| |]
                   |})

    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` = "line"
                zoomType = "x"
                styledMode = false // <- set this to 'true' for CSS styling
            |}
        series = allSeries state
        xAxis = xAxis
        yAxis = yAxis
        legend = {| enabled = false |}
        tooltip = pojo {|
                          formatter = fun () ->
                              tooltipFormatter state allSeries jsThis
                          shared = true
                          useHTML = true
                        |}
    |}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> chartFromWindow
        ]
    ]

let renderRegionSelector (regionConfig: RegionRenderingConfiguration) dispatch =
    let style =
        if regionConfig.Visible
        then [ style.backgroundColor regionConfig.Color
               style.borderColor regionConfig.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleRegionVisible regionConfig.Key |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (regionConfig.Visible, "metric-selector--selected") ]
        prop.style style
        prop.text (I18N.tt "region" regionConfig.Key) ]

let renderRegionsSelectors (state: RegionsChartState) dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            [ Html.div [
                prop.onClick (fun _ -> ToggleAllRegions ( if state.ShowAll then false else true ) |> dispatch)
                prop.className "btn btn-sm metric-selector"
                prop.text ( if state.ShowAll then I18N.t "charts.common.hideAll" else I18N.t "charts.common.showAll" ) ] ]
            |> List.append ( state.RegionsConfig |> List.map (fun metric -> renderRegionSelector metric dispatch ) )
        ) ]

let renderMetricTypeSelectors (activeMetricType: MetricType) dispatch =
    let renderMetricTypeSelector (typeSelector: MetricType) =
        let active = typeSelector = activeMetricType
        Html.div [
            prop.onClick (fun _ -> dispatch typeSelector)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (typeSelector |> MetricType.GetName)
        ]

    let metricTypesSelectors =
        [ NewCases7Days; ActiveCases; ConfirmedCases;  MetricType.Deceased ]
        |> List.map renderMetricTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (metricTypesSelectors)
    ]

let render (state : RegionsChartState) dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderMetricTypeSelectors
                state.MetricType (MetricTypeChanged >> dispatch)
            Utils.renderScaleSelector
                state.ScaleType (ScaleTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch
        renderRegionsSelectors state dispatch

        match state.MetricType with
        | MetricType.Deceased ->
            Html.div [
                prop.className "disclaimer"
                prop.children [
                    Html.text (I18N.t "charts.regions.disclaimer")
                ]
            ]
        | _ -> Html.none
    ]

let renderChart
    (config: RegionsChartConfig) (props : {| data : RegionsData |}) =
    React.elmishComponent
        ("RegionsChart", init config props.data, update, render)
