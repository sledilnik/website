# TODO

## Countries comparison

- remove OurWorldInData tests
- fetch data from REST instead of built-in
    - remove Data.CountriesData.countriesRawData
    - search by country ISO code, not name
    - specify country sets and their country codes
- show day numbers instead of dates on the graph
    - calculate day 0 when deaths/1 M reach certain value
    - still record dates, but separately for each country
        - show these dates in tooltips
- skip values of 0 (make None)
