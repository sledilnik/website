[<RequireQualifiedAccess>]
module Map

open System

open Fable.SimpleHttp

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Highcharts
open Types

type MapToDisplay = Municipality | Region

let munGeoJsonUrl = "/maps/municipalities-gurs-simplified-3857.geojson"
let regGeoJsonUrl = "/maps/statistical-regions-gurs-simplified-3857.geojson"

let excludedMunicipalities = Set.ofList ["kraj" ; "tujina"]

type TotalCasesForDate =
    { Date : System.DateTime
      TotalConfirmedCases : int option
      TotalDeceasedCases : int option }

type Area =
    { Id : string
      Code : string
      Name : string
      Population : int
      Cases : TotalCasesForDate seq option }

type ContentType =
    | ConfirmedCases
    | Deceased

    override this.ToString() =
       match this with
       | ConfirmedCases -> I18N.t "charts.map.confirmedCases"
       | Deceased       -> I18N.t "charts.map.deceased"

let (|ConfirmedCasesMsgCase|DeceasedMsgCase|) str =
    if str = I18N.t "charts.map.confirmedCases"
    then ConfirmedCasesMsgCase
    else DeceasedMsgCase

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
        | LastDays 1 -> I18N.t "charts.map.yesterday"
        | LastDays days -> I18N.tOptions "charts.map.last_x_days" {| count = days |}

let dataTimeIntervals =
    [ LastDays 1
      LastDays 7
      LastDays 14
      LastDays 21
      Complete ]

type GeoJson = RemoteData<obj, string>

type State =
    { MapToDisplay : MapToDisplay
      GeoJson : GeoJson
      Data : Area seq
      DataTimeInterval : DataTimeInterval
      ContentType : ContentType
      DisplayType : DisplayType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | DataTimeIntervalChanged of DataTimeInterval
    | ContentTypeChanged of string
    | DisplayTypeChanged of DisplayType

let loadMunGeoJson =
    async {
        let! (statusCode, response) = Http.get munGeoJsonUrl

        if statusCode <> 200 then
            return GeoJsonLoaded (sprintf "Error loading map: %d" statusCode |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded (data |> Success)
            with
            | ex -> return GeoJsonLoaded (sprintf "Error loading map: %s" ex.Message |> Failure)
    }

let loadRegGeoJson =
    async {
        let! (statusCode, response) = Http.get regGeoJsonUrl

        if statusCode <> 200 then
            return GeoJsonLoaded (sprintf "Error loading map: %d" statusCode |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded (data |> Success)
            with
            | ex -> return GeoJsonLoaded (sprintf "Error loading map: %s" ex.Message |> Failure)
    }

let init (mapToDisplay : MapToDisplay) (regionsData : RegionsData) : State * Cmd<Msg> =
    let dataTimeInterval = LastDays 14

    let municipalityDataMap =
        seq {
            for regionsDataPoint in regionsData do
                for region in regionsDataPoint.Regions do
                    for municipality in region.Municipalities do
                        if not (Set.contains municipality.Name excludedMunicipalities) then
                            yield {| Date = regionsDataPoint.Date
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
            ( municipalityKey, totalCases ) )
        |> Map.ofSeq

    let munData =
        seq {
            for municipality in Utils.Dictionaries.municipalities do
                match Map.tryFind municipality.Key municipalityDataMap with
                | None ->
                    yield { Id = municipality.Key
                            Code = municipality.Value.Code
                            Name = municipality.Value.Name
                            Population = municipality.Value.Population
                            Cases = None }
                | Some cases ->
                    yield { Id = municipality.Key
                            Code = municipality.Value.Code
                            Name = municipality.Value.Name
                            Population = municipality.Value.Population
                            Cases = Some cases }
        }

    let regDataMap =
        seq {
            for regionsDataPoint in regionsData do
                for region in regionsDataPoint.Regions do
                    yield {| Date = regionsDataPoint.Date
                             RegionKey = region.Name
                             TotalConfirmedCases =
                                region.Municipalities
                                 |> Seq.sumBy (fun municipality -> municipality.ConfirmedToDate |> Option.defaultValue 0)
                             TotalDeceasedCases =
                                region.Municipalities
                                |> Seq.sumBy (fun municipality -> municipality.DeceasedToDate |> Option.defaultValue 0) |} }
        |> Seq.groupBy (fun dp -> dp.RegionKey)
        |> Seq.map (fun (regionKey, dp) ->
            let totalCases =
                dp
                |> Seq.map (fun dp ->
                    { Date = dp.Date
                      TotalConfirmedCases = Some dp.TotalConfirmedCases
                      TotalDeceasedCases = Some dp.TotalDeceasedCases } )
                |> Seq.sortBy (fun dp -> dp.Date)
            ( regionKey, totalCases ) )
        |> Map.ofSeq

    let regData =
        seq {
            for region in Utils.Dictionaries.regions do
                match Map.tryFind region.Key regDataMap with
                | None ->
                    yield { Id = region.Key
                            Code = region.Key
                            Name = I18N.tt "region" region.Key
                            Population = region.Value.Population |> Option.defaultValue 0
                            Cases = None }
                | Some cases ->
                    yield { Id = region.Key
                            Code = region.Key
                            Name = I18N.tt "region" region.Key
                            Population = region.Value.Population |> Option.defaultValue 0
                            Cases = Some cases }
        }

    let data =
        match mapToDisplay with
        | Municipality -> munData
        | Region -> regData

    { MapToDisplay = mapToDisplay
      GeoJson = NotAsked
      Data = data
      DataTimeInterval = dataTimeInterval
      ContentType = ConfirmedCases
      DisplayType = RegionPopulationWeightedValues
    }, Cmd.ofMsg GeoJsonRequested

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | GeoJsonRequested ->
        let cmd =
            match state.MapToDisplay with
            | Municipality -> Cmd.OfAsync.result loadMunGeoJson
            | Region -> Cmd.OfAsync.result loadRegGeoJson
        { state with GeoJson = Loading }, cmd
    | GeoJsonLoaded geoJson ->
        { state with GeoJson = geoJson }, Cmd.none
    | DataTimeIntervalChanged dataTimeInterval ->
        { state with DataTimeInterval = dataTimeInterval }, Cmd.none
    | ContentTypeChanged contentType ->
        let newContentType =
            match contentType with
            | ConfirmedCasesMsgCase -> ConfirmedCases
            | DeceasedMsgCase -> Deceased
        { state with ContentType = newContentType }, Cmd.none
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none

let seriesData (state : State) =

    seq {
        for areaData in state.Data do
            let dlabel, value, absolute, value100k, totalConfirmed, population =
                match areaData.Cases with
                | None -> None, 0.0001, 0, 0.0, 0, areaData.Population
                | Some totalCases ->
                    let confirmedCasesValue = totalCases |> Seq.map (fun dp -> dp.TotalConfirmedCases) |> Seq.choose id |> Seq.toArray
                    let deceasedValue = totalCases |> Seq.map (fun dp -> dp.TotalDeceasedCases) |> Seq.choose id |> Seq.toArray
                    let values =
                        match state.ContentType with
                        | ConfirmedCases -> confirmedCasesValue
                        | Deceased -> deceasedValue

                    let totalConfirmed = confirmedCasesValue |> Array.tryLast

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
                    | None -> None, 0.0001, 0, 0.0, 0, areaData.Population
                    | Some lastValue ->
                        let absolute = lastValue
                        let value100k =
                            float absolute * 100000. / float areaData.Population
                        let dlabel, value =
                            match state.DisplayType with
                            | AbsoluteValues                 -> ((Some absolute) |> Utils.zeroToNone), absolute
                            | RegionPopulationWeightedValues -> None,  10. * value100k |> System.Math.Round |> int  //factor 10 for better resolution in graph
                        let scaled =
                            match state.ContentType with
                            | ConfirmedCases -> 
                                if state.DisplayType = AbsoluteValues 
                                then if absolute > 0 then float absolute else 0.0001 
                                else if value100k > 0.0 then value100k else 0.0001
                            | Deceased ->
                                match value with
                                | 0 -> 0.
                                | x -> float x + Math.E |> Math.Log
                        dlabel, scaled, absolute, value100k, totalConfirmed.Value, areaData.Population  
            {| 
                code = areaData.Code
                area = areaData.Name
                value = value
                absolute = absolute
                value100k = value100k
                totalConfirmed = totalConfirmed
                population = population
                dlabel = dlabel
                dataLabels = {| enabled = true; format = "{point.dlabel}" |} 
            |}
    } |> Seq.toArray


let renderMap (state : State) =

    match state.GeoJson with
    | NotAsked
    | Loading -> Html.none
    | Failure str -> Html.text str
    | Success geoJson ->
        let data = seriesData state

        let key =
            match state.MapToDisplay with
            | Municipality -> "isoid"
            | Region -> "code"

        let series geoJson =
            {| visible = true
               mapData = geoJson
               data = data
               keys = [| "code" ; "value" |]
               joinBy = [| key ; "code" |]
               nullColor = "white"
               borderColor = "#888"
               borderWidth = 0.5
               mapline = {| animation = {| duration = 0 |} |}
               states =
                {| normal = {| animation = {| duration = 0 |} |}
                   hover = {| borderColor = "black" ; animation = {| duration = 0 |} |} |}
           |}

        let tooltipFormatter state jsThis =
            let points = jsThis?point
            let area = points?area
            let absolute = points?absolute
            let value100k = points?value100k
            let totalConfirmed = points?totalConfirmed
            let population = points?population
            let pctPopulation = float absolute * 100.0 / float population
            let fmtStr = sprintf "%s: <b>%d</b>" (I18N.t "charts.map.populationC") population
            let label =
                match state.ContentType with
                | ConfirmedCases ->
                    let label = fmtStr + sprintf "<br>%s: <b>%d</b>" (I18N.t "charts.map.confirmedCases") absolute
                    if absolute > 0 then
                        label 
                            + sprintf " (%s %% %s)" (Utils.formatTo3DecimalWithTrailingZero pctPopulation) (I18N.t "charts.map.population")
                            + sprintf "<br>%s: <b>%0.1f</b> %s" (I18N.t "charts.map.confirmedCases") value100k (I18N.t "charts.map.per100k")
                    else
                        label
                | Deceased ->
                    let label = fmtStr + sprintf "<br>%s: <b>%d</b>" (I18N.t "charts.map.deceased") absolute
                    if absolute > 0 && state.DataTimeInterval = Complete then // deceased
                        label + sprintf " (%s %% %s)"
                                (Utils.formatTo3DecimalWithTrailingZero pctPopulation)
                                (I18N.t "charts.map.population")
                            + sprintf "<br>%s: <b>%d</b> (%s %% %s)"
                                (I18N.t "charts.map.confirmedCases")
                                totalConfirmed (Utils.formatTo3DecimalWithTrailingZero (float totalConfirmed * 100.0 / float population))
                                (I18N.t "charts.map.population")
                            + sprintf "<br>%s: <b>%s %%</b>"
                                (I18N.t "charts.map.mortalityOfConfirmedCases")
                                (Utils.formatTo1DecimalWithTrailingZero (float absolute * 100.0 / float totalConfirmed))
                    else
                        label
            sprintf "<b>%s</b><br/>%s<br/>" area label

        let legend =
            let enabled = state.ContentType = ConfirmedCases
            {| enabled = enabled
               title = {| text = null |}
               align = "right"
               verticalAlign = "bottom"
               layout = "vertical"
               floating = true
               borderWidth = 1
               backgroundColor = "white"
               valueDecimals = 0 |}
            |> pojo

        let colorAxis = 
            match state.ContentType with
                | Deceased ->  
                    {| 
                        ``type`` = "linear"
                        stops =
                            [|
                                (0.0, "#ffffff")
                                (0.111, "#efedf5")
                                (0.222, "#dadaeb")
                                (0.333, "#bcbddc")
                                (0.444, "#9e9ac8")
                                (0.556, "#807dba")
                                (0.667, "#6a51a3")
                                (0.778, "#54278f")
                                (0.889, "#3f007d")
                            |] 
                    |} |> pojo
                | ConfirmedCases -> 
                    {| 
                        ``type`` = "logarithmic"
                        min = 1.0001
                        stops =
                            [|
                                (0.0, "#ffffff")
                                (0.083, "#ffeda0")
                                (0.166, "#fed976")
                                (0.25, "#feb24c")
                                (0.333, "#fd8d3c")
                                (0.417, "#fc4e2a")
                                (0.5, "#e31a1c")
                                (0.583, "#bd0026")
                                (0.666, "#800026")
                                (0.75, "#930044")
                                (0.833, "#5b005c")
                                (0.917, "#26002b")
                            |]
                    |} |> pojo

        {| Highcharts.optionsWithOnLoadEvent "covid19-map" with
            title = null
            series = [| series geoJson |]
            legend = legend
            colorAxis = colorAxis 
            tooltip =
                {|
                    formatter = fun () -> tooltipFormatter state jsThis
                    useHTML = true
                    distance = 50
                |} |> pojo
            credits =
                {|
                    enabled = true
                    text =
                        sprintf "%s: %s, %s"
                            (I18N.t "charts.common.dataSource")
                            (I18N.t "charts.common.dsNIJZ")
                            (I18N.t "charts.common.dsMZ")
                    mapTextFull = ""
                    mapText = ""
                    href = "https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19"
                    position = {| align = "right" ; verticalAlign = "bottom" ; x = -10 ; y = -5 |}
                    style = {| color = "#999999" ; cursor = "pointer" ; fontSize = "9px" |}
                |}
        |}
        |> Highcharts.map

let renderSelector option currentOption dispatch =
    let defaultProps =
        [ prop.text (option.ToString())
          Utils.classes
              [(true, "chart-display-property-selector__item")
               (option = currentOption, "selected") ] ]
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
        prop.children (renderSelectors [ AbsoluteValues; RegionPopulationWeightedValues ] currentDisplayType dispatch)
    ]

let renderDataTimeIntervalSelector currentDataTimeInterval dispatch =
    Html.div [
        prop.className "chart-data-interval-selector"
        prop.children ( Html.text "" :: renderSelectors dataTimeIntervals currentDataTimeInterval dispatch )
    ]

let renderContentTypeSelector selected dispatch =
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
        prop.value (selected.ToString())
        prop.className "form-control form-control-sm filters__type"
        prop.children renderedTypes
        prop.onChange (ContentTypeChanged >> dispatch)
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

let mapChart (props : {| mapToDisplay : MapToDisplay; data : RegionsData |}) =
    React.elmishComponent("MapChart", init props.mapToDisplay props.data, update, render)
