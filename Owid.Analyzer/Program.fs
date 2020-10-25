// Learn more about F# at http://fsharp.org

open System
open System.Globalization
open System.Net

type Row = string[]

type LatestCountryData = {
    IsoCode: string
    CountryName: string
    NewCasesPer100k: float
    ActiveCasesPer100k: float
    TotalDeathsPer100k: float
    NewDeathsPer100k: float
}
with
    override this.ToString() =
        sprintf "%s,%s,%f,%f,%f,%f" this.IsoCode this.CountryName
            this.NewCasesPer100k
            this.ActiveCasesPer100k
            this.TotalDeathsPer100k
            this.NewDeathsPer100k

    static member CsvHeader() =
            "iso_code,country_name,new_cases_per_million," +
            "active_cases_per_million," +
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

    if countryRows.Length >= 14 then
        let rowsLast14Days = countryRows |> Array.rev |> Array.take 14
        let rowsLast7Days = countryRows |> Array.rev |> Array.take 14

        let activeCasesPer100k =
            (rowsLast14Days
             |> Array.sumBy (fun row -> row.[11] |> parseFloat)) / 10.

        let newCasesPer100k =
            (rowsLast7Days
             |> Array.sumBy (fun row -> row.[11] |> parseFloat)) / 70.

        let totalDeathsPer100k =
            (rowsLast7Days
             |> Array.sumBy (fun row -> row.[13] |> parseFloat)) / 70.

        let newDeathsPer100k =
            (rowsLast7Days
             |> Array.sumBy (fun row -> row.[14] |> parseFloat)) / 70.

        let lastRow = countryRows.[countryRows.Length - 1]
        let location = lastRow.[2]

        { IsoCode = iso_code
          CountryName = location
          ActiveCasesPer100k = activeCasesPer100k
          NewCasesPer100k = newCasesPer100k
          TotalDeathsPer100k = totalDeathsPer100k
          NewDeathsPer100k = newDeathsPer100k } |> Some
    else
        None

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

//    propertyNames
//    |> Array.iteri (fun i property -> printf "%d %s\n" i property)

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
        |> Array.choose id

    printf "%s\n" (LatestCountryData.CsvHeader())
    countriesLatestData
    |> Array.iter
        (fun latestData ->
            printf "%s\n" (latestData.ToString())
            ignore())

    0
