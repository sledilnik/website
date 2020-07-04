[<RequireQualifiedAccess>]
module AgeGroupsTimelineViz.Rendering

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
        let colors =
            [| "#FFEEBA"; "#FFDA6B";"#E9B825";"#AEEFDB";"#52C4A2";"#33AB87"
               "#189A73";"#F4B2E0";"#D559B0";"#B01C83" |]
        colors.[ageGroupIndex]

    let mapPoint (pointData: CasesInAgeGroupForDay) =
        let date = pointData.Date
        let cases = pointData.Cases

        pojo {|
             x = date |> jsTime12h :> obj
             y = cases
             date = I18N.tOptions "days.longerDate" {| date = date |}
        |}

    let mapAllPoints (groupTimeline: CasesInAgeGroupTimeline) =
        groupTimeline
        |> List.map mapPoint
        |> List.toArray

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
        tooltip = pojo {|
                          formatter = fun () -> tooltipFormatter jsThis
                          shared = true
                          useHTML = true
                        |}
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
