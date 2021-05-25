module CountriesChartViz.CountrySets

open CountriesChartViz.Analysis
open Synthesis

let setNeighboringCountries = {
      Label = "groupNeighbouring"
      CountriesCodes = [| "AUT"; "CZE"; "DEU"; "HRV"; "HUN"; "ITA"; "SVK" |]
    }

let setHighestNewCases = {
    Label = "groupHighestNewCases"
    CountriesCodes = [| "BHR"; "URY"; "ARG"; "CRI"; "PRY"; "TTO"; "COL"; "CHL"; "BRA"; "SWE" |]
}

let setHighestActiveCases = {
    Label = "groupHighestActiveCases"
    CountriesCodes = [| "BHR"; "URY"; "ARG"; "CRI"; "PRY"; "SWE"; "TTO"; "COL"; "LTU"; "CHL" |]
}

let setHighestNewDeaths = {
    Label = "groupHighestNewDeaths"
    CountriesCodes = [| "URY"; "PRY"; "ARG"; "TTO"; "COL"; "BRA"; "BHR"; "PER"; "MKD"; "HRV" |]
}

let setHighestTotalDeaths = {
    Label = "groupHighestTotalDeaths"
    CountriesCodes = [| "HUN"; "CZE"; "BIH"; "MKD"; "BGR"; "MNE"; "SVK"; "BEL"; "BRA"; "ITA" |]
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
    | NewCasesPer100k ->
        [| setNeighboringCountries; setHighestNewCases
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | ActiveCasesPer100k ->
        [| setNeighboringCountries; setHighestActiveCases
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | NewDeathsPer100k ->
        [| setNeighboringCountries; setHighestNewDeaths
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | TotalDeathsPer100k ->
        [| setNeighboringCountries; setHighestTotalDeaths
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | DeathsPerCases ->
        [| setNeighboringCountries
           setLargestEuCountries; setLargestWorldCountries
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]

