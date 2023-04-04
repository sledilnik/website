module Data.SewageCases

open System
open Fable.Core
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "https://api.sledilnik.org/api/sewage-cases"

type GeneMeasurement = {
   raw: float
   norm: float
}

type CasesEstimate = {
   estimated: float
   active100k: int
}

type SewageCases = {
    year: int
    month: int
    day: int
    station: string
    flow: int
    cod: int
    n3: GeneMeasurement
    cases: CasesEstimate
    lat: float
    lon: float
    region: string
    population: int
    coverageRatio: float
} with
    member lt.Date = System.DateTime(lt.year, lt.month, lt.day)
    member lt.JsDate12h = System.DateTime(lt.year, lt.month, lt.day) |> Highcharts.Helpers.jsTime12h


let getOrFetch =
    let fetch () = async {
        let! response_code, json = Http.get url
        return
            match response_code with
            | 200 ->
                let data = json
                           |> SimpleJson.parseNative
                           |> Json.tryConvertFromJsonAs<SewageCases array>

                match data with
                | Error err ->
                    Error (sprintf "Napaka pri nalaganju statističnih podatkov o čistilnih napravah: %s" err)
                | Ok data ->
                    Ok data
            | _ ->
                Error (sprintf "Napaka pri nalaganju statističnih podatkov o čistilnih napravah: %d" response_code)
    }
    Utils.memoize fetch
