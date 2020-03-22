[<RequireQualifiedAccess>]
module DataTable

open Feliz

open Types

let render data metrics =
    let header =
      [ "Datum"
        "Testi"
        "Testi skupaj"
        "Pozitivni testi"
        "Pozitivni testi skupaj"
        "Hospitalizirani"
        "Intenzivna nega"
        "Umrli"
        "Umrli skupaj" ]
      |> List.map (fun label -> Html.th [ Html.text label ])
      |> Html.tr

    let renderOption option =
        match option with
        | None -> Html.none
        | Some value -> value.ToString() |> Html.text

    let renderNumber number =
        Html.td
            [ prop.className "has-text-right"
              prop.children [ renderOption number ] ]

    let renderRow (row : DataPoint) =
        Html.tr [
            Html.td [ sprintf "%d-%02d-%02d" row.Date.Date.Year row.Date.Date.Month row.Date.Date.Day |> Html.text ]
            renderNumber row.Tests
            renderNumber row.TotalTests
            renderNumber row.Cases
            renderNumber row.TotalCases
            renderNumber row.Hospitalized
            renderNumber row.HospitalizedIcu
            renderNumber row.Deaths
            renderNumber row.TotalDeaths
        ]

    Html.div [
        prop.className "table-responsive"
        prop.children [
            Html.table [
                prop.className "data-table table table-bordered table-sm"
                prop.children [
                    Html.thead [ header ]
                    Html.tbody (data |> List.map renderRow)
                ]
            ]
        ]
    ]
