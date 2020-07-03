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

let geoJsonUrl = "/maps/europe.geo.json"

type GeoJson = RemoteData<obj, string>

type OwdData = Data.OurWorldInData.OurWorldInDataRemoteData

type ChartType =
    | TwoWeekIncidence
    | Restrictions

    override this.ToString() =
       match this with
       | TwoWeekIncidence   -> I18N.t "charts.europe.twoWeekIncidence"
       | Restrictions       -> I18N.t "charts.europe.restrictions"


type State =
    { GeoJson : GeoJson
      OwdData : OwdData
      ChartType : ChartType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | OwdDataRequested
    | OwdDataReceived of OwdData
    | ChartTypeChanged of ChartType

let countries = ["ALB" ; "AND" ; "AUT" ; "BLR" ; "BEL" ; "BIH" ; "BGR" ; "HRV" ; "CYP" ; "CZE" ; "DNK" ; "EST" ; "FRO" ; "FIN" ; "FRA" ; "DEU" ; "GRC" ; "HUN" ; "ISL" ; "IRL" ; "ITA" ; "LVA" ; "LIE" ; "LTU" ; "LUX" ; "MKD" ; "MLT" ; "MDA" ; "MCO" ; "MNE" ; "NLD" ; "NOR" ; "POL" ; "PRT" ; "SRB" ; "ROU" ; "RUS" ; "SMR" ; "SVK" ; "SVN" ; "ESP" ; "SWE" ; "CHE" ; "TUR" ; "UKR" ; "GBR" ; "VAT" ; "RKS" ; "NCY" ; "NMA" ]
let owdCountries = countries |> List.map (fun code -> if code = "RKS" then "OWID_KOS" else code ) // hack for Kosovo code

let greenCountries = Set.ofList [ "AUT"; "CYP"; "CZE"; "DNK"; "EST"; "FIN"; "FRA"; "GRC"; "HRV"; "IRL"; "ISL"; "ITA"; "LVA"; "LIE"; "LTU"; "HUN"; "MLT"; "DEU"; "NOR"; "SVK"; "ESP"; "CHE" ]
let redCountries = Set.ofList [ "QAT"; "BHR"; "CHL"; "KWT"; "PER"; "ARM"; "DJI"; "OMN"; "BRA"; "PAN"; "BLR"; "AND"; "SGP"; "SWE"; "MDV"; "STP"; "ARE"; "USA"; "SAU"; "RUS"; "MDA"; "GIB"; "BOL"; "PRI"; "GAB"; "CYM"; "DOM"; "ZAF"; "IRN"; "GBR"; "MKD"; "BIH"; "SRB"; "RKS"; "PRT"; "ALB" ]
let importedFrom = Map.ofList [ ("BIH", 16); ("SRB", 13); ("RKS", 4); ("HRV", 2); ("KAZ", 1); ("SWE", 1); ("USA", 1); ]
let importedDate = DateTime(2020,6,28)

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

let init (regionsData : StatsData) : State * Cmd<Msg> =
    let cmdGeoJson = Cmd.ofMsg GeoJsonRequested
    let cmdOwdData = Cmd.ofMsg OwdDataRequested
    { GeoJson = NotAsked ; OwdData = NotAsked ; ChartType = Restrictions }, (cmdGeoJson @ cmdOwdData)

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | GeoJsonRequested ->
        { state with GeoJson = Loading }, Cmd.OfAsync.result loadGeoJson
    | GeoJsonLoaded geoJson ->
        { state with GeoJson = geoJson }, Cmd.none
    | OwdDataRequested ->
        let twoWeeksAgo = System.DateTime.Today.AddDays(-14.0)
        { state with OwdData = Loading }, Cmd.OfAsync.result (Data.OurWorldInData.loadCountryIncidence owdCountries twoWeeksAgo OwdDataReceived)
    | OwdDataReceived result ->
        { state with OwdData = result }, Cmd.none
    | ChartTypeChanged chartType ->
        { state with ChartType = chartType }, Cmd.none

let getOwdDate (data : Data.OurWorldInData.DataPoint list) =
    data
    |> List.groupBy (fun dp -> dp.CountryCode)
    |> List.map (fun (code, dps) ->
        dps
        |> List.map (fun dp -> dp.Date)
        |> List.max)
    |> List.max

let calculateOwdIncidence (data : Data.OurWorldInData.DataPoint list) =
    data
    |> List.groupBy (fun dp -> dp.CountryCode)
    |> List.map (fun (code, dps) ->
        let sum =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> List.sum
        let fixedCode = if code = "OWID_KOS" then "RKS" else code // hack for Kosovo code
        let country = I18N.tt "country" code
        {| code = fixedCode ; country = country ; value = sum ; color = null ; dataLabels = {| enabled = false |}|} )
    |> List.toArray

let renderIncidenceMap state geoJson owdData =

    let owdDate = getOwdDate owdData
    let owdIncidence = calculateOwdIncidence owdData

    let pointFormat =
        sprintf "<b>{point.country}</b><br/>%s: <b>{point.value}</b>"
            (I18N.t "charts.europe.incidence1M")

    let series geoJson =
        {| visible = true
           mapData = geoJson
           data = owdIncidence
           joinBy = [| "iso-a3" ; "code" |]
           nullColor = "white"
           borderColor = "#888"
           borderWidth = 0.5
           mapline = {| animation = {| duration = 0 |} |}
           states =
            {| normal = {| animation = {| duration = 0 |} |}
               hover = {| borderColor = "black" ; animation = {| duration = 0 |} |} |}
           tooltip = pojo
            {| distance = 50
               valueDecimals = 0
               headerFormat = ""
               pointFormat = pointFormat |}
        |}

    {| Highcharts.optionsWithOnLoadEvent "covid19-europe-map" with
        title = null
        series = [| series geoJson |]
        colorAxis = pojo
            {| dataClassColor = "category"
               dataClasses =
                [|
                    {| from = 0 ; color = "#ffffb2" |}
                    {| from = 10 ; color = "#fed976" |}
                    {| from = 100 ; color = "#feb24c" |}
                    {| from = 160 ; color = "#fd8d3c" |}
                    {| from = 320 ; color = "#fc4e2a" |}
                    {| from = 400 ; color = "#e31a1c" |}
                    {| from = 800 ; color = "#b10026" |}
                |]
            |} |> pojo
        legend = pojo
            {|
                enabled = true
                title = {| text = null |}
                align = "left"
                verticalAlign = "bottom"
                layout = "vertical"
                floating = true
                borderWidth = 1
                backgroundColor = "white"
                valueDecimals = 0
            |}
        credits = pojo
            {|
                enabled = true
                text = sprintf "%s: %s @ %s"
                        (I18N.t "charts.common.dataSource")
                        (I18N.t "charts.common.dsOWD")
                        (I18N.tOptions "days.date" {| date = owdDate |})
                mapTextFull = ""
                mapText = ""
                href = "https://ourworldindata.org/coronavirus"
            |}

    |}
    |> Highcharts.map


let restrictedCountries =
    countries
    |> List.map (fun code ->
        let rType, rColor =
            if code = "SVN" then I18N.t "charts.europe.statusNone", "#10829a"
            else if greenCountries.Contains(code) then I18N.t "charts.europe.statusGreen", "#C4DE6F"
            else if redCountries.Contains(code) then I18N.t "charts.europe.statusRed", "#FF5348"
            else I18N.t "charts.europe.statusYellow", "#FEF65C"
        let imported = importedFrom.TryFind(code) |> Option.defaultValue 0
        let label = imported > 0
        let country = I18N.tt "country" code
        {| code = code ; country = country ; value = imported ; rType = rType ; color = rColor ; dataLabels = {| enabled = label |} |} )
    |> List.toArray

let renderRestrictionsMap state geoJson =

    let pointFormat =
        sprintf "<b>{point.country}</b><br/>%s: <b>{point.value}</b><br/>%s: <b>{point.rType}</b>"
            (I18N.t "charts.europe.importedCases")
            (I18N.t "charts.europe.countryStatus")

    let series geoJson =
        {| visible = true
           mapData = geoJson
           data = restrictedCountries
           joinBy = [| "iso-a3" ; "code" |]
           nullColor = "white"
           borderColor = "#888"
           borderWidth = 0.5
           mapline = {| animation = {| duration = 0 |} |}
           states =
            {| normal = {| animation = {| duration = 0 |} |}
               hover = {| borderColor = "black" ; animation = {| duration = 0 |} |} |}
           tooltip = pojo
            {| distance = 50
               valueDecimals = 0
               headerFormat = ""
               pointFormat = pointFormat |}
        |} |> pojo

    {| Highcharts.optionsWithOnLoadEvent "covid19-europe-map" with
        title = null
        series = [| series geoJson |]
        legend = pojo {| enabled = false |}
        credits = pojo
            {|
                enabled = true
                text = sprintf "%s: %s @ %s"
                        (I18N.t "charts.common.dataSource")
                        (I18N.t "charts.common.dsNIJZ")
                        (I18N.tOptions "days.date" {| date = importedDate |})
                mapTextFull = ""
                mapText = ""
                href = "https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19"
            |}
    |}
    |> Highcharts.map

let renderMap state geoJson owdData =
    match state.ChartType with
    | TwoWeekIncidence -> renderIncidenceMap state geoJson owdData
    | Restrictions -> renderRestrictionsMap state geoJson

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

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            Html.text (I18N.t "charts.common.view")
            renderChartSelector Restrictions
            renderChartSelector TwoWeekIncidence
        ]
    ]


let render (state : State) dispatch =

    let chart =
        match state.GeoJson, state.OwdData with
        | Success geoJson, Success owdData -> renderMap state geoJson owdData
        | Failure err, _ -> Utils.renderErrorLoading err
        | _ , Failure err -> Utils.renderErrorLoading err
        | _ -> Utils.renderLoading

    Html.div [
        prop.children [
            Utils.renderChartTopControls [
                renderChartTypeSelectors state.ChartType (ChartTypeChanged >> dispatch)
            ]
            Html.div [
                prop.style [ style.height 550 ]
                prop.className "map"
                prop.children [ chart ]
            ]
        ]
    ]


let mapChart (props : {| data : StatsData |}) =
    React.elmishComponent("EuropeMapChart", init props.data, update, render)
