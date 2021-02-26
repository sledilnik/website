// Learn more about F# at http://fsharp.org

open System
open System.Globalization
open System.Net

type Row = string[]

type LatestCountryData = {
    IsoCode: string
    CountryName: string
    Continent: string
    Population: float
    NewCasesPer100k: float
    ActiveCasesPer100k: float
    TotalDeathsPer100k: float
    NewDeathsPer100k: float
}
with
    override this.ToString() =
        sprintf "%s,%s,%s,%f,%f,%f,%f,%f"
            this.IsoCode this.CountryName this.Continent
            this.Population
            this.NewCasesPer100k
            this.ActiveCasesPer100k
            this.TotalDeathsPer100k
            this.NewDeathsPer100k

    static member CsvHeader() =
            "iso_code,country_name,continent,population," +
            "new_cases_per_100k,active_cases_per_100k," +
            "total_deaths_per_100k,new_deaths_per_100k"

let isNotEmpty (text: string) = not(String.IsNullOrWhiteSpace(text))

let parseFloat (value: string) =
    match value with
    | "" -> 0.
    | value -> Double.Parse(value, CultureInfo.InvariantCulture)

let parseInt (value: string) =
    match value with
    | "" -> 0
    | value ->
        try
            Int32.Parse(value, CultureInfo.InvariantCulture)
        with
        | ex ->
            printf "invalid value: %s\n" value
            raise ex


let parseCsvLine (line: string) : Row =
    line.Split(",")


let getLatestCountryData (countryData: (string * Row[])) =
    let (iso_code, countryRows) = countryData

    if countryRows.Length >= 14 then
        let rowsLast14Days = countryRows |> Array.rev |> Array.take 14
        let rowsLast7Days = countryRows |> Array.rev |> Array.take 7

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
        let continent = lastRow.[1]
        let population = lastRow.[44] |> parseFloat

        { IsoCode = iso_code
          CountryName = location
          Continent = continent
          Population = population
          ActiveCasesPer100k = activeCasesPer100k
          NewCasesPer100k = newCasesPer100k
          TotalDeathsPer100k = totalDeathsPer100k
          NewDeathsPer100k = newDeathsPer100k } |> Some
    else
        None

let dumpCsvPropertyNames (csvLines: string[]) =
    let header = csvLines.[0]
    let propertyNames = header.Split(",")
    propertyNames
    |> Array.iteri (fun i property -> printf "%d %s\n" i property)


let fetchCountriesLatestData() =
    let csvUrl =
        "https://github.com/owid/covid-19-data/raw/master/public/data/" +
        "owid-covid-data.csv"

    let client = new WebClient()
    let csvContent = client.DownloadString(csvUrl)

    let csvLines = csvContent.Split("\n")

//    csvLines |> dumpCsvPropertyNames

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

    countriesLatestData


let renderCsvFile countriesLatestData =
    printf "%s\n" (LatestCountryData.CsvHeader())
    countriesLatestData
    |> Array.iter
        (fun latestData ->
            printf "%s\n" (latestData.ToString())
            ignore())
    0


let renderHighestCountriesCodes label orderBy
        (countriesLatestData: LatestCountryData[]) =
    printfn label

    let highestIsoCodes =
        countriesLatestData
        |> Array.sortByDescending orderBy
        // filter out smaller countries, but include Montenegro
        |> Array.filter (fun country -> country.Population > 600000.)
        |> Array.map (fun country -> country.IsoCode)
        |> Array.filter (fun isoCode -> isoCode <> "SVN")
        |> Array.take 10
        |> Array.map (fun isoCode -> "\"" + isoCode + "\"")

    let line = "[| " + String.Join("; ", highestIsoCodes) + " |]"
    printfn "%s" line
    printfn ""

    countriesLatestData

[<EntryPoint>]
let main args =
    let countriesLatestData = fetchCountriesLatestData()

    if args |> Array.length = 0 then
        countriesLatestData |> renderCsvFile
    else
        match args.[0] with
        | "highest" ->
            countriesLatestData
            |> renderHighestCountriesCodes
                   "By new cases / 100k:"
                   (fun country -> country.NewCasesPer100k)
            |> renderHighestCountriesCodes
                   "By active cases / 100k:"
                   (fun country -> country.ActiveCasesPer100k)
            |> renderHighestCountriesCodes
                   "By new deaths / 100k:"
                   (fun country -> country.NewDeathsPer100k)
            |> renderHighestCountriesCodes
                   "By total deaths / 100k:"
                   (fun country -> country.TotalDeathsPer100k)
            |> ignore
            0
        | _ ->
            printfn "Unknown command."
            1
