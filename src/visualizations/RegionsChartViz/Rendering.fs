module RegionsChartViz.Rendering

open Types

open Browser.Types
open Elmish
open Fable.Core
open Highcharts
open JsInterop
open Feliz
open Feliz.ElmishComponents

open System.Text

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

let excludedRegions = Set.ofList ["t"]

type Metric =
    { Key : string
      Color : string
      Visible : bool }

type MetricType =
    | ActiveCases
    | ConfirmedCases
    | NewCases7Days
    | Deceased
  with
    static member getName = function
        | ActiveCases -> I18N.chartText "regions" "activeCases"
        | ConfirmedCases -> I18N.chartText "regions" "confirmedCases"
        | NewCases7Days -> I18N.chartText "regions" "newCases7Days"
        | Deceased -> I18N.chartText "regions" "deceased"

type MetricRelativeTo = Absolute | Pop100k

type RegionsChartConfig = {
    RelativeTo: MetricRelativeTo
    ChartTextsGroup: string
}

type State =
    {
      ChartConfig: RegionsChartConfig
      ScaleType : ScaleType
      MetricType : MetricType
      Data : RegionsData
      Regions : Region list
      Metrics : Metric list
      RangeSelectionButtonIndex: int
    }

type Msg =
    | ToggleRegionVisible of string
    | MetricTypeChanged of MetricType
    | ScaleTypeChanged of ScaleType
    | RangeSelectionChanged of int

let regionTotal (region : Region) : int =
    region.Municipalities
    |> List.map (fun city -> city.ActiveCases)
    |> List.choose id
    |> List.sum

let init (config: RegionsChartConfig) (data : RegionsData) : State * Cmd<Msg> =
    let lastDataPoint = List.last data

    let regionsWithoutExcluded =
        lastDataPoint.Regions
        |> List.filter (fun region ->
            not (excludedRegions |> Set.contains region.Name))

    let regionsByTotalCases =
        regionsWithoutExcluded
        |> List.sortByDescending regionTotal

    let metrics =
        regionsByTotalCases
        |> List.map (fun region ->
            let regionKey = region.Name
            let color = regionsInfo.[regionKey].Color
            { Key = regionKey
              Color = color
              Visible = true } )

    { ScaleType = Linear; MetricType = ActiveCases
      ChartConfig = config
      Data = data ; Regions = regionsByTotalCases ; Metrics = metrics
      RangeSelectionButtonIndex = 0 },
    Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleRegionVisible regionKey ->
        let newMetrics =
            state.Metrics
            |> List.map (fun m ->
                if m.Key = regionKey
                then { m with Visible = not m.Visible }
                else m)
        { state with Metrics = newMetrics }, Cmd.none
    | MetricTypeChanged newMetricType ->
        { state with MetricType = newMetricType }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with ScaleType = scaleType }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let tooltipValueFormatter (state: State) value =
    match state.ChartConfig.RelativeTo with
    // todo igor: format to int for absolute values
    | Absolute -> Utils.formatToInt value
    | Pop100k -> Utils.formatTo1DecimalWithTrailingZero value

let tooltipFormatter (state: State) _ jsThis =
    let points: obj[] = jsThis?points

    match points with
    | [||] -> ""
    | _ ->
        let s = StringBuilder()
        // todo igor: extract date
//        let date = points.[0]?point?date
//        s.AppendFormat ("{0}<br/>", date.ToString()) |> ignore
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
                            "<td><span style='color:%s'>●</span></td><td>%s</td><td style='text-align: right; padding-left: 10px'>%A</td>"
                            regionColor
                            regionName
                            (tooltipValueFormatter state dataValue)
                    s.Append regionTooltip |> ignore
                    s.Append "</tr>" |> ignore
                )

        s.Append "</table>" |> ignore
        s.ToString()

let renderChartOptions (state : State) dispatch =

    let metricsToRender =
        state.Metrics
        |> List.filter (fun metric -> metric.Visible)

    let renderRegionPoint metricToRender (point : RegionsDataPoint) =
        let ts = point.Date |> jsTime12h
        let region =
            point.Regions
            |> List.find (fun reg -> reg.Name = metricToRender.Key)

        let municipalityMetricValue muni =
            match state.MetricType with
            | ActiveCases -> muni.ActiveCases
            | ConfirmedCases -> muni.ConfirmedToDate
            | NewCases7Days -> muni.ConfirmedToDate
            | Deceased -> muni.DeceasedToDate
            |> Option.defaultValue 0

        let totalSum =
            region.Municipalities
            |> Seq.sumBy municipalityMetricValue
            |> float

        let finalValue =
            match state.ChartConfig.RelativeTo with
            | Absolute -> totalSum
            | Pop100k ->
                let regionPopulation =
                    Utils.Dictionaries.regions.[region.Name].Population
                    |> Option.get
                    |> float

                let regionPopBy100k = regionPopulation / 100000.0
                totalSum / regionPopBy100k

        ts, finalValue

    let allSeries =
        metricsToRender
        |> List.map (fun metric ->
            let renderPoint = renderRegionPoint metric

            let data =
                state.Data
                |> Seq.map renderPoint
                |> Array.ofSeq

            {|
                visible = metric.Visible
                color = metric.Color
                name = I18N.tt "region" metric.Key
                data = data
            |}
            |> pojo
        )
        |> List.toArray

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions
            state.ScaleType "covid19-regions"
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    let xAxis =
            baseOptions.xAxis
            |> Array.map(fun xAxis -> {| xAxis with gridZIndex = 1 |})

    let chartTextGroup = state.ChartConfig.ChartTextsGroup

    let redThreshold = 140
    let yAxis =
            baseOptions.yAxis
            |> Array.map
                   (fun yAxis ->
                {| yAxis with
                       min = None
                       gridZIndex = 1
                       plotLines =
                           match state.ChartConfig.RelativeTo, state.MetricType with
                           | Pop100k, ActiveCases -> [|
                               {| value=redThreshold
                                  label={|
                                           text=I18N.chartText chartTextGroup "red"
                                           align="left"
                                           verticalAlign="bottom"
                                            |}
                                  color="red"
                                  width=1
                                  dashStyle="longdashdot"
                                  zIndex=2
                                |}
                            |]
                           | _ -> [| |]
                       plotBands =
                           match state.ChartConfig.RelativeTo, state.MetricType with
                           | Pop100k, ActiveCases -> [|
                               {| from=redThreshold; ``to``=100000.0
                                  color="#FCD5CF30"
                                |}
                            |]
                           | _ -> [| |]
                   |})

    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` = "spline"
                zoomType = "x"
                styledMode = false // <- set this to 'true' for CSS styling
            |}
        series = allSeries
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

let renderMetricSelector (metric : Metric) dispatch =
    let style =
        if metric.Visible
        then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
        else [ ]
    Html.div [
        prop.onClick (fun _ -> ToggleRegionVisible metric.Key |> dispatch)
        Utils.classes
            [(true, "btn  btn-sm metric-selector")
             (metric.Visible, "metric-selector--selected") ]
        prop.style style
        prop.text (I18N.tt "region" metric.Key) ]

let renderMetricsSelectors metrics dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            metrics
            |> List.map (fun metric ->
                renderMetricSelector metric dispatch
            ) ) ]

let renderMetricTypeSelectors (activeMetricType: MetricType) dispatch =
    let renderMetricTypeSelector (typeSelector: MetricType) =
        let active = typeSelector = activeMetricType
        Html.div [
            prop.onClick (fun _ -> dispatch typeSelector)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
            prop.text (typeSelector |> MetricType.getName)
        ]

    let metricTypesSelectors =
        [ ActiveCases; ConfirmedCases; NewCases7Days; Deceased ]
        |> List.map renderMetricTypeSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (metricTypesSelectors)
    ]

let render (state : State) dispatch =
    Html.div [
        Utils.renderChartTopControls [
            renderMetricTypeSelectors
                state.MetricType (MetricTypeChanged >> dispatch)
            Utils.renderScaleSelector
                state.ScaleType (ScaleTypeChanged >> dispatch)
        ]
        renderChartContainer state dispatch
        renderMetricsSelectors state.Metrics dispatch
    ]

let renderChart
    (config: RegionsChartConfig) (props : {| data : RegionsData |}) =
    React.elmishComponent
        ("RegionsChart", init config props.data, update, render)
