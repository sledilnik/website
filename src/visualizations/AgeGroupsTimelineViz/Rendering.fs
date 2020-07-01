[<RequireQualifiedAccess>]
module AgeGroupsTimelineViz.Rendering

open Analysis
open Fable.Core
open Synthesis
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Highcharts
open Types

type DayValueIntMaybe = JsTimestamp*int option
type DayValueFloat = JsTimestamp*float

type DisplayType = {
    Id: string
}

let availableDisplayTypes: DisplayType array = [|
    {   Id = "all"; }
|]

type State = {
    DisplayType : DisplayType
    Data : StatsData
    RangeSelectionButtonIndex: int
}

type Msg =
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

let init data : State * Cmd<Msg> =
    let state = {
        Data = data
        DisplayType = availableDisplayTypes.[0]
        RangeSelectionButtonIndex = 0
    }
    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeDisplayType rt ->
        { state with DisplayType=rt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions state dispatch =

    // map state data into a list needed for calculateCasesByAgeTimeline
    let totalCasesByAgeGroups =
        state.Data
        |> List.map (fun point -> (point.Date, point.StatePerAgeToDate))

    // calculate complete merged timeline
    let timeline = calculateCasesByAgeTimeline totalCasesByAgeGroups

    // get keys of all age groups
    let allGroupsKeys = listAgeGroups timeline

    let colorOfAgeGroup ageGroupIndex =
//        let (minColorR, minColorG, minColorB) = (0xff, 0xc0, 0xc0)
        let minColor = (0x85, 0xe2, 0x85) //85E285
//        let (maxColorR, maxColorG, maxColorB) = (0xb3, 0x3b, 0x3b) // 0BBC0B
        let maxColor = (0xad, 0x7a, 0x40) // AD7A40

        let mixRatio = (float ageGroupIndex)
                        / (float allGroupsKeys.Length)
        Utils.mixColors minColor maxColor mixRatio

    let mapPoint (pointData: CasesInAgeGroupForDay) =
        let date = pointData.Date
        let cases = pointData.Cases

        pojo {|
             x = date |> jsTime12h :> obj
             y = cases
             date = I18N.tOptions "days.longerDate" {| date = date |}
        |}

    let mapAllPoints (groupTimeline: CasesInAgeGroupTimeline) =
        groupTimeline |> List.map mapPoint |> List.toArray

    // generate all series
    let allSeries =
        allGroupsKeys
        |> List.mapi (fun index ageGroupKey ->
            let points =
                timeline
                |> extractTimelineForAgeGroup ageGroupKey
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

    let className = "covid19-infections"
    let baseOptions =
        Highcharts.basicChartOptions
            ScaleType.Linear className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
        chart = pojo
            {|
                animation = false
                ``type`` = "column"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        title = pojo {| text = None |}
        series = allSeries
        xAxis = baseOptions.xAxis
        yAxis = baseOptions.yAxis

        plotOptions = pojo
            {|
                column = pojo
                        {|
                          groupPadding = 0
                          pointPadding = 0
                          borderWidth = 0 |}
                series = pojo {| stacking = "normal" |}
            |}
        legend = pojo {| enabled = true ; layout = "horizontal" |}
    |}

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> Highcharts.chartFromWindow
        ]
    ]

let render state dispatch =
    Html.div [
        renderChartContainer state dispatch
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent
        ("AgeGroupsTimelineViz/Chart", init props.data, update, render)
