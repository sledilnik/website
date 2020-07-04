module AgeGroupsTimelineViz.LowCharts

open Browser

open Highcharts

let prepareChart series =
    let chart =
            pojo {|
                 ``type`` = "bar"
            |}

    let title = pojo {| text = "Historic World Population by Region" |}

    let xAxis =
        [| {|
             categories = [|"Africa"; "America"; "Asia"; "Europe"; "Oceania"|]
        |} |]
    let yAxis = [| {| |} |]

    {|
         chart = pojo
                {|
                ``type`` = "bar"
                |}
//         xAxis = xAxis
//         yAxis = yAxis
         series = series
    |}
