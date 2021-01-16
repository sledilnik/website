[<RequireQualifiedAccess>]
module WeeklyDemographicsViz.Rendering

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
      RangeSelectionButtonIndex: int
      }

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

    let processedTimelineData =
        allGroupsKeys
        |> List.map
            ( fun ageGroupKey ->
                timeline
                |> newExtractTimelineForAgeGroup ageGroupKey)


    let mapPoint
        (weeksFromStartDate: int)
        (ageGroupId: int)
        (ageGroupData: ProcessedAgeGroupData)
        (value: float)
        :obj=

        let date = ageGroupData.StartDate |> Days.add  (7 *  weeksFromStartDate)
        let dateTo = date |> Days.add 6

        let point = {|
            x = date |> jsTime12h
            y = ageGroupId
            value = value
            weeks = weeksFromStartDate
            ageGroupKey = ageGroupData.AgeGroupKey.Label
            totalCases = ageGroupData.Data.All
            maleCases = ageGroupData.Data.Male
            femaleCases = ageGroupData.Data.Female
            dateSpan = I18N.tOptions "days.weekYearFromToDate" {| date = date; dateTo = dateTo |}
        |}

        point |> pojo


    let generateSeries
        (state: State)
        (ageGroupId:int)
        (ageGroupData:ProcessedAgeGroupData)
        : obj =

        let ageGroupKey = ageGroupData.AgeGroupKey
        let populationStats = populationStatsForAgeGroup ageGroupKey

        let totalPopulation = populationStats.Male + populationStats.Female |> float
        let malePopulation = populationStats.Male |> float
        let femalePopulation = populationStats.Female |> float

        let totalRelCases =
            ageGroupData.Data.All
            |> Array.map ((*)  (100000./totalPopulation))
        let relMaleCases =
            ageGroupData.Data.Male
            |> Array.map ((*) (100000./malePopulation))
        let relFemaleCases =
            ageGroupData.Data.Female
            |> Array.map ((*) (100000./femalePopulation))

        let timelineArray =
            match state.Metrics.MetricsType with
            | NewCases ->
                Array.map (( ( * ) (100000./ totalPopulation)) >> (fun x -> Math.Log (x + Math.E))) ageGroupData.Data.All
            | CasesRatio ->
                let computeRatio
                    (x:float)
                    (y:float)
                    :float =
                    match x, y with
                    | 0., 0. -> 1.
                    | _, 0. -> 2.
                    | _, _ -> min (x / y) 2. |> max (0.5) // Trims the values to avoid infinities

                Array.map2 (computeRatio) relFemaleCases relMaleCases

        let tooltipAgeGroupData = { ageGroupData with Data = {All = totalRelCases; Male = relMaleCases; Female = relFemaleCases}}

        let points =
            timelineArray
            |> Array.mapi (fun index value -> mapPoint index ageGroupId tooltipAgeGroupData value)


        let series = {|
            colsize = 3600 * 1000 * 24 * 7
            visible = true
            name = ageGroupKey.Label
            data = points
        |}

        series |> pojo

    let allSeries =
        processedTimelineData
        |> List.mapi (generateSeries state)
        |> List.toArray


    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let sparklineFormatter
        (state: State)
        (maleCases: float[])
        (femaleCases: float[])
        : string =

        let series =
            let maleData =
                maleCases
                |> Array.mapi (fun i value -> {| x=i; y=value |} |> pojo)

            let femaleData =
                match state.Metrics.MetricsType with
                | NewCases ->
                    femaleCases
                    |> Array.mapi (fun i value ->
                        {|
                            x=i
                            y= -value
                        |} |> pojo)
                | CasesRatio ->
                    femaleCases
                    |> Array.mapi (fun i value ->
                        {|
                            x=i
                            y= value
                        |} |> pojo)



            [|
                {|
                    animation = false
                    data = maleData
                    color = "#73CCD5"
                    borderColor = "#73CCD5"
                    pointWidth=2
                |}|> pojo
                {|
                    animation = false
                    data = femaleData
                    color = "#D99A91"
                    borderColor = "#D99A91"
                    pointWidth=2
                |}|> pojo
            |]

        let maximum =
            match state.Metrics.MetricsType with
            | NewCases -> max (Array.max femaleCases) (Array.max maleCases)
            | CasesRatio -> 100.
        let minimum =
            match state.Metrics.MetricsType with
            | NewCases -> - maximum
            | CasesRatio -> 0.

        let options =
            {|
                chart =
                    {|
                        ``type`` = "column"
                        backgroundColor = "transparent"
                    |}|> pojo
                legend = {|enable = false|}

                title = {| enable = false|}
                series = series

                plotOptions =
                    {|
                        column =
                            match state.Metrics.MetricsType with
                            | NewCases -> {| stacking = "normal"|} |> pojo
                            | CasesRatio -> {| stacking = "percent" |} |> pojo
                    |}|>pojo

                xAxis =
                    {|
                        visible = false
                        labels = {| enabled = false|}
                        title = {| enabled = false|}
                        linkedto = Some 0.
                    |} |> pojo
                yAxis =
                    {|
                        min = minimum
                        max = maximum
                        opposite = true
                        labels = {| enabled = false|}
                        visible = true
                        title = {| enabled = false|}
                        allowDecimals = false
                        showFirstLabel = true
                        showLastLabel = true
                    |}

                credits = {| enable  = false|}
            |}

        Fable.Core.JS.setTimeout (fun () -> sparklineChart("tooltip-chart-weekly-demographics", options)) 10 |> ignore
        """<div id="tooltip-chart-weekly-demographics"; class="tooltip-chart";><div/>"""


    let tooltipFormatter
        (jsThis: obj)
        (state: State) =

        let (point:obj) = jsThis?point
        let (date:string) = point?dateSpan
        let (ageGroupKey:string) = point?ageGroupKey
        let (maleCases:float[]) = point?maleCases
        let (femaleCases:float[]) = point?femaleCases
        let (week:int) = point?weeks

        let maleCasesForWeek = maleCases.[week]
        let femaleCasesForWeek = femaleCases.[week]

        let colorCategories = {| Male = "#73CCD5" ; Female = "#D99A91" |}

        let sparkline = sparklineFormatter state maleCases femaleCases

        let label = sprintf "<b> %s </b>" date

        label
            + sprintf "<br>%s: <b>%s</b>" (I18N.t "charts.weeklyDemographics.ageGroup") (ageGroupKey)
            + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorCategories.Male) (I18N.t "charts.weeklyDemographics.male") (Utils.formatTo1DecimalWithTrailingZero (maleCasesForWeek:float)) (I18N.t "charts.weeklyDemographics.per100k")
            + sprintf "<br><span style='color: %s'>●</span> %s: <b>%s</b> %s" (colorCategories.Female) (I18N.t "charts.weeklyDemographics.female") (Utils.formatTo1DecimalWithTrailingZero(femaleCasesForWeek:float)) (I18N.t "charts.weeklyDemographics.per100k")
            + sparkline

    let colorAxis =
        match state.Metrics.MetricsType with
            | NewCases ->
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
            | CasesRatio ->
                    {|
                        ``type`` = "linear"
                        min = 0.6
                        stops = [|
                            (0.000, "#73CCD5")
                            (0.500, "#FFFFFF")
                            (0.999, "#D99A91")
                        |]
                    |} |> pojo

    let className = "covid19-infection-weekly-demographics"


    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {|
    baseOptions with
        chart = pojo {| ``type`` = "heatmap" ; animation = false|}
        series = allSeries
        xAxis =
            {|
                ``type`` = "datetime"
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
        colorAxis = colorAxis
        tooltip =
           pojo
               {| formatter = fun () -> tooltipFormatter jsThis state
                  shared = true
                  useHTML = true |}
        boost = {| useGPUTranslations = true |} |> pojo

        responsive = pojo
            {|
                rules =
                    [| {|
                        condition = {| maxWidth = 768 |}
                        chartOptions =
                            {|
                                yAxis = [| {| visible = false |} |]
                            |}
                    |} |]
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

        Html.div [ prop.text (I18N.chartText "weeklyDemographics" metrics.Id)
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
    React.elmishComponent ("weeklyDemographics", init props.data, update, render)
