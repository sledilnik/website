module Data.Sewage

open System
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "/sewage.json"


type WastewaterTreatmentPlantStats = {
   covN1: float
}

type SewageStats = {
    year: int
    month: int
    day: int
    wastewaterTreatmentPlants: Map<string, WastewaterTreatmentPlantStats>

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
                           |> SimpleJson.parse
                           |> SimpleJson.mapKeys (function
                               | "wastewater_treatment_plants" -> "wastewaterTreatmentPlants"
                               | "cp-luc-pmmov-rawpmmov-n1" -> "covN1"
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
