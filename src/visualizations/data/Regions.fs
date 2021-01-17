module Data.Regions

open FsToolkit.ErrorHandling
open Fable.SimpleHttp

open Types

type Metric =
    | ActiveCases
    | ConfirmedToDate
    | DeceasedToDate

type DataPoint = {
    Region : string
    Metric : Metric
    Value : int option
}

let parseRegionsData (csv : string) =
    let rows = csv.Split("\n")
    let header = rows.[0].Split(",")

    // Parse regions header (region and metric)
    let headerRegions =
        header.[1..]
        |> Array.map (fun col ->
            match col.Split(".") with
            | [| "region" ; region ; "cases" ; "active" |] ->
                Some { Region = region ; Metric = ActiveCases ; Value = None }
            | [| "region" ; region ; "cases" ; "confirmed" ; "todate" |] ->
                Some { Region = region ; Metric = ConfirmedToDate ; Value = None }
            | [| "region" ; region ; "deceased" ; "todate" |] ->
                Some { Region = region ; Metric = DeceasedToDate ; Value = None }
            | unknown ->
                printfn "Error parsing regions header: %s" col
                None
        )

    // Parse data rows
    rows.[1..]
    |> Array.map (fun row ->
        result {
            let columns = row.Split(",")

            if headerRegions.Length <> columns.[1..].Length then
                return! Error ""
            else
                // Date is in the first column
                let! date = Utils.parseDate(columns.[0])
                // Merge regions header information with data columns
                let data =
                    Array.map2 (fun header value ->
                        match header with
                        | None _ -> None
                        | Some header -> Some { header with Value = Utils.nativeParseInt value }
                    ) headerRegions columns.[1..]
                    |> Array.choose id
                    // Group by region
                    |> Array.groupBy (fun dp -> dp.Region)
                    |> Array.map (fun (region, dps) ->
                        dps
                        |> Array.fold (fun state dp ->
                            match dp.Metric with
                            | ActiveCases ->
                                { state with ActiveCases = dp.Value }
                            | ConfirmedToDate ->
                                { state with ConfirmedToDate = dp.Value }
                            | DeceasedToDate ->
                                { state with DeceasedToDate = dp.Value }
                        ) { Name = region
                            ActiveCases = None
                            ConfirmedToDate = None
                            DeceasedToDate = None })
                
                let dataPoint : RegionsDataPoint = { Date = date ; Regions = data |> Array.toList } 
                return dataPoint
        })
    |> Array.choose (fun row ->
        match row with
        | Ok row -> Some row
        | Error _ -> None)
    |> Array.toList

let load(apiEndpoint: string) =
    async {
        let! (statusCode, response) = Http.get (sprintf "%s/api/regions?format=csv" apiEndpoint)

        if statusCode <> 200 then
            return RegionsDataLoaded (sprintf "Napaka pri nalaganju podatkov o regijah: %d" statusCode |> Failure)
        else
            let data = parseRegionsData response
            return RegionsDataLoaded (Success data)
    }
