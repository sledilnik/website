[<RequireQualifiedAccess>]
module EuropeMap

open System

open Fable.SimpleHttp

open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types

let geoJsonUrl = "/maps/europe.geo.json"

type GeoJson = RemoteData<obj, string>

type State =
    { GeoJson : GeoJson }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson

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
    { GeoJson = NotAsked
    }, Cmd.ofMsg GeoJsonRequested

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | GeoJsonRequested ->
        { state with GeoJson = Loading }, Cmd.OfAsync.result loadGeoJson
    | GeoJsonLoaded geoJson ->
        { state with GeoJson = geoJson }, Cmd.none

// TODO
let chartLoadedEvent () =
    // trigger event for iframe resize
    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true)
    document.dispatchEvent(evt) |> ignore

let renderMap (state : State) =

    let data =
        [|
            ["dk", 0],
            ["fo", 1],
            ["hr", 2],
            ["nl", 3],
            ["ee", 4],
            ["bg", 5],
            ["es", 6],
            ["it", 7],
            ["sm", 8],
            ["va", 9],
            ["tr", 10],
            ["mt", 11],
            ["fr", 12],
            ["no", 13],
            ["de", 14],
            ["ie", 15],
            ["ua", 16],
            ["fi", 17],
            ["se", 18],
            ["ru", 19],
            ["gb", 20],
            ["cy", 21],
            ["pt", 22],
            ["gr", 23],
            ["lt", 24],
            ["si", 25],
            ["ba", 26],
            ["mc", 27],
            ["al", 28],
            ["cnm", 29],
            ["nc", 30],
            ["rs", 31],
            ["ro", 32],
            ["me", 33],
            ["li", 34],
            ["at", 35],
            ["sk", 36],
            ["hu", 37],
            ["ad", 38],
            ["lu", 39],
            ["ch", 40],
            ["be", 41],
            ["kv", 42],
            ["pl", 43],
            ["mk", 44],
            ["lv", 45],
            ["by", 46],
            ["is", 47],
            ["md", 48],
            ["cz", 49]
        |]

    let series =
        {| visible = true
           mapData = "custom/europe"
           data = data
           keys = [| "key" ; "value" |]
           joinBy = "key"
           tooltip =
            {| distance = 50
               headerFormat = "<b>{point.key}</b><br>"
               pointFormat = "{point.label}" |}
        |}

    {| Highcharts.optionsWithOnLoadEvent "covid19-europe-map" with
        chart = {| map = "custom/europe" |}
        title = null
        series = [| series  |]
        legend = {| enabled = false |}
        colorAxis = {| minColor = "white" ; maxColor = "red" |}
    |}
    |> Highcharts.map

let render (state : State) dispatch =
    Html.div [
        prop.children [
            Html.div [
                prop.className "map"
                prop.children [ renderMap state ]
            ]
        ]
    ]

let mapChart (props : {| data : StatsData |}) =
    React.elmishComponent("EuropeMapChart", init props.data, update, render)