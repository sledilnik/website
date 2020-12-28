[<RequireQualifiedAccess>]
module EuropeMap

open System
open Feliz
open Elmish
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Fable.SimpleHttp
open Browser

open Highcharts
open Types
open Data.OurWorldInData

let chartText = I18N.chartText "europe"

type MapToDisplay = Europe | World

let europeGeoJsonUrl = "/maps/europe.geo.json"
let worldGeoJsonUrl = "/maps/world-robinson.geo.json"

type GeoJson = RemoteData<obj, string>

type OwdData = OurWorldInDataRemoteData

type CountryData =
    { Country: string
      // OWD data
      TwoWeekIncidence100k: float
      TwoWeekIncidence: float []
      TwoWeekIncidenceMaxValue: float
      NewCases: int list
      OwdDate: DateTime
      // NIJZ data
      RestrictionColor: string
      RestrictionText: string
      RestrictionAltText: string
      ImportedFrom: int
      ImportedDate: DateTime }

type CountriesMap = Map<string, CountryData>

type ChartType =
    | TwoWeekIncidence
    | Restrictions
    | WeeklyIncrease

    static member All = [ TwoWeekIncidence; WeeklyIncrease; Restrictions ]
    static member Default = TwoWeekIncidence
    member this.GetName =
        match this with
        | TwoWeekIncidence -> chartText "twoWeekIncidence"
        | Restrictions -> chartText "restrictions"
        | WeeklyIncrease -> chartText "weeklyIncrease"

type State =
    { MapToDisplay : MapToDisplay
      Data : WeeklyStatsData
      Countries : CountrySelection
      GeoJson: GeoJson
      OwdData: OwdData
      CountryData: CountriesMap
      ChartType: ChartType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | OwdDataRequested
    | OwdDataReceived of OwdData
    | ChartTypeChanged of ChartType

let worldCountries =
    [ "AFG" ; "ALB" ; "DZA" ; "ASM" ; "AND" ; "AGO" ; "AIA" ; "ATA" ; "ATG" ; "ARG" ; "ARM" ; "ABW" ; "AUS" ; "AUT" ; "AZE" ; "BHS" ; "BHR" ; "BGD" ; "BRB" ; "BLR" ; "BEL" ; "BLZ" ; "BEN" ; "BMU" ; "BTN" ; "BOL" ; "BES" ; "BIH" ; "BWA" ; "BVT" ; "BRA" ; "IOT" ; "BRN" ; "BGR" ; "BFA" ; "BDI" ; "CPV" ; "KHM" ; "CMR" ; "CAN" ; "CYM" ; "CAF" ; "TCD" ; "CHL" ; "CHN" ; "CXR" ; "CCK" ; "COL" ; "COM" ; "COD" ; "COG" ; "COK" ; "CRI" ; "HRV" ; "CUB" ; "CUW" ; "CYP" ; "CZE" ; "CIV" ; "DNK" ; "DJI" ; "DMA" ; "DOM" ; "ECU" ; "EGY" ; "SLV" ; "GNQ" ; "ERI" ; "EST" ; "SWZ" ; "ETH" ; "FLK" ; "FRO" ; "FJI" ; "FIN" ; "FRA" ; "GUF" ; "PYF" ; "ATF" ; "GAB" ; "GMB" ; "GEO" ; "DEU" ; "GHA" ; "GIB" ; "GRC" ; "GRL" ; "GRD" ; "GLP" ; "GUM" ; "GTM" ; "GGY" ; "GIN" ; "GNB" ; "GUY" ; "HTI" ; "HMD" ; "VAT" ; "HND" ; "HKG" ; "HUN" ; "ISL" ; "IND" ; "IDN" ; "IRN" ; "IRQ" ; "IRL" ; "IMN" ; "ISR" ; "ITA" ; "JAM" ; "JPN" ; "JEY" ; "JOR" ; "KAZ" ; "KEN" ; "KIR" ; "PRK" ; "KOR" ; "KWT" ; "KGZ" ; "LAO" ; "LVA" ; "LBN" ; "LSO" ; "LBR" ; "LBY" ; "LIE" ; "LTU" ; "LUX" ; "MAC" ; "MDG" ; "MWI" ; "MYS" ; "MDV" ; "MLI" ; "MLT" ; "MHL" ; "MTQ" ; "MRT" ; "MUS" ; "MYT" ; "MEX" ; "FSM" ; "MDA" ; "MCO" ; "MNG" ; "MNE" ; "MSR" ; "MAR" ; "MOZ" ; "MMR" ; "NAM" ; "NRU" ; "NPL" ; "NLD" ; "NCL" ; "NZL" ; "NIC" ; "NER" ; "NGA" ; "NIU" ; "NFK" ; "MNP" ; "NOR" ; "OMN" ; "PAK" ; "PLW" ; "PSE" ; "PAN" ; "PNG" ; "PRY" ; "PER" ; "PHL" ; "PCN" ; "POL" ; "PRT" ; "PRI" ; "QAT" ; "MKD" ; "ROU" ; "RUS" ; "RWA" ; "REU" ; "BLM" ; "SHN" ; "KNA" ; "LCA" ; "MAF" ; "SPM" ; "VCT" ; "WSM" ; "SMR" ; "STP" ; "SAU" ; "SEN" ; "SRB" ; "SYC" ; "SLE" ; "SGP" ; "SXM" ; "SVK" ; "SVN" ; "SLB" ; "SOM" ; "ZAF" ; "SGS" ; "SSD" ; "ESP" ; "LKA" ; "SDN" ; "SUR" ; "SJM" ; "SWE" ; "CHE" ; "SYR" ; "TWN" ; "TJK" ; "TZA" ; "THA" ; "TLS" ; "TGO" ; "TKL" ; "TON" ; "TTO" ; "TUN" ; "TUR" ; "TKM" ; "TCA" ; "TUV" ; "UGA" ; "UKR" ; "ARE" ; "GBR" ; "UMI" ; "USA" ; "URY" ; "UZB" ; "VUT" ; "VEN" ; "VNM" ; "VGB" ; "VIR" ; "WLF" ; "ESH" ; "YEM" ; "ZMB" ; "ZWE" ; "ALA" ; "XKX"]

let euCountries =
    [ "ALB"
      "AND"
      "AUT"
      "BLR"
      "BEL"
      "BIH"
      "BGR"
      "HRV"
      "CYP"
      "CZE"
      "DNK"
      "EST"
      "FRO"
      "FIN"
      "FRA"
      "DEU"
      "GRC"
      "HUN"
      "ISL"
      "IRL"
      "ITA"
      "LVA"
      "LIE"
      "LTU"
      "LUX"
      "MKD"
      "MLT"
      "MDA"
      "MCO"
      "MNE"
      "NLD"
      "NOR"
      "POL"
      "PRT"
      "SRB"
      "ROU"
      "RUS"
      "SMR"
      "SVK"
      "SVN"
      "ESP"
      "SWE"
      "CHE"
      "TUR"
      "UKR"
      "GBR"
      "VAT"
      "XKX"
      "NCY"
      "NMA" ]

let greenCountries =
    Map.empty

let redCountries =
    Map.ofList
        [
            ("AFG", "")
            ("ALB", "")
            ("DZA", "")
            ("AND", "")
            ("AGO", "")
            ("ARG", "")
            ("ARM", "")
            ("AUT", "")
            ("AZE", "")
            ("BAH", "")
            ("BHR", "")
            ("BGD", "")
            ("BEL", "")
            ("BLZ", "")
            ("BLR", "")
            ("BEN", "")
            ("BGR", "")
            ("BOL", "")
            ("BIH", "")
            ("BWA", "")
            ("BRA", "")
            ("BFA", "")
            ("BDI", "")
            ("BTN", "")
            ("CYP", "")
            ("TCD", "")
            ("CZE", "")
            ("CHL", "")
            ("MNE", "")
            ("DNK", "administrativna enota: vse razen pokrajina Grenlandija")
            ("DOM", "")
            ("EGY", "")
            ("ECU", "")
            ("GNQ", "")
            ("ERI", "")
            ("EST", "")
            ("SWZ", "")
            ("ETH", "")
            ("PHL", "")
            ("FIN", "administrativna enota: Uusimaa")
            ("FRA", "vse administrativne enote celinske Francije, razen Bretanje in Korzike, ter vsa čezmorska ozemlja, razen ozemelj Guadeloupe, Martinique in La Reunion")
            ("GAB", "")
            ("GMB", "")
            ("GHA", "")
            ("GRC", "vse administrativne enote, razen Južnoegejskih otokov in Jonskih otokov")
            ("GEO", "")
            ("GUY", "")
            ("GTM", "")
            ("GIN", "")
            ("GNB", "")
            ("HTI", "")
            ("HND", "")
            ("HRV", "")
            ("IND", "")
            ("IDN", "")
            ("IRQ", "")
            ("IRN", "")
            ("ITA", "")
            ("ISR", "")
            ("JAM", "")
            ("YEM", "")
            ("JOR", "")
            ("ZAF", "")
            ("SSD", "")
            ("CMR", "")
            ("CAN", "")
            ("QAT", "")
            ("KAZ", "")
            ("KEN", "")
            ("KGZ", "")
            ("COL", "")
            ("COM", "")
            ("COG", "")
            ("COD", "")
            ("XKX", "")
            ("CRI", "")
            ("KWT", "")
            ("LVA", "")
            ("LSO", "")
            ("LBN", "")
            ("LBR", "")
            ("LBY", "")
            ("LIE", "")
            ("LTU", "")
            ("LUX", "")
            ("MDG", "")
            ("HUN", "")
            ("MWL", "")
            ("MDV", "")
            ("MLI", "")
            ("MLT", "")
            ("MAR", "")
            ("MRT", "")
            ("MEX", "")
            ("MDA", "")
            ("MNG", "")
            ("MOZ", "")
            ("MCO", "")
            ("DEU", "")
            ("NPL", "")
            ("NIG", "")
            ("NGA", "")
            ("NIC", "")
            ("NLD", "")
            ("NOR", "administrativni enoti: Oslo, Viken")
            ("OMN", "")
            ("PAK", "")
            ("PAN", "")
            ("PNG", "")
            ("PRY", "")
            ("PER", "")
            ("POL", "vse administrativne enote, razen Podkarpatske administrativne enote")
            ("PRT", "vse administrativne enote, razen avtonomne regije Madeira")
            ("ROU", "")
            ("RUS", "")
            ("SLV", "")
            ("SMR", "")
            ("STP", "")
            ("SAU", "")
            ("SEN", "")
            ("PRK", "")
            ("MKD", "")
            ("SLE", "")
            ("SYR", "")
            ("CIV", "")
            ("SVK", "")
            ("SOM", "")
            ("SRB", "")
            ("CAF", "")
            ("SUR", "")
            ("ESP", "administrativne enote: vse razen Kanarskih otokov")
            ("SWE", "")
            ("CHE", "")
            ("TJK", "")
            ("TZA", "")
            ("TGO", "")
            ("TTO", "")
            ("TUN", "")
            ("TUR", "")
            ("TKM", "")
            ("UKR", "")
            ("URY", "")
            ("UZB", "")
            ("VAT", "")
            ("VEN", "")
            ("TLS", "")
            ("ZMB", "")
            ("USA", "")
            ("ARE", "")
            ("GBR", "")
            ("CPV", "")
            ("ZWE", "")
        ]

let loadEuropeGeoJson =
    async {
        let! (statusCode, response) = Http.get europeGeoJsonUrl

        if statusCode <> 200 then
            return GeoJsonLoaded
                       (sprintf "Error loading map: %d" statusCode
                        |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded(data |> Success)
            with ex ->
                return GeoJsonLoaded
                           (sprintf "Error loading map: %s" ex.Message
                            |> Failure)
    }

let loadWorldGeoJson =
    async {
        let! (statusCode, response) = Http.get worldGeoJsonUrl

        if statusCode <> 200 then
            return GeoJsonLoaded
                       (sprintf "Error loading map: %d" statusCode
                        |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded(data |> Success)
            with ex ->
                return GeoJsonLoaded
                           (sprintf "Error loading map: %s" ex.Message
                            |> Failure)
    }

let init (mapToDisplay: MapToDisplay) (data: WeeklyStatsData): State * Cmd<Msg> =
    let cmdGeoJson = Cmd.ofMsg GeoJsonRequested
    let cmdOwdData = Cmd.ofMsg OwdDataRequested
    { MapToDisplay = mapToDisplay
      Data = data
      Countries =
        match mapToDisplay with
        | World -> CountrySelection.All
        | Europe -> CountrySelection.Selected euCountries
      GeoJson = NotAsked
      OwdData = NotAsked
      CountryData = Map.empty
      ChartType = TwoWeekIncidence },
    (cmdGeoJson @ cmdOwdData)

let prepareCountryData (data: DataPoint list) (weeklyData: WeeklyStatsData) =
    let dataForLastTwoWeeks = Array.sub weeklyData (weeklyData.Length - 2) 2
    let importedFrom = dataForLastTwoWeeks |> Data.WeeklyStats.countryTotals |> Map.ofArray
    let importedDate = (Array.last dataForLastTwoWeeks).DateTo

    let last n xs = List.toSeq xs |> Seq.skip (xs.Length - n) |> Seq.toList

    data
    |> List.groupBy (fun dp -> dp.CountryCode)
    |> List.map (fun (code, dps) ->
        let fixedCode =
            if code = "OWID_KOS" then "XKX" else code // hack for Kosovo code

        let country = I18N.tt "country" code // TODO: change country code in i18n for Kosovo

        let incidence100k =
            (dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> last 14 // select the last two weeks
            |> List.sum)
            / 10.

        let incidence =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> last 14
            |> List.toArray

        let incidenceMaxValue =
            if incidence.Length = 0
            then 0.
            else incidence |> Array.toList  |> last 14 |> List.max

        let newCases =
            dps |> List.map (fun dp -> dp.NewCases |> Utils.optionToInt)

        let owdDate =
            dps |> List.map (fun dp -> dp.Date) |> List.max

        let red, green =
            redCountries.TryFind(fixedCode),
            greenCountries.TryFind(fixedCode)
        let rText, rColor, rAltText =
            match fixedCode with
            | "SVN" -> chartText "statusNone", "#10829a", ""
            | _ ->
                match red with
                | Some redNote ->
                    if redNote.Length > 0
                    then chartText "statusRed", "#FF9057", redNote
                    else chartText "statusRed", "#FF5348", redNote
                | _ -> chartText "statusGreen", "#F8F8F8", ""  // all non-red are open now

        let imported =
            importedFrom.TryFind(fixedCode)
            |> Option.defaultValue 0

        let cd: CountryData =
            { CountryData.Country = country
              CountryData.TwoWeekIncidence100k = incidence100k
              CountryData.TwoWeekIncidence = incidence
              CountryData.TwoWeekIncidenceMaxValue = incidenceMaxValue
              CountryData.NewCases = newCases
              CountryData.OwdDate = owdDate
              CountryData.RestrictionColor = rColor
              CountryData.RestrictionText = rText
              CountryData.RestrictionAltText = rAltText
              CountryData.ImportedFrom = imported
              CountryData.ImportedDate = importedDate }

        (fixedCode, cd))
    |> Map.ofList

let update (msg: Msg) (state: State): State * Cmd<Msg> =

    let owdCountries =
        match state.Countries with
        | CountrySelection.All ->
            CountrySelection.All
        | CountrySelection.Selected countries ->
            countries
            |> List.map (fun code -> if code = "XKX" then "OWID_KOS" else code) // hack for Kosovo code
            |> CountrySelection.Selected

    match msg with
    | GeoJsonRequested ->
        let cmd =
            match state.MapToDisplay with
            | Europe -> Cmd.OfAsync.result loadEuropeGeoJson
            | World -> Cmd.OfAsync.result loadWorldGeoJson
        { state with GeoJson = Loading }, cmd
    | GeoJsonLoaded geoJson -> { state with GeoJson = geoJson }, Cmd.none
    | OwdDataRequested ->
        let someWeeksAgo = DateTime.Today.AddDays(-21.0) // increased to 21 days from 14
        let cmd = Cmd.OfAsync.result (loadData {
            Countries = owdCountries
            DateFrom = Some someWeeksAgo
            DateTo = None }  OwdDataReceived)
        { state with OwdData = Loading }, cmd
    | OwdDataReceived result ->
        let ret =
            match result with
            | Success owdData ->
                { state with
                      OwdData = result
                      CountryData = prepareCountryData owdData state.Data }
            | _ -> { state with OwdData = result }

        ret, Cmd.none
    | ChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none


let mapData state =
    let countries =
        match state.Countries with
        | CountrySelection.All -> worldCountries
        | CountrySelection.Selected countries -> countries

    countries
    |> List.map (fun code ->
        match state.CountryData.TryFind(code) with
        | Some cd ->
            let incidence100k = cd.TwoWeekIncidence100k |> int
            let incidence = cd.TwoWeekIncidence
            let incidenceMaxValue = cd.TwoWeekIncidenceMaxValue

            let nc =
                cd.NewCases
                |> List.tryLast
                |> Option.defaultValue 0

            let cases = cd.NewCases |> List.toArray

            let casesLastWeek = Array.sub cases (cases.Length - 7) 7 |> Array.sum
            let casesWeekBefore = Array.sub cases (cases.Length - 14) 7 |> Array.sum
            let relativeIncrease =
                if casesWeekBefore > 0
                    then 100. * (float casesLastWeek/ float casesWeekBefore - 1.) |> min 500.
                else
                    0.

            let last n xs = List.toSeq xs |> Seq.skip (xs.Length - n) |> Seq.toList
            let twoWeekCaseNumbers =
                cd.NewCases
                |> List.filter(fun x -> x > 0) // filter out date with missing data
                |> last 14 // take the last 14 non zero data points
                |> List.toArray
                |> Array.map float

            let ncDate =
                (I18N.tOptions "days.date" {| date = cd.OwdDate |})

            let impDate =
                (I18N.tOptions "days.date" {| date = cd.ImportedDate |})

            let baseRec =
                {| code = code
                   country = cd.Country
                   incidence100k = incidence100k
                   incidence = incidence
                   incidenceMaxValue = incidenceMaxValue
                   newCases = nc
                   weeklyIncrease = relativeIncrease
                   twoWeekCases = twoWeekCaseNumbers
                   ncDate = ncDate
                   rType = cd.RestrictionText
                   rAltText = cd.RestrictionAltText
                   imported = cd.ImportedFrom
                   impDate = impDate |}

            match state.ChartType with
            | TwoWeekIncidence ->
                {| baseRec with
                       value = max (float incidence100k) 0.001
                       color = null
                       dataLabels = {| enabled = false |} |}
            | Restrictions ->
                {| baseRec with
                       value = float cd.ImportedFrom
                       color = cd.RestrictionColor
                       dataLabels = {| enabled = cd.ImportedFrom > 0 |} |}
            | WeeklyIncrease ->
                {| baseRec with
                       value = relativeIncrease
                       color = null
                       dataLabels = {| enabled = false |} |}
        | _ ->
            {| code = code
               country = ""
               value = 0.1
               color = null
               dataLabels = {| enabled = false |}
               incidence100k = 0
               incidence = null
               incidenceMaxValue = 0.0
               newCases = 0
               weeklyIncrease = 0.
               twoWeekCases = [| |]
               ncDate = ""
               rType = ""
               rAltText = ""
               imported = 0
               impDate = "" |})
    |> List.toArray

let renderMap state geoJson _ =

    let legend =
        let enabled = state.ChartType <> Restrictions
        {| enabled = enabled
           title = {| text = null |}
           align = if state.MapToDisplay = World then "left" else "right"
           verticalAlign = if state.MapToDisplay = World then "bottom" else "top"
           layout = "vertical"
           floating = true
           borderWidth = 1
           backgroundColor = "white"
           valueDecimals = 0
           width = 70
        |}
        |> pojo

    let colorAxis =
        match state.ChartType with
        | TwoWeekIncidence ->
            {|
                ``type`` = "logarithmic"
                tickInterval = 0.4
                max = 7000
                min = 1
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
                        formatter = fun() -> jsThis?value
                    |} |> pojo
            |} |> pojo
        | Restrictions ->
            {|
                ``type`` = "linear"
                tickInterval = 0.4
                max = 7000
                min = 1
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
                        formatter = fun() -> jsThis?value
                    |} |> pojo
            |} |> pojo
        | WeeklyIncrease ->
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
                   formatter = fun() -> sprintf "%s%%" jsThis?value
                |} |> pojo
            |} |> pojo


    let sparklineFormatter newCases =
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

        let maxCases = newCases |> Array.max
        let tickScale = max 1. (10. ** round (Math.Log10 (maxCases + 1.) - 1.))

        let color1 = "#bda506"
        let color2 = desaturateColor color1 0.6

        let columnColors = [| ([|color2 |] |> Array.replicate 7 |> Array.concat); ([| color1 |] |> Array.replicate 7 |> Array.concat)  |] |> Array.concat
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
                        labels = {| enabled = false |}
                        title = {| enabled = false |}
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
                        tickInterval = tickScale
                        endOnTick = true
                        startOnTick = false
                        allowDecimals = false
                        showFirstLabel = true
                        showLastLabel = true
                        gridLineColor = "#000000"
                        gridLineDashStyle = "dot"
                    |} |> pojo
                title = {| text = "" |}
                legend = {| enabled = false |}
                series =
                    [|
                        {|
                            data = newCases |> Array.map ( max 0.)
                            animation = false
                            colors = columnColors
                            borderColor = columnColors
                            pointWidth = 15 //
                            colorByPoint = true
                        |} |> pojo
                    |]
            |} |> pojo
        match state.MapToDisplay with
        | Europe ->
            Fable.Core.JS.setTimeout (fun () -> sparklineChart("tooltip-chart-eur", options)) 10 |> ignore
            """<div id="tooltip-chart-eur"; class="tooltip-chart";></div>"""
        | World ->
            Fable.Core.JS.setTimeout (fun () -> sparklineChart("tooltip-chart-world", options)) 10 |> ignore
            """<div id="tooltip-chart-world"; class="tooltip-chart";></div>"""

    let tooltipFormatter jsThis =
        let points = jsThis?point
        let twoWeekIncidence = points?incidence
        let country = points?country
        let incidence100k = points?incidence100k
        let newCases = points?newCases
        let twoWeekCases = points?twoWeekCases
        let ncDate = points?ncDate
        let imported = points?imported
        let impDate = points?impDate
        let rType = points?rType
        let rAltText = points?rAltText
        let weeklyIncrease = points?weeklyIncrease


        let textHtml =
            sprintf "<b>%s</b><br/>
            %s: <b>%s<br/>%s</br></br></b>
            %s: <b>%s</b> (%s)<br/><br/>
            %s: <b>%s</b><br/>
            %s: <b>%s</b> (%s)<br/>"
                country
                (chartText "countryStatus") rType rAltText
                (chartText "importedCases") imported impDate
                (chartText "incidence100k") incidence100k
                (chartText "newCases") (I18N.NumberFormat.formatNumber(newCases:int)) ncDate
            + sprintf "<br>%s: <b>%s%s %%</b>" (I18N.t "charts.map.relativeIncrease") (if weeklyIncrease < 500. then "" else ">") (weeklyIncrease |> Utils.formatTo1DecimalWithTrailingZero)

        match twoWeekIncidence with
        | null -> chartText "noData"
        | _ ->
            if (twoWeekCases |> Array.max) > 0. then
                textHtml + sparklineFormatter twoWeekCases
            else
                textHtml

    let series geoJson =
        {| visible = true
           mapData = geoJson
           data = mapData state
           joinBy = [| "iso-a3"; "code" |]
           nullColor = "white"
           borderColor = "#888"
           borderWidth = 0.5
           mapline = {| animation = {| duration = 0 |} |}
           states =
               {| normal = {| animation = {| duration = 0 |} |}
                  hover =
                      {| borderColor = "black"
                         animation = {| duration = 0 |} |} |} |}
        |> pojo

    {| optionsWithOnLoadEvent "covid19-europe-map" with
           title = null
           series = [| series geoJson |]
           colorAxis = colorAxis
           legend = legend
           tooltip =
               pojo
                   {| formatter = fun () -> tooltipFormatter jsThis
                      useHTML = true
                      distance = 50 |}
           credits =
               pojo
                   {| enabled = true
                      text =
                          sprintf "%s: %s, %s" (I18N.t "charts.common.dataSource") (I18N.t "charts.common.dsNIJZ")
                              (I18N.t "charts.common.dsOWD")
                      mapTextFull = ""
                      mapText = ""
                      href = "https://ourworldindata.org/coronavirus" |} |}
    |> map


let renderChartTypeSelectors (activeChartType: ChartType) dispatch =
    let renderChartSelector (chartSelector: ChartType) =
        let active = chartSelector = activeChartType
        Html.div [
            prop.onClick (fun _ -> dispatch chartSelector)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected")]
            prop.text chartSelector.GetName
        ]

    Html.div
        [ prop.className "chart-display-property-selector"
          prop.children (ChartType.All |> Seq.map renderChartSelector)
        ]


let render (state: State) dispatch =

    let chart =
        match state.GeoJson, state.OwdData with
        | Success geoJson, Success owdData -> renderMap state geoJson owdData
        | Failure err, _ -> Utils.renderErrorLoading err
        | _, Failure err -> Utils.renderErrorLoading err
        | _ -> Utils.renderLoading

    Html.div
        [ prop.children
            [ Utils.renderChartTopControls [ renderChartTypeSelectors state.ChartType (ChartTypeChanged >> dispatch) ]
              Html.div
                  [ prop.style [ style.height 550 ]
                    prop.className "map"
                    prop.children [ chart ] ] ] ]

let mapChart (props : {| mapToDisplay : MapToDisplay; data : WeeklyStatsData |}) =
    React.elmishComponent ("EuropeMapChart", init props.mapToDisplay props.data, update, render)
