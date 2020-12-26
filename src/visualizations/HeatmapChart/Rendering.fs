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

type DayValueIntMaybe = JsTimestamp*int option
type DayValueFloat = JsTimestamp*float

type State = {
    Metrics : DisplayMetrics
    Data : StatsData
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangeMetrics of DisplayMetrics
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        Metrics = availableDisplayMetrics.[0]
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeMetrics metrics -> { state with Metrics=metrics }, Cmd.none
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
    let timeline = calculateCasesByAgeTimeline totalCasesByAgeGroups

    // get keys of all age groups
    let allGroupsKeys = listAgeGroups timeline

    let colorOfAgeGroup ageGroupIndex =
        let colors =
            [| "#FFEEBA"; "#FFDA6B";"#E9B825";"#AEEFDB";"#52C4A2";"#33AB87"
               "#189A73";"#F4B2E0";"#D559B0";"#B01C83" |]
        colors.[ageGroupIndex]

    let mapPoint
        (startDate: DateTime)
        (daysFromStartDate: int)
        (cases: CasesInAgeGroupForDay) =
        let date = startDate |> Days.add daysFromStartDate

        pojo {|
             x = date |> jsTime12h :> obj
             y = cases
             date = I18N.tOptions "days.longerDate" {| date = date |}
        |}

    let mapAllPoints (groupTimeline: CasesInAgeGroupTimeline) =
        let startDate = groupTimeline.StartDate
        let timelineArray = groupTimeline.Data

        timelineArray
        |> Array.mapi (fun i cases -> mapPoint startDate i cases)

    // generate all series
    let allSeries =
        allGroupsKeys
        |> List.mapi (fun index ageGroupKey ->
            let points =
                timeline
                |> extractTimelineForAgeGroup
                       ageGroupKey state.Metrics.MetricsType
                |> mapAllPoints

            pojo {|
                 visible = true
                 name = ageGroupKey.Label
                 color = colorOfAgeGroup index
                 data = points
            |}
        )
        |> List.toArray

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res


    let temp0 = (timeline |> extractTimelineForAgeGroup allGroupsKeys.[0] state.Metrics.MetricsType ).Data
    let temp1 = (timeline |> extractTimelineForAgeGroup allGroupsKeys.[1] state.Metrics.MetricsType ).Data

    printfn "%A" (temp0 |> Array.length)
    printfn "%A" (temp1 |> Array.length)
    
    let data0 = temp0 |> Array.mapi ( fun index number ->  poja [|index;0;number|])
    let data1 = temp1 |> Array.mapi ( fun index number ->  poja [|index;1;number|])
    printfn "%A" (data0 )
    printfn "%A" (data1 )

    let className = "covid19-infections"
    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    // let data = [| [|0;0;1|];[|1;0;2|]; [|0;1;3|]; [|1;1;4|]|] |> Array.map poja |> poja 
    let data = Array.concat [|data0; data1|]


    {| optionsWithOnLoadEvent "covid19-heatmap" with
        chart = pojo {| ``type`` = "heatmap" |}

        colorAxis = pojo {| min = 0 |}
        
        series = [| {| data = data|}|> pojo |]
    |}

    // {| baseOptions with
    //     chart = pojo
    //         {|
    //             animation = false
    //             ``type`` = "column"
    //             zoomType = "x"
    //             className = className
    //             events = pojo {| load = onLoadEvent(className) |}
    //         |}
    //     title = pojo {| text = None |}
    //     series = allSeries
    //     xAxis = baseOptions.xAxis
    //     yAxis = baseOptions.yAxis

    //     plotOptions = pojo
    //         {|
    //             column = pojo
    //                     {|
    //                       dataGrouping = pojo {| enabled = false |}
    //                       groupPadding = 0
    //                       pointPadding = 0
    //                       borderWidth = 0 |}
    //             series =
    //                 match state.Metrics.ChartType with
    //                 | StackedBarNormal -> pojo {| stacking = "normal" |}
    //                 | StackedBarPercent -> pojo {| stacking = "percent" |}
    //         |}
    //     legend = pojo {| enabled = true ; layout = "horizontal" |}
    //     tooltip = pojo {|
    //                       formatter = fun () -> tooltipFormatter jsThis
    //                       shared = true
    //                       useHTML = true
    //                     |}
    // |}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> Highcharts.chartFromWindow
        ]
    ]

let renderMetricsSelectors activeMetrics dispatch =
    let renderSelector (metrics : DisplayMetrics) =
        let active = metrics = activeMetrics
        Html.div [
            prop.text (I18N.chartText "ageGroupsTimeline" metrics.Id)
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected")]
            if not active then prop.onClick (fun _ -> dispatch metrics)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        availableDisplayMetrics
        |> Array.map renderSelector
        |> prop.children
    ]

let render state dispatch =
    Html.div [
        renderChartContainer state dispatch
        renderMetricsSelectors state.Metrics (ChangeMetrics >> dispatch)
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent
        ("heatmapChart", init props.data, update, render)
