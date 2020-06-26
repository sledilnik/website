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

type State =
    { OwdData : OwdData }

type Msg =
    | OwdDataRequested
    | OwdDataReceived of OwdData

let init (regionsData : StatsData) : State * Cmd<Msg> =
    { OwdData = NotAsked }, Cmd.ofMsg OwdDataRequested

let countries = ["ALB" ; "AND" ; "AUT" ; "BLR" ; "BEL" ; "BIH" ; "BGR" ; "HRV" ; "CYP" ; "CZE" ; "DNK" ; "EST" ; "FRO" ; "FIN" ; "FRA" ; "DEU" ; "GRC" ; "HUN" ; "ISL" ; "IRL" ; "ITA" ; "LVA" ; "LIE" ; "LTU" ; "LUX" ; "MKD" ; "MLT" ; "MDA" ; "MCO" ; "MNE" ; "NLD" ; "NOR" ; "POL" ; "PRT" ; "SRB" ; "ROU" ; "RUS" ; "SMR" ; "SVK" ; "SVN" ; "ESP" ; "SWE" ; "CHE" ; "TUR" ; "UKR" ; "GBR" ; "VAT"]

let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | OwdDataRequested ->
        let twoWeeksAgo = System.DateTime.Today.AddDays(-14.0)
        let twoWeeksAgoString = sprintf "%d-%02d-%02d" twoWeeksAgo.Year twoWeeksAgo.Month twoWeeksAgo.Day
        { state with OwdData = Loading }, Cmd.OfAsync.result (Data.OurWorldInData.loadCountryIncidence countries twoWeeksAgoString OwdDataReceived)
    | OwdDataReceived result ->
        { state with OwdData = result }, Cmd.none

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

let renderMap owdData =

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
           dataLabels =
            {| enabled = true
               color = "#FFFFFF"
               format = "{point.code}" |}
           tooltip =
            {| distance = 50
               headerFormat = "<b>{point.key}</b><br>"
               pointFormat = "{point.value}" |}
        |}

    {| Highcharts.optionsWithOnLoadEvent "covid19-europe-map" with
        title = null
        series = [| series geoJson |]
        legend = {| enabled = false |}
        colorAxis =
            {| min = 0
               minColor = "#FFFFFF"
               maxColor = "#e03000"
            |}
        // colorAxis =
        //     {| dataClasses =
        //         [|
        //             {| from = 0 ; ``to`` = 10 ; color = "red" |}
        //             {| from = 11 ; ``to`` = 20 ; color = "orange" |}
        //             {| from = 21 ; ``to`` = 30 ; color = "green" |}
        //             {| from = 31 ; ``to`` = 50 ; color = "blue" |}
        //         |]
        //     |}
    |}
    |> Highcharts.map

let render (state : State) dispatch =
    let chart =
        match state.OwdData with
        | NotAsked | Loading -> Utils.renderLoading
        | Failure err -> Utils.renderErrorLoading err
        | Success owdData -> renderMap owdData
    Html.div [
        prop.style [ style.height 600 ]
        prop.className "map"
        prop.children [ chart ]
    ]

let mapChart (props : {| data : StatsData |}) =
    React.elmishComponent("EuropeMapChart", init props.data, update, render)
