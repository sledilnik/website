module ExcessDeathsChart.Main

open Feliz
open Browser
open Fable.DateFunctions

open Types

let init statsData =
    { StatsData = statsData
      WeeklyDeathsData = Loading
      DisplayType = AbsoluteDeaths
    }

let update state msg =
    match msg with
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }
    | DailyDeathsDataReceived data ->
        match data with
        | Error err ->
            { state with WeeklyDeathsData = Failure err }
        | Ok data ->
            // Aggregate daily data into ISO weeks
            let weeklyDeathsData =
                data
                |> List.map (fun dp ->
                    let date = System.DateTime(dp.year, dp.month, dp.day)
                    {| year = Utils.getISOWeekYear(date) ; week = date.GetISOWeek() ; date = date ; deceased = dp.deceased |} )
                |> List.groupBy (fun dp -> (dp.year, dp.week))
                |> List.map (fun ((year, week), dps) ->
                    { Year = year
                      Week = week
                      WeekStartDate = dps |> List.map (fun dp -> dp.date) |> List.head
                      WeekEndDate = dps |> List.map (fun dp -> dp.date) |> List.last
                      Deceased = dps |> List.sumBy (fun dp -> dp.deceased) } )

            { state with WeeklyDeathsData = Success weeklyDeathsData }

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
        let! data = Data.DailyDeaths.loadData ()
        dispatch (DailyDeathsDataReceived data)
    }

    React.useEffect(loadData >> Async.StartImmediate, [| |])

    match state.WeeklyDeathsData with
    | NotAsked
    | Loading -> React.fragment [ ]
    | Failure error -> Html.div [ Html.text error ]
    | Success data ->
        Html.div [
            Utils.renderChartTopControls [
                renderDisplayTypeSelectors state dispatch ]
            Html.div [
                prop.style [ style.height 450 ]
                prop.className "highcharts-wrapper"
                prop.children [
                    match state.DisplayType with
                    | AbsoluteDeaths ->
                        React.keyedFragment (1, [
                            Html.div [
                                prop.style [ style.height 420 ]
                                prop.children [
                                    Absolute.renderChartOptions data |> Highcharts.chart ] ]
                            Html.div [
                                prop.className "disclaimer"
                                prop.children [
                                    Html.text (I18N.chartText "excessDeaths" "absolute.disclaimer")
                                    Html.text " "
                                    Html.a [
                                        prop.href "https://medium.com/sledilnik/koliko-preveč-a9afd320653b"
                                        prop.children [ Html.text "Koliko preveč?"] ] ] ] ] )
                    | ExcessDeaths ->
                        React.keyedFragment (2, [
                            Html.div [
                                prop.style [ style.height 420 ]
                                prop.children [
                                    Relative.renderChartOptions data state.StatsData |> Highcharts.chart ] ]
                            Html.div [
                                prop.className "disclaimer"
                                prop.children [
                                    Html.text (I18N.chartText "excessDeaths" "excess.disclaimer")
                                    Html.text " "
                                    Html.a [
                                        prop.href "https://medium.com/sledilnik/koliko-preveč-a9afd320653b"
                                        prop.children [ Html.text "Koliko preveč?"] ] ] ] ] )
                ]
            ]
        ]
    )
