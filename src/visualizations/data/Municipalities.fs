module Data.Municipalities

open FsToolkit.ErrorHandling
open Fable.SimpleHttp

open Types

type Metric =
    | ActiveCases
    | ConfirmedToDate
    | DeceasedToDate

type DataPoint = {
    Region : string
    Municipality : string
    Metric : Metric
    Value : int option
}

let parseMunicipalitiesData (csv : string) =
    let rows = csv.Split("\n")
    let header = rows.[0].Split(",")

    // Parse municipality header (region, municipality and metric)
    let headerMunicipalities =
        header.[1..]
        |> Array.map (fun col ->
            match col.Split(".") with
            | [| "region" ; region ; municipality ; "cases" ; "active" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = ActiveCases ; Value = None }
            | [| "region" ; region ; municipality ; "cases" ; "confirmed" ; "todate" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = ConfirmedToDate ; Value = None }
            | [| "region" ; region ; municipality ; "deceased" ; "todate" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = DeceasedToDate ; Value = None }
            | unknown ->
                printfn "Error parsing municipalities header: %s" col
                None
        )

    // Parse data rows
    rows.[1..]
    |> Array.map (fun row ->
        result {
            let columns = row.Split(",")

            if headerMunicipalities.Length <> columns.[1..].Length then
                return! Error ""
            else
                // Date is in the first column
                let! date = Utils.parseDate(columns.[0])
                // Merge municipality header information with data columns
                let data =
                    Array.map2 (fun header value ->
                        match header with
                        | None _ -> None
                        | Some header -> Some { header with Value = Utils.nativeParseInt value }
                    ) headerMunicipalities columns.[1..]
                    |> Array.choose id
                    // Group by region
                    |> Array.groupBy (fun dp -> dp.Region)
                    |> Array.map (fun (region, dps) ->
                        let municipalities =
                            dps
                            // Group by municipality and combine values
                            |> Array.groupBy (fun dp -> dp.Municipality)
                            |> Array.map (fun (municipality, dps) ->
                                dps
                                |> Array.fold (fun state dp ->
                                    match dp.Metric with
                                    | ActiveCases ->
                                        { state with ActiveCases = dp.Value }
                                    | ConfirmedToDate ->
                                        { state with ConfirmedToDate = dp.Value }
                                    | DeceasedToDate ->
                                        { state with DeceasedToDate = dp.Value }
                                ) { Name = municipality
                                    ActiveCases = None
                                    ConfirmedToDate = None
                                    DeceasedToDate = None })
                        // Region
                        { Name = region
                          Municipalities = municipalities |> Array.toList }
                    )
                let dataPoint : MunicipalitiesDataPoint = { Date = date ; Regions = data |> Array.toList } 
                return dataPoint
        })
    |> Array.choose (fun row ->
        match row with
        | Ok row -> Some row
        | Error _ -> None)
    |> Array.toList

let load(apiEndpoint: string) =
    async {
        let! (statusCode, response) = Http.get (sprintf "%s/api/municipalities?format=csv" apiEndpoint)

        if statusCode <> 200 then
            return MunicipalitiesDataLoaded (sprintf "Napaka pri nalaganju podatkov o občinah: %d" statusCode |> Failure)
        else
            let data = parseMunicipalitiesData response
            return MunicipalitiesDataLoaded (Success data)
    }
