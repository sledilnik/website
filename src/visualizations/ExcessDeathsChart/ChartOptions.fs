[<RequireQualifiedAccess>]
module ExcessDeathsChart.ChartOptions

open Highcharts

let responsive =
    {| rules =
        [| {|
            condition = {| maxWidth = 768 |}
            chartOptions =
                {|
                    yAxis = [| {| labels = pojo {| enabled = false |} |} |]
                |}
        |} |]
    |}
