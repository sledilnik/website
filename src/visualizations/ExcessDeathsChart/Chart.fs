module ExcessDeathsChart.Chart

open Feliz
open Browser
open Fable.Core.JsInterop

open Types

open Highcharts

let init statsData =
    { StatsData = statsData
      MonthlyDeathsData = Loading
      DisplayType = AbsoluteDeaths
    }

let update state msg =
    match msg with
    | MonthlyDeathsDataReceived data ->
        match data with
        | Error err ->
            { state with MonthlyDeathsData = Failure err }
        | Ok data ->
            { state with MonthlyDeathsData = Success data }
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }

let renderDisplayTypeSelectors state dispatch =
    let selectors =
        DisplayType.available
        |> List.map (fun dt ->
            Html.div [
                prop.onClick (fun _ -> DisplayTypeChanged dt |> dispatch)
                Utils.classes
                    [(true, "chart-display-property-selector__item")
                     (state.DisplayType = dt, "selected")]
                prop.text (DisplayType.getName dt) ] )

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children selectors
    ]

let chart = React.functionComponent("ExcessDeathsChart", fun (props : {| statsData : Types.StatsData |}) ->
    let (state, dispatch) = React.useReducer(update, init props.statsData)

    let loadData () = async {
        let! data = Data.MonthlyDeaths.loadData ()
        dispatch (MonthlyDeathsDataReceived data)
    }

    React.useEffect(loadData >> Async.StartImmediate, [| |])

    match state.MonthlyDeathsData with
    | NotAsked
    | Loading -> React.fragment [ ]
    | Failure error -> Html.div [ Html.text error ]
    | Success data ->
        Html.div [
            Utils.renderChartTopControls [
                renderDisplayTypeSelectors state dispatch ]
            Html.div [
                prop.className "highcharts-wrapper"
                prop.children [
                    match state.DisplayType with
                    | AbsoluteDeaths ->
                        React.keyedFragment (1, [
                            Html.div [
                                prop.style [ style.height 450 ]
                                prop.children [
                                    Absolute.renderChartOptions data |> Highcharts.chart ] ] ] )
                            Html.div [
                                prop.className "disclaimer"
                                prop.children [
                                    Html.text (I18N.chartText "excessDeaths" "absolute.disclaimer") ] ] ] )
                    | ExcessDeaths ->
                        React.keyedFragment (2, [
                            Html.div [
                                prop.style [ style.height 397 ]
                                prop.children [ Relative.renderChartOptions data state.StatsData |> Highcharts.chart ] ]
                            Html.div [
                                prop.className "disclaimer"
                                prop.children [
                                    Html.text (I18N.chartText "excessDeaths" "excess.disclaimer") ] ] ] )
                ]
            ]
        ]
    )
