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

open Utils.AgePopulationStats

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
          Metrics = availableDisplayMetrics.[0]
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

    // let mapPoint 
    //     (startDate: DateTime) 
    //     (weeksFromStartDate: int) 
    //     (ageGroup: int) 
    //     (ageGroupKey: AgeGroupKey)
    //     (cases) =



    //     let value = 
    //         match state.Metrics.MetricsType with
    //         | DifferenceInCases -> cases
    //         | RelativeCases -> 
    //             let populationStats = populationStatsForAgeGroup ageGroupKey
    //             let totalPopulation = populationStats.Female + populationStats.Male |> float
    //             let relCases = cases * 100000./totalPopulation

    //             Math.Log(relCases + Math.E)


    // let mapAllPoints 
    //     (ageGroup: int) 
    //     (ageGroupKey: AgeGroupKey) 
    //     (groupTimeline)=

    //     let startDate = groupTimeline.StartDate
    //     let timelineArray = groupTimeline.Data

    //     timelineArray
    //     |> Array.mapi (fun i cases -> mapPoint startDate i ageGroup ageGroupKey cases)

    // generate all series

    let processedTimelineData =
        allGroupsKeys
        |> List.map    
            ( fun ageGroupKey ->
                timeline 
                |> newExtractTimelineForAgeGroup ageGroupKey)
    

    let mapPoint startDate weeksFromStartDate ageGroupId ageGroupKey value=

        let date = startDate |> Days.add  (7 *  weeksFromStartDate)
        let dateTo = date |> Days.add 6

        let point = {|
            x = date |> jsTime12h
            // x = weeksFromStartDate
            y = ageGroupId
            value = value
            weeks = weeksFromStartDate
            ageGroupKey = ageGroupKey
            dateSpan = I18N.tOptions "days.weekYearFromToDate" {| date = date; dateTo = dateTo |} 
        |} 

        // printfn "%A" point

        point |> pojo
        // |> pojo


    let generateSeries 
        (ageGroupId:int) 
        (ageGroupData:ProcessedAgeGroupData)
        : obj = 

        let ageGroupKey = ageGroupData.AgeGroupKey
        let populationStats = populationStatsForAgeGroup ageGroupKey
        let startDate = ageGroupData.StartDate

        let timelineArray = 
            match state.Metrics.MetricsType with
            | RelativeCases -> 
                let totalPopulation = populationStats.Male + populationStats.Female |> float

                Array.map (((*) (100000./ totalPopulation)) >> (fun x -> Math.Log (x + Math.E))) ageGroupData.Data.All

            | DifferenceInCases -> 
                let malePopulation = populationStats.Male |> float
                let femalePopulation = populationStats.Female |> float

                let relMaleCases = 
                    ageGroupData.Data.Male 
                    |> Array.map ((*) (100000./malePopulation))
                let relFemaleCases = 
                    ageGroupData.Data.Female 
                    |> Array.map ((*) (100000./femalePopulation))

                Array.map2 (/) relFemaleCases relMaleCases 


        let points = 
            timelineArray 
            |> Array.mapi (fun index value -> mapPoint startDate index ageGroupId ageGroupKey value)


        let series = {|
            colsize = 3600 * 1000 * 24 * 7 
            visible = true
            name = ageGroupKey.Label
            data = points
        |} 
        
        series |> pojo



    let allSeries = 
        processedTimelineData 
        |> List.mapi (fun index data -> generateSeries index data)
        |> List.toArray

    // let allSeries =  
    //     let df = [| [|0;0;1|];[|1;0;2|]; [|0;1;3|]; [|1;1;4|]|] 

    //     df |> Array.map (fun arr -> 
    //             {|
    //                 x = arr.[0]
    //                 y = arr.[1]
    //                 value = arr.[2] 
    //             |} |> pojo
    //         ) 
        


    // let allSeries =
    //     allGroupsKeys
    //     |> List.mapi
    //         (fun index ageGroupKey ->
    //             let points =
    //                 timeline
    //                 // |> extractTimelineForAgeGroup ageGroupKey state.Metrics.MetricsType //TODO: transfer this logic to synthesis
    //                 // |> accumulateWeeklyCases // chunks the data into weeks and sums to get weekly case number
    //                 |> newExtractTimelineForAgeGroup ageGroupKey
    //                 |> mapAllPoints index ageGroupKey
    //             {| 
    //                 colsize = 3600 * 1000 * 24 * 7 // set column size to 1 week  (default is 1px = 1ms)
    //                 visible = true
    //                 name = ageGroupKey.Label
    //                 data = points 
    //             |}
    //             |> pojo)
    //     |> List.toArray

    let className = "covid19-infection-heatmap"

    let startDate = timeline.StartDate 
    let daysFromStartDate = Array.length timeline.Data
    let endDate = startDate |> Days.add (daysFromStartDate - (daysFromStartDate % 7)) // We drop the data for the incomplete week. 
    
    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let colorAxis = 
        match state.Metrics.MetricsType with
            | RelativeCases -> 
                   {| 
                        ``type`` = "linear"
                        min = 1.0
                        stops =
                            [|    
                                (0.000, "#009e94")
                                (0.256, "#6eb49d")
                                (0.350, "#b2c9a7")
                                (0.433, "#f0deb0")
                                (0.700, "#e3b656")
                                (0.900, "#cc8f00")
                                (0.999, "#b06a00") 
                            |]
                    |} |>pojo
            | DifferenceInCases -> 
                    {|
                        ``type`` = "linear"
                        min = 0.
                        stops =
                            [|    
                                (0.001, "#b2182b")
                                (0.050, "#d6604d")
                                (0.150, "#f4a582")
                                (0.275, "#fddbc7")
                                (0.500, "#ffffff")
                                (0.725, "#d1e5f0")
                                (0.850, "#92c5de")
                                (0.950, "#4393c3")
                                (0.999, "#2166ac") 
                            |] 
                    |} |> pojo

    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| 
    baseOptions with
        chart = pojo {| ``type`` = "heatmap" |}
        series = allSeries
        xAxis = 
            {| 
                ``type`` = "datetime"
                max = endDate |> jsTime12h
            |} |> pojo
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
        colorAxis = colorAxis
        tooltip =
           pojo
               {| formatter = fun () -> tooltipFormatter jsThis
                  shared = true
                  useHTML = true |}
        credits = credictsOptions 
        boost = {| useGPUTranslations = true |} |> pojo
        navigator = pojo {| enabled = false |}
        scrollbar = pojo {| enabled = false |}

        responsive = pojo {| |}
        rangeSelector =
            pojo 
                {|
                    enabled = true
                    allButtonsEnabled = true
                    selected = 2
                    inputDateFormat = I18N.t "charts.common.numDateFormat"
                    inputEditDateFormat = I18N.t "charts.common.numDateFormat"
                    inputDateParser = parseDate
                    x = 0
                    inputBoxBorderColor = "#ced4da"
                    buttonTheme = pojo {| r = 6; states = pojo {| select = pojo {| fill = "#ffd922" |} |} |}
                    buttons = [|
                        {|
                            ``type`` = "month"
                            count = 2
                            text = I18N.tOptions "charts.common.x_months" {| count = 2 |}
                            // events = pojo {| click = onRangeSelectorButtonClick 0 |}
                        |}
                        {|
                            ``type`` = "month"
                            count = 3
                            text = I18N.tOptions "charts.common.x_months" {| count = 3 |}
                            // events = pojo {| click = onRangeSelectorButtonClick 1 |}
                        |}
                        {|
                            ``type`` = "all"
                            count = 1
                            text = I18N.t "charts.common.all"
                            // events = pojo {| click = onRangeSelectorButtonClick 2 |}
                        |}
                    |]
                |}

    |}


let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 480 ]
               prop.className "highcharts-wrapper"
               prop.children [ renderChartOptions state dispatch
                               |> Highcharts.chartFromWindow ] ]

let renderMetricsSelectors activeMetrics dispatch =
    let renderSelector (metrics: DisplayMetrics) =
        let active = metrics = activeMetrics

        Html.div [ prop.text (I18N.chartText "heatmapChart" metrics.Id)
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
               renderMetricsSelectors state.Metrics (ChangeMetrics >> dispatch)
                ]

let renderChart (props: {| data: StatsData |}) =
    React.elmishComponent ("heatmap", init props.data, update, render)
