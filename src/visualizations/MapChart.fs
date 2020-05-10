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

type TotalCasesForDate =
    { Date : System.DateTime
      TotalConfirmedCases : int option
      TotalDeceasedCases : int option }

type Municipality =
    { Municipality : Utils.Dictionaries.Municipality
      Region : Utils.Dictionaries.Region option
      Cases : TotalCasesForDate seq option }

type ContentType =
    | ConfirmedCases
    | Deceased

    override this.ToString() =
       match this with
       | ConfirmedCases -> "Potrjeni primeri"
       | Deceased -> "Umrli"

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
      ContentType : string 
      DisplayType : DisplayType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | DataTimeIntervalChanged of DataTimeInterval
    | ContentTypeChanged of string
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
    let dataTimeInterval = LastDays 14

    let municipalityDataMap =
        seq {
            for regionsDataPoint in regionsData do
                for region in regionsDataPoint.Regions do
                    for municipality in region.Municipalities do
                        if not (Set.contains municipality.Name excludedMunicipalities) then
                            yield {| Date = regionsDataPoint.Date
                                     RegionKey = region.Name
                                     MunicipalityKey = municipality.Name
                                     TotalConfirmedCases = municipality.ConfirmedToDate
                                     TotalDeceasedCases = municipality.DeceasedToDate |} }
        |> Seq.groupBy (fun dp -> dp.MunicipalityKey)
        |> Seq.map (fun (municipalityKey, dp) ->
            let totalCases =
                dp
                |> Seq.map (fun dp -> 
                    { Date = dp.Date
                      TotalConfirmedCases = dp.TotalConfirmedCases
                      TotalDeceasedCases = dp.TotalDeceasedCases } )
                |> Seq.sortBy (fun dp -> dp.Date)
            ( municipalityKey,
              {|
                Region = (dp |> Seq.tryLast) |> Option.map (fun dp -> Utils.Dictionaries.regions.TryFind dp.RegionKey) |> Option.flatten
                Cases = totalCases |} ) )
        |> Map.ofSeq

    let data =
        seq {
            for municipality in Utils.Dictionaries.municipalities do
                match Map.tryFind municipality.Key municipalityDataMap with
                | None ->
                    yield { Municipality = municipality.Value
                            Region = None
                            Cases = None }
                | Some data ->
                    yield { Municipality = municipality.Value
                            Region = data.Region
                            Cases = Some data.Cases }
        }

    { GeoJson = NotAsked
      Data = data
      DataTimeInterval = dataTimeInterval
      ContentType = (ConfirmedCases.ToString()) 
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
    | ContentTypeChanged contentType ->
        { state with ContentType = contentType }, Cmd.none
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
        let weightedFmt = sprintf "%d,%03d %%" (weighted / 10000) (weighted % 10000 / 10)
        let mutable fmtStr = sprintf "Prebivalcev: <b>%d</b>" population
        if state.ContentType = Deceased.ToString()
        then fmtStr <- fmtStr + sprintf "<br>Umrlih skupaj: <b>%d</b>" absolute
        else fmtStr <- fmtStr + sprintf "<br>Potrjeno okuženih skupaj: <b>%d</b>" absolute
        if absolute > 0 then
            if state.ContentType = Deceased.ToString()
            then fmtStr <- fmtStr + sprintf "<br>Delež umrlih: <b>%s</b>" weightedFmt
            else fmtStr <- fmtStr + sprintf "<br>Delež okuženih: <b>%s</b>" weightedFmt
        fmtStr

    seq {
        for municipalityData in state.Data do
            let value, label =
                match municipalityData.Cases with
                | None -> 0., renderLabel 0 0
                | Some totalCases ->
                    let values =
                        totalCases
                        |> Seq.map (fun dp -> 
                            if state.ContentType = Deceased.ToString() 
                            then dp.TotalDeceasedCases 
                            else dp.TotalConfirmedCases)
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
                        let weighted = float absolute * 1000000. / float municipalityData.Municipality.Population |> System.Math.Round |> int
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

        let maxColor = if state.ContentType = Deceased.ToString() then "#808080" else "#e03000"
        {| Highcharts.optionsWithOnLoadEvent "covid19-map" with
            title = null
            series = [| series geoJson |]
            legend = {| enabled = false |}
            colorAxis = {| minColor = "white" ; maxColor = maxColor |}
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
        prop.children ( Html.text "" :: renderSelectors dataTimeIntervals currentDataTimeInterval dispatch )
    ]

let renderContentTypeSelector (selected : string) dispatch =
    let renderedTypes = seq {
        yield Html.option [
            prop.text (ContentType.ConfirmedCases.ToString())
            prop.value (ContentType.ConfirmedCases.ToString())
        ]
        yield Html.option [
            prop.text (ContentType.Deceased.ToString())
            prop.value (ContentType.Deceased.ToString())
        ]
    }

    Html.select [
        prop.value selected
        prop.className "form-control form-control-sm filters__type"
        prop.children renderedTypes
        prop.onChange (fun (value : string) -> ContentTypeChanged value |> dispatch)
    ]

let render (state : State) dispatch =
    Html.div [
        prop.children [
            Utils.renderChartTopControls [
                Html.div [
                    prop.className "filters"
                    prop.children [
                        renderContentTypeSelector state.ContentType dispatch
                        renderDataTimeIntervalSelector state.DataTimeInterval (DataTimeIntervalChanged >> dispatch)
                    ]
                ]
                renderDisplayTypeSelector
                    state.DisplayType (DisplayTypeChanged >> dispatch)
            ]
            Html.div [
                prop.className "map"
                prop.children [ renderMap state ]
            ]
        ]
    ]

let mapChart (props : {| data : RegionsData |}) =
    React.elmishComponent("MapChart", init props.data, update, render)
