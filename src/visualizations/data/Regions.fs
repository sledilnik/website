module Data.Regions

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let url = "https://api.sledilnik.org/api/regions"

let parseRegionsData data =
    data
    |> SimpleJson.parse
    |> function
        | JArray regions ->
            regions
            |> List.map (fun dataPoint ->
                match dataPoint with
                | JObject dict ->
                    let value key = Map.tryFind key dict
                    [value "year" ; value "month" ; value "day" ; value "regions" ]
                    |> List.choose id
                    |> function
                        | [JNumber year ; JNumber month ; JNumber day ; JObject regionMap] ->
                            let date = System.DateTime(int year, int month, int day)
                            let regions =
                                regionMap
                                |> Map.toList
                                |> List.map (fun (regionKey, regionValue) ->
                                    let municipalities =
                                        match regionValue with
                                        | JObject cityMap ->
                                            cityMap
                                            |> Map.toList
                                            |> List.map (fun (cityKey, cityValue) ->
                                                match cityValue with
                                                | JNumber num -> { Name = cityKey ; PositiveTests = Some (int num) }
                                                | JNull -> { Name = cityKey ; PositiveTests = None }
                                                | _ -> failwith (sprintf "nepričakovan format podatkov za mesto %s" cityKey)
                                            )
                                        | _ -> failwith (sprintf "nepričakovan format podatkov za regijo %s" regionKey)
                                    { Name = regionKey
                                      Municipalities = municipalities }
                                )
                            { Date = date
                              Regions = regions }
                        | _ -> failwith "nepričakovan format regijskih podatkov"
                | _ -> failwith "nepričakovan format regijskih podatkov"
            )
        | _ -> failwith "nepričakovan format regijskih podatkov"

let load =
    async {
        let! (statusCode, response) = Http.get url

        if statusCode <> 200 then
            return RegionsDataLoaded (sprintf "Napaka pri nalaganju statističnih podatkov: %d" statusCode |> Failure)
        else
            try
                let data = parseRegionsData response
                return RegionsDataLoaded (Success data)
            with
                | ex -> return RegionsDataLoaded (sprintf "Napaka pri branju statističnih podatkov: %s" ex.Message |> Failure)
    }
