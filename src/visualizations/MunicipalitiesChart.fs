[<RequireQualifiedAccess>]
module MunicipalitiesChart

open Elmish
open Browser
open Fable.Core.JsInterop

open Feliz
open Feliz.ElmishComponents

open Types

let barMaxHeight = 55
let showMaxBars = 30
let collapsedMunicipalityCount = 24

let excludedMunicipalities = Set.ofList ["kraj"]

type Region =
    { Key : string
      Name : string }

type TotalsForDate =
    { Date : System.DateTime
      ConfirmedToDate : int option
      DeceasedToDate : int option
    }

type Municipality =
    { Key : string
      Name : string option
      RegionKey : string
      DoublingTime : float option
      ActiveCases : int option
      MaxConfirmedCases : int option
      LastConfirmedCase : System.DateTime
      DaysSinceLastCase : int
      TotalsForDate : TotalsForDate array }

type SortBy =
    | ActiveCases
    | TotalConfirmedCases
    | LastConfirmedCase
    | DoublingTime

type Query (query : obj, regions : Region list) =
    member this.Query = query
    member this.Regions =
        regions
        |> List.map (fun region -> region.Key)
        |> Set.ofList
    member this.Region =
        match query?("region") with
        | Some (region : string) when Set.contains (region.ToLower()) this.Regions ->
            Some (region.ToLower())
        | _ -> None
    member this.SortBy =
        match query?("sort") with
        | Some (sort : string) ->
            match sort.ToLower() with
            | "active-cases" -> Some ActiveCases
            | "total-confirmed-cases" -> Some TotalConfirmedCases
            | "last-confirmed-case" -> Some LastConfirmedCase
            | "time-to-double" ->
                match Highcharts.showExpGrowthFeatures with
                    | true -> Some DoublingTime
                    | _ -> None
            | _ -> None
        | _ -> None

type State =
    { Municipalities : Municipality seq
      Regions : Region list
      ShowAll : bool
      SearchQuery : string
      FilterByRegion : string
      SortBy : SortBy }

type Msg =
    | ToggleShowAll
    | SearchInputChanged of string
    | RegionFilterChanged of string
    | SortByChanged of SortBy

let init (queryObj : obj) (data : RegionsData) : State * Cmd<Msg> =
    let lastDataPoint = List.last data

    let regions =
        lastDataPoint.Regions
        |> List.filter (fun region -> Set.contains region.Name Utils.Dictionaries.excludedRegions |> not)
        |> List.map (fun reg -> { Key = reg.Name ; Name = I18N.tt "region" reg.Name })
        |> List.sortBy (fun region -> region.Name)

    let query = Query(queryObj, regions)

    let municipalities =
        seq {
            for regionsDataPoint in data do
                for region in regionsDataPoint.Regions do
                    for municipality in region.Municipalities do
                        if not (Set.contains municipality.Name excludedMunicipalities) then
                            yield {| Date = regionsDataPoint.Date
                                     RegionKey = region.Name
                                     MunicipalityKey = municipality.Name
                                     ConfirmedToDate = municipality.ConfirmedToDate
                                     DeceasedToDate = municipality.DeceasedToDate |} }
        |> Seq.groupBy (fun dp -> dp.MunicipalityKey)
        |> Seq.map (fun (municipalityKey, dp) ->
            let totalsForDate =
                dp
                |> Seq.map (
                    fun dp -> {
                        Date = dp.Date
                        ConfirmedToDate = dp.ConfirmedToDate
                        DeceasedToDate = dp.DeceasedToDate } )
                |> Seq.sortBy (fun dp -> dp.Date)
                |> Seq.toArray
            let doublingTime =
                dp
                |> Seq.map (fun dp -> {| Date = dp.Date ; Value = dp.ConfirmedToDate |})
                |> Seq.toList
                |> Utils.findDoublingTime
            let maxValue =
                try
                    dp
                    |> Seq.map (fun dp -> dp.ConfirmedToDate)
                    |> Seq.filter Option.isSome
                    |> Seq.max
                with
                    | _ -> None
            let maxDay = dp |> Seq.filter (fun p -> p.ConfirmedToDate = maxValue) |> Seq.head
            { Key = municipalityKey
              Name = (Utils.Dictionaries.municipalities.TryFind municipalityKey) |> Option.map (fun municipality -> municipality.Name)
              RegionKey = (dp |> Seq.last).RegionKey
              DoublingTime = doublingTime
              ActiveCases =
                    let dayR = totalsForDate |> Array.tryItem (totalsForDate.Length - 15)
                    let dayL = totalsForDate |> Array.tryLast
                    match dayR, dayL with
                    | None, None -> Some 0
                    | None, Some b -> b.ConfirmedToDate
                    | Some a, None -> Some 0
                    | Some a, Some b -> Some (b.ConfirmedToDate.Value - a.ConfirmedToDate.Value)
              MaxConfirmedCases = maxValue
              LastConfirmedCase = maxDay.Date
              DaysSinceLastCase = System.DateTime.Today.Subtract(maxDay.Date).Days
              TotalsForDate = totalsForDate
            })

    let state =
        { Municipalities = municipalities
          Regions = regions
          ShowAll = false
          SearchQuery = ""
          FilterByRegion =
            match query.Region with
            | None -> ""
            | Some region -> region
          SortBy =
            match query.SortBy with
            | None -> LastConfirmedCase
            | Some sortBy -> sortBy }

    state, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    // trigger event for iframe resize
    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true)
    document.dispatchEvent(evt) |> ignore

    match msg with
    | ToggleShowAll ->
        { state with ShowAll = not state.ShowAll }, Cmd.none
    | SearchInputChanged query ->
        { state with SearchQuery = query }, Cmd.none
    | RegionFilterChanged region ->
        { state with FilterByRegion = region }, Cmd.none
    | SortByChanged sortBy ->
        { state with SortBy = sortBy }, Cmd.none

let renderMunicipality (municipality : Municipality) =

    let renderLastCase =
        let label, value =
            match municipality.DaysSinceLastCase with
            | 0 -> I18N.t "charts.municipalities.lastCase", I18N.t "charts.municipalities.today"
            | 1 -> I18N.t "charts.municipalities.lastCase", I18N.t "charts.municipalities.yesterday"
            | x -> I18N.t "charts.municipalities.lastCase", I18N.tOptions "days.x_days_ago" {| count = x |}

        Html.div [
            prop.className "last-case-days"
            prop.children [
                Html.span [
                    prop.className "label"
                    prop.text label
                ]
                Html.span [
                    prop.className "value"
                    prop.text value
                ]
            ]
        ]

    let renderedDoublingTime =
        match municipality.DoublingTime with
        | None -> Html.none
        | Some value ->
            let displayValue = int (round value)
            Html.div [
                prop.className "doubling-time"
                prop.children [
                    Html.span [
                        prop.className "label"
                        prop.text (I18N.t "charts.municipalities.doubles")
                    ]
                    Html.span [
                        prop.className "value"
                        prop.text (I18N.tOptions "days.in_x_days"  {| count = displayValue |})
                    ]
                ]
            ]

    let renderedBars =
        match municipality.MaxConfirmedCases with
        | None -> Seq.empty
        | Some maxValue ->
            seq {
                let dpLen = Array.length municipality.TotalsForDate
                let dpStart = if dpLen > showMaxBars then dpLen - showMaxBars else 0
                for i = dpStart to dpLen - 1 do
                    let dp = municipality.TotalsForDate.[i]
                    match dp.ConfirmedToDate with
                    | None ->
                        yield Html.div [
                            prop.className "bar bar--empty"
                        ]
                    | Some confirmedToDate ->
                        yield Html.div [
                            prop.className "bar-wrapper"
                            prop.children [
                                let deceasedToDate = dp.DeceasedToDate.Value
                                let recoveredToDate =
                                    if i >= 14 && municipality.TotalsForDate.[i-14].ConfirmedToDate.Value > deceasedToDate
                                    then municipality.TotalsForDate.[i-14].ConfirmedToDate.Value - deceasedToDate
                                    else 0
                                let activeCases = confirmedToDate - deceasedToDate - recoveredToDate
                                let dHeight = deceasedToDate * barMaxHeight / maxValue
                                let aHeight = activeCases * barMaxHeight / maxValue
                                let rHeight = confirmedToDate * barMaxHeight / maxValue - dHeight - aHeight
                                Html.div [
                                    prop.className "bar"
                                    prop.children [
                                        Html.div [
                                            prop.style [ style.height dHeight ]
                                            prop.className "bar--deceased" ]
                                        Html.div [
                                            prop.style [ style.height rHeight ]
                                            prop.className "bar--recovered" ]
                                        Html.div [
                                            prop.style [ style.height aHeight ]
                                            prop.className "bar--active" ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "total-and-date total-and-date--hover"
                                    prop.children [
                                        Html.div [
                                            prop.className "date"
                                            prop.text (I18N.tOptions "days.date" {| date = dp.Date |} )]
                                        Html.div [
                                            if (deceasedToDate > 0) then
                                                prop.className "deceased"
                                                prop.children [
                                                    Html.span [ prop.text (I18N.t "charts.municipalities.deceased") ]
                                                    Html.b [ prop.text deceasedToDate ] ] ]
                                        Html.div [
                                            if (recoveredToDate > 0) then
                                                prop.className "recovered"
                                                prop.children [
                                                    Html.span [ prop.text (I18N.t "charts.municipalities.recovered") ]
                                                    Html.b [ prop.text recoveredToDate ] ] ]
                                        Html.div [
                                            if (activeCases > 0) then
                                                prop.className "active"
                                                prop.children [
                                                    Html.span [ prop.text (I18N.t "charts.municipalities.active") ]
                                                    Html.b [ prop.text activeCases ] ] ]
                                        Html.div [
                                            prop.className "confirmed"
                                            prop.children [
                                                Html.span [ prop.text (I18N.t "charts.municipalities.all") ]
                                                Html.b [ prop.text confirmedToDate ] ] ]
                                    ]
                                ]
                            ]
                        ]
                }

    Html.div [
        prop.className "municipality"
        prop.children [
            Html.div [
                prop.className "name"
                prop.text (
                    match municipality.Name with
                    | None -> municipality.Key
                    | Some name -> name)
            ]
            Html.div [
                prop.className "positive-tests"
                prop.children [
                    Html.div [
                        prop.className "bars"
                        prop.children renderedBars
                    ]
                    Html.div [
                        prop.className "total-and-date"
                        prop.children [
                            Html.div [
                                prop.className "active"
                                prop.text (sprintf "%d" municipality.ActiveCases.Value) ]
                            Html.div [
                                prop.className "total"
                                prop.text (sprintf "%d" municipality.MaxConfirmedCases.Value) ]
                            Html.div [
                                prop.className "date"
                                prop.text (I18N.tOptions "days.date" {| date = municipality.LastConfirmedCase.Date |})]
                        ]
                    ]
                ]
            ]
            if Highcharts.showExpGrowthFeatures then
                renderedDoublingTime
            else
                renderLastCase
        ]
    ]

let renderMunicipalities (state : State) _ =

    let dataFilteredByQuery =
        let query = state.SearchQuery.Trim().ToLower() |> Utils.transliterateCSZ
        if  query = ""
        then state.Municipalities
        else
            state.Municipalities
            |> Seq.filter (fun municipality ->
               let name =
                match municipality.Name with
                | None -> municipality.Key
                | Some name -> name
               (name.ToLower() |> Utils.transliterateCSZ).Contains(query))

    let dataFilteredByRegion =
        dataFilteredByQuery
        |> Seq.filter (fun municipality ->
            if state.FilterByRegion = ""
            then true
            else municipality.RegionKey = state.FilterByRegion
        )

    let compareStringOption s1 s2 =
        match s1, s2 with
        | None, None -> 0
        | Some _, None -> 1
        | None, Some _ -> -1
        | Some s1, Some s2 -> System.String.Compare(s1, s2)

    let compareActiveCases m1 m2 =
        if m1.ActiveCases < m2.ActiveCases then 1
        else if m1.ActiveCases > m2.ActiveCases then -1
        else compareStringOption m1.Name m2.Name

    let compareMaxCases m1 m2 =
        if m1.MaxConfirmedCases < m2.MaxConfirmedCases then 1
        else if m1.MaxConfirmedCases > m2.MaxConfirmedCases then -1
        else compareStringOption m1.Name m2.Name

    let sortedMunicipalities =
        match state.SortBy with
        | ActiveCases ->
            dataFilteredByRegion
            |> Seq.sortWith (fun m1 m2 -> compareActiveCases m1 m2)
        | TotalConfirmedCases ->
            dataFilteredByRegion
            |> Seq.sortWith (fun m1 m2 -> compareMaxCases m1 m2)
        | DoublingTime ->
            dataFilteredByRegion
            |> Seq.sortWith (fun m1 m2 ->
                match m1.DoublingTime, m2.DoublingTime with
                | None, None -> compareStringOption m1.Name m2.Name
                | Some _, None -> -1
                | None, Some _ -> 1
                | Some d1, Some d2 ->
                    if d1 > d2 then 1
                    else if d1 < d2 then -1
                    else compareActiveCases m1 m2)
        | LastConfirmedCase ->
            dataFilteredByRegion
            |> Seq.sortWith (fun m1 m2 ->
                if m1.LastConfirmedCase < m2.LastConfirmedCase then 1
                else if m1.LastConfirmedCase > m2.LastConfirmedCase then -1
                else compareActiveCases m1 m2)

    let truncatedData, displayShowAllButton =
        if state.ShowAll = true
        then sortedMunicipalities, true
        else if Seq.length sortedMunicipalities <= collapsedMunicipalityCount then sortedMunicipalities, false
        else Seq.take collapsedMunicipalityCount sortedMunicipalities, true

    (truncatedData |> Seq.map (fun municipality -> renderMunicipality municipality), displayShowAllButton)

let renderShowMore showAll dispatch =
    Html.div [
        prop.className "show-all"
        prop.children [
            Html.div [
                Html.button [
                    prop.className "btn btn-primary"
                    prop.text (if showAll then I18N.t "charts.municipalities.showLess" else I18N.t "charts.municipalities.showAll")
                    prop.onClick (fun _ -> dispatch ToggleShowAll)
                ]
            ]
        ]
    ]

let renderSearch (query : string) dispatch =
    Html.input [
        prop.className "form-control form-control-sm filters__query"
        prop.type' .text
        prop.placeholder (I18N.t "charts.municipalities.search")
        prop.valueOrDefault query
        prop.onChange (fun query -> SearchInputChanged query |> dispatch)
    ]

let renderRegionSelector (regions : Region list) (selected : string) dispatch =
    let renderedRegions = seq {
        yield Html.option [
            prop.text (I18N.t "charts.municipalities.allRegions")
            prop.value ""
        ]

        for region in regions do
            yield Html.option [
                prop.text region.Name
                prop.value region.Key
            ]
    }

    Html.select [
        prop.value selected
        prop.className "form-control form-control-sm filters__region"
        prop.children renderedRegions
        prop.onChange (fun (value : string) -> RegionFilterChanged value |> dispatch)
    ]

let renderSortBy (currentSortBy : SortBy) dispatch =

    let renderSelector (sortBy : SortBy) (label : string) =
        let defaultProps =
            [ prop.text label
              prop.className [
                  true, "chart-display-property-selector__item"
                  sortBy = currentSortBy, "selected" ] ]
        if sortBy = currentSortBy
        then Html.div defaultProps
        else Html.div ((prop.onClick (fun _ -> SortByChanged sortBy |> dispatch)) :: defaultProps)

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            Html.text (I18N.t "charts.municipalities.sortBy")
            renderSelector SortBy.ActiveCases (I18N.t "charts.municipalities.sortActive")
            renderSelector SortBy.TotalConfirmedCases (I18N.t "charts.municipalities.sortTotal")
            if Highcharts.showExpGrowthFeatures then
                renderSelector SortBy.DoublingTime (I18N.t "charts.municipalities.sortDoublingTime")
            renderSelector SortBy.LastConfirmedCase (I18N.t "charts.municipalities.sortLast")
        ]
    ]

let render (state : State) dispatch =
    let renderedMunicipalities, showMore = renderMunicipalities state dispatch

    let element = Html.div [
        prop.children [
            Utils.renderChartTopControls [
                Html.div [
                    prop.className "filters"
                    prop.children [
                        renderRegionSelector state.Regions state.FilterByRegion dispatch
                        renderSearch state.SearchQuery dispatch
                    ]
                ]
                renderSortBy state.SortBy dispatch
            ]
            Html.div [
                prop.className "municipalities"
                prop.children renderedMunicipalities ]
            (if showMore then renderShowMore state.ShowAll dispatch else Html.none)
        ]
    ]

    // trigger event for iframe resize
    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true)
    document.dispatchEvent(evt) |> ignore

    element

let municipalitiesChart (props : {| query : obj ; data : RegionsData |}) =
    React.elmishComponent("MunicipalitiesChart", init props.query props.data, update, render)
