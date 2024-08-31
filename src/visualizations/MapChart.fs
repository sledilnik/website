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

type MapToDisplay = MunicipalityMap | RegionMap

let munGeoJsonUrl = "/maps/municipalities-regions-gurs-simplified-3857.geojson"
let regGeoJsonUrl = "/maps/statistical-regions-gurs-simplified-3857.geojson"

let excludedMunicipalities = Set.ofList ["kraj" ; "tujina"]

type TotalCasesForDate =
    { Date : DateTime
      TotalConfirmedCases : int option
      TotalDeceasedCases : int option
      TotalVaccinated1st : int option
      TotalVaccinated2nd : int option
      TotalVaccinated3rd : int option }

type Area =
    { Id : string
      Code : string
      Name : string
      Population : int
      Cases : TotalCasesForDate seq option }

type ContentType =
    | ConfirmedCases
    | Vaccinated1st
    | Vaccinated2nd
    | Vaccinated3rd
    | Deceased
    with
    static member Default mapToDisplay =
        match mapToDisplay with
        | MunicipalityMap -> ConfirmedCases
        | RegionMap       -> ConfirmedCases
    static member GetName = function
       | ConfirmedCases -> I18N.t "charts.map.confirmedCases"
       | Vaccinated1st  -> I18N.t "charts.map.vaccinated1st"
       | Vaccinated2nd  -> I18N.t "charts.map.vaccinated2nd"
       | Vaccinated3rd  -> I18N.t "charts.map.vaccinated3rd"
       | Deceased       -> I18N.t "charts.map.deceased"

let (|ConfirmedCasesMsgCase|DeceasedMsgCase|) str =
    if str = I18N.t "charts.map.confirmedCases"
    then ConfirmedCasesMsgCase
    else DeceasedMsgCase

type DisplayType =
    | AbsoluteValues
    | Bubbles
    | RegionPopulationWeightedValues
    | RelativeIncrease
with
    static member Default = RegionPopulationWeightedValues
    static member All mapToDisplay contentType =
        match mapToDisplay, contentType with
        | MunicipalityMap, ConfirmedCases ->
            [ RelativeIncrease; AbsoluteValues; RegionPopulationWeightedValues; Bubbles ]
        | RegionMap, ConfirmedCases ->
            [ RelativeIncrease; AbsoluteValues; RegionPopulationWeightedValues ]
        | _, Vaccinated1st | _, Vaccinated2nd | _, Vaccinated3rd ->
            [ AbsoluteValues; RegionPopulationWeightedValues ]
        | _, Deceased ->
            [ AbsoluteValues; RegionPopulationWeightedValues ]

    member this.GetName =
       match this with
       | AbsoluteValues                 -> I18N.t "charts.map.absolute"
       | RegionPopulationWeightedValues -> I18N.t "charts.map.populationShare"
       | RelativeIncrease               -> I18N.t "charts.map.relativeIncrease"
       | Bubbles                        -> I18N.t "charts.map.bubbles"

type DataTimeInterval =
    | Complete
    | LastDays of int
    with
    static member All =
        [ LastDays 1
          LastDays 7
          LastDays 14
          LastDays 21
          Complete ]

    member this.GetName =
        match this with
        | Complete -> I18N.t "charts.map.all"
        | LastDays days -> I18N.tOptions "charts.map.last_x_days" {| count = days |}

type GeoJson = RemoteData<obj, string>

type State =
    { MapToDisplay : MapToDisplay
      GeoJson : GeoJson
      Data : Area seq
      DataTimeInterval : DataTimeInterval
      ContentType : ContentType
      DisplayType : DisplayType }

// we do not have all historical data for vaccinations by municipaality - do not show date intervals
let mapWithoutHistoricalData state =
    state.MapToDisplay = MunicipalityMap && (state.ContentType = Vaccinated1st || state.ContentType = Vaccinated2nd || state.ContentType = Vaccinated3rd)

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | DataTimeIntervalChanged of DataTimeInterval
    | ContentTypeChanged of ContentType
    | DisplayTypeChanged of DisplayType

type Query (query : obj) =
    member this.Query = query
    member this.Date =
        match query?("date") with
        | Some (dateStr : string) -> Some (DateTime.Parse dateStr)
        | _ -> None

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

let processData (queryObj : obj) (municipalitiesData : MunicipalitiesData) : Area seq =
    let query = Query(queryObj)

    let municipalityDataMap =
        seq {
            for municipalitiesDataPoint in municipalitiesData do
                for region in municipalitiesDataPoint.Regions do
                    for municipality in region.Municipalities do
                        if not (Set.contains municipality.Name excludedMunicipalities) then
                            yield {| Date = municipalitiesDataPoint.Date
                                     Name = municipality.Name
                                     TotalConfirmedCases = municipality.ConfirmedToDate
                                     TotalDeceasedCases = municipality.DeceasedToDate
                                     TotalVaccinated1st = municipality.Vaccinated1stToDate
                                     TotalVaccinated2nd = municipality.Vaccinated2ndToDate
                                     TotalVaccinated3rd = municipality.Vaccinated3rdToDate |} }
        |> Seq.groupBy (fun dp -> dp.Name)
        |> Seq.map (fun (name, dp) ->
            let totalCases =
                dp
                |> Seq.filter (fun dp ->
                    match query.Date with
                    | Some date -> dp.Date <= date
                    | None -> true)
                |> Seq.map (fun dp ->
                    { Date = dp.Date
                      TotalConfirmedCases = dp.TotalConfirmedCases
                      TotalDeceasedCases = dp.TotalDeceasedCases
                      TotalVaccinated1st = dp.TotalVaccinated1st
                      TotalVaccinated2nd = dp.TotalVaccinated2nd
                      TotalVaccinated3rd = dp.TotalVaccinated3rd } )
                |> Seq.sortBy (fun dp -> dp.Date)
            ( name, totalCases ) )
        |> Map.ofSeq

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

let processRegionsData (queryObj : obj) (regionsData : RegionsData) : Area seq =
    let query = Query(queryObj)

    let regDataMap =
        seq {
            for regionsDataPoint in regionsData do
                    for region in regionsDataPoint.Regions do
                        if not (Set.contains region.Name Utils.Dictionaries.excludedRegions) then
                            yield {| Date = regionsDataPoint.Date
                                     Name = region.Name
                                     TotalConfirmedCases = region.ConfirmedToDate
                                     TotalDeceasedCases = region.DeceasedToDate
                                     TotalVaccinated1st = region.Vaccinated1stToDate
                                     TotalVaccinated2nd = region.Vaccinated2ndToDate
                                     TotalVaccinated3rd = region.Vaccinated3rdToDate |} }
        |> Seq.groupBy (fun dp -> dp.Name)
        |> Seq.map (fun (name, dp) ->
            let totalCases =
                dp
                |> Seq.filter (fun dp ->
                    match query.Date with
                    | Some date -> dp.Date <= date
                    | None -> true)
                |> Seq.map (fun dp ->
                    { Date = dp.Date
                      TotalConfirmedCases = dp.TotalConfirmedCases
                      TotalDeceasedCases = dp.TotalDeceasedCases
                      TotalVaccinated1st = dp.TotalVaccinated1st
                      TotalVaccinated2nd = dp.TotalVaccinated2nd
                      TotalVaccinated3rd = dp.TotalVaccinated3rd } )
                |> Seq.sortBy (fun dp -> dp.Date)
            ( name, totalCases ) )
        |> Map.ofSeq

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


let init (mapToDisplay : MapToDisplay) (data : Area seq) : State * Cmd<Msg> =
    let dataTimeInterval = LastDays 7

    { MapToDisplay = mapToDisplay
      GeoJson = NotAsked
      Data = data
      DataTimeInterval = dataTimeInterval
      ContentType = ContentType.Default mapToDisplay
      DisplayType = DisplayType.Default
    }, Cmd.ofMsg GeoJsonRequested

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | GeoJsonRequested ->
        let cmd =
            match state.MapToDisplay with
            | MunicipalityMap -> Cmd.OfAsync.result loadMunGeoJson
            | RegionMap -> Cmd.OfAsync.result loadRegGeoJson
        { state with GeoJson = Loading }, cmd
    | GeoJsonLoaded geoJson ->
        { state with GeoJson = geoJson }, Cmd.none
    | DataTimeIntervalChanged dataTimeInterval ->
        { state with DataTimeInterval = dataTimeInterval }, Cmd.none
    | ContentTypeChanged contentType ->
        let supportedDisplayTypes = DisplayType.All state.MapToDisplay contentType
        let newDisplayType =
            match (supportedDisplayTypes |> List.tryFind (fun dt -> dt = state.DisplayType)) with
            | Some dt -> dt
            | _ -> DisplayType.Default
        { state with ContentType = contentType; DisplayType = newDisplayType }, Cmd.none
    | DisplayTypeChanged displayType ->
        { state with DisplayType = displayType }, Cmd.none

let seriesData (state : State) =

    seq {
        for areaData in state.Data do
            let dlabel, value, absolute, value100k, totalConfirmed,
                    weeklyIncrease, population, newCases =

                match areaData.Cases with
                | None -> None, 0.0001, 0, 0., 0, 0., areaData.Population, null
                | Some totalCases ->
                    let confirmedCasesValue = totalCases |> Seq.map (fun dp -> dp.TotalConfirmedCases) |> Seq.choose id |> Seq.toArray
                    let vaccinated1stValue = totalCases |> Seq.map (fun dp -> dp.TotalVaccinated1st) |> Seq.choose id |> Seq.toArray
                    let vaccinated2ndValue = totalCases |> Seq.map (fun dp -> dp.TotalVaccinated2nd) |> Seq.choose id |> Seq.toArray
                    let vaccinated3rdValue = totalCases |> Seq.map (fun dp -> dp.TotalVaccinated3rd) |> Seq.choose id |> Seq.toArray
                    let chartValue =
                        match state.ContentType with
                        | Vaccinated1st -> vaccinated1stValue
                        | Vaccinated2nd -> vaccinated2ndValue
                        | Vaccinated3rd -> vaccinated3rdValue
                        | _ -> confirmedCasesValue
                    let newCases =
                        chartValue
                        |> Array.mapi (fun i cc -> if i > 0 then cc - chartValue.[i-1] else cc)
                        |> Array.skip (chartValue.Length - 56) // we only show last 56 days
                        |> Seq.toArray
                    let deceasedValue = totalCases |> Seq.map (fun dp -> dp.TotalDeceasedCases) |> Seq.choose id |> Seq.toArray
                    let values =
                        match state.ContentType with
                        | ConfirmedCases -> confirmedCasesValue
                        | Vaccinated1st -> vaccinated1stValue
                        | Vaccinated2nd -> vaccinated2ndValue
                        | Vaccinated3rd -> vaccinated3rdValue
                        | Deceased -> deceasedValue

                    let totalConfirmed = confirmedCasesValue |> Array.tryLast

                    let lastValueTotal = values |> Array.tryLast
                    let lastValueRelative =
                        let dateInterval =
                            if state.DisplayType = RelativeIncrease
                            then LastDays 7     // for weekly relative increase we force 7 day interval for display in tooltip
                            else if mapWithoutHistoricalData state
                            then Complete
                            else state.DataTimeInterval
                        match dateInterval with
                        | Complete -> lastValueTotal
                        | LastDays days ->
                            let firstValueTotal = values |> Array.tryItem (values.Length - days - 1)
                            match firstValueTotal, lastValueTotal with
                            | None, None -> None
                            | None, Some b -> Some b
                            | Some a, None -> Some a
                            | Some a, Some b -> Some (b - a)

                    match lastValueRelative with
                    | None -> None, 0.0001, 0, 0., 0, 0., areaData.Population, null
                    | Some lastValue ->
                        let absolute = lastValue
                        let value100k =
                            float absolute * 100000. / float areaData.Population
                        let dlabel, value =
                            match state.DisplayType with
                            | AbsoluteValues ->
                                ((Some absolute) |> Utils.zeroToNone), absolute
                            | Bubbles ->
                                None, absolute
                            | RegionPopulationWeightedValues ->
                                // factor 10 for better resolution in graph
                                None,  10. * value100k |> Math.Round |> int
                            | RelativeIncrease ->
                                None, absolute
                        let weeklyIncrease =
                            let parseNumber x =
                                match x with
                                | None -> 0.
                                | Some x -> x |> float
                            let casesNow = values |> Array.tryItem(values.Length - 1) |> parseNumber
                            let cases7dAgo = values |> Array.tryItem(values.Length - 8) |> parseNumber
                            let cases14dAgo = values |> Array.tryItem(values.Length - 15) |> parseNumber

                            let increaseThisWeek = casesNow - cases7dAgo
                            let increaseLastWeek = cases7dAgo - cases14dAgo

                            if (increaseThisWeek, increaseLastWeek) = (0.,0.) then 0.
                            else 100. * min ( increaseThisWeek/increaseLastWeek - 1.) 5. // Set the maximum value to 5 to cut off infinities
                        let scaled =
                            match state.ContentType with
                            | ConfirmedCases ->
                                match state.DisplayType with
                                | AbsoluteValues ->
                                    if absolute > 0 then float absolute
                                    else 0.0001
                                | Bubbles ->
                                    if absolute > 0 then float absolute
                                    else 0.0001
                                | RegionPopulationWeightedValues ->
                                    if value100k > 0.0 then value100k
                                    else 0.0001
                                | RelativeIncrease ->
                                    min weeklyIncrease 200. // for colorAxis limit to 200%
                            | Vaccinated1st | Vaccinated2nd | Vaccinated3rd ->
                                match state.DisplayType with
                                | AbsoluteValues ->
                                    if absolute > 0 then float absolute
                                    else 0.0001
                                | _ ->
                                    float value / 10000.
                            | Deceased ->
                                match value with
                                | 0 -> 0.
                                | x -> float x + Math.E |> Math.Log

                        dlabel, scaled, absolute, value100k,
                        totalConfirmed.Value, weeklyIncrease,
                        areaData.Population, newCases
            {|
                code = areaData.Code
                area = areaData.Name
                value = value
                absolute = absolute
                value100k = value100k
                totalConfirmed = totalConfirmed
                weeklyIncrease = weeklyIncrease
                population = population
                dlabel = dlabel
                dataLabels = pojo {| enabled = true; format = "{point.dlabel}" |}
                newCases = newCases
                z = value
                name = dlabel
            |}
    } |> Seq.toArray



let sparklineFormatter newCases color state =
    let desaturateColor (rgb:string) (sat:float) =
        let argb = Int32.Parse (rgb.Replace("#", ""), Globalization.NumberStyles.HexNumber)
        let r = (argb &&& 0x00FF0000) >>> 16
        let g = (argb &&& 0x0000FF00) >>> 8
        let b = (argb &&& 0x000000FF)
        let avg = (float(r + g + b) / 3.0) * 1.6
        let newR = int (Math.Round (float(r) * sat + avg * (1.0 - sat)))
        let newG = int (Math.Round (float(g) * sat + avg * (1.0 - sat)))
        let newB = int (Math.Round (float(b) * sat + avg * (1.0 - sat)))
        sprintf "#%02x%02x%02x" newR newG newB

    let color1 = color
    let color2 = desaturateColor color1 0.6
    let color3 = desaturateColor color1 0.3

    let temp = [|([| color3 |] |> Array.replicate 42 |> Array.concat )
                 ([|color2 |] |> Array.replicate 7 |> Array.concat)|]
               |> Array.concat
    let columnColors =
        [| temp; ([| color1 |] |> Array.replicate 7 |> Array.concat)  |]
        |> Array.concat

    let options =
        {|
            chart =
                {|
                    ``type`` = "column"
                    backgroundColor = "transparent"
                |} |> pojo
            credits = {| enabled = false |}
            xAxis =
                {|
                    visible = true
                    labels = {| enabled = false |} |> pojo
                    title = {| enabled = false |} |> pojo
                    tickInterval = 7
                    lineColor = "#696969"
                    tickColor = "#696969"
                    tickLength = 4
                |}
            yAxis =
                {|
                    title = {| enabled = false |}
                    visible = true
                    opposite = true
                    min = 0.
                    max = newCases |> Array.max
                    tickInterval = 5
                    endOnTick = true
                    startOnTick = false
                    allowDecimals = false
                    showFirstLabel = true
                    showLastLabel = true
                    gridLineColor = "#000000"
                    gridLineDashStyle = "dot"
                |}
            title = {| text = "" |}
            legend = {| enabled = false |}
            series =
                [|
                    {|
                        data = newCases |> Array.map ( max 0.)
                        animation = false
                        colors = columnColors
                        borderColor = columnColors
                        pointWidth = 2
                        colorByPoint = true
                    |} |> pojo
                |]
        |} |> pojo
    match state.MapToDisplay with
    | MunicipalityMap ->
        Fable.Core.JS.setTimeout
            (fun () -> sparklineChart("tooltip-chart-mun", options)) 10
        |> ignore
        """<div id="tooltip-chart-mun" class="tooltip-chart"></div>"""
    | RegionMap ->
        Fable.Core.JS.setTimeout
            (fun () -> sparklineChart("tooltip-chart-reg", options)) 10
        |> ignore
        """<div id="tooltip-chart-reg" class="tooltip-chart"></div>"""



let tooltipFormatter state jsThis =
    let points = jsThis?point
    let area = points?area
    let absolute = points?absolute
    let value100k = points?value100k
    let totalConfirmed = points?totalConfirmed
    let weeklyIncrease = points?weeklyIncrease
    let newCases= points?newCases
    let population = points?population
    let pctPopulation = float absolute * 100.0 / float population
    let fmtStr = sprintf "%s: <b>%s</b>" (I18N.t "charts.map.populationC") (Utils.formatToInt population)

    let label =
        match state.ContentType with
        | ConfirmedCases ->
            let label = fmtStr + sprintf "<br>%s: <b>%s</b>" (I18N.t "charts.map.confirmedCases") (Utils.formatToInt absolute)
            if totalConfirmed > 0 then
                label
                    + sprintf " (%s %% %s)" (Utils.formatTo1DecimalWithTrailingZero(pctPopulation)) (I18N.t "charts.map.population")
                    + sprintf "<br>%s: <b>%s</b> %s" (I18N.t "charts.map.confirmedCases") (Utils.formatTo1DecimalWithTrailingZero(value100k:float)) (I18N.t "charts.map.per100k")
                    + sprintf "<br>%s: <b>%s%s %%</b>" (I18N.t "charts.map.relativeIncrease") (if weeklyIncrease < 500. then "" else ">") (weeklyIncrease |> Utils.formatTo1DecimalWithTrailingZero)
                    + if (Array.max newCases) > 0.
                      then sparklineFormatter newCases "#bda506" state
                      else ""
            else
                label
        | Vaccinated1st | Vaccinated2nd | Vaccinated3rd ->
            let label = fmtStr + sprintf "<br>%s: <b>%s</b>" ((ContentType.GetName state.ContentType)) (Utils.formatToInt absolute)
            let chart =
                if not (mapWithoutHistoricalData state) && (Array.max newCases) > 0.
                then sparklineFormatter newCases "#189a73" state
                else ""
            if absolute > 0. then
                label + sprintf " (%s %% %s)"
                        (Utils.formatTo1DecimalWithTrailingZero pctPopulation)
                        (I18N.t "charts.map.population")
                    + sprintf "<br>%s: <b>%s</b> (%s %% %s)"
                        (I18N.t "charts.map.confirmedCases")
                        (I18N.NumberFormat.formatNumber totalConfirmed) (Utils.formatTo1DecimalWithTrailingZero(float totalConfirmed * 100.0 / float population))
                        (I18N.t "charts.map.population")
                    + chart
            else
                label + chart
        | Deceased ->
            let label = fmtStr + sprintf "<br>%s: <b>%s</b>" (I18N.t "charts.map.deceased") (Utils.formatToInt absolute)
            if absolute > 0. && state.DataTimeInterval = Complete then // deceased
                label + sprintf " (%s %% %s)"
                        (I18N.NumberFormat.formatNumber pctPopulation)
                        (I18N.t "charts.map.population")
                    + sprintf "<br>%s: <b>%s</b> (%s %% %s)"
                        (I18N.t "charts.map.confirmedCases")
                        (I18N.NumberFormat.formatNumber totalConfirmed) (Utils.formatTo1DecimalWithTrailingZero(float totalConfirmed * 100.0 / float population))
                        (I18N.t "charts.map.population")
                    + sprintf "<br>%s: <b>%s %%</b>"
                        (I18N.t "charts.map.mortalityOfConfirmedCases")
                        (I18N.NumberFormat.formatNumber(float absolute * 100.0 / float totalConfirmed))
            else
                label
    sprintf "<b>%s</b><br/>%s<br/>" area label



let renderMap (state : State) =
    match state.GeoJson with
    | NotAsked
    | Loading -> Html.none
    | Failure str -> Html.text str
    | Success geoJson ->
        let data = seriesData state

        // needed to calculate the color scale for bubbles adjusted to
        // actual numbers
        let minValue100k() =
            (data
             // for bubbles, do not include municipalities without
             // any covid cases
             |> Array.filter (fun x -> x.value100k > 0.)
             |> Array.minBy (fun x -> x.value100k)).value100k
        let maxValue100k() =
            (data |> Array.maxBy (fun x -> x.value100k)).value100k


        let key =
            match state.MapToDisplay with
            | MunicipalityMap -> "isoid"
            | RegionMap -> "code"

        let series geoJson =
            {| visible = true
               ``type`` = null
               mapData = geoJson
               data = data
               keys = [| "code" ; "value" |]
               joinBy = [| key ; "code" |]
               colorKey = "value"
               nullColor = "transparent"
               borderColor = "#000"
               borderWidth = 0.2
               minSize = 1
               maxSize = "6%"
               mapline = pojo {| animation = pojo {| duration = 0 |} |}
               states =
                pojo {| normal = pojo {| animation = pojo {| duration = 0 |} |}
                        hover = pojo {| brightness = 0 ; borderColor = "black" ; animation = pojo {| duration = 0 |} |} |}
               colorAxis =
                   match state.DisplayType with
                   // white-ish background color for municipalities when we use
                   // bubbles
                   | Bubbles -> 1
                   | _ -> 0
               enableMouseTracking = true
               stickyTracking = (state.DisplayType = Bubbles)
           |}

        let bubbleSeries geoJson =
            {| visible = true
               ``type`` = "mapbubble"
               mapData = geoJson
               data = data
               keys = [| "code" ; "value" |]
               joinBy = [| key ; "code" |]
               colorKey = "value100k"
               nullColor = "white"
               borderColor = "#000"
               borderWidth = 0.2
               minSize = 0 // minimal size of a bubble - 0 means it won't be shown for 0
               maxSize = "10%"
               mapline = pojo {| animation = pojo {| duration = 0 |} |}
               states =
                pojo {| normal = pojo {| animation = pojo {| duration = 0 |} |}
                        hover = pojo {| brightness = 0 ; borderColor = "black" ; animation = pojo {| duration = 0 |} |} |}
               colorAxis = 0
               enableMouseTracking = true
               stickyTracking = (state.DisplayType = Bubbles)
           |}

        let legend =
            let enabled = state.ContentType <> Deceased
            {| enabled = enabled
               title = {| text = null |}
               align = "right"
               verticalAlign = "bottom"
               layout = "vertical"
               floating = true
               borderWidth = 1
               backgroundColor = "white"
               width = 70
            |}
            |> pojo

        let colorMax =
            match state.ContentType, state.DisplayType with
            | ConfirmedCases, Bubbles -> maxValue100k()
            | ConfirmedCases, _ ->
                match state.DataTimeInterval with
                | Complete -> 40000.
                | LastDays days ->
                    match days with
                        | 21 -> 15000.
                        | 14 -> 10000.
                        | 7 -> 5000.
                        | 1 -> 1500.
                        | _ -> 200.
            | Vaccinated1st, _ | Vaccinated2nd, _ | Vaccinated3rd, _ ->
                let dataMax = data |> Seq.map(fun dp -> dp.value) |> Seq.max
                if dataMax < 1. then 1. else dataMax
            | Deceased, _ ->
                let dataMax = data |> Seq.map(fun dp -> dp.value) |> Seq.max
                if dataMax < 1. then 1. else dataMax

        let colorMin =
            match state.ContentType, state.DisplayType with
                | _, RegionPopulationWeightedValues -> colorMax / 7000.
                | _, AbsoluteValues -> 0.9
                | _, Bubbles -> minValue100k()
                | _, RelativeIncrease -> -100.

        let whiteMuniColorAxis =
            {|
                ``type`` = "linear"
                visible = false
                stops = [| (0.000, "#ffffff") |]
            |} |> pojo

        let relativeColorAxis =
            {|
                ``type`` = "logarithmic"
                tickInterval = 0.4
                max = colorMax
                min = colorMin
                endOnTick = false
                startOnTick = false
                stops =
                    [|
                        (0.000,"#ffffff")
                        (0.001,"#fff7db")
                        (0.200,"#ffefb7")
                        (0.280,"#ffe792")
                        (0.360,"#ffdf6c")
                        (0.440,"#ffb74d")
                        (0.520,"#ff8d3c")
                        (0.600,"#f85d3a")
                        (0.680,"#ea1641")
                        (0.760,"#d0004e")
                        (0.840,"#ad005b")
                        (0.920,"#800066")
                        (0.999,"#43006e")
                    |]
                reversed = true
                labels =
                    {|
                        formatter = fun() -> Utils.formatToInt jsThis?value
                    |} |> pojo
            |} |> pojo


        let colorAxis =
            match state.ContentType with
                | Vaccinated1st | Vaccinated2nd | Vaccinated3rd ->
                    {|
                        ``type`` = "linear"
                        tickInterval = 0.4
                        max = colorMax
                        min = colorMin
                        endOnTick = false
                        startOnTick = false
                        stops =
                            [|
                                (0.000, "#ffffff")
                                (0.111, "#e5f5f9")
                                (0.222, "#ccece6")
                                (0.333, "#99d8c9")
                                (0.444, "#66c2a4")
                                (0.556, "#41ae76")
                                (0.667, "#238b45")
                                (0.778, "#006d2c")
                                (0.889, "#00441b")
                            |]
                        labels =
                            {|
                                formatter = fun() ->
                                    if state.DisplayType = RegionPopulationWeightedValues
                                    then sprintf "%s %%" (Utils.formatToInt jsThis?value)
                                    else Utils.formatToInt (Utils.roundToInt jsThis?value)
                            |} |> pojo
                    |} |> pojo
                | Deceased ->
                    {|
                        ``type`` = "linear"
                        tickInterval = 0.4
                        max = colorMax
                        min = colorMin
                        endOnTick = false
                        startOnTick = false
                        stops =
                            [|
                                (0.000, "#ffffff")
                                (0.111, "#efedf5")
                                (0.222, "#dadaeb")
                                (0.333, "#bcbddc")
                                (0.444, "#9e9ac8")
                                (0.556, "#807dba")
                                (0.667, "#6a51a3")
                                (0.778, "#54278f")
                                (0.889, "#3f007d")
                            |]
                        labels =
                            {|
                                formatter = fun() -> Utils.formatToInt jsThis?value
                            |} |> pojo
                    |} |> pojo
                | ConfirmedCases ->
                    match state.DisplayType with
                    | AbsoluteValues ->
                        {|
                            ``type`` = "logarithmic"
                            tickInterval = 0.4
                            max = colorMax
                            min = colorMin
                            endOnTick = false
                            startOnTick = false
                            stops =
                                [|
                                    (0.000,"#ffffff")
                                    (0.001,"#fff7db")
                                    (0.200,"#ffefb7")
                                    (0.280,"#ffe792")
                                    (0.360,"#ffdf6c")
                                    (0.440,"#ffb74d")
                                    (0.520,"#ff8d3c")
                                    (0.600,"#f85d3a")
                                    (0.680,"#ea1641")
                                    (0.760,"#d0004e")
                                    (0.840,"#ad005b")
                                    (0.920,"#800066")
                                    (0.999,"#43006e")
                                |]
                            reversed = true
                            labels =
                                {|
                                    formatter = fun() -> Utils.formatToInt jsThis?value
                                |} |> pojo
                        |} |> pojo

                    | Bubbles -> relativeColorAxis
                    | RegionPopulationWeightedValues -> relativeColorAxis
                    | RelativeIncrease ->
                        {|
                            ``type`` = "linear"
                            tickInterval = 50
                            max = 200
                            min = -100
                            endOnTick = false
                            startOnTick = false
                            stops =
                                [|
                                    (0.000,"#009e94")
                                    (0.166,"#6eb49d")
                                    (0.250,"#b2c9a7")
                                    (0.333,"#f0deb0")
                                    (0.500,"#e3b656")
                                    (0.600,"#cc8f00")
                                    (0.999,"#b06a00")
                                |]
                            reversed=false
                            labels =
                            {|
                               formatter = fun() -> sprintf "%s %%" jsThis?value
                            |} |> pojo
                        |} |> pojo

        let lastDate =
            state.Data
            |> Seq.map (fun a ->
                match a.Cases with
                | Some c -> c |> Seq.tryLast
                |_ -> None )
            |> Seq.pick (fun c ->
                match c with
                | Some c -> Some c.Date
                | _ -> None)

        let dateText = (I18N.tOptions "charts.common.dataDate" {| date = lastDate  |})

        {| optionsWithOnLoadEvent "covid19-map" with
            chart = {| animation = false |} |> pojo
            title = null
            subtitle = {| text = dateText ; align="left"; verticalAlign="bottom" |}
            series =
                match state.DisplayType with
                | Bubbles -> [| series geoJson; bubbleSeries geoJson |]
                | _ -> [| series geoJson |]
            legend = legend
            colorAxis = [|
                colorAxis; whiteMuniColorAxis
            |]
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
                        sprintf "%s: %s"
                            (I18N.t "charts.common.dataSource")
                            (I18N.t "charts.common.dsNIJZ")
                    mapTextFull = ""
                    mapText = ""
                    href = urlNijzCovid
                    position = {| align = "right" ; verticalAlign = "bottom" ; x = -10 ; y = -5 |}
                    style = {| color = "#999999" ; cursor = "pointer" ; fontSize = "9px" |}
                |}
        |}
        |> map

let inline renderSelector (option: ^T when ^T : (member GetName: string)) (currentOption: ^T) dispatch =
    let defaultProps =
        [ prop.text ( (^T: (member GetName: string) option))
          Utils.classes
              [(true, "chart-display-property-selector__item")
               (option = currentOption, "selected") ] ]
    if option = currentOption
    then Html.div defaultProps
    else Html.div ((prop.onClick (fun _ -> dispatch option)) :: defaultProps)

let inline renderSelectors options currentOption dispatch =
    options
    |> List.map (fun option ->
        renderSelector option currentOption dispatch)

let renderDisplayTypeSelector state dispatch =
    let selectors = DisplayType.All state.MapToDisplay state.ContentType
    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (renderSelectors selectors state.DisplayType dispatch)
    ]

let renderDataTimeIntervalSelector state dispatch =
    if mapWithoutHistoricalData state || state.DisplayType = RelativeIncrease
    then
        Html.none
    else
        Html.div [
            prop.className "chart-data-interval-selector"
            prop.children ( Html.text "" :: renderSelectors DataTimeInterval.All state.DataTimeInterval dispatch )
        ]

let renderContentTypeSelector state dispatch =
    let contentTypes =
        if state.MapToDisplay = RegionMap then
            [("ConfirmedCases", ContentType.ConfirmedCases)
             // ("Vaccinated2nd", ContentType.Vaccinated2nd)
             // ("Vaccinated1st", ContentType.Vaccinated1st)
             // ("Vaccinated3rd", ContentType.Vaccinated3rd)
             ("Deceased", ContentType.Deceased)]
         else
            [("ConfirmedCases", ContentType.ConfirmedCases)
             // ("Vaccinated2nd", ContentType.Vaccinated2nd)
             // ("Vaccinated1st", ContentType.Vaccinated1st)
            ] // no 3rd dose for municipality yet

    let renderedTypes = contentTypes |>  Seq.map (fun (id, ct) ->
        Html.option [
            prop.text (ContentType.GetName ct)
            prop.value (id)
        ])

    Html.select [
        prop.value (state.ContentType.ToString())
        prop.className "form-control form-control-sm filters__type"
        prop.children renderedTypes
        prop.onChange ( fun ct ->  Map.find ct (contentTypes |> Map.ofList) |> ContentTypeChanged |> dispatch)
    ]

let render (state : State) dispatch =
    Html.div [
        prop.children [
            Utils.renderChartTopControls [
                Html.div [
                    prop.className "filters"
                    prop.children [
                        renderContentTypeSelector state dispatch
                        renderDataTimeIntervalSelector state (DataTimeIntervalChanged >> dispatch)
                    ]
                ]
                renderDisplayTypeSelector state (DisplayTypeChanged >> dispatch)
            ]
            Html.div [
                prop.className "map"
                prop.children [ renderMap state ]
            ]
            match state.ContentType with
            | Deceased ->
                let disclaimerID =
                    if state.MapToDisplay = RegionMap
                    then "charts.map.disclaimerRegion"
                    else "charts.map.disclaimer"
                Html.div [
                    prop.className "disclaimer"
                    prop.children [
                        Html.text (I18N.t disclaimerID)
                    ]
                ]
            | _ -> Html.none
        ]
    ]

let mapMunicipalitiesChart (props : {| query : obj ; data : MunicipalitiesData |}) =
    let data = processData props.query props.data
    React.elmishComponent
        ("MapChart", init MunicipalityMap data, update, render)

let mapRegionChart (props : {| query : obj ; data : RegionsData |}) =
    let data = processRegionsData props.query props.data
    React.elmishComponent
        ("MapChart", init RegionMap data, update, render)
