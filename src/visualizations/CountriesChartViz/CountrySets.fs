module CountriesChartViz.CountrySets

open CountriesChartViz.Analysis
open Synthesis

let setNeighboringCountries = {
      Label = "groupNeighbouring"
      CountriesCodes = [| "AUT"; "CZE"; "DEU"; "HRV"; "HUN"; "ITA"; "SVK" |]
    }

let setHighestNewCases = {
    Label = "groupHighestNewCases"
    CountriesCodes = [| "SRB"; "GEO"; "MNE"; "HRV"; "LTU"; "HUN"; "SWE"; "AUT"; "USA"; "MKD" |]
}

let setHighestActiveCases = {
    Label = "groupHighestActiveCases"
    CountriesCodes = [| "GEO"; "SRB"; "MNE"; "HRV"; "LTU"; "AUT"; "USA"; "HUN"; "PRT"; "MKD" |]
}

let setHighestNewDeaths = {
    Label = "groupHighestNewDeaths"
    CountriesCodes = [| "BGR"; "MKD"; "BIH"; "HRV"; "HUN"; "POL"; "MNE"; "CZE"; "ITA"; "AUT" |]
}

let setHighestTotalDeaths = {
    Label = "groupHighestTotalDeaths"
    CountriesCodes = [| "BEL"; "PER"; "ESP"; "ITA"; "GBR"; "ARG"; "MEX"; "MKD"; "BRA"; "USA" |]
}

let setLargestEuCountries = {
    Label = "groupLargestEuCountries"
    CountriesCodes = [| "DEU"; "GBR"; "FRA"; "ITA"; "ESP"; "POL"; "ROU"; "NLD"; "BEL" |]
}

let setLargestWorldCountries = {
    Label = "groupLargestWorldCountries"
    CountriesCodes = [| "CHN"; "IND"; "USA"; "IDN"; "PAK"; "BRA"; "NGA"; "BGD"; "RUS"; "MEX"; "JPN" |]
}

let setNordic = {
    Label = "groupNordic"
    CountriesCodes = [| "DNK"; "FIN"; "ISL"; "NOR"; "SWE" |]
}

let setExYU = {
    Label = "groupExYu"
    CountriesCodes = [| "BIH"; "HRV"; "MKD"; "MNE"; "OWID_KOS"; "SRB" |]
}

let setEastAsiaOceania = {
    Label = "groupEastAsiaOceania"
    CountriesCodes = [| "AUS"; "CHN"; "JPN"; "KOR"; "NZL"; "SGP"; "TWN" |]
}

let setLatinAmerica = {
    Label = "groupLatinAmerica"
    CountriesCodes = [| "ARG"; "BRA"; "CHL"; "COL"; "ECU"; "MEX"; "PER" |]
}

let countriesDisplaySets (metric: MetricToDisplay) =
    match metric with
    | NewCasesPer1M ->
        [| setNeighboringCountries; setHighestNewCases
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | ActiveCasesPer1M ->
        [| setNeighboringCountries; setHighestActiveCases
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | NewDeathsPer1M ->
        [| setNeighboringCountries; setHighestNewDeaths
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | TotalDeathsPer1M ->
        [| setNeighboringCountries; setHighestTotalDeaths
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | DeathsPerCases ->
        [| setNeighboringCountries
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]

