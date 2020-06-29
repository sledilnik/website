module Data.OurWorldInData

open Fable.SimpleHttp

open EdelweissData.Base.Identifiers
open EdelweissData.Base.Routes
open EdelweissData.Base.Queries
open EdelweissData.Base.Values

open Types

let apiUrl = Url "https://api.edelweissdata.com/"

let datasetId = PublishedDatasetId (Guid "b55b229d-6338-4e41-a507-0cf4d3297b54")

let datasetIdAndVersion =
    { Id = datasetId
      VersionOrLatest = Latest }

let (Url datasetUrl) =
    createRoutePublishedDatasetGetByIdSpecificVersion apiUrl datasetIdAndVersion

let (Url dataUrl) =
    createRoutePublishedDatasetGetData apiUrl datasetIdAndVersion

type CountryIsoCode = string

type DataPoint = {
    CountryCode : CountryIsoCode
    Date : string
    NewCases : int
    NewCasesPerMillion : float option
    TotalCases: int
    TotalCasesPerMillion : float option
    TotalDeaths: int
    TotalDeathsPerMillion : float option
}

type OurWorldInDataRemoteData = RemoteData<DataPoint list, string>

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

let intOfDoubleResult row column =
    match row.Data.TryFind column with
    | None -> 0
    | Some value ->
        match value with
        | ValuePrimitive value ->
            match value with
            | ValueDouble value -> int value
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


let load query msg =
    let mapRow row =
        {
            CountryCode = stringOfResult row "iso_code"
            Date = stringOfResult row "date"
            NewCases = intOfDoubleResult row "new_cases"
            NewCasesPerMillion = floatOptionOfResult row "new_cases_per_million"
            TotalCases = intOfDoubleResult row "total_cases"
            TotalCasesPerMillion = floatOptionOfResult row "total_cases_per_million"
            TotalDeaths = intOfDoubleResult row "total_deaths"
            TotalDeathsPerMillion = floatOptionOfResult row "total_deaths_per_million"
        }

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
                    Http.post dataUrl (query |> EdelweissData.Thoth.Queries.DataQuery.toString)

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
                        return data.Results |> List.map mapRow |> Success |> msg
    }

let loadCountryComparison countries msg =

    let query =
        let countryMatch country =
            ExactSearch(QueryExpression.Column(QueryColumn.UserColumn "iso_code"), country)
        let countriesMatchCondition =
            Or (List.map countryMatch countries)
        { DataQuery.Default with
            Condition = Some(countriesMatchCondition) }

    load query msg


let loadCountryIncidence countries fromDate msg =

    let query =
        let countryMatch country =
            ExactSearch(QueryExpression.Column(QueryColumn.UserColumn "iso_code"), country)
        let countriesMatchCondition =
            Or (List.map countryMatch countries)
        let dateCondition =
            Relation(GreaterThan, QueryExpression.Column(QueryColumn.UserColumn "date"), Constant(ValueString fromDate))
        { DataQuery.Default with
            Condition = Some(And[countriesMatchCondition ; dateCondition]) }

    load query msg
