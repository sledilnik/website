module Data.Sewage

open System
open Fable.Core
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "https://api-stage.sledilnik.org/api/sewage"

type PlantStats = {
   covN1Compensated: float option
   covN2Compensated: float option
}

type SewageStats = {
    year: int
    month: int
    day: int
    plants: Map<string, PlantStats>

} with
    member lt.Date = System.DateTime(lt.year, lt.month, lt.day)
    member lt.JsDate12h = System.DateTime(lt.year, lt.month, lt.day) |> Highcharts.Helpers.jsTime12h



let getOrFetch =
    let fetch () = async {
        let! response_code, json = Http.get url
        return
            match response_code with
            | 200 ->
                let data = json |> JS.JSON.parse |> JS.JSON.stringify // TODO: This is an ugly hack but it's needed, otherwise SimpleJson parser complains
                           |> SimpleJson.parse
                           |> SimpleJson.mapKeys (function
                               | "wastewater_treatment_plants" -> "plants"
                               | "cp-luc-pmmov-rawpmmov-n1" -> "covN1Compensated"
                               | "cp-luc-pmmov-rawpmmov-n2" -> "covN2Compensated"
                               | key -> key)
                           |> Json.tryConvertFromJsonAs<SewageStats array>

                match data with
                | Error err ->
                    Error (sprintf "Napaka pri nalaganju statističnih podatkov o čistilnih napravah: %s" err)
                | Ok data ->
                    Ok data
            | _ ->
                Error (sprintf "Napaka pri nalaganju statističnih podatkov o čistilnih napravah: %d" response_code)
    }
    Utils.memoize fetch
