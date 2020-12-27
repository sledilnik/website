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

    let mapPoint
        (startDate: DateTime)
        (daysFromStartDate: int)
        (ageGroup: int)
        (cases: CasesInAgeGroupForDay) =
        // let date = startDate |> Days.add (daysFromStartDate + 200)
        let date = daysFromStartDate

        let temp = startDate |> Days.add daysFromStartDate

        {|
             x = temp |> jsTime12h
             y = ageGroup 
             value = Math.Log (float cases + 1.)
             cases = cases
             date = I18N.tOptions "days.longerDate" {| date = date |}
        |} |> pojo

    let mapAllPoints 
        (ageGroup: int)
        (groupTimeline: CasesInAgeGroupTimeline) =
        let startDate = groupTimeline.StartDate
        printfn "%A" startDate
        let timelineArray = groupTimeline.Data

        timelineArray
        |> Array.mapi (fun i cases -> mapPoint startDate i ageGroup cases)

    // generate all series
    let allSeries =
        allGroupsKeys
        |> List.mapi (fun index ageGroupKey ->
            let points =
                timeline
                |> extractTimelineForAgeGroup
                       ageGroupKey state.Metrics.MetricsType
                |> mapAllPoints index
            {|
                colsize = 3600* 1000 * 24
                visible = true
                name = ageGroupKey.Label
                data = points
            |} |> pojo
        )
        |> List.toArray
        // |> Array.concat

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res


    let className = "covid19-infection-heatmap"

    // let testData = 
    //     let df = [| [|0;0;1|];[|1;0;2|]; [|0;1;3|]; [|1;1;4|]|] 

    //     let arr = 
    //         df |> Array.map (fun arr -> 
    //             {|
    //                 // x = arr.[0]
    //                 x = jsTime12h <| DateTime( 2020, 12, 1 + arr.[0])
    //                 y = arr.[1]
    //                 value = arr.[2] 
    //             |} |> pojo
    //         ) 

    //     {|
    //         colsize = 24 * 3600 * 100 * 12
    //         visible = true
    //         data = arr
    //     |}



    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick


    let data = allSeries 
    let categ = allGroupsKeys |> List.toArray


    // printfn "%A" dataSeries

    {| baseOptions  with
        chart = pojo {| ``type`` = "heatmap" |}
       
        // series = [| testData |]
        series = allSeries

        xAxis = pojo
            {|
                ``type`` = "datetime"
            |}

        yAxis = pojo 
            {| 
                max = 9 
                opposite = true
                labels = 
                    {|
                        reserveSpace= false 
                    |}
            |}
        colorAxis = pojo 
            {| 
                ``type`` = "linear"
                min = 0.0 
                stops =
                    [|
                        (0.000,"#ffffff")
                        (0.001,"#fff7db")
                        (0.200,"#ffefb7")
                        (0.280,"#ffe792")
                        (0.360,"#ffdf6c")
                        (0.440,"#ffb74d")
                        (0.520,"#ff8d3c")
                        (0.600,"#f85d3a")
                        (0.680,"#ea1641")
                        (0.760,"#d0004e")
                        (0.840,"#ad005b")
                        (0.920,"#800066")
                        (0.999,"#43006e")
                    |]
            |}
        
        tooltip = pojo {|
                          formatter = fun () -> tooltipFormatter jsThis
                          shared = true
                          useHTML = true
                        |}

        boost = {| useGPUTranslations = true|} |> pojo

        // tooltip = {|enabled = false|}
        
        navigator = pojo {| enabled = false |}
        scrollbar = pojo {| enabled = false |}
        // rangeSelector = pojo {| enabled = false |}
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
        // renderMetricsSelectors state.Metrics (ChangeMetrics >> dispatch)
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent
        ("heatmapChart", init props.data, update, render)
