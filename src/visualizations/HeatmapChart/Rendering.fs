[<RequireQualifiedAccess>]
module HeatmapChart.Rendering

open System
open Analysis
open Synthesis
open Highcharts
open Types

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser.Types

type DayValueIntMaybe = JsTimestamp * int option
type DayValueFloat = JsTimestamp * float

type State =
    { Metrics: DisplayMetrics
      Data: StatsData
      RangeSelectionButtonIndex: int}


type Msg =
    | ChangeMetrics of DisplayMetrics
    | RangeSelectionChanged of int

let init data: State * Cmd<Msg> =
    let state =
        { Data = data
          Metrics = availableDisplayMetrics.[1]
          RangeSelectionButtonIndex = 2
          }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ChangeMetrics metrics -> { state with Metrics = metrics }, Cmd.none    
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions state dispatch =

    // map state data into a list needed for calculateCasesByAgeTimeline
    let totalCasesByAgeGroupsList =
        state.Data
        |> List.map (fun point -> (point.Date, point.StatePerAgeToDate))

    let totalCasesByAgeGroups =
        mapDateTuplesListToArray totalCasesByAgeGroupsList

    // calculate complete merged timeline
    let timeline =
        calculateCasesByAgeTimeline totalCasesByAgeGroups

    // get keys of all age groups
    let allGroupsKeys = listAgeGroups timeline

    let mapPoint (startDate: DateTime) (weeksFromStartDate: int) (ageGroup: int) (cases: CasesInAgeGroupForDay) =

        let date = startDate |> Days.add  (7 *  weeksFromStartDate)
        let dateTo = date |> Days.add 6

        {| x = date |> jsTime12h
           y = ageGroup
           value = Math.Log(float cases + 1.)
           cases = cases
           week = weeksFromStartDate
           dateSpan =
              I18N.tOptions "days.weekYearFromToDate" {| date = date; dateTo = dateTo |} |}
        |> pojo

    let mapAllPoints (ageGroup: int) (groupTimeline: CasesInAgeGroupTimeline) =
        let startDate = groupTimeline.StartDate
        let timelineArray = groupTimeline.Data

        timelineArray
        |> Array.mapi (fun i cases -> mapPoint startDate i ageGroup cases)

    // generate all series
    let allSeries =
        allGroupsKeys
        |> List.mapi
            (fun index ageGroupKey ->
                let points =
                    timeline
                    |> extractTimelineForAgeGroup ageGroupKey state.Metrics.MetricsType
                    |> accumulateWeeklyCases // chunks the data into weeks and sums to get weekly case number
                    |> mapAllPoints index

                {| colsize = 3600 * 1000 * 24 * 7 // set column size to 1 week  (default is 1px = 1ms)
                   visible = true
                   name = ageGroupKey.Label
                   data = points |}
                |> pojo)
        |> List.toArray

    let className = "covid19-infection-heatmap"

    let startDate = timeline.StartDate 
    let daysFromStartDate = Array.length timeline.Data
    let endDate = startDate |> Days.add (daysFromStartDate - (daysFromStartDate % 7)-2) // We drop the data for the incomplete week. 
    
    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res


    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           chart = pojo {| ``type`` = "heatmap" |}
           series = allSeries
           xAxis = 
                pojo 
                    {| 
                        ``type`` = "datetime"
                        max = endDate |> jsTime12h
                    |}
           yAxis =
               pojo
                    {| 
                        categories = allGroupsKeys |> List.map (fun x -> x.Label) |> List.toArray 
                        opposite = true 
                        tickWidth = 1
                        tickLength = 60
                        labels =
                            {|
                                align="center"
                                y =5
                            |} |> pojo
                    |}
                    //   labels = {| reserveSpace = false |} |}
           colorAxis =
               pojo
                   {| ``type`` = "linear"
                      min = 0.0
                      stops =
                          [| (0.000, "#009e94")
                             (0.166, "#6eb49d")
                             (0.250, "#b2c9a7")
                             (0.433, "#f0deb0")
                             (0.700, "#e3b656")
                             (0.900, "#cc8f00")
                             (0.999, "#b06a00") |] |}
           tooltip =
               pojo
                   {| formatter = fun () -> tooltipFormatter jsThis
                      shared = true
                      useHTML = true |}
           credits = credictsOptions 
           boost = {| useGPUTranslations = true |} |> pojo
    |}


let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderChartOptions state dispatch
                               |> Highcharts.chartFromWindow ] ]

let renderMetricsSelectors activeMetrics dispatch =
    let renderSelector (metrics: DisplayMetrics) =
        let active = metrics = activeMetrics

        Html.div [ prop.text (I18N.chartText "ageGroupsTimeline" metrics.Id)
                   Utils.classes [ (true, "btn btn-sm metric-selector")
                                   (active, "metric-selector--selected selected") ]
                   if not active then prop.onClick (fun _ -> dispatch metrics)
                   if active
                   then prop.style [ style.backgroundColor "#808080" ] ]

    Html.div [ prop.className "metrics-selectors"
               availableDisplayMetrics
               |> Array.map renderSelector
               |> prop.children ]

let render state dispatch =
    Html.div [ renderChartContainer state dispatch
               // renderMetricsSelectors state.Metrics (ChangeMetrics >> dispatch)
                ]

let renderChart (props: {| data: StatsData |}) =
    React.elmishComponent ("heatmapChart", init props.data, update, render)
