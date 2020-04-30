[<RequireQualifiedAccess>]
module Map

open System

open Fable.SimpleHttp

open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types

let geoJsonUrl = "https://raw.githubusercontent.com/sledilnik/website/0d160782b4382f384ac0755a542948541e6d8b49/src/assets/maps/municipalities-gurs-simplified-3857.geojson"

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

type GeoJson = RemoteData<obj, string>

type State =
    { GeoJson : GeoJson
      Data : Data list
      DisplayType : DisplayType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | DisplayTypeChanged of DisplayType

let loadGeoJson =
    async {
        let! (statusCode, response) = Http.get geoJsonUrl

        if statusCode <> 200 then
            return GeoJsonLoaded (sprintf "Napaka pri nalaganju zemljevida: %d" statusCode |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded (data |> Success)
            with
                | ex -> return GeoJsonLoaded (sprintf "Napaka pri nalaganju zemljevida: %s" ex.Message |> Failure)
    }

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

    { Data = data
      GeoJson = NotAsked
      DisplayType = RegionPopulationWeightedValues
    }, Cmd.ofMsg GeoJsonRequested

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | GeoJsonRequested ->
        { state with GeoJson = Loading }, Cmd.OfAsync.result loadGeoJson
    | GeoJsonLoaded geoJson ->
        { state with GeoJson = geoJson }, Cmd.none
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none

// TODO
let chartLoadedEvent () =
    // trigger event for iframe resize
    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true)
    document.dispatchEvent(evt) |> ignore

let seriesData state =
    let lastDataPoint = List.last state.Data
    seq {
        for region in lastDataPoint.Regions do
            for municipality in region.Municipalities do
                let absolute = Option.defaultValue 0 municipality.TotalPositiveTests
                let weighted = Option.defaultValue 0 municipality.TotalPositiveTestsWeightedRegionPopulation
                let weightedFmt = sprintf "%d,%03d %%" (weighted / 1000) (weighted % 1000)
                let label = sprintf "Prebivalcev: <b>%d</b><br>Potrjeno okuženih skupaj: <b>%d</b><br>Delež okuženih: <b>%s</b>" municipality.Municipality.Population absolute weightedFmt
                let value =
                    match state.DisplayType with
                    | AbsoluteValues ->
                        absolute
                    | RegionPopulationWeightedValues ->
                        weighted
                let scaled =
                    match value with
                    | 0 -> 0.
                    | x -> float x |> Math.Log
                {| isoid = municipality.Municipality.Code ; value = scaled ; label = label |}

    } |> Seq.toArray

let renderMap (state : State) =

    let series geoJson =
        {| visible = true
           mapData = geoJson
           data = seriesData state
           keys = [| "isoid" ; "value" |]
           joinBy = "isoid"
           nullColor = "white"
           borderColor = "#888"
           borderWidth = 0.5
           mapline = {| animation = {| duration = 0 |} |}
           states =
            {| normal = {| animation = {| duration = 0 |} |}
               hover = {| borderColor = "black" ; animation = {| duration = 0 |} |} |}
           tooltip =
            {| distance = 50
               headerFormat = "<b>{point.key}</b><br>"
               pointFormat = "{point.label}" |}
        |}

    // Adjust the color scale for better readibility
    let colorAxisMin =
        match state.DisplayType with
        | AbsoluteValues -> 0.
        | RegionPopulationWeightedValues -> Math.E

    match state.GeoJson with
    | NotAsked
    | Loading -> Html.none
    | Failure str -> Html.text str
    | Success geoJson ->
        {| title = null
           series = [| series geoJson |]
           legend = {| enabled = false |}
           colorAxis = {| min = colorAxisMin ; minColor = "white" ; maxColor = "#e03000" |}
        |}
        |> Highcharts.map

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
            Html.text "Prikaži:"
            renderSelector AbsoluteValues displayType "Absolutno"
            renderSelector RegionPopulationWeightedValues displayType "Delež prebivalstva"
        ]
    ]

let render (state : State) dispatch =
    Html.div [
        prop.children [
            renderDisplayTypeSelector state.DisplayType (DisplayTypeChanged >> dispatch)
            Html.div [
                prop.className "map"
                prop.children [ renderMap state ]
            ]
        ]
    ]

let mapChart (props : {| data : RegionsData |}) =
    React.elmishComponent("MapChart", init props.data, update, render)
