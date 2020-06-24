[<RequireQualifiedAccess>]
module EuropeMap

open Fable.SimpleHttp

open Feliz
open Elmish
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser

open Types

let geoJson : obj = importDefault "@highcharts/map-collection/custom/europe.geo.json"

type State = unit

let init (regionsData : StatsData) : State * Cmd<Msg> =
    (), Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    state, Cmd.none

let data =
    [|
        {| code = "dk" ; value = Some 0 |}
        {| code = "fo" ; value = Some 1 |}
        {| code = "hr" ; value = Some 2 |}
        {| code = "nl" ; value = Some 3 |}
        {| code = "ee" ; value = Some 4 |}
        {| code = "bg" ; value = Some 5 |}
        {| code = "es" ; value = Some 6 |}
        {| code = "it" ; value = Some 7 |}
        {| code = "sm" ; value = Some 8 |}
        {| code = "va" ; value = Some 9 |}
        {| code = "tr" ; value = Some 10 |}
        {| code = "mt" ; value = Some 11 |}
        {| code = "fr" ; value = Some 12 |}
        {| code = "no" ; value = Some 13 |}
        {| code = "de" ; value = None |}
        {| code = "ie" ; value = Some 15 |}
        {| code = "ua" ; value = Some 16 |}
        {| code = "fi" ; value = Some 17 |}
        {| code = "se" ; value = Some 18 |}
        {| code = "ru" ; value = Some 19 |}
        {| code = "gb" ; value = Some 20 |}
        {| code = "cy" ; value = Some 21 |}
        {| code = "pt" ; value = Some 22 |}
        {| code = "gr" ; value = Some 23 |}
        {| code = "lt" ; value = Some 24 |}
        {| code = "si" ; value = Some 25 |}
        {| code = "ba" ; value = Some 26 |}
        {| code = "mc" ; value = Some 27 |}
        {| code = "al" ; value = Some 28 |}
        {| code = "cnm" ; value = Some 29 |}
        {| code = "nc" ; value = Some 30 |}
        {| code = "rs" ; value = Some 31 |}
        {| code = "ro" ; value = Some 32 |}
        {| code = "me" ; value = Some 33 |}
        {| code = "li" ; value = Some 34 |}
        {| code = "at" ; value = Some 35 |}
        {| code = "sk" ; value = Some 36 |}
        {| code = "hu" ; value = Some 37 |}
        {| code = "ad" ; value = Some 38 |}
        {| code = "lu" ; value = Some 39 |}
        {| code = "ch" ; value = Some 40 |}
        {| code = "be" ; value = Some 41 |}
        {| code = "kv" ; value = Some 42 |}
        {| code = "pl" ; value = Some 43 |}
        {| code = "mk" ; value = Some 44 |}
        {| code = "lv" ; value = Some 45 |}
        {| code = "by" ; value = Some 46 |}
        {| code = "is" ; value = Some 47 |}
        {| code = "md" ; value = Some 48 |}
        {| code = "cz" ; value = Some 49 |}
    |]


let renderMap (state : State) =

    let series geoJson =
        {| visible = true
           mapData = geoJson
           data = data
           joinBy = [| "hc-key" ; "code" |]
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
        legend = {| enabled = false |}
        colorAxis =
            {| dataClasses =
                [|
                    {| from = 0 ; ``to`` = 10 ; color = "red" |}
                    {| from = 11 ; ``to`` = 20 ; color = "orange" |}
                    {| from = 21 ; ``to`` = 30 ; color = "green" |}
                    {| from = 31 ; ``to`` = 50 ; color = "blue" |}
                |]
            |}
    |}
    |> Highcharts.map

let render (state : State) dispatch =
    Html.div [
        prop.children [
            Html.div [
                prop.style [ style.height 600 ]
                prop.className "map"
                prop.children [ renderMap state ]
            ]
        ]
    ]

let mapChart (props : {| data : StatsData |}) =
    React.elmishComponent("EuropeMapChart", init props.data, update, render)
