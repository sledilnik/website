module Data.OurWorldInData

open Fable.SimpleHttp

open EdelweissData.Base.Identifiers
open EdelweissData.Base.Routes
open EdelweissData.Base.Queries
open EdelweissData.Base.Values

open Types

let apiUrl = Url "https://api.develop.edelweiss.douglasconnect.com/"

let datasetId = PublishedDatasetId (Guid "cc898cd8-5d61-4d69-b15e-6a9e35bc5906")

let datasetIdAndVersion =
    { Id = datasetId
      VersionOrLatest = Latest }

let (Url datasetUrl) =
    createRoutePublishedDatasetGetByIdSpecificVersion apiUrl datasetIdAndVersion
let (Url dataUrl) =
    createRoutePublishedDatasetGetData apiUrl datasetIdAndVersion

let createQuery (countries : string list) =
    let countryMatch country =
        ExactSearch
            (Cast
                 (QueryExpression.Column
                      (QueryColumn.UserColumn "location"),
                      EdelweissData.Base.Types.TypeString),
                 country)
    { DataQuery.Default with
        Condition = Some (Or (List.map countryMatch countries)) }

type DataPoint = {
    Location : string
    Date : string
    TotalCases : int
    TotalCasesPerMillion : float option
    TotalDeathsPerMillion : float option
}

type Data = RemoteData<DataPoint list, string>

let stringOfResult row column =
    match row.Data.TryFind column with
    | None -> ""
    | Some value ->
        match value with
        | ValuePrimitive value ->
            match value with
            | ValueString value -> value
            | _ -> ""
        | _ -> ""

let intOfResult row column =
    match row.Data.TryFind column with
    | None -> 0
    | Some value ->
        match value with
        | ValuePrimitive value ->
            match value with
            | ValueInt value -> int value
            | _ -> 0
        | _ -> 0

let floatOptionOfResult row column =
    match row.Data.TryFind column with
    | None -> None
    | Some value ->
        match value with
        | ValuePrimitive value ->
            match value with
            | ValueDouble value -> Some value
            | _ -> None
        | _ -> None

let load countries msg =
    async {
        let! statusCode, response = Http.get datasetUrl

        match statusCode = 200 with
        | false ->
            return sprintf
                       "Napaka pri nalaganju OurWorldInData podatkov: %d"
                       statusCode |> Failure |> msg
        | true ->
            match EdelweissData.Thoth.Datasets.PublishedDataset.fromString
                      response with
            | Error error ->
                return sprintf
                           "Napaka pri nalaganju OurWorldInData: %s"
                           (error.ToString()) |> Failure |> msg
            | Ok dataset ->
                let! statusCode, response =
                    Http.post dataUrl
                        (createQuery countries
                         |> EdelweissData.Thoth.Queries.DataQuery.toString)

                match statusCode = 200 with
                | false ->
                    return sprintf
                        "Napaka pri nalaganju OurWorldInData podatkov: %d"
                        statusCode |> Failure |> msg
                | true ->
                    match
                        EdelweissData.Thoth.Queries.DataQueryResponse.fromString
                            dataset.Schema response with
                    | Error error ->
                        return sprintf
                                   "Napaka pri nalaganju OurWorldInData podatkov: %s"
                                   (error.ToString()) |> Failure |> msg
                    | Ok data ->
                        let dataPoints =
                            data.Results
                            |> List.map (fun row ->
                            {
                                Location = stringOfResult row "location"
                                Date = stringOfResult row "date"
                                TotalCases = intOfResult row "total_cases"
                                TotalCasesPerMillion =
                                  floatOptionOfResult
                                      row "total_cases_per_million"
                                TotalDeathsPerMillion =
                                  floatOptionOfResult
                                      row "total_deaths_per_million"
                            })
                        return dataPoints |> Success |> msg
    }
