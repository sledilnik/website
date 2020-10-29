module PhaseDiagram.Data

open Types

let totalVsWeekData metric statsData =
    let windowSize = 7

    statsData
    |> List.map (fun (dp : StatsDataPoint) ->
        match metric with
        | Cases ->
            {| Date = dp.Date
               Count = dp.Cases.ConfirmedToday
               CountToDate = dp.Cases.ConfirmedToDate
            |}
        | Deceased ->
            {| Date = dp.Date
               Count = dp.StatePerTreatment.Deceased
               CountToDate = dp.StatePerTreatment.DeceasedToDate
            |}
        )
    |> List.windowed windowSize
    |> List.map (fun window ->
        let last =
            window
            |> List.toArray
            |> Array.last

        match last.CountToDate with
        | None -> None
        | Some countToDate ->
            let countInWindow =
                window
                |> List.map (fun dp -> dp.Count)
                |> List.choose id
                |> List.sum
            { x = countToDate
              y = countInWindow
              date = last.Date
            } |> Some)
    |> List.choose id
    |> List.toArray

let weekVsWeekBeforeData metric statsData =
    let windowSize = 7

    statsData
    |> List.map (fun (dp : StatsDataPoint) ->
        {| Date = dp.Date
           Count =
            match metric with
            | Cases -> dp.Cases.ConfirmedToday
            | Deceased -> dp.StatePerTreatment.Deceased
        |} )
    |> List.filter (fun dp -> dp.Count |> Option.isSome)
    |> List.windowed (windowSize * 2)
    |> List.map (fun doubleWindow ->
        let firstWindow, secondWindow = List.splitAt windowSize doubleWindow
        let firstWindowSum = firstWindow |> List.map (fun dp -> dp.Count |> Option.defaultValue 0) |> List.sum
        let secondWindowSum = secondWindow |> List.map (fun dp -> dp.Count |> Option.defaultValue 0) |> List.sum
        if firstWindowSum = 0 then
            None
        else
            Some { x = firstWindowSum
                   y = (float secondWindowSum) / (float firstWindowSum) * 100.0 |> System.Convert.ToInt32
                   date = (List.head secondWindow).Date })
    |> List.choose id
    |> List.toArray

let displayData metric diagramKind statsData =
    match diagramKind with
    | TotalVsWeek ->
        totalVsWeekData metric statsData
    | WeekVsWeekBefore ->
        weekVsWeekBeforeData metric statsData
