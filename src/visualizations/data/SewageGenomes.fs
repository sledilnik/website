module Data.SewageGenomes

open System
open Fable.Core
open Fable.SimpleHttp
open Fable.SimpleJson

let url = "https://api.sledilnik.org/api/sewage-genomes"

type SewageGenomes =
    { year: int
      month: int
      day: int
      station: string
      genome: string
      ratio: float
      region: string }

    member lt.Date = System.DateTime(lt.year, lt.month, lt.day)

    member lt.JsDate12h =
        System.DateTime(lt.year, lt.month, lt.day) |> Highcharts.Helpers.jsTime12h


let getOrFetch =
    let fetch () =
        async {
            let! response_code, json = Http.get url

            return
                match response_code with
                | 200 ->
                    let data =
                        json |> SimpleJson.parseNative |> Json.tryConvertFromJsonAs<SewageGenomes array>

                    match data with
                    | Error err ->
                        Error(sprintf "Napaka pri nalaganju statističnih podatkov o čistilnih napravah: %s" err)
                    | Ok data -> Ok data
                | _ ->
                    Error(sprintf "Napaka pri nalaganju statističnih podatkov o čistilnih napravah: %d" response_code)
        }

    Utils.memoize fetch
