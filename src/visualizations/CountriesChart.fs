[<RequireQualifiedAccess>]
module CountriesChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Browser

open Highcharts
open Types
open Data.OurWorldInData

type DailyData = {
    Day: DateTime
    TotalDeathsPerMillion: float
}

type CountryIsoCode = string

type CountryData = {
    CountryIsoCode: CountryIsoCode
    CountryName: string
    Data: DailyData[]
}

type CountriesData = Map<CountryIsoCode, CountryData>

type CountriesSelection =
    | Scandinavia

type State = {
    Data: CountriesData
    OurWorldInData: Data.OurWorldInData.Data
    DisplayedCountries: CountriesSelection
}

let ColorPalette =
    [ "#ffa600"
      "#dba51d"
      "#afa53f"
      "#777c29"
      "#70a471"
      "#457844"
      "#f95d6a"
      "#d45087"
      "#a05195"
      "#665191"
      "#10829a"
      "#024a66" ]

type Msg =
    | DataRequested
    | DataLoaded of Data.OurWorldInData.Data
    | ChangeCountriesSelection of CountriesSelection

let parseCountriesCsv(): CountriesData =

    let countriesData =
        Data.CountriesData.countriesRawData
        |> Seq.map (fun entry ->
            let (countryIsoCode, countryName, dateStr, deathsPerMillion)
                = entry

            let date = DateTime.Parse(dateStr)

            (countryIsoCode, countryName, date, deathsPerMillion))
        |> Seq.sortBy (fun (isoCode, _, _, _) -> isoCode)
        |> Seq.groupBy (fun (isoCode, countryName, _, _)
                         -> (isoCode, countryName))
        |> Seq.map (fun ((isoCode, countryName), countryLines) ->
            let dailyEntries =
                countryLines
                |> Seq.map(fun (_, _, date, deathsPerMillion) ->
                    { Day = date; TotalDeathsPerMillion = deathsPerMillion })
                |> Seq.toArray
            { CountryIsoCode = isoCode
              CountryName = countryName
              Data = dailyEntries }
            )

    countriesData
    |> Seq.map (fun x -> (x.CountryIsoCode, x))
    |> Map.ofSeq

let init data : State * Cmd<Msg> =
    let state = {
        Data = parseCountriesCsv()
        OurWorldInData = NotAsked
        DisplayedCountries = Scandinavia
    }
    state, Cmd.ofMsg DataRequested

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeCountriesSelection countries ->
        { state with DisplayedCountries=countries }, Cmd.none
    | DataRequested ->
        let countries = ["Slovenia" ; "Italy" ; "Norway"]
        { state with OurWorldInData = Loading }, Cmd.OfAsync.result (Data.OurWorldInData.load countries DataLoaded)
    | DataLoaded remoteData ->

        match remoteData with
        | NotAsked ->
            printfn "Not asked"
        | Loading ->
            printfn "Loading"
        | Failure error ->
            printfn "Error: %s" error
        | Success data ->
            printfn "Success %A" data

        { state with OurWorldInData = remoteData }, Cmd.none

let renderChartOptions (state: State) =
    let myLoadEvent(name: String) =
        let ret(event: Event) =
            let evt = document.createEvent("event")
            evt.initEvent("chartLoaded", true, true);
            document.dispatchEvent(evt)
        ret

    let colorsInPalette = ColorPalette |> List.length
    let countriesCount = state.Data |> Seq.length

    let allSeries =
        state.Data
        |> Map.toArray
        |> Array.mapi (fun countryIndex (_, countryData) ->
             pojo
                {|
                visible = true
                color =
                    let colorIndex =
                        countryIndex * colorsInPalette / countriesCount
                    ColorPalette.[colorIndex]
                name = countryData.CountryName
                data =
                    countryData.Data
                    |> Array.map (fun entry ->
                        ((entry.Day |> jsTime12h), entry.TotalDeathsPerMillion))
                marker = pojo {| enabled = false |}
                |}
            )

    let legend =
        {|
            enabled = true
            title = ""
            align = "left"
            verticalAlign = "top"
            borderColor = "#ddd"
            borderWidth = 1
            //labelFormatter = string //fun series -> series.name
            layout = "vertical"
            floating = true
            x = 20
            y = 30
            backgroundColor = "rgba(255,255,255,0.5)"
            reversed = true
        |}

    let baseOptions = basicChartOptions Linear "covid19-metrics-comparison"
    {| baseOptions with
        chart = pojo
            {|
                ``type`` = "spline"
                zoomType = "x"
                events = {| load = myLoadEvent("countries") |}
            |}
        title = pojo {| text = None |}
        series = allSeries
        xAxis = baseOptions.xAxis |> Array.map (fun ax ->
            {| ax with
                plotBands = shadedWeekendPlotBands
                plotLines = [||]
            |})
        plotOptions = pojo
            {|
                series = pojo {| stacking = ""; |}
            |}
        legend = pojo {| legend with enabled = true |}
    |}

let renderChartContainer state =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state
            |> chart
        ]
    ]

let render state dispatch =
    Html.div [
        renderChartContainer state
//        renderDisplaySelectors state.DisplayType (ChangeDisplayType >> dispatch)

        Html.div [
            prop.className "disclaimer"
        ]
    ]

let renderChart (props : {| data : StatsData |}) =
    React.elmishComponent("CountriesChart", init props.data, update, render)
