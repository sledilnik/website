[<RequireQualifiedAccess>]
module TestsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types
open Highcharts

open Data.LabTests

type DisplayType =
    | Total
    | Data of string
    | Lab of string
with
    static member GetName = function
        | Total -> I18N.t "charts.tests.allTesting"
        | Data typ -> I18N.tt "charts.tests" typ
        | Lab facility -> Utils.Dictionaries.GetFacilityName(facility)

type State = {
    LabData: LabTestsStats array
    Error: string option
    AllLabs : string list
    DisplayType: DisplayType
    RangeSelectionButtonIndex: int
}

let GetAllDisplayTypes state =  
    seq {
        for typ in [ "regular"; "nsapr20" ] do
            yield Data typ
        yield Total
        for lab in state.AllLabs do
            yield Lab lab
    }

type Msg =
    | ConsumeLabTestsData of Result<LabTestsStats array, string>
    | ConsumeServerError of exn
    | ChangeDisplayType of DisplayType
    | RangeSelectionChanged of int

let init : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either getOrFetch () ConsumeLabTestsData ConsumeServerError
    let state = {
        LabData = [||]
        Error = None
        AllLabs = []
        DisplayType = Data "regular"
        RangeSelectionButtonIndex = 0
    }
    state, cmd

let getLabsList (data : LabTestsStats array) =
    data.[data.Length-1].labs
    |> Map.toSeq
    |> Seq.filter (fun (_, stats) -> stats.performed.toDate.IsSome)
    |> Seq.map (fun (lab, stats) -> lab, stats.performed.toDate)
    |> Seq.fold (fun labs (lab,cnt) -> labs |> Map.add lab cnt) Map.empty // all
    |> Map.toList
    |> List.sortBy (fun (_,cnt) -> cnt |> Option.defaultValue -1 |> ( * ) -1)
    |> List.map (fst)

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeLabTestsData (Ok data) ->
        { state with LabData = data; AllLabs = getLabsList data }, Cmd.none
    | ConsumeLabTestsData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | ChangeDisplayType dt ->
        { state with DisplayType = dt }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions (state : State) dispatch =
    let className = "tests-chart"
    let scaleType = ScaleType.Linear

    let positiveTests (dp: LabTestsStats) =
        match state.DisplayType with
        | Total     -> dp.total.positive.today |> Option.defaultValue 0
        | Data typ  -> dp.data.[typ].positive.today |> Option.defaultValue 0
        | Lab lab   -> dp.labs.[lab].positive.today |> Option.defaultValue 0    
    let negativeTests (dp: LabTestsStats) =
        match state.DisplayType with
        | Total     -> (dp.total.performed.today |> Option.defaultValue 0) - (dp.total.positive.today |> Option.defaultValue 0)
        | Data typ  -> (dp.data.[typ].performed.today |> Option.defaultValue 0) - (dp.data.[typ].positive.today |> Option.defaultValue 0)
        | Lab lab   -> (dp.labs.[lab].performed.today |> Option.defaultValue 0) - (dp.labs.[lab].positive.today |> Option.defaultValue 0)
    let percentPositive (dp: LabTestsStats) =
        let positive = positiveTests dp
        let performed = positiveTests dp + negativeTests dp
        Math.Round(float positive / float performed * float 100.0, 2)

    let allYAxis = [|
        {|
            index = 0
            title = {| text = null |}
            labels = pojo {| format = "{value}"; align = "center"; x = -15; reserveSpace = false; |}
            opposite = true
            visible = true
            crosshair = true
        |}
        {|
            index = 1
            title = {| text = null |}
            labels = pojo {| format = "{value}%"; align = "center"; x = 10; reserveSpace = false; |}
            opposite = false
            visible = true
            crosshair = true
        |}
    |]

    let allSeries = [
        yield pojo
            {|
                name = I18N.t "charts.tests.negativeTests"
                ``type`` = "column"
                color = "#19aebd"
                yAxis = 0
                data = state.LabData //|> Seq.filter (fun dp -> dp.Tests.Positive.Today.IsSome )
                    |> Seq.map (fun dp -> (dp.JsDate12h, negativeTests dp)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.tests.positiveTests"
                ``type`` = "column"
                color = "#d5c768"
                yAxis = 0
                data = state.LabData //|> Seq.filter (fun dp -> dp.Tests.Positive.Today.IsSome )
                    |> Seq.map (fun dp -> (dp.JsDate12h, positiveTests dp)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.tests.shareOfPositive"
                ``type`` = "line"
                color = "#665191"
                yAxis = 1
                data = state.LabData //|> Seq.filter (fun dp -> dp.Tests.Positive.Today.IsSome )
                    |> Seq.map (fun dp -> (dp.JsDate12h, percentPositive dp)) |> Seq.toArray
            |}
    ]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        Highcharts.basicChartOptions
            scaleType className
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
    {| baseOptions with
        yAxis = allYAxis
        series = List.toArray allSeries
        plotOptions = pojo
            {|
                column = pojo {| dataGrouping = pojo {| enabled = false |} |}
                series = {| stacking = "normal"; crisp = false; borderWidth = 0; pointPadding = 0; groupPadding = 0 |}
            |}

        legend = pojo {| enabled = true ; layout = "horizontal" |}

        responsive = pojo
            {|
                rules =
                    [| {|
                        condition = {| maxWidth = 768 |}
                        chartOptions =
                            {|
                                yAxis = [|
                                    {| labels = {| enabled = false |} |}
                                    {| labels = {| enabled = false |} |}
                                |]
                            |}
                    |} |]
            |}
    |}

let renderChartContainer (state : State) dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> Highcharts.chartFromWindow
        ]
    ]

let renderSelector state (dt: DisplayType) dispatch =
    Html.div [
        let isActive = state.DisplayType = dt
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (isActive, "metric-selector--selected")]
        prop.text (DisplayType.GetName dt) ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            GetAllDisplayTypes state
            |> Seq.map (fun dt -> renderSelector state dt dispatch) ) ]

let render (state: State) dispatch =
    match state.LabData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            renderChartContainer state dispatch
            renderDisplaySelectors state dispatch
        ]

let testsChart() =
    React.elmishComponent("TestsChart", init, update, render)
