module AgeGroupsTimelineViz.LowCharts

open Browser

open Highcharts

let prepareChart() =
    let chart =
            pojo {|
                 ``type`` = "bar"
            |}

    let title = pojo {| text = "Historic World Population by Region" |}

    let xAxis =
        pojo {|
             categories = [|"Africa"; "America"; "Asia"; "Europe"; "Oceania"|]
        |}
    let yAxis = pojo {| |}

    let series =
        [|
            pojo {|
                 name = "Year 1800"
                 data = [107, 31, 635, 203, 2]
            |}
        |]

    pojo {|
         chart = chart
         title = title
         xAxis = xAxis
         yAxis = yAxis
         series = series
    |}
