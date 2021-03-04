module ExcessDeathsChart.Main

open Feliz
open Browser
open Fable.DateFunctions

open Types

let init statsData =
    { DisplayType = DisplayType.Default
      StatsData = statsData
      DailyDeathsData = Loading
      WeeklyDeathsData = Loading
    }

let update state msg =
    match msg with
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }
    | DailyDeathsDataReceived data ->
        match data with
        | Error err ->
            { state with DailyDeathsData = Failure err ; WeeklyDeathsData = Failure err }
        | Ok data ->
            // Aggregate daily data into ISO weeks
            let weeklyDeathsData =
                data
                |> List.map (fun dp ->
                    {| year = Utils.getISOWeekYear(dp.Date) ; week = dp.Date.GetISOWeek() ; date = dp.Date ; deceased = dp.Deceased |} )
                |> List.groupBy (fun dp -> (dp.year, dp.week))
                |> List.map (fun ((year, week), dps) ->
                    { Year = year
                      Week = week
                      WeekStartDate = dps |> List.map (fun dp -> dp.date) |> List.head
                      WeekEndDate = dps |> List.map (fun dp -> dp.date) |> List.last
                      Deceased = dps |> List.sumBy (fun dp -> dp.deceased) } )

            { state with DailyDeathsData = Success data ; WeeklyDeathsData = Success weeklyDeathsData }

let renderDisplayTypeSelectors state dispatch =
    let selectors =
        DisplayType.All
        |> List.map (fun dt ->
            let selected =
                match state.DisplayType, dt with
                | ExcessDeathsByAgeGroup _, ExcessDeathsByAgeGroup _ -> true
                | ExcessDeaths, ExcessDeaths -> true
                | AbsoluteDeaths, AbsoluteDeaths -> true
                | _ -> false

            Html.div [
                prop.onClick (fun _ -> DisplayTypeChanged dt |> dispatch)
                prop.text dt.GetName
                Utils.classes
                    [ (true, "chart-display-property-selector__item")
                      (selected, "selected") ] ] )

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children selectors
    ]

let renderDisplayTypeOptions (options) (selectedOption) dispatch =
    let renderSelector option =
        let active = option = selectedOption
        Html.div [
            prop.text (option :> IOption).Label
            Utils.classes
                [(true, "btn btn-sm metric-selector")
                 (active, "metric-selector--selected selected")]
            if not active then prop.onClick (fun _ -> dispatch option)
            if active then prop.style [ style.backgroundColor "#808080" ]
          ]

    Html.div [
        prop.className "metrics-selectors"
        options
        |> List.map renderSelector
        |> prop.children
    ]

let chart = React.functionComponent("ExcessDeathsChart", fun (props : {| statsData : Types.StatsData |}) ->
    let (state, dispatch) = React.useReducer(update, init props.statsData)

    let loadData () = async {
        let! data = Data.DailyDeaths.loadData ()
        dispatch (DailyDeathsDataReceived data)
    }

    React.useEffect(loadData >> Async.StartImmediate, [| |])

    let renderRemoteData (data : RemoteData<'T, string>) renderer =
        match data with
        | NotAsked
        | Loading -> React.fragment [ ]
        | Failure error -> Html.div [ Html.text error ]
        | Success data -> renderer data

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
                            prop.style [ style.height 420 ]
                            prop.children [
                                renderRemoteData state.WeeklyDeathsData (Absolute.renderChartOptions >> Highcharts.chart) ] ]
                        Html.div [
                            prop.className "disclaimer"
                            prop.children [
                                Html.text (I18N.chartText "excessDeaths" "absolute.disclaimer")
                                Html.text " "
                                Html.a [
                                    prop.href "https://medium.com/sledilnik/prese%C5%BEna-smrtnost-v-letu-2020-99840508e337"
                                    prop.children [ Html.text "Presežna smrtnost v letu 2020"] ] ] ] ] )
                | ExcessDeaths ->
                    React.keyedFragment (2, [
                        Html.div [
                            prop.style [ style.height 420 ]
                            prop.children [
                                renderRemoteData state.WeeklyDeathsData (Relative.renderChartOptions state.StatsData >> Highcharts.chart) ] ]
                        Html.div [
                            prop.className "disclaimer"
                            prop.children [
                                Html.text (I18N.chartText "excessDeaths" "excess.disclaimer")
                                Html.text " "
                                Html.a [
                                    prop.href "https://medium.com/sledilnik/koliko-preveč-a9afd320653b"
                                    prop.children [ Html.text "Koliko preveč?"] ]
                                Html.text " Več o poročanju COVID-19 smrti s strani NIJZ in vlade pa v članku "
                                Html.a [
                                    prop.href "https://medium.com/sledilnik/zakaj-razlike-v-%C5%A1tevilu-umrlih-185e6a94d6d2"
                                    prop.children [ Html.text "Zakaj razlike v številu umrlih?"] ]
                            ] ] ] )
                | ExcessDeathsByAgeGroup sex ->
                    React.keyedFragment (3, [
                        Html.div [
                            prop.style [ style.height 420 ]
                            prop.children [
                                renderRemoteData state.DailyDeathsData (RelativeByAgeGroup.renderChartOptions sex >> Highcharts.chart) ] ]
                        renderDisplayTypeOptions Sex.All sex (ExcessDeathsByAgeGroup >> DisplayTypeChanged >> dispatch)
                        Html.div [
                            prop.className "disclaimer"
                            prop.children [
                                Html.text (I18N.chartText "excessDeaths" "excessByAgeGroup.disclaimer") ] ] ] )
            ]
        ]
    ]
)
