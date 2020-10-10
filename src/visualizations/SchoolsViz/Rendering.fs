[<RequireQualifiedAccess>]
module SchoolsViz.Rendering

open Data.Schools
open Fable.Core
open Feliz

open Elmish
open Feliz.ElmishComponents

type State = {
//    Metrics : DisplayMetrics
    Data : SchoolsStats
    RangeSelectionButtonIndex: int
    Error: string option
} with
    static member Initial = {
//        Metrics = availableDisplayMetrics.[0]
        Data = [||]
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
    match msg with
    | ConsumeData (Ok data) ->
        invalidOp "todo"
    | ConsumeData (Error error) ->
        { state with Error = Some error }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
//    | ChangeMetrics metrics -> { state with Metrics=metrics }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
//        prop.children [
//            renderChartOptions state dispatch
//            |> chartFromWindow
//        ]
    ]

let render (state: State) dispatch: ReactElement =
    match state.Data, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
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

let renderChart state: ReactElement =
    React.elmishComponent("SchoolsViz/Chart", init(), update, render)
