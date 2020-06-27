[<RequireQualifiedAccess>]
module EuropeMap

open Feliz
open Elmish
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Types

let geoJson : obj = importDefault "@highcharts/map-collection/custom/europe.geo.json"

type OwdData = Data.OurWorldInData.OurWorldInDataRemoteData

type ChartType =
    | TwoWeekIncidence
    | Restrictions

    override this.ToString() =
       match this with
       | TwoWeekIncidence   -> I18N.t "charts.europe.twoWeekIncidence"
       | Restrictions       -> I18N.t "charts.europe.restrictions"

type State =
    { OwdData : OwdData
      ChartType : ChartType }

type Msg =
    | OwdDataRequested
    | OwdDataReceived of OwdData
    | ChartTypeChanged of ChartType


let init (regionsData : StatsData) : State * Cmd<Msg> =
    { OwdData = NotAsked ; ChartType = TwoWeekIncidence }, Cmd.ofMsg OwdDataRequested

let countries = ["ALB" ; "AND" ; "AUT" ; "BLR" ; "BEL" ; "BIH" ; "BGR" ; "HRV" ; "CYP" ; "CZE" ; "DNK" ; "EST" ; "FRO" ; "FIN" ; "FRA" ; "DEU" ; "GRC" ; "HUN" ; "ISL" ; "IRL" ; "ITA" ; "LVA" ; "LIE" ; "LTU" ; "LUX" ; "MKD" ; "MLT" ; "MDA" ; "MCO" ; "MNE" ; "NLD" ; "NOR" ; "POL" ; "PRT" ; "SRB" ; "ROU" ; "RUS" ; "SMR" ; "SVK" ; "SVN" ; "ESP" ; "SWE" ; "CHE" ; "TUR" ; "UKR" ; "GBR" ; "VAT"]
let greenCountries = Set.ofList [ "AUT"; "CYP"; "CZE"; "DNK"; "EST"; "FIN"; "FRA"; "GRC"; "HRV"; "IRL"; "ISL"; "ITA"; "LVA"; "LIE"; "LTU"; "HUN"; "MLT"; "DEU"; "NOR"; "SVK"; "ESP"; "CHE" ]
let redCountries = Set.ofList [ "QAT"; "BHR"; "CHL"; "KWT"; "PER"; "ARM"; "DJI"; "OMN"; "BRA"; "PAN"; "BLR"; "AND"; "SGP"; "SWE"; "MDV"; "STP"; "ARE"; "USA"; "SAU"; "RUS"; "MDA"; "GIB"; "BOL"; "PRI"; "GAB"; "CYM"; "DOM"; "ZAF"; "IRN"; "GBR"; "MKD"; "BIH"; "SRB"; "-99"; "PRT"; "ALB" ]

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | OwdDataRequested ->
        let twoWeeksAgo = System.DateTime.Today.AddDays(-14.0)
        let twoWeeksAgoString = sprintf "%d-%02d-%02d" twoWeeksAgo.Year twoWeeksAgo.Month twoWeeksAgo.Day
        { state with OwdData = Loading }, Cmd.OfAsync.result (Data.OurWorldInData.loadCountryIncidence countries twoWeeksAgoString OwdDataReceived)
    | OwdDataReceived result ->
        { state with OwdData = result }, Cmd.none
    | ChartTypeChanged chartType ->
        { state with ChartType = chartType }, Cmd.none

let calculateOwdIncidence (data : Data.OurWorldInData.DataPoint list) =
    data
    |> List.groupBy (fun dp -> dp.CountryCode)
    |> List.map (fun (code, dps) ->
        let sum =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> List.sum
        {| code = code ; value = sum |} )
    |> List.toArray

let restrictedCountries =
    countries
    |> List.map (fun code ->
        let color = 
            if code = "SVN" then 0.0
            else if greenCountries.Contains(code) then 1.0
            else if redCountries.Contains(code) then 3.0 
            else 2.0
        {| code = code ; value = color |} )
    |> List.toArray

let renderIncidenceMap state owdData =

    let owdIncidence = calculateOwdIncidence owdData

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
           tooltip =
            {| distance = 50
               headerFormat = "<b>{point.key}</b><br>"
               pointFormat = "{point.value}" |}
        |}

    {| Highcharts.optionsWithOnLoadEvent "covid19-europe-map" with
        title = null
        series = [| series geoJson |]
        //legend = {| enabled = true ; layout = "vertical"  |}
        colorAxis = 
            {| dataClassColor = "category"
               dataClasses =
                [|
                    {| from = 0 ; color = "white" |}
                    {| from = 1 ; color = "#FFFFC2" |}
                    {| from = 10 ; color = "#FEF001" |}
                    {| from = 50 ; color = "#FFCE03" |}
                    {| from = 100 ; color = "#FD9A01" |}
                    {| from = 500 ; color = "#FD6104" |}
                    {| from = 1000 ; color = "#FF2C05" |}
                    {| from = 1500 ; color = "#F00505" |}
                |]
            |}
    |}
    |> Highcharts.map

let renderRestrictionsMap state =

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
           tooltip =
            {| distance = 50
               headerFormat = "<b>{point.key}</b><br>"
               pointFormat = "{point.value}" |}
        |}

    {| Highcharts.optionsWithOnLoadEvent "covid19-europe-map" with
        title = null
        series = [| series geoJson |]
        //legend = {| enabled = false |}
        colorAxis = 
            {| dataClassColor = "category"
               dataClasses =
                [|
                    {| from = 0 ; color = "white" |}
                    {| from = 1 ; color = "#C4DE6F" |}
                    {| from = 2 ; color = "#FEF65C" |}
                    {| from = 3 ; color = "#FF5348" |}
                |]
            |}
    |} 
    |> Highcharts.map

let renderMap state owdData =
    match state.ChartType with 
    | TwoWeekIncidence -> renderIncidenceMap state owdData
    | Restrictions -> renderRestrictionsMap state

let renderChartTypeSelectors (activeChartType: ChartType) dispatch =
    let renderChartSelector (chartSelector: ChartType) =
        let active = chartSelector = activeChartType
        Html.div [
            prop.onClick (fun _ -> dispatch chartSelector)
            prop.className [ true, "chart-display-property-selector__item"; active, "selected" ]
            prop.text (chartSelector.ToString())
        ]

    let chartTypeSelectors =
        [ TwoWeekIncidence; Restrictions ]
        |> List.map renderChartSelector

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children ( chartTypeSelectors )
    ]


let render (state : State) dispatch =
    let chart =
        match state.OwdData with
        | NotAsked | Loading -> Utils.renderLoading
        | Failure err -> Utils.renderErrorLoading err
        | Success owdData -> renderMap state owdData

    Html.div [
        prop.children [
            Utils.renderChartTopControls [
                renderChartTypeSelectors state.ChartType (ChartTypeChanged >> dispatch)
            ]
            Html.div [
                prop.style [ style.height 600 ]
                prop.className "map"
                prop.children [ chart ]
            ]
        ]
    ]


let mapChart (props : {| data : StatsData |}) =
    React.elmishComponent("EuropeMapChart", init props.data, update, render)
