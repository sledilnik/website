module MonthlyDeathsChart.Chart

open Feliz

open Types

let init statsData =
    { StatsData = statsData
      MonthlyDeathsData = Loading
    }

let update state msg =
    match msg with
    | MonthlyDeathsDataReceived data ->
        match data with
        | Error err ->
            { state with MonthlyDeathsData = Failure err }
        | Ok data ->
            { state with MonthlyDeathsData = Success data }


let chart = React.functionComponent("MonthlyDeathsChart", fun (props : {| statsData : Types.StatsData |}) ->
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
            Absolute.renderChart data state dispatch
        ]
)
