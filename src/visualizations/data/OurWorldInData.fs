module Data.OurWorldInData

open System
open Fable.SimpleHttp
open Fable.Extras.Web

open Types

let apiUrl = "https://api.sledilnik.org/api/owid"

type CountryIsoCode = string

type DataPoint = {
    CountryCode : CountryIsoCode
    Date : DateTime
    NewCases : int
    NewCasesPerMillion : float option
    TotalCases : int
    TotalCasesPerMillion : float option
    TotalDeaths : int
    TotalDeathsPerMillion : float option
}

[<RequireQualifiedAccess>]
type CountrySelection =
    | All
    | Selected of CountryIsoCode list

type Query =
    { DateFrom : DateTime option
      DateTo : DateTime option
      Countries : CountrySelection
    }

    with

    member this.URLSearchParams =
        [ this.DateFrom |> Option.map (fun date-> ("from", date.ToString("yyyy-MM-dd")))
          this.DateTo |> Option.map (fun date-> ("to", date.ToString("yyyy-MM-dd")))
          match this.Countries with
            | CountrySelection.All -> None
            | CountrySelection.Selected countries -> Some ("countries", (String.Join(",", countries))) ]
        |> List.choose id
        |> JSe.URLSearchParams

type OurWorldInDataRemoteData = RemoteData<DataPoint list, string>

let parseInt = Utils.nativeParseInt >> Utils.optionToInt

let parseFloat = Utils.nativeParseFloat

let loadData (query : Query) msg =
    async {
        let url = JSe.URL apiUrl
        url.Search <- query.URLSearchParams.ToString()

        let! response =
            Http.request (url.ToString())
            |> Http.method GET
            |> Http.header (Headers.accept "text/csv")
            |>Http.send

        match response.statusCode = 200 with
        | false ->
            return sprintf
                "Napaka pri nalaganju OurWorldInData podatkov: %d" response.statusCode |> Failure |> msg
        | true ->
            let csv = response.content.ToString()
            let data =
                csv.Split("\n").[1..]
                |> Array.map (fun rowString ->
                    let row = rowString.Split(";")
                    try
                        Some {
                            Date = DateTime.Parse(row.[0])
                            CountryCode = row.[1]
                            NewCases = parseInt row.[2]
                            NewCasesPerMillion = parseFloat row.[3]
                            TotalCases = parseInt row.[4]
                            TotalCasesPerMillion = parseFloat row.[5]
                            TotalDeaths = parseInt row.[6]
                            TotalDeathsPerMillion = parseFloat row.[7]
                        }
                    with
                    | _ -> None
                )
                |> Array.choose id
                |> Array.toList

            return data |> Success |> msg
    }
