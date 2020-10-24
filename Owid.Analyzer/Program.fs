// Learn more about F# at http://fsharp.org

open System
open System.Globalization
open System.Net

type Row = string[]

type LatestCountryData = {
    IsoCode: string
    CountryName: string
    NewCasesPerMillion: float
    TotalDeathsPerMillion: float
    NewDeathsPerMillion: float
}
with
    override this.ToString() =
        sprintf "%s,%s,%f,%f,%f" this.IsoCode this.CountryName
            this.NewCasesPerMillion this.TotalDeathsPerMillion
            this.NewDeathsPerMillion

    static member CsvHeader() =
            "iso_code,country_name,new_cases_per_million," +
            "total_deaths_per_million,new_deaths_per_million"

let isNotEmpty (text: string) = not(String.IsNullOrWhiteSpace(text))

let parseFloat (value: string) =
    match value with
    | "" -> 0.
    | value -> Double.Parse(value, CultureInfo.InvariantCulture)

let parseCsvLine (line: string) : Row =
    line.Split(",")

let getLatestCountryData (countryData: (string * Row[])) =
    let (iso_code, countryRows) = countryData
    let lastRow = countryRows.[countryRows.Length - 1]
    let location = lastRow.[2]
    let newCasesPerMillion = lastRow.[10]
    let totalDeathsPerMillion = lastRow.[12]
    let newDeathsPerMillion = lastRow.[13]

    { IsoCode = iso_code
      CountryName = location
      NewCasesPerMillion = newCasesPerMillion |> parseFloat
      TotalDeathsPerMillion = totalDeathsPerMillion |> parseFloat
      NewDeathsPerMillion = newDeathsPerMillion |> parseFloat }

[<EntryPoint>]
let main argv =
    let csvUrl =
        "https://github.com/owid/covid-19-data/raw/master/public/data/" +
        "owid-covid-data.csv"

    let client = new WebClient()
    let csvContent = client.DownloadString(csvUrl)

    let csvLines = csvContent.Split("\n")

    let header = csvLines.[0]
    let propertyNames = header.Split(",")

    let dataLines = csvLines |> Array.skip(1)
    let dataRows =
        dataLines
        |> Array.filter (fun line -> line |> isNotEmpty )
        |> Array.map parseCsvLine


    let rowsByCountries =
        dataRows
        |> Array.groupBy (fun rows -> rows.[0])
        |> Array.filter (fun (iso_code, _) -> iso_code |> isNotEmpty)

    let countriesLatestData =
        rowsByCountries
        |> Array.map getLatestCountryData

    printf "%s\n" (LatestCountryData.CsvHeader())
    countriesLatestData
    |> Array.iter
        (fun latestData ->
            printf "%s\n" (latestData.ToString())
            ignore())

    0
