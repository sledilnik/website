[<RequireQualifiedAccess>]
module Map

open Elmish
open System
open Feliz
open Feliz.ElmishComponents

open Browser

open GoogleCharts

open Types

let excludedRegions = Set.ofList ["t"]

type Municipality = {
    Municipality : Utils.Dictionaries.Municipality
    TotalPositiveTests : int option
    TotalPositiveTestsWeightedRegionPopulation : int option }

type Region = {
    Region : Utils.Dictionaries.Region
    Municipalities : Municipality list }

type Data = {
    Date : System.DateTime
    Regions : Region list }

type DisplayType =
    | AbsoluteValues
    | RegionPopulationWeightedValues

type State =
    { Data : Data list
      DisplayType : DisplayType }

type Msg =
    | DisplayTypeChanged of DisplayType

let init (regionsData : RegionsData) : State * Cmd<Msg> =
    let data =
        regionsData
        |> List.map (fun datapoint ->
            let municipalityData =
                datapoint.Regions
                |> List.map (fun region ->
                    region.Municipalities
                    |> List.map (fun municipality -> (municipality.Name, municipality.PositiveTests)))
                |> List.concat
            let municipalityDataMap =
                municipalityData
                |> Map.ofList
            let regions =
                seq {
                    for region in datapoint.Regions do
                        match Utils.Dictionaries.regions.TryFind region.Name with
                        | None -> ()
                        | Some reg ->
                            if Set.contains reg.Key Utils.Dictionaries.excludedRegions then ()
                            else
                                let municipalities =
                                    seq {
                                        for municipality in Utils.Dictionaries.municipalities do
                                            match Map.tryFind municipality.Key municipalityDataMap with
                                            | None ->
                                                yield { Municipality = municipality.Value
                                                        TotalPositiveTests = None
                                                        TotalPositiveTestsWeightedRegionPopulation = None }
                                            | Some totalPositiveTests ->
                                                yield { Municipality = municipality.Value
                                                        TotalPositiveTests = totalPositiveTests
                                                        TotalPositiveTestsWeightedRegionPopulation =
                                                            match totalPositiveTests with
                                                            | None -> None
                                                            | Some tests -> float tests / float municipality.Value.Population * 100000. |> System.Math.Round |> int |> Some }
                                    } |> List.ofSeq
                                yield { Region = reg
                                        Municipalities = municipalities }
                } |> List.ofSeq

            { Date = datapoint.Date
              Regions = regions })

    { Data = data ; DisplayType = RegionPopulationWeightedValues }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none

let renderMap state =
    let header =
        match state.DisplayType with
        | AbsoluteValues -> ("Občina", "Ime", "Potrjeno okuženi skupaj", "Delež potrjeno okuženih")
        | RegionPopulationWeightedValues -> ("Občina", "Ime", "Delež potrjeno okuženih", "Potrjeno okuženi skupaj")
        |> box

    let data =
        let lastDataPoint = List.last state.Data
        seq {
            for region in lastDataPoint.Regions do
                for municipality in region.Municipalities do
                    let absolute = Option.defaultValue 0 municipality.TotalPositiveTests
                    let weighted = Option.defaultValue 0 municipality.TotalPositiveTestsWeightedRegionPopulation
                    let weightedFmt = sprintf "%d na 100.000 prebivalcev" weighted
                    match state.DisplayType with
                    | AbsoluteValues ->
                        // how to render logarithmic color scale:
                        // https://stackoverflow.com/questions/56275333/display-google-geochart-in-log-scale
                        let scaled = Math.Log (float absolute + 2.0)
                        yield box((municipality.Municipality.Code,
                                   municipality.Municipality.Name,
                                   box {| v=scaled; f=string absolute |},
                                   box {| v=weighted; f=weightedFmt |}))
                    | RegionPopulationWeightedValues ->
                        let scaled = Math.Log (float weighted + 10.0)
                        yield box((municipality.Municipality.Code,
                                   municipality.Municipality.Name,
                                   box {| v=scaled; f=weightedFmt |},
                                   box {| v=absolute; f=absolute |} ))

        } |> List.ofSeq

    Html.div [
        prop.style [ ]
        prop.className "highcharts-wrapper"
        prop.children [
            Chart [
                Props.chartType ChartType.GeoChart
                Props.width "100%"
                // Props.height 450
                Props.data (header :: data)
                Props.options [
                    Options.Region "SI"
                    Options.Resolution Resolution.Provinces
                    Options.DatalessRegionColor "white"
                    Options.DefaultColor "white"
                    Options.ColorAxis {| colors = ("#fefefe", "#e03000") |}
                    Options.Legend "none" // legend doesn't make sense with log scale
                ]
            ]
        ]
    ]

let renderDisplayTypeSelector displayType dispatch =
    let renderSelector (displayType : DisplayType) (currentDisplayType : DisplayType) (label : string) =
        let defaultProps =
            [ prop.text label
              prop.className [
                  true, "chart-display-property-selector__item"
                  displayType = currentDisplayType, "selected" ] ]
        if displayType = currentDisplayType
        then Html.div defaultProps
        else Html.div ((prop.onClick (fun _ -> dispatch displayType)) :: defaultProps)

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            Html.text "Prikazane vrednosti: "
            renderSelector RegionPopulationWeightedValues displayType "Utežene na 100.000 prebivalcev"
            renderSelector AbsoluteValues displayType "Absolutne"
        ]
    ]

let render (state : State) dispatch =
    let elm = Html.div [
        prop.children [
            renderDisplayTypeSelector state.DisplayType (DisplayTypeChanged >> dispatch)
            renderMap state
        ]
    ]

    // trigger event for iframe resize
    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true);
    document.dispatchEvent(evt) |> ignore

    elm

let mapChart (props : {| data : RegionsData |}) =
    React.elmishComponent("MapChart", init props.data, update, render)
