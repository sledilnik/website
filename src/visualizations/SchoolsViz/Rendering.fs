[<RequireQualifiedAccess>]
module SchoolsViz.Rendering

open Data.Schools
open Synthesis
open Types

open Browser.Types
open Elmish
open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Feliz.ElmishComponents
open Highcharts

type State = {
//    Metrics : DisplayMetrics
    Data : SchoolsStats option
    RangeSelectionButtonIndex: int
    Error: string option
} with
    static member Initial = {
//        Metrics = availableDisplayMetrics.[0]
        Data = None
        RangeSelectionButtonIndex = 0
        Error = None
    }

type Msg =
    | ConsumeData of Result<SchoolsStats, string>
    | ConsumeServerError of exn
//    | ChangeMetrics of DisplayMetrics
    | RangeSelectionChanged of int

let init () : State * Cmd<Msg> =
    let cmdFetchData = Cmd.OfAsync.either
                           getOrFetch () ConsumeData ConsumeServerError
    State.Initial, cmdFetchData

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    let logMsg = sprintf "update: %A" msg
    JS.console.log logMsg

    match msg with
    | ConsumeData (Ok data) ->
        { state with Data = Some data; Error = None}, Cmd.none
    | ConsumeData (Error error) ->
        { state with Error = Some error }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
//    | ChangeMetrics metrics -> { state with Metrics=metrics }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

type BarchartOptions = {
    ClassName: string
}

let renderBarChart
    state
    dispatch
    (options: BarchartOptions)
    (allSeries: ChartSeries[]) =

    let convertSeriesEntryToHighchartPoint
        ((date, value): ChartSeriesEntry) =
        pojo {|
             x = date.ToJs() :> obj
             y = value
             date = I18N.tOptions "days.longerDate"
                        {| date = date.ToDateTime() |}
        |}

    let convertSeriesEntriesToHighchartPoints entries =
        entries
        |> Array.map convertSeriesEntryToHighchartPoint

    let convertSeriesToHighchart (series: ChartSeries) =
        pojo {|
             visible = true
             name = series.Title
             color = series.Color
             data = convertSeriesEntriesToHighchartPoints series.Entries
        |}

    let convertDataToHighchart allSeriesData =
        allSeriesData |> Array.map convertSeriesToHighchart

    let className = options.ClassName

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions
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
        series = allSeries |> convertDataToHighchart
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
//        tooltip = pojo {|
//                          formatter = fun () -> tooltipFormatter jsThis
//                          shared = true
//                          useHTML = true
//                        |}
    |}

let renderChart state dispatch =
    JS.console.log "renderChart"

    // todo igor: create covid19-schools CSS class
    let options = { ClassName = "covid19-infections" }
    renderBarChart state dispatch options allSeries

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChart state dispatch |> chartFromWindow
        ]
    ]

let render (state: State) dispatch: ReactElement =
    match state.Data, state.Error with
//    | Some _, None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
//            renderBreakdownSelectors state dispatch
//            Html.div [
//                prop.style [ style.overflow.scroll ]
//                prop.children [
//                    renderTable state dispatch
//                ]
//            ]
        ]

let renderViz state: ReactElement =
    React.elmishComponent("SchoolsViz/Chart", init(), update, render)
