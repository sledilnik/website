module SourcesChart

open Elmish
open Feliz
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Browser
open Types
open Highcharts

let chartText = I18N.chartText "sources"

let countryColors =
    [
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
      "#024a66"
      "#f95d6a"
      "#a05195"
      "#024a66"
      "#665191"
      "#10829a"
      "#dba51d"
      "#afa53f"
      "#777c29"
      "#70a471"
      "#457844"
      "#ffa600"
      "#d45087"
      ]

type DisplayType =
    | Quarantine
    | QuarantineRelative
    | ByLocation
    | ByLocationRelative
    | BySource
    | BySourceRelative
    | BySourceCountry
    | BySourceCountryRelative
  with
    static member All =
        [ ByLocation
          ByLocationRelative
          BySource
          BySourceRelative
          BySourceCountry
          BySourceCountryRelative
          Quarantine
          QuarantineRelative ]

    static member Default = ByLocation

    member this.GetName =
        match this with
        | Quarantine                -> chartText "quarantine"
        | QuarantineRelative        -> chartText "quarantineRelative"
        | ByLocation                -> chartText "byLocation"
        | ByLocationRelative        -> chartText "byLocationRelative"
        | BySource                  -> chartText "bySource"
        | BySourceRelative          -> chartText "bySourceRelative"
        | BySourceCountry           -> chartText "bySourceCountry"
        | BySourceCountryRelative   -> chartText "bySourceCountryRelative"

// ---------------------------
// State management
// ---------------------------
type State =
    { displayType: DisplayType
      data: WeeklyStatsData
      RangeSelectionButtonIndex: int }

type Msg =
    | RangeSelectionChanged of int
    | ChangeDisplayType of DisplayType

let init data: State * Cmd<Msg> =
    let state =
        { displayType = DisplayType.Default
          data = data
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | RangeSelectionChanged buttonIndex ->
        { state with
              RangeSelectionButtonIndex = buttonIndex },
        Cmd.none
    | ChangeDisplayType displayType ->
        { state with displayType = displayType },
        Cmd.none

// ---------------------------
// Display Type Selection
// ---------------------------
let renderDisplaySelector state dt dispatch =
    Html.div [
        prop.onClick (fun _ -> ChangeDisplayType dt |> dispatch)
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (state.displayType = dt, "metric-selector--selected") ]
        prop.text dt.GetName
    ]

let renderDisplaySelectors state dispatch =
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            DisplayType.All
            |> List.map (fun dt -> renderDisplaySelector state dt dispatch) ) ]

type Series =
    | ConfirmedCases
    | SentToQuarantine
    | ConfirmedFromQuarantine
    | ImportedCases
    | ImportRelatedCases
    | SourceUnknown
    | LocalSource
    | Family
    | Work
    | School
    | Hospital
    | OtherHealthcare
    | RetirementHome
    | Prison
    | Transport
    | Shop
    | Restaurant
    | Sport
    | GatheringPrivate
    | GatheringOrganized
    | LocationOther
    | LocationUnknown

module Series =
    let quarantine = [ SentToQuarantine; ConfirmedCases; ConfirmedFromQuarantine ]
    let quarantineRelative = [  ConfirmedCases; ConfirmedFromQuarantine ]
    let bySource = [ImportedCases; ImportRelatedCases; LocalSource; SourceUnknown; ]
    let byLocation = [Family; Work; School; Hospital; OtherHealthcare; RetirementHome; Prison; Transport; Shop; Restaurant; Sport; GatheringPrivate; GatheringOrganized; LocationOther; LocationUnknown ]

    let getSeriesInfo =
        function
        | SentToQuarantine ->  "#cccccc", "sentToQuarantine", 0
        | ConfirmedFromQuarantine ->  "#665191", "confirmedFromQuarantine", 1
        | ConfirmedCases ->  "#d5c768", "confirmedCases", 1

        | ImportedCases -> "#d559b0", "importedCases", 0
        | ImportRelatedCases -> "#f4b2e0", "importRelatedCases", 0
        | SourceUnknown -> "#f95d6a", "sourceUnknown", 0
        | LocalSource ->"#F59C9C", "localSource", 0

        | Family -> "#457844", "family", 0
        | Work -> "#f95d6a", "work", 0
        | School -> "#ffa600", "school", 0
        | Hospital -> "#dba51d", "hospital", 0
        | OtherHealthcare -> "#10829a", "otherHealthcare", 0
        | RetirementHome -> "#665191", "retirementHome", 0
        | Prison -> "#d45087", "prison", 0
        | Transport -> "#024a66", "transport", 0
        | Shop -> "#afa53f", "shop", 0
        | Restaurant -> "#a05195", "restaurant", 0
        | Sport -> "#70a471", "sport", 0
        | GatheringPrivate -> "#024a66", "gatheringPrivate", 0
        | GatheringOrganized -> "#dba51d", "gatheringOrganized", 0
        | LocationOther -> "#10829a", "locationOther", 0
        | LocationUnknown -> "#cccccc", "locationUnknown", 0

let tooltipFormatter jsThis =
    let pts: obj [] = jsThis?points
    let fmtWeekYearFromTo = pts.[0]?point?fmtWeekYearFromTo
    let arrows p = match p?point?seriesId with
                                   | "confirmedFromQuarantine"  -> "↳ "
                                   |_ -> ""

    fmtWeekYearFromTo
    + "<br>"
    + (pts
       |> Seq.map (fun p ->
           sprintf """%s<span style="color:%s">●</span> %s: <b>%s</b>""" (arrows p) p?series?color p?series?name (I18N.NumberFormat.formatNumber (p?point?fmtTotal : int)))
       |> String.concat "<br>")

let tooltipFormatterWithTotal totalText jsThis =
    let pts: obj [] = jsThis?points
    let total = pts |> Array.map (fun p -> p?point?y |> Utils.optionToInt) |> Array.sum
    tooltipFormatter jsThis + sprintf """<br><br><span style="color: rgba(0,0,0,0)">●</span> %s: <b>%s</b>""" totalText (I18N.NumberFormat.formatNumber total)



// ---------------------------
// Data Massaging
// ---------------------------
let splitOutFromTotal (split) (total)  =
    match split, total with
    | Some split_, Some total_ -> Some (total_ - split_)
    | None, Some _ -> total
    | _ -> None


// ---------------------------
// Chart Rendering w Highcharts
// ---------------------------
let renderSeriesImportedByCountry (state: State) =
    let countryCodesSortedByTotal = state.data |> Data.WeeklyStats.countryTotals |> Array.map (fun (countryCode, _) -> countryCode)

    let countriesToShowInLegend = Array.sub countryCodesSortedByTotal 0 10 |> Set.ofArray// Top 10
    //let countriesToShowInLegend = countriesToShowInLegend |> Set.ofArray // All

    let countryToSeries (countryIndex:int) (countryCode:string) =
                                                                      {|
                                                                      stack = 0
                                                                      animation = false
                                                                      legendIndex = countryIndex
                                                                      color = countryColors.[countryIndex% countryColors.Length]
                                                                      name = I18N.tt "country" countryCode
                                                                      showInLegend = Set.contains countryCode countriesToShowInLegend
                                                                      data = state.data |> Seq.map (fun dp -> {|
                                                                                                               x = jsDatesMiddle dp.Date dp.DateTo
                                                                                                               y = dp.ImportedFrom.Item countryCode
                                                                                                               fmtTotal = dp.ImportedFrom.Item countryCode |> string
                                                                                                               fmtWeekYearFromTo =
                                                                                                                  I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |}

                                                                                                              |} |> pojo) |> Array.ofSeq
                                                                      |} |> pojo

    countryCodesSortedByTotal |> Seq.mapi countryToSeries

let renderSeries state = Seq.mapi (fun legendIndex series ->
    let getPoint: (WeeklyStatsDataPoint -> int option) =
        match series with
        | ConfirmedCases -> fun dp -> dp.ConfirmedCases |> splitOutFromTotal dp.Source.FromQuarantine
        | SentToQuarantine -> fun dp -> dp.SentToQuarantine
        | ConfirmedFromQuarantine -> fun dp -> dp.Source.FromQuarantine
        | ImportedCases -> fun dp -> dp.Source.Import
        | ImportRelatedCases -> fun dp -> dp.Source.ImportRelated
        | SourceUnknown -> fun dp -> dp.Source.Unknown
        | LocalSource -> fun dp -> dp.Source.Local
        | Family -> fun dp -> dp.Location.Family
        | Work -> fun dp -> dp.Location.Work
        | School -> fun dp -> dp.Location.School
        | Hospital -> fun dp -> dp.Location.Hospital
        | OtherHealthcare -> fun dp -> dp.Location.OtherHealthcare
        | RetirementHome -> fun dp -> dp.Location.RetirementHome
        | Prison -> fun dp -> dp.Location.Prison
        | Transport -> fun dp -> dp.Location.Transport
        | Shop -> fun dp -> dp.Location.Shop
        | Restaurant -> fun dp -> dp.Location.Restaurant
        | Sport -> fun dp -> dp.Location.Sport
        | GatheringPrivate -> fun dp -> dp.Location.GatheringPrivate
        | GatheringOrganized -> fun dp -> dp.Location.GatheringOrganized
        | LocationOther -> fun dp -> dp.Location.Other
        | LocationUnknown -> fun dp -> dp.Location.Unknown

    let getPointTotal: (WeeklyStatsDataPoint -> int option) =
        match series with
        | ConfirmedCases -> fun dp -> dp.ConfirmedCases
        | SentToQuarantine -> fun dp -> dp.SentToQuarantine
        | ConfirmedFromQuarantine -> fun dp -> dp.Source.FromQuarantine
        | ImportedCases -> fun dp -> dp.Source.Import
        | ImportRelatedCases -> fun dp -> dp.Source.ImportRelated
        | SourceUnknown -> fun dp -> dp.Source.Unknown
        | LocalSource -> fun dp -> dp.Source.Local
        | Family -> fun dp -> dp.Location.Family
        | Work -> fun dp -> dp.Location.Work
        | School -> fun dp -> dp.Location.School
        | Hospital -> fun dp -> dp.Location.Hospital
        | OtherHealthcare -> fun dp -> dp.Location.OtherHealthcare
        | RetirementHome -> fun dp -> dp.Location.RetirementHome
        | Prison -> fun dp -> dp.Location.Prison
        | Transport -> fun dp -> dp.Location.Transport
        | Shop -> fun dp -> dp.Location.Shop
        | Restaurant -> fun dp -> dp.Location.Restaurant
        | Sport -> fun dp -> dp.Location.Sport
        | GatheringPrivate -> fun dp -> dp.Location.GatheringPrivate
        | GatheringOrganized -> fun dp -> dp.Location.GatheringOrganized
        | LocationOther -> fun dp -> dp.Location.Other
        | LocationUnknown -> fun dp -> dp.Location.Unknown

    let color, seriesId, stack = Series.getSeriesInfo (series)
    {|
       color = color
       name = chartText seriesId
       stack = stack
       animation = false
       legendIndex = legendIndex
       data =
           state.data
           |> Seq.map (fun dp ->
               {| x = jsDatesMiddle dp.Date dp.DateTo
                  y = getPoint dp
                  fmtTotal = getPointTotal dp |> string
                  seriesId = seriesId
                  fmtWeekYearFromTo =
                      I18N.tOptions "days.weekYearFromToDate" {| date = dp.Date; dateTo = dp.DateTo |} |}
               |> pojo)
           |> Array.ofSeq |}
    |> pojo)

let renderChartOptions (state: State) dispatch =
    let onRangeSelectorButtonClick (buttonIndex: int) =
        let res (_: Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true

        res

    let lastWeek = state.data.[state.data.Length-1]

    let className = "covid19-weekly-stats"
    let baseOptions =
        basicChartOptions ScaleType.Linear className state.RangeSelectionButtonIndex onRangeSelectorButtonClick

    {| baseOptions with
           chart = pojo
            {|
                animation = false
                ``type`` = "column"
                zoomType = "x"
                className = className
                events = pojo {| load = onLoadEvent(className) |}
            |}
           series = (match state.displayType with
                    | Quarantine -> Series.quarantine |> renderSeries state
                    | QuarantineRelative -> Series.quarantineRelative |> renderSeries state
                    | ByLocation | ByLocationRelative -> Series.byLocation |> renderSeries state
                    | BySource | BySourceRelative -> Series.bySource |> renderSeries state
                    | BySourceCountry | BySourceCountryRelative -> renderSeriesImportedByCountry state
                    ) |> Seq.toArray
           yAxis =
               baseOptions.yAxis
               |> Array.map (fun yAxis -> {| yAxis with
                                              min = None
                                              labels = match state.displayType with
                                                       | QuarantineRelative | ByLocationRelative | BySourceRelative | BySourceCountryRelative ->pojo {| format = "{value} %" |}
                                                       | _ -> pojo {| format = "{value}" |}

                                              reversedStacks = match state.displayType with
                                                               | Quarantine | QuarantineRelative -> true
                                                               | _ -> false |})
           xAxis =
               baseOptions.xAxis
               |> Array.map (fun xAxis ->
                   {| xAxis with
                          tickInterval = 86400000 * 7
                          plotBands =
                                [|
                                   {| from=jsTime <| lastWeek.Date
                                      ``to``=jsTime <| lastWeek.DateTo
                                      color="#ffffe0"
                                    |}
                                |]
                         |})
           tooltip =
               pojo
                   {| shared = true
                      split = false
                      useHTML = true
                      formatter = match state.displayType with
                                  | Quarantine | QuarantineRelative -> fun () -> tooltipFormatter jsThis
                                  | ByLocation | ByLocationRelative -> fun () -> tooltipFormatterWithTotal (chartText "totalConfirmed") jsThis
                                  | BySource | BySourceRelative -> fun () -> tooltipFormatterWithTotal (chartText "totalConfirmed") jsThis
                                  | BySourceCountry | BySourceCountryRelative -> fun () -> tooltipFormatterWithTotal (chartText "totalImported") jsThis
                      |}
           legend =
               pojo
                   {| enabled = true
                      layout = "horizontal" |}
           plotOptions = pojo {|
                                column = pojo {|
                                                stacking = match state.displayType with
                                                           | QuarantineRelative | ByLocationRelative | BySourceRelative | BySourceCountryRelative -> "percent"
                                                           | _ -> "normal" |}

                                |}
           rangeSelector = configureRangeSelector state.RangeSelectionButtonIndex
                                        [|
                                            {|
                                                ``type`` = "week" // Customized to align x axis to exactly 9 weeks
                                                count = 9
                                                text = I18N.tOptions "charts.common.x_months" {| count = 2 |}
                                                events = pojo {| click = onRangeSelectorButtonClick 0 |}
                                            |}
                                            {|
                                                ``type`` = "month"
                                                count = 3
                                                text = I18N.tOptions "charts.common.x_months" {| count = 3 |}
                                                events = pojo {| click = onRangeSelectorButtonClick 1 |}
                                            |}
                                            {|
                                                ``type`` = "all"
                                                count = 1
                                                text = I18N.t "charts.common.all"
                                                events = pojo {| click = onRangeSelectorButtonClick 2 |}
                                            |}
                                        |]

                                    |}

let renderChartContainer state dispatch =
    Html.div [ prop.style [ style.height 450 ]
               prop.className "highcharts-wrapper"
               prop.children
                   [ renderChartOptions state dispatch
                     |> chartFromWindow ] ]


let render (state: State) dispatch =
    let disclaimer =
        match state.displayType with
        | Quarantine | QuarantineRelative -> "disclaimer"
        | _ -> "disclaimerGeneral"

    Html.div [
        renderChartContainer state dispatch
        renderDisplaySelectors state dispatch

        Html.div [
            prop.className "disclaimer"
            prop.children [
                Html.text (chartText disclaimer)
            ]
        ]
    ]

let sourcesChart (props: {| data: WeeklyStatsData |}) =
    React.elmishComponent ("sourcesChart", init props.data, update, render)
