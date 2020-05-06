[<RequireQualifiedAccess>]
module Map

open System

open Fable.SimpleHttp

open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types

let geoJsonUrl = "/maps/municipalities-gurs-simplified-3857.geojson"

let excludedMunicipalities = Set.ofList ["kraj" ; "tujina"]

type TotalConfirmedCasesForDate =
    { Date : System.DateTime
      TotalConfirmedCases : int option }

type Municipality =
    { Municipality : Utils.Dictionaries.Municipality
      Region : Utils.Dictionaries.Region option
      TotalConfirmedCases : TotalConfirmedCasesForDate seq option }

type DisplayType =
    | AbsoluteValues
    | RegionPopulationWeightedValues

    override this.ToString() =
       match this with
       | AbsoluteValues -> "Absolutno"
       | RegionPopulationWeightedValues -> "Delež prebivalstva"

type DataTimeInterval =
    | Complete
    | LastDays of int

    override this.ToString() =
        match this with
        | Complete -> "Vsi"
        | LastDays days -> sprintf "%d dni" days

let dataTimeIntervals =
    [ LastDays 7
      LastDays 14
      LastDays 21
      Complete ]

type GeoJson = RemoteData<obj, string>

type State =
    { GeoJson : GeoJson
      Data : Municipality seq
      DataTimeInterval : DataTimeInterval
      DisplayType : DisplayType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | DataTimeIntervalChanged of DataTimeInterval
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
    let dataTimeInterval = LastDays 21

    let municipalityDataMap =
        seq {
            for regionsDataPoint in regionsData do
                for region in regionsDataPoint.Regions do
                    for municipality in region.Municipalities do
                        if not (Set.contains municipality.Name excludedMunicipalities) then
                            yield {| Date = regionsDataPoint.Date
                                     RegionKey = region.Name
                                     MunicipalityKey = municipality.Name
                                     TotalConfirmedCases = municipality.ConfirmedToDate |} }
        |> Seq.groupBy (fun dp -> dp.MunicipalityKey)
        |> Seq.map (fun (municipalityKey, dp) ->
            let totalConfirmedCases =
                dp
                |> Seq.map (fun dp -> { Date = dp.Date ; TotalConfirmedCases = dp.TotalConfirmedCases })
                |> Seq.sortBy (fun dp -> dp.Date)
            ( municipalityKey,
              {|
                Region = (dp |> Seq.tryLast) |> Option.map (fun dp -> Utils.Dictionaries.regions.TryFind dp.RegionKey) |> Option.flatten
                TotalConfirmedCases = totalConfirmedCases |} ) )
        |> Map.ofSeq

    let data =
        seq {
            for municipality in Utils.Dictionaries.municipalities do
                match Map.tryFind municipality.Key municipalityDataMap with
                | None ->
                    yield { Municipality = municipality.Value
                            Region = None
                            TotalConfirmedCases = None }
                | Some data ->
                    yield { Municipality = municipality.Value
                            Region = data.Region
                            TotalConfirmedCases = Some data.TotalConfirmedCases }
        }
        
    { GeoJson = NotAsked
      Data = data
      DataTimeInterval = dataTimeInterval
      DisplayType = RegionPopulationWeightedValues
    }, Cmd.ofMsg GeoJsonRequested

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | GeoJsonRequested ->
        { state with GeoJson = Loading }, Cmd.OfAsync.result loadGeoJson
    | GeoJsonLoaded geoJson ->
        { state with GeoJson = geoJson }, Cmd.none
    | DataTimeIntervalChanged dataTimeInterval ->
        { state with DataTimeInterval = dataTimeInterval }, Cmd.none
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none

// TODO
let chartLoadedEvent () =
    // trigger event for iframe resize
    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true)
    document.dispatchEvent(evt) |> ignore

let seriesData (state : State) =
    let renderLabel absolute weighted population =
        let weightedFmt = sprintf "%d,%03d %%" (weighted / 1000) (weighted % 1000)
        sprintf "Prebivalcev: <b>%d</b><br>Potrjeno okuženih skupaj: <b>%d</b><br>Delež okuženih: <b>%s</b>" population absolute weightedFmt

    seq {
        for municipalityData in state.Data do
            let value, label =
                match municipalityData.TotalConfirmedCases with
                | None -> 0., renderLabel 0 0
                | Some totalConfirmedCases ->
                    let values =
                        totalConfirmedCases
                        |> Seq.map (fun dp -> dp.TotalConfirmedCases)
                        |> Seq.choose id
                        |> Seq.toArray

                    let lastValueTotal = values |> Array.tryLast

                    let lastValueRelative =
                        match state.DataTimeInterval with
                        | Complete -> lastValueTotal
                        | LastDays days ->
                            let firstValueTotal = values |> Array.tryItem (values.Length - days)
                            match firstValueTotal, lastValueTotal with
                            | None, None -> None
                            | None, Some b -> Some b
                            | Some a, None -> Some a
                            | Some a, Some b -> Some (b - a)

                    match lastValueRelative with
                    | None -> 0., renderLabel 0 0
                    | Some lastValue ->
                        let absolute = lastValue
                        let weighted = float absolute / float municipalityData.Municipality.Population * 100000. |> System.Math.Round |> int
                        let value =
                            match state.DisplayType with
                            | AbsoluteValues ->
                                absolute
                            | RegionPopulationWeightedValues ->
                                weighted
                        let scaled =
                            match value with
                            | 0 -> 0.
                            | x -> float x + Math.E |> Math.Log
                        scaled, renderLabel absolute (weighted |> int)
            {| isoid = municipalityData.Municipality.Code ; value = value ; label = label municipalityData.Municipality.Population |}
    } |> Seq.toArray

let renderMap (state : State) =

    match state.GeoJson with
    | NotAsked
    | Loading -> Html.none
    | Failure str -> Html.text str
    | Success geoJson ->
        let data = seriesData state

        let series geoJson =
            {| visible = true
               mapData = geoJson
               data = data
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

        {| Highcharts.optionsWithOnLoadEvent "covid19-map" with
            title = null
            series = [| series geoJson |]
            legend = {| enabled = false |}
            colorAxis = {| minColor = "white" ; maxColor = "#e03000" |}
        |}
        |> Highcharts.map

let renderSelector option currentOption dispatch =
    let defaultProps =
        [ prop.text (option.ToString())
          prop.className [
              true, "chart-display-property-selector__item"
              option = currentOption, "selected" ] ]
    if option = currentOption
    then Html.div defaultProps
    else Html.div ((prop.onClick (fun _ -> dispatch option)) :: defaultProps)

let renderSelectors options currentOption dispatch =
    options
    |> List.map (fun option ->
        renderSelector option currentOption dispatch)

let renderDisplayTypeSelector currentDisplayType dispatch =
    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (Html.text "Prikaži:" :: renderSelectors [ AbsoluteValues; RegionPopulationWeightedValues ] currentDisplayType dispatch)
    ]

let renderDataTimeIntervalSelector currentDataTimeInterval dispatch =
    Html.div [
        prop.className "chart-data-interval-selector"
        prop.children (Html.text "Filter:" :: renderSelectors dataTimeIntervals currentDataTimeInterval dispatch)
    ]

let render (state : State) dispatch =
    Html.div [
        prop.children [
            Html.div [
                prop.className "filter-and-display"
                prop.children [
                    Html.div [
                        renderDataTimeIntervalSelector state.DataTimeInterval (DataTimeIntervalChanged >> dispatch)
                        renderDisplayTypeSelector state.DisplayType (DisplayTypeChanged >> dispatch)
                    ] 
                ]
            ]
            Html.div [
                prop.className "map"
                prop.children [ renderMap state ]
            ]
        ]
    ]

let mapChart (props : {| data : RegionsData |}) =
    React.elmishComponent("MapChart", init props.data, update, render)
