module CountriesChartViz.CountrySets

open CountriesChartViz.Analysis
open Synthesis

let setNeighboringCountries = {
      Label = "groupNeighbouring"
      CountriesCodes = [| "AUT"; "CZE"; "DEU"; "HRV"; "HUN"; "ITA"; "SVK" |]
    }

let setHighestNewCasesEU = {
    Label = "groupHighestNewCasesEU"
    CountriesCodes = [| "BEL"; "ESP"; "CZE"; "FRA"; "NLD"; "ISL"; "MKD"; "CHE"|]
}

let setHighestActiveCasesEU = {
    Label = "groupHighestActiveCasesEU"
    CountriesCodes = [| "CZE"; "BEL"; "NLD"; "SVN"; "FRA"; "CHE"; "ESP"; "SVK"; "GBR"|]
}

let setCriticalEU = {
    Label = "groupCriticalEU"
    CountriesCodes = [| "BEL"; "ESP"; "FRA"; "GBR"; "ITA"; "SWE" |]
}

let setHighestActiveCasesWorld = {
    Label = "groupHighestActiveCasesWorld"
    CountriesCodes = [| "CZE"; "BEL"; "NLD"; "ARM"; "SVN"; "FRA"; "CHE"; "ARG"; "ESP" |]
}

let setCriticalWorld = {
    Label = "groupCriticalWorld"
    CountriesCodes = [| "BRA"; "ECU"; "ITA"; "RUS"; "SWE"; "USA" |]
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
        [| setNeighboringCountries; setHighestNewCasesEU; setCriticalWorld
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | ActiveCasesPer1M ->
        [| setNeighboringCountries; setHighestActiveCasesEU
           setHighestActiveCasesWorld
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | TotalDeathsPer1M ->
        [| setNeighboringCountries; setCriticalEU; setCriticalWorld
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]
    | DeathsPerCases ->
        [| setNeighboringCountries; setCriticalEU; setCriticalWorld
           setNordic; setExYU; setEastAsiaOceania; setLatinAmerica
        |]

