module Highcharts

open System
open Fable.Core
open Fable.React
open Browser

open Types

[<Import("renderChart", from="./_highcharts")>]
let chart: obj -> ReactElement = jsNative

[<Import("renderChartFromWindow", from="./_highcharts")>]
let chartFromWindow: obj -> ReactElement = jsNative

[<Import("renderMap", from="./_highcharts")>]
let map: obj -> ReactElement = jsNative

[<Import("sparklineChart", from="./_highcharts")>]
let sparklineChart (documentElementId : string, options : obj) : unit = jsNative

[<AutoOpen>]
module Helpers =
    // Plain-Old-Javascript-Object (i.e. box)
    let inline pojo o = JsInterop.toPlainJsObj o

    // plain old javascript object
    [<Emit """Array.prototype.slice.call($0)""">]
    let poja (a: 'T[]) : obj = jsNative

    type JsTimestamp = float

    [<Emit("$0.getTime()")>]
    let jsTime (x: DateTime): JsTimestamp = jsNative

    let jsNoon : JsTimestamp = 43200000.0

    let jsTime12h = jsTime >> ( + ) jsNoon

    [<Emit("(new Date($0.getFullYear(), $0.getMonth(), $0.getDate())).getTime()")>]
    let jsTimeMidnight (x: DateTime): JsTimestamp = jsNative

    /// Given two dates it calculates the middle point between the midnight for the first date and end of day for the second date
    let jsDatesMiddle (a: DateTime) (b: DateTime): JsTimestamp = ( + ) (0.5 * jsTimeMidnight a) (0.5 * jsTimeMidnight b) + 43200000.0

type DashStyle =
    | Solid
    | ShortDash
    | ShortDot
    | ShortDashDot
    | ShortDashDotDot
    | Dot
    | Dash
    | LongDash
    | DashDot
    | LongDashDot
    | LongDashDotDot
  with
    static member toString = function
        | Solid -> "Solid"
        | ShortDash -> "ShortDash"
        | ShortDot -> "ShortDot"
        | ShortDashDot -> "ShortDashDot"
        | ShortDashDotDot -> "ShortDashDotDot"
        | Dot -> "Dot"
        | Dash -> "Dash"
        | LongDash -> "LongDash"
        | DashDot -> "DashDot"
        | LongDashDot -> "LongDashDot"
        | LongDashDotDot -> "LongDashDotDot"


let shadedWeekendPlotBands =
    let saturday = DateTime(2020, 02, 22)
    let nWeeks = (DateTime.Today-saturday).TotalDays / 7.0 |> int
    let oneDay = 86400000.0
    let origin = jsTime saturday
    [|
        for i in 0..nWeeks+2 do
            yield
                {|
                    ``from`` = origin + oneDay * 7.0 * float i
                    ``to`` = origin + oneDay * 7.0 * float i + oneDay * 2.0
                    color = "rgb(0,0,0,0.025)"
                    label = None
                |}
    |]

// if set to true:
// - SpreadChart will show exponential growth pages
let showExpGrowthFeatures =
    false

let addContainmentMeasuresFlags
    (startDate: JsTimestamp)
    (endDate: JsTimestamp option) =
    let events = [|
    // day, mo, color,    i18n
        4,  3, 2020, "#FFFFFF", "firstCase"
        6,  3, 2020, "#FFe6e6", "retirementHomes"
        8,  3, 2020, "#FFFFFF", "checkpoints"
        10, 3, 2020, "#FFe6e6", "borders"
        12, 3, 2020, "#FFFFFF", "epidemic"
        14, 3, 2020, "#FFe6e6", "publicTransport"
        16, 3, 2020, "#FFe6e6", "schools"
        20, 3, 2020, "#FFe6e6", "gatherings"
        30, 3, 2020, "#FFe6e6", "municipality"
        4,  4, 2020, "#e6f0ff", "shops"
        12, 4, 2020, "#FFe6e6", "quarantine"
        18, 4, 2020, "#ebfaeb", "liftVacationHomes"
        20, 4, 2020, "#ebfaeb", "liftService"
        21, 4, 2020, "#FFFFFF", "nationalStudy"
        29, 4, 2020, "#ebfaeb", "liftMuseums"
        30, 4, 2020, "#ebfaeb", "liftMunicipality"
        4,  5, 2020, "#ebfaeb", "liftFoodMarkets"
        11, 5, 2020, "#ebfaeb", "liftPublicTransport"
        15, 5, 2020, "#ebfaeb", "liftQuarantine"
        18, 5, 2020, "#ebfaeb", "liftSchools1to3"
        26, 5, 2020, "#FFe6e6", "quarantine14days"
        1,  6, 2020, "#ebfaeb", "liftSchools4to5"
        15, 6, 2020, "#ebfaeb", "liftGatherings500"
        19, 6, 2020, "#FFe6e6", "quarantineStrict"
        30, 6, 2020, "#FFe6e6", "gatherings50"
        9,  7, 2020, "#FFe6e6", "gatherings10"
        17, 8, 2020, "#FFFFFF", "app"
        21, 8, 2020, "#FFe6e6", "croatia"
        1,  9, 2020, "#ebfaeb", "allSchools"
        13, 9, 2020, "#ebfaeb", "quarantine10days"
        19, 9, 2020, "#FFe6e6", "masks"
        28, 9, 2020, "#FFe6e6", "testOrQuarantine"
        7, 10, 2020, "#FFe6e6", "cnk"
        9, 10, 2020, "#FFe6e6", "gatherings10max"
        16,10, 2020, "#FFe6e6", "regions"
        19,10, 2020, "#FFe6e6", "schools6+"
        20,10, 2020, "#FFe6e6", "movement"
        24,10, 2020, "#FFe6e6", "restaurants"
        27,10, 2020, "#FFe6e6", "municipality2"
        31,10, 2020, "#ebfaeb", "liftLibraries"
        6, 11, 2020, "#ebfaeb", "liftShops"
        13,11, 2020, "#FFe6e6", "gatherings2"
        16,11, 2020, "#FFe6e6", "services"
        3, 12, 2020, "#FFFFFF", "planRelaxation"
        7, 12, 2020, "#FFe6e6", "alcoDrinks"
        15,12, 2020, "#ebfaeb", "liftServices"
        19,12, 2020, "#ebfaeb", "liftReligiousCeremonies"
        24,12, 2020, "#FFe6e6", "services2"
        28,12, 2020, "#ebfaeb", "liftFoodMarkets2"
        4,  1, 2021, "#ebfaeb", "liftGym"
        8,  1, 2021, "#FFFFFF", "planRelaxation2"
        9,  1, 2021, "#FFe6e6", "gym"
        23, 1, 2021, "#ebfaeb", "liftRegions"
        26, 1, 2021, "#ebfaeb", "liftSchools3"
        1,  2, 2021, "#FFe6e6", "closeSchools3"
        5,  2, 2021, "#FFe6e6", "bordersStrict"
        6,  2, 2021, "#ebfaeb", "liftShops2"
        9,  2, 2021, "#ebfaeb", "liftSchools3All"
        13, 2, 2021, "#ebfaeb", "liftBorders"
        15, 2, 2021, "#ebfaeb", "liftMunicipality2"
        27, 2, 2021, "#FFe6e6", "okRed"
        8,  3, 2021, "#ebfaeb", "liftHighSchools"
        20, 3, 2021, "#ebfaeb", "liftOkPn"
        26, 3, 2021, "#FFe6e6", "okGoKoRed"
        29, 3, 2021, "#FFe6e6", "bordersClosed"
        1,  4, 2021, "#FFe6e6", "elevenDays"
        4,  4, 2021, "#ebfaeb", "liftEaster"
        10, 4, 2021, "#ebfaeb", "liftMasksOutside"
        12, 4, 2021, "#ebfaeb", "liftMovement"
        19, 4, 2021, "#ebfaeb", "liftTerrace"
        23, 4, 2021, "#ebfaeb", "liftRegions2"
        24, 4, 2021, "#ebfaeb", "liftTerrace2"
        26, 4, 2021, "#ebfaeb", "liftSport"
        10, 5, 2021, "#ebfaeb", "liftHotels"
        15, 5, 2021, "#ebfaeb", "liftGatherings50"
        17, 5, 2021, "#ebfaeb", "liftHighSchools2"
        22, 5, 2021, "#ebfaeb", "liftGatherings"
        7,  6, 2021, "#ebfaeb", "liftCapacity75p"
        21, 6, 2021, "#ebfaeb", "liftServicesAll"
        5,  7, 2021, "#ebfaeb", "liftMasksOnEvents"
        16, 8, 2021, "#FFe6e6", "masksOnEvents"
        23, 8, 2021, "#FFe6e6", "pctSports"
        6,  9, 2021, "#FFe6e6", "pctContacts"
        15, 9, 2021, "#FFe6e6", "pctEverywhere"
        30, 9, 2021, "#ebfaeb", "liftMasksRV"
        8, 11, 2021, "#FFe6e6", "gatherings3"
        19, 2, 2022, "#ebfaeb", "liftQuarantineAll"
        21, 2, 2022, "#ebfaeb", "liftPCT"
        7,  3, 2022, "#ebfaeb", "liftMasksSchools"
        14, 4, 2022, "#ebfaeb", "liftMasksInside"
        31, 5, 2022, "#ebfaeb", "liftEverything"
        1,  4, 2023, "#ebfaeb", "noHospitalData"
    |]
    {|
        ``type`` = "flags"
        shape = "flag"
        showInLegend = false
        color = "#444"
        data =
            events |> Array.choose (fun (d,m,y,color,i18n) ->
                let ts = DateTime(y,m,d) |> jsTime
                let showMeasure =
                    match startDate, endDate with
                    | startDate, None -> ts >= startDate
                    | startDate, Some endDate ->
                        ts >= startDate && ts <= endDate

                let title = "cm." + i18n + ".title"
                let text = "cm." + i18n + ".description"
                if showMeasure then
                    Some {| x=ts;fillColor=color; title=I18N.t title; text=I18N.t text |}
                else None
            )
    |}

(* Trigger document event for iframe resizing *)
let onLoadEvent (name : String) =
    let res (e : Event) =
        let event = document.createEvent("Event")
        event.initEvent("chartLoaded", true, true)
        document.dispatchEvent(event)
    res

let optionsWithOnLoadEvent (className : string) =
    {| chart = pojo
        {| events = pojo
            {| load = onLoadEvent(className) |}
        |}
    |}

let parseDate (value: String) =
    match I18N.t "charts.common.numDateFormat" with
    | "%m/%d/%Y" -> // EN, ME
        let date = value.Replace(" ", "").Split('/')
        DateTime(date.[2] |> int, date.[0] |> int, date.[1] |> int)
            .Subtract(DateTime(1970,1,1))
            .TotalMilliseconds
    | "%d/%m/%Y" -> // IT
        let date = value.Replace(" ", "").Split('/')
        DateTime(date.[2] |> int, date.[1] |> int, date.[0] |> int)
            .Subtract(DateTime(1970,1,1))
            .TotalMilliseconds
    | _ -> // DE, HR, MK, SL, SQ
        let date = value.Replace(" ", "").Split('.')
        DateTime(date.[2] |> int, date.[1] |> int, date.[0] |> int)
            .Subtract(DateTime(1970,1,1))
            .TotalMilliseconds

let configureRangeSelector selectedRangeSelectionButtonIndex buttons =
           pojo {|
                enabled = true
                allButtonsEnabled = true
                selected = selectedRangeSelectionButtonIndex
                inputDateFormat = I18N.t "charts.common.numDateFormat"
                inputEditDateFormat = I18N.t "charts.common.numDateFormat"
                inputDateParser = parseDate
                x = 0
                inputBoxBorderColor = "#ced4da"
                buttonTheme = pojo {| r = 6; states = pojo {| select = pojo {| fill = "#ffd922" |} |} |}
                buttons = buttons
            |}

let urlNijzCovid = "https://nijz.si/nalezljive-bolezni/koronavirus/spremljanje-okuzb-s-sars-cov-2-covid-19/"
let urlMinistrstvoZaZdravje = "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-zdravje/"

let chartCreditsNIJZ =
    {|
        enabled = true
        text = sprintf "%s: %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsNIJZ") {| context = localStorage.getItem ("contextCountry") |})
        href = urlNijzCovid
    |} |> pojo

let chartCreditsNIJZNLZOH =
    {| enabled = true
       text = sprintf "%s: %s, %s"
            (I18N.t "charts.common.dataSource")
            (I18N.tOptions ("charts.common.dsNIJZ") {| context = localStorage.getItem ("contextCountry") |})
            (I18N.tOptions ("charts.common.dsNLZOH") {| context = localStorage.getItem ("contextCountry") |})
       href = "https://modeliranje.nijz.si/epivode/epivode-c19.html#metodologija-in-interpretacija"
    |} |> pojo

let chartCreditsNIJZMZHospitals =
    {| enabled = true
       text = sprintf "%s: %s, %s, %s"
            (I18N.t "charts.common.dataSource")
            (I18N.tOptions ("charts.common.dsNIJZ") {| context = localStorage.getItem ("contextCountry") |})
            (I18N.tOptions ("charts.common.dsMZ") {| context = localStorage.getItem ("contextCountry") |})
            (I18N.tOptions ("charts.common.dsHospitals") {| context = localStorage.getItem ("contextCountry") |})
       href = urlNijzCovid
    |} |> pojo

let chartCreditsMZ =
    {|
        enabled = true
        text = sprintf "%s: %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsMZ") {| context = localStorage.getItem ("contextCountry") |})
        href = urlMinistrstvoZaZdravje
    |} |> pojo

let chartCreditsMZHospitals =
    {| enabled = true
       text = sprintf "%s: %s, %s"
            (I18N.t "charts.common.dataSource")
            (I18N.tOptions ("charts.common.dsMZ") {| context = localStorage.getItem ("contextCountry") |})
            (I18N.tOptions ("charts.common.dsHospitals") {| context = localStorage.getItem ("contextCountry") |})
       href = urlMinistrstvoZaZdravje
    |} |> pojo

let chartCreditsHospitals =
    {|
        enabled = true
        text = sprintf "%s: %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsHospitals") {| context = localStorage.getItem ("contextCountry") |})
        href = "https://www.gov.si/teme/mreza-javne-zdravstvene-sluzbe/"
    |} |> pojo

let chartCreditsMIZS =
    {|
        enabled = true
        text = sprintf "%s: %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsMIZS") {| context = localStorage.getItem ("contextCountry") |})
        href = "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-izobrazevanje-znanost-in-sport/"
    |} |> pojo

let chartCreditsMNZ =
    {|
        enabled = true
        text = sprintf "%s: %s"
                    (I18N.t "charts.common.dataSource")
                    (I18N.tOptions ("charts.common.dsMNZ") {| context = localStorage.getItem ("contextCountry") |})
        href = "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-notranje-zadeve/"
    |} |> pojo

let basicChart
    (scaleType:ScaleType)
    (className:string)
    =
    {|
        chart = pojo
            {|
                animation = false
                ``type`` = "line"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
        title = pojo {| text = None |}
        xAxis = [|
            {|
                index=0; crosshair=true; ``type``="datetime"
                gridLineWidth=1
                gridZIndex = -1
                tickInterval=86400000
                labels = pojo {| align = "center"; y = 30; reserveSpace = true; distance = -20; |}
                plotLines=[|
                    {| value=jsTime <| DateTime(2020,3,13); label=Some {| text=I18N.t "phase.2.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,3,20); label=Some {| text=I18N.t "phase.3.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,4,8);  label=Some {| text=I18N.t "phase.4.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,4,15); label=Some {| text=I18N.t "phase.5.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,4,21); label=Some {| text=I18N.t "phase.6.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,5,15); label=Some {| text=I18N.t "phase.7.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,9,10); label=Some {| text=I18N.t "phase.8.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,10,9); label=Some {| text=I18N.t "phase.9.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,10,17);label=Some {| text=I18N.t "phase.10.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,10,19);label=Some {| text=I18N.t "phase.11.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,10,26);label=Some {| text=I18N.t "phase.12.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,11,6); label=Some {| text=I18N.t "phase.13.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2020,12,21);label=Some {| text=I18N.t "phase.14.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2021,2,1);  label=Some {| text=I18N.t "phase.15.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2021,2,13); label=Some {| text=I18N.t "phase.16.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2021,6,15); label=Some {| text=I18N.t "phase.17.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2021,8,23); label=Some {| text=I18N.t "phase.18.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2021,11,8); label=Some {| text=I18N.t "phase.19.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2022,1,19); label=Some {| text=I18N.t "phase.20.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2022,2,1);  label=Some {| text=I18N.t "phase.21.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2022,2,21); label=Some {| text=I18N.t "phase.22.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                    {| value=jsTime <| DateTime(2023,4,1);  label=Some {| text=I18N.t "phase.23.description"; rotation=270; align="right"; x=12 |} |} |> pojo
                |]
                plotBands=[|
                    {| ``from``=jsTime <| DateTime(2020,2,29);
                       ``to``=jsTime <| DateTime(2020,3,13);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.1.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,3,13);
                       ``to``=jsTime <| DateTime(2020,3,20);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.2.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,3,20);
                       ``to``=jsTime <| DateTime(2020,4,8);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.3.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,4,8);
                       ``to``=jsTime <| DateTime(2020,4,15);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.4.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,4,15);
                       ``to``=jsTime <| DateTime(2020,4,21);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.5.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,4,21);
                       ``to``=jsTime <| DateTime(2020,5,15);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.6.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,5,15);
                       ``to``=jsTime <| DateTime(2020,9,10);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.7.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,9,10);
                       ``to``=jsTime <| DateTime(2020,10,9);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.8.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,10,9);
                       ``to``=jsTime <| DateTime(2020,10,17);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.9.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,10,17);
                       ``to``=jsTime <| DateTime(2020,10,19);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.10.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,10,19);
                       ``to``=jsTime <| DateTime(2020,10,26);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.11.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,10,26);
                       ``to``=jsTime <| DateTime(2020,11,6);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.12.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,11,6);
                       ``to``=jsTime <| DateTime(2020,12,21);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.13.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2020,12,21);
                       ``to``=jsTime <| DateTime(2021,2,1);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.14.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2021,2,1);
                       ``to``=jsTime <| DateTime(2021,2,13);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.15.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2021,3,13);
                       ``to``=jsTime <| DateTime(2021,6,15);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.16.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2021,6,15);
                       ``to``=jsTime <| DateTime(2021,8,23);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.17.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2021,8,23);
                       ``to``=jsTime <| DateTime(2021,11,8);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.18.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2021,11,8);
                       ``to``=jsTime <| DateTime(2022,1,19);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.19.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2022,1,19);
                       ``to``=jsTime <| DateTime(2022,2,1);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.20.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2022,2,1);
                       ``to``=jsTime <| DateTime(2022,2,21);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.21.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2022,2,21);
                       ``to``=jsTime <| DateTime(2023,4,1);
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.22.title" |}
                    |}
                    {| ``from``=jsTime <| DateTime(2023,4,1);
                       ``to``=jsTime <| DateTime.Today;
                       color="transparent"
                       label=Some {| align="center"; text=I18N.t "phase.23.title" |}
                    |}
                    yield! shadedWeekendPlotBands
                |]
                dateTimeLabelFormats = pojo
                    {|
                        week = I18N.t "charts.common.shortDateFormat"
                        day = I18N.t "charts.common.shortDateFormat"
                    |}
            |}
        |]
        yAxis = [|
            {|
                index = 0
                ``type`` = if scaleType=Linear then "linear" else "logarithmic"
                min = if scaleType=Linear then None else Some 0.5
                max = None
                opposite = true
                title = {| text = null |}
                showFirstLabel = None
                tickInterval = if scaleType=Linear then None else Some 0.25
                gridZIndex = -1
                plotLines = [| {| value = 0; color = "black" |} |]
                crosshair = true
            |}
        |]

        tooltip = pojo
            {|
                shared = true
                split = false
                xDateFormat = "<b>" + I18N.t "charts.common.dateFormat" + "</b>"
            |}

        legend =
            {|
                enabled = false
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                layout = "vertical"
            |}

        navigator = pojo {| enabled = false |}
        scrollbar = pojo {| enabled = false |}
        responsive = pojo
            {|
                rules =
                    [| {|
                        condition = {| maxWidth = 768 |}
                        chartOptions =
                            {|
                                yAxis = [| {| labels = pojo {| enabled = false |} |} |]
                            |}
                    |} |]
            |}

        plotOptions = pojo
            {|
                line = pojo
                    {|
                        marker = pojo {| symbol = "circle"; radius = 3 |}
                    |}
            |}

        credits = chartCreditsNIJZ
    |}


let basicChartOptions
    (scaleType:ScaleType)
    (className:string)
    (selectedRangeSelectionButtonIndex: int)
    (rangeSelectorButtonClickHandler: int -> (Event -> bool))
    =
    {| basicChart scaleType className with

        rangeSelector = configureRangeSelector selectedRangeSelectionButtonIndex [|
                        {|
                            ``type`` = "month"
                            count = 2
                            text = I18N.tOptions "charts.common.x_months" {| count = 2 |}
                            events = pojo {| click = rangeSelectorButtonClickHandler 0 |}
                        |}
                        {|
                            ``type`` = "month"
                            count = 4
                            text = I18N.tOptions "charts.common.x_months" {| count = 4 |}
                            events = pojo {| click = rangeSelectorButtonClickHandler 1 |}
                        |}
                        {|
                            ``type`` = "all"
                            count = 1
                            text = I18N.t "charts.common.all"
                            events = pojo {| click = rangeSelectorButtonClickHandler 2 |}
                        |}
                    |]
    |}
