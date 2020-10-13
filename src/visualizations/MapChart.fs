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
      DisplayType = AbsoluteValues
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

    let renderLabel population absolute totalConfirmed =
        let pctPopulation = float absolute * 100.0 / float population
        let fmtStr = sprintf "%s: <b>%d</b>" (I18N.t "charts.map.populationC") population
        match state.ContentType with
        | ConfirmedCases ->
            let label = fmtStr + sprintf "<br>%s: <b>%d</b>" (I18N.t "charts.map.confirmedCases") absolute
            if absolute > 0 then
                label + sprintf " (%s %% %s)" (Utils.formatTo3DecimalWithTrailingZero pctPopulation) (I18N.t "charts.map.population")
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

    seq {
        for areaData in state.Data do
            let dlabel, value, label =
                match areaData.Cases with
                | None -> None, 0., (renderLabel areaData.Population 0 0)
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
                    | None -> None, 0., (renderLabel areaData.Population 0 0)
                    | Some lastValue ->
                        let absolute = lastValue
                        let weighted =
                            float absolute * 1000000. / float areaData.Population
                            |> System.Math.Round |> int
                        let dlabel, value =
                            match state.DisplayType with
                            | AbsoluteValues                 -> ((Some absolute) |> Utils.zeroToNone), absolute
                            | RegionPopulationWeightedValues -> None, weighted
                        let scaled =
                            match value with
                            | 0 -> 0.
                            | x -> float x + Math.E |> Math.Log
                        dlabel, scaled, (renderLabel areaData.Population absolute totalConfirmed.Value)
            {| code = areaData.Code ; area = areaData.Name ; value = value ; label = label; dlabel = dlabel; dataLabels = {| enabled = true; format = "{point.dlabel}" |} |}
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

        let tooltipFormatter jsThis =
            let points = jsThis?point
            let area = points?area
            let label = points?label
            sprintf "<b>%s</b><br/>%s<br/>" area label

        let colors =
            match state.ContentType with
            | Deceased -> [|"#fcfbfd";"#efedf5";"#dadaeb";"#bcbddc";"#9e9ac8";"#807dba";"#6a51a3";"#4a1486"|] //purple scale
            | ConfirmedCases -> [| "#ffffcc";"#ffeda0";"#fed976";"#feb24c";"#fd8d3c";"#fc4e2a";"#e31a1c";"#b10026"|] //colors from EuropeMapChart

        let maxValue = data |> Seq.map (fun dp -> dp.value) |> Seq.max

        let classes = 
            let widths = [|0.0; 0.125; 0.25; 0.375; 0.5; 0.625; 0.75; 0.875 |] 
            let scale = float maxValue
            widths |> Seq.map ( (*) scale ) //equal width classes between 0 and maxValue (data is logarithmic)

        let colorAxis = 
            if maxValue = 0. then 
                {| dataClassColor = "category"; dataClasses = [| {| from = 0; color = colors.[0] |} |] |} |> pojo //override for empty map with initial color
            else
                let dataClasses = 
                    Seq.zip classes colors 
                    |> Seq.map( fun (cls, clr) -> {| from = cls; color = clr |} )        
                    |> Seq.toArray
                {| dataClassColor = "category"; dataClasses = dataClasses |} |> pojo
        
        {| Highcharts.optionsWithOnLoadEvent "covid19-map" with
            title = null
            series = [| series geoJson |]
            legend = {| enabled = false |}
            colorAxis = colorAxis
            tooltip =
                {|
                    formatter = fun () -> tooltipFormatter jsThis
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
        prop.children (Html.text (I18N.t "charts.common.view") :: renderSelectors [ AbsoluteValues; RegionPopulationWeightedValues ] currentDisplayType dispatch)
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
