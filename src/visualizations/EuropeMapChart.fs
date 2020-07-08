[<RequireQualifiedAccess>]
module EuropeMap

open System
open System.Text
open Feliz
open Elmish
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Fable.SimpleHttp
open Browser

open Highcharts
open Types

let geoJsonUrl = "/maps/europe.geo.json"

type GeoJson = RemoteData<obj, string>

type OwdData = Data.OurWorldInData.OurWorldInDataRemoteData

type CountryData =
    { Country: string
      // OWD data
      TwoWeekIncidence1M: float
      TwoWeekIncidence: float []
      TwoWeekIncidenceMaxValue: float
      NewCases: int list
      OwdDate: DateTime
      // NIJZ data
      RestrictionColor: string
      RestrictionText: string
      ImportedFrom: int
      ImportedDate: DateTime }

type CountriesMap = Map<string, CountryData>

type ChartType =
    | TwoWeekIncidence
    | Restrictions

    override this.ToString() =
        match this with
        | TwoWeekIncidence -> I18N.t "charts.europe.twoWeekIncidence"
        | Restrictions -> I18N.t "charts.europe.restrictions"

type State =
    { GeoJson: GeoJson
      OwdData: OwdData
      CountryData: CountriesMap
      ChartType: ChartType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | OwdDataRequested
    | OwdDataReceived of OwdData
    | ChartTypeChanged of ChartType

let countries =
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
      "RKS"
      "NCY"
      "NMA" ]

let owdCountries =
    countries
    |> List.map (fun code -> if code = "RKS" then "OWID_KOS" else code) // hack for Kosovo code

let greenCountries =
    Set.ofList
        [ "AUT"
          "CYP"
          "CZE"
          "DNK"
          "EST"
          "FIN"
          "GRC"
          "FRA"
          "IRL"
          "ISL"
          "ITA"
          "LVA"
          "LIE"
          "LTU"
          "HUN"
          "MLT"
          "DEU"
          "NOR"
          "SVK"
          "ESP"
          "CHE"
          "BEL"
          "NLD" ]

let redCountries =
    Set.ofList
        [ "QAT"
          "BHR"
          "CHL"
          "KWT"
          "PER"
          "ARM"
          "DJI"
          "OMN"
          "BRA"
          "PAN"
          "BLR"
          "AND"
          "SGP"
          "SWE"
          "MDV"
          "STP"
          "ARE"
          "USA"
          "SAU"
          "RUS"
          "MDA"
          "GIB"
          "BOL"
          "PRI"
          "GAB"
          "CYM"
          "DOM"
          "ZAF"
          "IRN"
          "GBR"
          "MKD"
          "BIH"
          "SRB"
          "RKS"
          "PRT"
          "ALB" ]

let importedFrom =
    Map.ofList
        [ ("BIH", 18)
          ("SRB", 11)
          ("HRV", 10)
          ("RKS", 6)
          ("MNE", 3)
          ("KAZ", 2) ]

let importedDate = DateTime(2020, 7, 5)

let loadGeoJson =
    async {
        let! (statusCode, response) = Http.get geoJsonUrl

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

let init (regionsData: StatsData): State * Cmd<Msg> =
    let cmdGeoJson = Cmd.ofMsg GeoJsonRequested
    let cmdOwdData = Cmd.ofMsg OwdDataRequested
    { GeoJson = NotAsked
      OwdData = NotAsked
      CountryData = Map.empty
      ChartType = Restrictions },
    (cmdGeoJson @ cmdOwdData)

let prepareCountryData (data: Data.OurWorldInData.DataPoint list) =
    data
    |> List.groupBy (fun dp -> dp.CountryCode)
    |> List.map (fun (code, dps) ->
        let fixedCode =
            if code = "OWID_KOS" then "RKS" else code // hack for Kosovo code

        let country = I18N.tt "country" code // TODO: change country code in i18n for Kosovo

        let incidence1M =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> List.sum

        let incidence =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> List.toArray

        let incidenceMaxValue =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> List.max

        let newCases = dps |> List.map (fun dp -> dp.NewCases)

        let owdDate =
            dps |> List.map (fun dp -> dp.Date) |> List.max

        let rText, rColor =
            if fixedCode = "SVN"
            then I18N.t "charts.europe.statusNone", "#10829a"
            else if greenCountries.Contains(fixedCode)
            then I18N.t "charts.europe.statusGreen", "#C4DE6F"
            else if redCountries.Contains(fixedCode)
            then I18N.t "charts.europe.statusRed", "#FF5348"
            else I18N.t "charts.europe.statusYellow", "#FEF65C"

        let imported =
            importedFrom.TryFind(fixedCode)
            |> Option.defaultValue 0

        let cd: CountryData =
            { CountryData.Country = country
              CountryData.TwoWeekIncidence1M = incidence1M
              CountryData.TwoWeekIncidence = incidence
              CountryData.TwoWeekIncidenceMaxValue = incidenceMaxValue
              CountryData.NewCases = newCases
              CountryData.OwdDate = owdDate
              CountryData.RestrictionColor = rColor
              CountryData.RestrictionText = rText
              CountryData.ImportedFrom = imported
              CountryData.ImportedDate = importedDate }

        (fixedCode, cd))
    |> Map.ofList

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | GeoJsonRequested -> { state with GeoJson = Loading }, Cmd.OfAsync.result loadGeoJson
    | GeoJsonLoaded geoJson -> { state with GeoJson = geoJson }, Cmd.none
    | OwdDataRequested ->
        let twoWeeksAgo = System.DateTime.Today.AddDays(-14.0)
        { state with OwdData = Loading },
        Cmd.OfAsync.result (Data.OurWorldInData.loadCountryIncidence owdCountries twoWeeksAgo OwdDataReceived)
    | OwdDataReceived result ->
        let ret =
            match result with
            | Success owdData ->
                { state with
                      OwdData = result
                      CountryData = prepareCountryData owdData }
            | _ -> { state with OwdData = result }

        ret, Cmd.none
    | ChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none


let mapData state =
    countries
    |> List.map (fun code ->
        match state.CountryData.TryFind(code) with
        | Some cd ->
            let incidence1M = cd.TwoWeekIncidence1M |> int
            let incidence = cd.TwoWeekIncidence
            let incidenceMaxValue = cd.TwoWeekIncidenceMaxValue

            let nc =
                cd.NewCases
                |> List.tryLast
                |> Option.defaultValue 0

            let ncDate =
                (I18N.tOptions "days.date" {| date = cd.OwdDate |})

            let impDate =
                (I18N.tOptions "days.date" {| date = cd.ImportedDate |})

            let baseRec =
                {| code = code
                   country = cd.Country
                   incidence1M = incidence1M
                   incidence = incidence
                   incidenceMaxValue = incidenceMaxValue
                   newCases = nc
                   ncDate = ncDate
                   rType = cd.RestrictionText
                   imported = cd.ImportedFrom
                   impDate = impDate |}

            match state.ChartType with
            | TwoWeekIncidence ->
                {| baseRec with
                       value = incidence1M
                       color = null
                       dataLabels = {| enabled = false |} |}
            | Restrictions ->
                {| baseRec with
                       value = cd.ImportedFrom
                       color = cd.RestrictionColor
                       dataLabels = {| enabled = cd.ImportedFrom > 0 |} |}
        | _ ->
            {| code = code
               country = ""
               value = 0
               color = null
               dataLabels = {| enabled = false |}
               incidence1M = 0
               incidence = null
               incidenceMaxValue = 0.0
               newCases = 0
               ncDate = ""
               rType = ""
               imported = 0
               impDate = "" |})
    |> List.toArray

let renderMap state geoJson owdData =

    let legend =
        let enabled = state.ChartType = TwoWeekIncidence
        {| enabled = enabled
           title = {| text = null |}
           align = "left"
           verticalAlign = "bottom"
           layout = "vertical"
           floating = true
           borderWidth = 1
           backgroundColor = "white"
           valueDecimals = 0 |}
        |> pojo

    let colorAxis =
        {| dataClassColor = "category"
           dataClasses =
               [| {| from = 0; color = "#ffffb2" |}
                  {| from = 50; color = "#fed976" |}
                  {| from = 100; color = "#feb24c" |}
                  {| from = 160; color = "#fd8d3c" |}
                  {| from = 320; color = "#fc4e2a" |}
                  {| from = 400; color = "#e31a1c" |}
                  {| from = 800; color = "#b10026" |} |] |}
        |> pojo

    let tooltipFormatter jsThis =
        let points = jsThis?point
        let twoWeekIncidence = points?incidence
        let twoWeekIncidenceMaxValue = points?incidenceMaxValue + 1 // TODO: hack - added 1 because this is an integer rounded to floor for some reason (instead of float)
        let country = points?country
        let incidence1M = points?incidence1M
        let newCases = points?newCases
        let ncDate = points?ncDate
        let imported = points?imported
        let impDate = points?impDate
        let rType = points?rType

        let s = StringBuilder()
        let barMaxHeight = 50

        let textHtml =
            sprintf "<b>%s</b><br/>
            %s: <b>%s</b><br/>
            %s: <b>%s</b> (%s)<br/><br/>
            %s: <b>%s</b><br/>
            %s: <b>%s</b> (%s)<br/>"
                country
                (I18N.t "charts.europe.countryStatus") rType
                (I18N.t "charts.europe.importedCases") imported impDate
                (I18N.t "charts.europe.incidence1M") incidence1M
                (I18N.t "charts.europe.newCases") newCases ncDate

        s.Append textHtml |> ignore

        s.Append "<div class='bars'>" |> ignore

        match twoWeekIncidence with
        | null -> I18N.t "charts.europe.noData"
        | _ ->
        twoWeekIncidence
        |> Array.iter (fun country ->
            let barHeight = country * barMaxHeight / twoWeekIncidenceMaxValue

            let barHtml =
                sprintf "<div class='bar-wrapper'><div class='bar' style='height: %Apx'></div></div>" barHeight

            s.Append barHtml |> ignore)

        s.Append "</div>" |> ignore
        s.ToString()

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

    {| Highcharts.optionsWithOnLoadEvent "covid19-europe-map" with
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
    |> Highcharts.map


let renderChartTypeSelectors (activeChartType: ChartType) dispatch =
    let renderChartSelector (chartSelector: ChartType) =
        let active = chartSelector = activeChartType
        Html.div [
            prop.onClick (fun _ -> dispatch chartSelector)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected")]
            prop.text (chartSelector.ToString())
        ]

    Html.div
        [ prop.className "chart-display-property-selector"
          prop.children
              [ Html.text (I18N.t "charts.common.view")
                renderChartSelector Restrictions
                renderChartSelector TwoWeekIncidence ] ]


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


let mapChart (props: {| data: StatsData |}) =
    React.elmishComponent ("EuropeMapChart", init props.data, update, render)
