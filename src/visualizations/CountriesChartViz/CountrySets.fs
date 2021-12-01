module CountriesChartViz.CountrySets

open CountriesChartViz.Analysis
open Synthesis

let setNeighboringCountries =
    { Label = "groupNeighbouring"
      CountriesCodes =
          [| "AUT"
             "CZE"
             "DEU"
             "HRV"
             "HUN"
             "ITA"
             "SVK" |] }

let setHighestNewCases =
    { Label = "groupHighestNewCases"
      CountriesCodes =
          [| "SVK"
             "AUT"
             "CZE"
             "BEL"
             "NLD"
             "HRV"
             "GEO"
             "HUN"
             "IRL"
             "DNK" |] }

let setHighestActiveCases =
    { Label = "groupHighestActiveCases"
      CountriesCodes =
          [| "SVK"
             "AUT"
             "HRV"
             "CZE"
             "GEO"
             "BEL"
             "NLD"
             "IRL"
             "HUN"
             "LTU" |] }

let setHighestNewDeaths =
    { Label = "groupHighestNewDeaths"
      CountriesCodes =
          [| "BGR"
             "GEO"
             "LVA"
             "UKR"
             "ROU"
             "HRV"
             "HUN"
             "ARM"
             "MDA"
             "BIH" |] }

let setHighestTotalDeaths =
    { Label = "groupHighestTotalDeaths"
      CountriesCodes =
          [| "PER"
             "BGR"
             "BIH"
             "MNE"
             "MKD"
             "HUN"
             "CZE"
             "BRA"
             "ROU"
             "GEO" |] }

let setLargestEuCountries =
    { Label = "groupLargestEuCountries"
      CountriesCodes =
          [| "DEU"
             "GBR"
             "FRA"
             "ITA"
             "ESP"
             "POL"
             "ROU"
             "NLD"
             "BEL" |] }

let setLargestWorldCountries =
    { Label = "groupLargestWorldCountries"
      CountriesCodes =
          [| "CHN"
             "IND"
             "USA"
             "IDN"
             "PAK"
             "BRA"
             "NGA"
             "BGD"
             "RUS"
             "MEX"
             "JPN" |] }

let setNordic =
    { Label = "groupNordic"
      CountriesCodes = [| "DNK"; "FIN"; "ISL"; "NOR"; "SWE" |] }

let setExYU =
    { Label = "groupExYu"
      CountriesCodes =
          [| "BIH"
             "HRV"
             "MKD"
             "MNE"
             "OWID_KOS"
             "SRB" |] }

let setEastAsiaOceania =
    { Label = "groupEastAsiaOceania"
      CountriesCodes =
          [| "AUS"
             "CHN"
             "JPN"
             "KOR"
             "NZL"
             "SGP"
             "TWN" |] }

let setLatinAmerica =
    { Label = "groupLatinAmerica"
      CountriesCodes =
          [| "ARG"
             "BRA"
             "CHL"
             "COL"
             "ECU"
             "MEX"
             "PER" |] }

let countriesDisplaySets (metric: MetricToDisplay) =
    match metric with
    | NewCasesPer100k ->
        [| setNeighboringCountries
           setHighestNewCases
           setLargestEuCountries
           setLargestWorldCountries
           setNordic
           setExYU
           setEastAsiaOceania
           setLatinAmerica |]
    | ActiveCasesPer100k ->
        [| setNeighboringCountries
           setHighestActiveCases
           setLargestEuCountries
           setLargestWorldCountries
           setNordic
           setExYU
           setEastAsiaOceania
           setLatinAmerica |]
    | NewDeathsPer100k ->
        [| setNeighboringCountries
           setHighestNewDeaths
           setLargestEuCountries
           setLargestWorldCountries
           setNordic
           setExYU
           setEastAsiaOceania
           setLatinAmerica |]
    | TotalDeathsPer100k ->
        [| setNeighboringCountries
           setHighestTotalDeaths
           setLargestEuCountries
           setLargestWorldCountries
           setNordic
           setExYU
           setEastAsiaOceania
           setLatinAmerica |]
    | DeathsPerCases ->
        [| setNeighboringCountries
           setLargestEuCountries
           setLargestWorldCountries
           setNordic
           setExYU
           setEastAsiaOceania
           setLatinAmerica |]
