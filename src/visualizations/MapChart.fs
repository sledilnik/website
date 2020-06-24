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
       | ConfirmedCases -> I18N.t "charts.map.confirmedCases"
       | Deceased       -> I18N.t "charts.map.deceased"

type DisplayType =
    | AbsoluteValues
    | RegionPopulationWeightedValues

    override this.ToString() =
       match this with
       | AbsoluteValues                 -> I18N.t "charts.map.absolute"
       | RegionPopulationWeightedValues -> I18N.t "charts.map.populationShare"

type DataTimeInterval =
    | Complete
    | LastDays of int

    override this.ToString() =
        match this with
        | Complete -> I18N.t "charts.map.all"
        | LastDays days -> I18N.tOptions "charts.map.last_x_days" {| count = days |} 

let dataTimeIntervals =
    [ LastDays 1      
      LastDays 7
      LastDays 14
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
            return GeoJsonLoaded (sprintf "Error loading map: %d" statusCode |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded (data |> Success)
            with
                | ex -> return GeoJsonLoaded (sprintf "Error loading map: %s" ex.Message |> Failure)
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

    let renderLabel population absolute totalConfirmed =
        let pctPopulation = float absolute * 100.0 / float population 
        let mutable fmtStr = sprintf "%s: <b>%d</b>" (I18N.t "charts.map.populationC") population
        if state.ContentType = ConfirmedCases.ToString()
        then 
            fmtStr <- fmtStr + sprintf "<br>%s: <b>%d</b>" (I18N.t "charts.map.confirmedCases") absolute
            if absolute > 0 then
                fmtStr <- fmtStr + sprintf " (%s %% %s)" 
                    (Utils.formatTo3DecimalWithTrailingZero pctPopulation)
                    (I18N.t "charts.map.population")
        else // deceased
            fmtStr <- fmtStr + sprintf "<br>%s: <b>%d</b>" (I18N.t "charts.map.deceased") absolute
            if absolute > 0 && state.DataTimeInterval = Complete then // deceased
                fmtStr <- fmtStr + sprintf " (%s %% %s)" 
                    (Utils.formatTo3DecimalWithTrailingZero pctPopulation)
                    (I18N.t "charts.map.population")
                fmtStr <- fmtStr + sprintf "<br>%s: <b>%d</b> (%s %% %s)" (I18N.t "charts.map.confirmedCases") 
                    totalConfirmed (Utils.formatTo3DecimalWithTrailingZero (float totalConfirmed * 100.0 / float population))
                    (I18N.t "charts.map.population")
                fmtStr <- fmtStr + sprintf "<br>%s: <b>%s %%</b>" (I18N.t "charts.map.mortalityOfConfirmedCases")
                    (Utils.formatTo1DecimalWithTrailingZero (float absolute * 100.0 / float totalConfirmed))
        fmtStr

    seq {
        for municipalityData in state.Data do
            let value, label =
                match municipalityData.Cases with
                | None -> 0., (renderLabel municipalityData.Municipality.Population 0 0)
                | Some totalCases ->
                    let valC = totalCases |> Seq.map (fun dp -> dp.TotalConfirmedCases) |> Seq.choose id |> Seq.toArray
                    let valD = totalCases |> Seq.map (fun dp -> dp.TotalDeceasedCases) |> Seq.choose id |> Seq.toArray
                    let values = if state.ContentType = Deceased.ToString() then valD else valC

                    let totalConfirmed = valC |> Array.tryLast
                   
                    let lastValueTotal = values |> Array.tryLast
                    let lastValueRelative =
                        match state.DataTimeInterval with
                        | Complete -> lastValueTotal
                        | LastDays days ->
                            let firstValueTotal = values |> Array.tryItem (values.Length - days - 1)
                            match firstValueTotal, lastValueTotal with
                            | None, None -> None
                            | None, Some b -> Some b
                            | Some a, None -> Some a
                            | Some a, Some b -> Some (b - a)

                    match lastValueRelative with
                    | None -> 0., (renderLabel municipalityData.Municipality.Population 0 0)
                    | Some lastValue ->
                        let absolute = lastValue
                        let weighted = 
                            float absolute * 1000000. / float municipalityData.Municipality.Population 
                            |> System.Math.Round |> int
                        let value =
                            match state.DisplayType with
                            | AbsoluteValues                 -> absolute
                            | RegionPopulationWeightedValues -> weighted
                        let scaled =
                            match value with
                            | 0 -> 0.
                            | x -> float x + Math.E |> Math.Log
                        scaled, (renderLabel municipalityData.Municipality.Population absolute totalConfirmed.Value)
            {| isoid = municipalityData.Municipality.Code ; value = value ; label = label |}
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

        let maxValue = data |> Seq.map (fun dp -> dp.value) |> Seq.max
        let maxColor = 
            if maxValue = 0. then "white" // override for empty map
            else if state.ContentType = Deceased.ToString() then "#808080" else "#e03000"
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
        prop.children (Html.text (I18N.t "charts.map.view") :: renderSelectors [ AbsoluteValues; RegionPopulationWeightedValues ] currentDisplayType dispatch)
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