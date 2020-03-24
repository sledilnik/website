
[<RequireQualifiedAccess>]
module RegionsChart

open Elmish

open Feliz
open Feliz.ElmishComponents
open Feliz.Recharts

open Types

let formatDate (date : System.DateTime) =
    sprintf "%d.%d." date.Date.Day date.Date.Month

let renderChart (data : RegionsData) =

    let renderLineLabel (input: ILabelProperties) =
        Html.text [
            prop.x(input.x)
            prop.y(input.y)
            prop.fill color.black
            prop.textAnchor.middle
            prop.dy(-10)
            prop.fontSize 10
            prop.text input.value
        ]

    let renderRegion (region : Region) (dataKey : RegionsDataPoint -> int) =
        Recharts.line [
            line.name region.Name
            line.monotone
            line.stroke "#666"
            line.label renderLineLabel
            line.dataKey dataKey
        ]

    let regionsToRender =
        (List.last data).Regions
        |> List.map (fun region -> region)
        |> List.filter (fun region -> region.Name <> "regija" && region.Name <> "t")

    let children =
        seq {
            yield Recharts.xAxis [ xAxis.dataKey (fun point -> formatDate point.Date) ]
            yield Recharts.yAxis [ ]
            yield Recharts.tooltip [ ]
            yield Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]

            for regionToRender in regionsToRender do
                yield renderRegion regionToRender
                    (fun (point : RegionsDataPoint) ->
                        let region =
                            point.Regions
                            |> List.find (fun reg -> reg.Name = regionToRender.Name)
                        region.Cities
                        |> List.map (fun city -> city.PositiveTests)
                        |> List.choose id
                        |> List.sum
                )
        }

    Recharts.lineChart [
        lineChart.data data
        lineChart.children (Seq.toList children)
    ]

let renderChartContainer data =
    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 500
        responsiveContainer.chart (renderChart data)
    ]

// let renderMetricSelector (metric : Metric) metricMsg dispatch =
//     let style =
//         if metric.Visible
//         then [ style.backgroundColor metric.Color ; style.borderColor metric.Color ]
//         else [ ]
//     Html.div [
//         prop.onClick (fun _ -> ToggleMetricVisible metricMsg |> dispatch)
//         prop.className [ true, "btn  btn-sm metric-selector"; metric.Visible, "metric-selector--selected" ]
//         prop.style style
//         prop.text metric.Label ]

// let renderMetricsSelectors metrics dispatch =
//     Html.div [
//         prop.className "metrics-selectors"
//         prop.children [
//             renderMetricSelector metrics.Tests Tests dispatch
//             renderMetricSelector metrics.TotalTests TotalTests dispatch
//             renderMetricSelector metrics.PositiveTests PositiveTests dispatch
//             renderMetricSelector metrics.TotalPositiveTests TotalPositiveTests dispatch
//             renderMetricSelector metrics.Hospitalized Hospitalized dispatch
//             renderMetricSelector metrics.HospitalizedIcu HospitalizedIcu dispatch
//             renderMetricSelector metrics.Deaths Deaths dispatch
//             renderMetricSelector metrics.TotalDeaths TotalDeaths dispatch ] ]


type State =
    { Data : RegionsData }

type Msg = unit

let init data : State * Cmd<Msg> =
    { Data = data }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    state, Cmd.none

let render (state : State) dispatch =
    Html.div [
        renderChartContainer state.Data
        // renderMetricsSelectors dispatch
    ]

type Props = {
    data : RegionsData
}

let regionsChart (props : Props) =
    React.elmishComponent("RegionsChart", init props.data, update, render)
