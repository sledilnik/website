[<RequireQualifiedAccess>]
module HCentersChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types
open Data.HCenters
open Highcharts

type Region =
    { Key : string
      Name : string }

type State = {
    HcData : HcStats []
    Error: string option
    Regions : Region list
    FilterByRegion : string
    RangeSelectionButtonIndex: int
  }

type Msg =
    | ConsumeHcData of Result<HcStats [], string>
    | ConsumeServerError of exn
    | RegionFilterChanged of string
    | RangeSelectionChanged of int

let init () : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either Data.HCenters.getOrFetch () ConsumeHcData ConsumeServerError

    let state = {
        HcData = [| |]
        Error = None
        Regions = [ ]
        FilterByRegion = ""
        RangeSelectionButtonIndex = 0
    }

    state, (cmd)

let getRegionList hcData =
    hcData.municipalities
    |> Map.toList
    |> List.map fst
    |> List.map (fun reg -> { Key = reg ; Name = I18N.tt "region" reg })
    |> List.sortBy (fun region -> region.Name)

let update (msg: Msg) (state: State) : State * Cmd<Msg> =

    match msg with
    | ConsumeHcData (Ok data) ->
        { state with HcData = data; Regions = getRegionList (data |> Array.last) }, Cmd.none
    | ConsumeHcData (Error err) ->
        { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex ->
        { state with Error = Some ex.Message }, Cmd.none
    | RegionFilterChanged region ->
        { state with FilterByRegion = region }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let renderChartOptions (state : State) dispatch =
    let className = "hcenters-chart"
    let scaleType = ScaleType.Linear
    let startDate = DateTime(2020,3,18)
    let mutable startTime = startDate |> jsTime

    let getRegionStats region mp =
            mp |> Map.find region
            |> Map.fold ( fun total _ hc -> total + hc ) TotalHcStats.None

    let hcData =
        match state.FilterByRegion with
        | ""     -> state.HcData |> Seq.map (fun dp -> (dp.Date, dp.all)) |> Seq.toArray
        | region -> state.HcData |> Seq.map (fun dp -> (dp.Date, getRegionStats region dp.municipalities)) |> Seq.toArray

    let allSeries = [
        yield pojo
            {|
                name = I18N.t "charts.hCenters.emergencyExamination"
                ``type`` = "line"
                visible = false
                color = "#70a471"
                dashStyle = Dot |> DashStyle.toString
                data = hcData |> Seq.map (fun (date,dp) -> (date |> jsTime12h, dp.examinations.medicalEmergency)) |> Seq.toArray
            |}

        yield pojo
            {|
                name = I18N.t "charts.hCenters.suspectedCovidExamination"
                ``type`` = "line"
                color = "#a05195"
                dashStyle = Dot |> DashStyle.toString
                data = hcData |> Seq.map (fun (date,dp) -> (date |> jsTime12h, dp.examinations.suspectedCovid)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.hCenters.suspectedCovidPhone"
                ``type`` = "line"
                color = "#d45087"
                dashStyle = Dot |> DashStyle.toString
                data = hcData |> Seq.map (fun (date,dp) -> (date |> jsTime12h, dp.phoneTriage.suspectedCovid)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.hCenters.sentToSelfIsolation"
                ``type`` = "line"
                color = "#665191"
                data = hcData |> Seq.map (fun (date,dp) -> (date |> jsTime12h, dp.sentTo.selfIsolation)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.hCenters.testsPerformed"
                ``type`` = "line"
                color = "#19aebd"
                data = hcData |> Seq.map (fun (date,dp) -> (date |> jsTime12h, dp.tests.performed)) |> Seq.toArray
            |}
        yield pojo
            {|
                name = I18N.t "charts.hCenters.testsPositive"
                ``type`` = "line"
                color = "#d5c768"
                data = hcData |> Seq.map (fun (date,dp) -> (date |> jsTime12h, dp.tests.positive)) |> Seq.toArray
            |}
        yield addContainmentMeasuresFlags startTime None |> pojo

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
        series = List.toArray allSeries

        // need to hide negative label for addContainmentMeasuresFlags
        yAxis = baseOptions.yAxis |> Array.map (fun ax -> {| ax with showFirstLabel = false |})

        legend = pojo {| enabled = true ; layout = "horizontal" |}
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

let renderRegionSelector (regions : Region list) (selected : string) dispatch =
    let renderedRegions = seq {
        yield Html.option [
            prop.text (I18N.t "charts.hCenters.allRegions")
            prop.value ""
        ]

        for region in regions do
            yield Html.option [
                prop.text region.Name
                prop.value region.Key
            ]
    }

    Html.select [
        prop.value selected
        prop.className "form-control form-control-sm filters__region"
        prop.children renderedRegions
        prop.onChange (fun (value : string) -> RegionFilterChanged value |> dispatch)
    ]

let render (state : State) dispatch =
    match state.HcData, state.Error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | _, None ->
        Html.div [
            Utils.renderChartTopControls [
                Html.div [
                    prop.className "filters"
                    prop.children [
                        renderRegionSelector state.Regions state.FilterByRegion dispatch
                    ]
                ]
            ]
            renderChartContainer state dispatch
        ]


let hCentersChart () =
    React.elmishComponent("HCentersChart", init (), update, render)
