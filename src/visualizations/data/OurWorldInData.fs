module Data.OurWorldInData

open System
open Fable.SimpleHttp
open Fable.Extras.Web

open Types

//let apiUrl = "https://api.sledilnik.org/api/owid"
let apiUrl = "https://api-stage.sledilnik.org/api/owid"

type CountryIsoCode = string

type DataPoint = {
    CountryCode : CountryIsoCode
    Date : DateTime
    NewCases : int option
    NewCasesPerMillion : float option
    TotalCases : int option
    TotalCasesPerMillion : float option
    NewDeaths : int option
    NewDeathsPerMillion : float option
    TotalDeaths : int option
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
        [ Some ("columns",
                "new_cases,new_cases_per_million," +
                "total_cases,total_cases_per_million," +
                "new_deaths,new_deaths_per_million" +
                "total_deaths,total_deaths_per_million")
          this.DateFrom |> Option.map (fun date-> ("from", date.ToString("yyyy-MM-dd")))
          this.DateTo |> Option.map (fun date-> ("to", date.ToString("yyyy-MM-dd")))
          match this.Countries with
            | CountrySelection.All -> None
            | CountrySelection.Selected countries -> Some ("countries", (String.Join(",", countries))) ]
        |> List.choose id
        |> JSe.URLSearchParams

type OurWorldInDataRemoteData = RemoteData<DataPoint list, string>

let parseInt = Utils.nativeParseInt

let parseFloat = Utils.nativeParseFloat

let findIndexOfColumn columnName (csvColumns: string[]): int =
    csvColumns
    |> Array.findIndex(fun currentColumnName
                        ->  currentColumnName.Equals(columnName))

let loadData (query : Query) msg =
    async {
        let url = JSe.URL apiUrl
        url.Search <- query.URLSearchParams.ToString()

        let! response =
            Http.request (url.ToString())
            |> Http.method GET
            |> Http.header (Headers.accept "text/csv")
            |> Http.send

        match response.statusCode = 200 with
        | false ->
            return sprintf
                "Napaka pri nalaganju OurWorldInData podatkov: %d"
                response.statusCode |> Failure |> msg
        | true ->
            let csv = response.content.ToString()
            let data =
                let csvLines = csv.Split("\n")
                let csvHeader = csvLines.[0]

                // for some strange reason, the CSV header line we get starts
                // with "Text ", so we have to get rid of that.
                let csvHeaderCleaned =
                    if csvHeader.StartsWith("Text ") then
                        csvHeader.Substring(5)
                    else
                        csvHeader

                let csvColumns = csvHeaderCleaned.Split(";")

                let countryCodeIndex = csvColumns |> findIndexOfColumn "isoCode"
                let dateIndex = csvColumns |> findIndexOfColumn "date"
                let newCasesIndex = csvColumns |> findIndexOfColumn "new_cases"
                let newCasesPerMIndex =
                    csvColumns |> findIndexOfColumn "new_cases_per_million"
                let totalCasesIndex =
                    csvColumns |> findIndexOfColumn "total_cases"
                let totalCasesPerMIndex =
                    csvColumns |> findIndexOfColumn "total_cases_per_million"
                let newDeathsIndex =
                    csvColumns |> findIndexOfColumn "new_deaths"
                let newDeathsPerMIndex =
                    csvColumns |> findIndexOfColumn "new_deaths_per_million"
                let totalDeathsIndex =
                    csvColumns |> findIndexOfColumn "total_deaths"
                let totalDeathsPerMIndex =
                    csvColumns |> findIndexOfColumn "total_deaths_per_million"

                csvLines.[1..]
                |> Array.map (fun rowString ->
                    let row = rowString.Split(";")
                    try
                        Some {
                            Date = DateTime.Parse(row.[dateIndex])
                            CountryCode = row.[countryCodeIndex]
                            NewCases = parseInt row.[newCasesIndex]
                            NewCasesPerMillion =
                                parseFloat row.[newCasesPerMIndex]
                            TotalCases = parseInt row.[totalCasesIndex]
                            TotalCasesPerMillion =
                                parseFloat row.[totalCasesPerMIndex]
                            NewDeaths = parseInt row.[newDeathsIndex]
                            NewDeathsPerMillion =
                                parseFloat row.[newDeathsPerMIndex]
                            TotalDeaths = parseInt row.[totalDeathsIndex]
                            TotalDeathsPerMillion =
                                parseFloat row.[totalDeathsPerMIndex]
                        }

                    with
                    | _ -> None
                )
                |> Array.choose id
                |> Array.toList

            return data |> Success |> msg
    }
