module Data.Regions

open Fable.SimpleHttp
open Fable.SimpleJson

open System
open Types

let url = "https://api.sledilnik.org/api/municipalities"

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
                                        | JObject citiesMap ->
                                            citiesMap
                                            |> Map.toList
                                            |> List.map (fun (cityKey, cityValue) ->
                                                match cityValue with
                                                | JObject cityMap ->
                                                    let activeCases = 
                                                        match Map.tryFind "activeCases" cityMap with
                                                        | Some (JNumber num) -> Some (int num)
                                                        | Some (JNull) -> None
                                                        | _ -> failwith (sprintf "nepričakovan format podatkov za mesto %s in activeCases" cityKey)
                                                    let confirmedToDate =
                                                        match Map.tryFind "confirmedToDate" cityMap with
                                                        | Some (JNumber num) -> Some (int num)
                                                        | Some (JNull) -> None
                                                        | _ -> failwith (sprintf "nepričakovan format podatkov za mesto %s in confirmedToDate" cityKey)
                                                    let deceasedToDate =
                                                        match Map.tryFind "deceasedToDate" cityMap with
                                                        | Some (JNumber num) -> Some (int num)
                                                        | Some (JNull) -> None
                                                        | _ -> failwith (sprintf "nepričakovan format podatkov za mesto %s in deceasedToDate" cityKey)
                                                    { Name = cityKey 
                                                      ConfirmedToDate = confirmedToDate
                                                      ActiveCases = activeCases 
                                                      DeceasedToDate = deceasedToDate }
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
        // quick hack to only get last 60 days - enough to show last 30 days + 14 days to calculate active cases
        let startDate = DateTime.Now.AddDays -60.0
        let urlQuery = url + "?from=" + startDate.ToString("yyyy-MM-dd")

        let! (statusCode, response) = Http.get urlQuery

        if statusCode <> 200 then
            return RegionsDataLoaded (sprintf "Napaka pri nalaganju statističnih podatkov: %d" statusCode |> Failure)
        else
            try
                let data = parseRegionsData response
                return RegionsDataLoaded (Success data)
            with
                | ex -> return RegionsDataLoaded (sprintf "Napaka pri branju statističnih podatkov: %s" (ex.Message.Substring(0, 1000)) |> Failure)
    }
