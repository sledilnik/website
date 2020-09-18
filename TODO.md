# TODO

## Optimizations
- extract getting the data to a common module

- use ETag for comparison of data
    - how to get it?
        - replace existing Http.get with something that provides access
            to response headers
        - Fable.SimpleHttp.Http.get
    - wrap loaded data in a generic class so ETag is always present 

## New countries chart
- deaths per cases (windowed, averaged)

## Age groups timeline - active cases
- tooltip should include sum + percentages of each group

## World page
- each countries chart should have its own sets of countries
- charts:
    - deaths per cases
        - show as percentages
        - add SI descriptions
    - current deaths per 1M

- can we reuse OWID data across charts?
    - locking
    - caching multiple country sets?
