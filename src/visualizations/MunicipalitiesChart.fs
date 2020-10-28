[<RequireQualifiedAccess>]
module MunicipalitiesChart

open System
open Elmish
open Browser
open Fable.Core.JsInterop

open Feliz
open Feliz.ElmishComponents

open Types

let barMaxHeight = 65
let showMaxBars = 30
let collapsedMunicipalityCount = 16

let excludedMunicipalities = Set.ofList ["kraj"]

type Region =
    { Key : string
      Name : string }

type TotalsForDate =
    { Date : DateTime
      ActiveCases : int option
      ConfirmedToDate : int option
      DeceasedToDate : int option
    }

type Municipality =
    { Key : string
      Name : string option
      RegionKey : string
      DoublingTime : float option
      NewCases : int option
      ActiveCases : int option
      MaxActiveCases : int option
      MaxConfirmedCases : int option
      LastConfirmedCase : DateTime
      DaysSinceLastCase : int
      TotalsForDate : TotalsForDate list }

type View =
    | ActiveCases
    | TotalConfirmedCases
    | LastConfirmedCase
    | DoublingTime


type State =
    { Municipalities : Municipality seq
      Regions : Region list
      ShowAll : bool
      SearchQuery : string
      FilterByRegion : string
      View : View }

type Msg =
    | QueryParamsUpdated of QueryParams.State
    | ToggleShowAll
    | SearchInputChanged of string
    | RegionFilterChanged of string
    | ViewChanged of View

let init (data : RegionsData) : State * Cmd<Msg> =
    let lastDataPoint = List.last data

    let regions =
        lastDataPoint.Regions
        |> List.filter (fun region -> Set.contains region.Name Utils.Dictionaries.excludedRegions |> not)
        |> List.map (fun reg -> { Key = reg.Name ; Name = I18N.tt "region" reg.Name })
        |> List.sortBy (fun region -> region.Name)


    let municipalities =
        seq {
            for regionsDataPoint in data do
                for region in regionsDataPoint.Regions do
                    for municipality in region.Municipalities do
                        if not (Set.contains municipality.Name excludedMunicipalities) then
                            yield {| Date = regionsDataPoint.Date
                                     RegionKey = region.Name
                                     MunicipalityKey = municipality.Name
                                     ActiveCases = municipality.ActiveCases
                                     ConfirmedToDate = municipality.ConfirmedToDate
                                     DeceasedToDate = municipality.DeceasedToDate |} }
        |> Seq.groupBy (fun dp -> dp.MunicipalityKey)
        |> Seq.map (fun (municipalityKey, dp) ->
            let totals =
                dp
                |> Seq.map (
                    fun dp -> {
                        Date = dp.Date
                        ActiveCases = dp.ActiveCases
                        ConfirmedToDate = dp.ConfirmedToDate
                        DeceasedToDate = dp.DeceasedToDate } )
                |> Seq.sortBy (fun dp -> dp.Date)
                |> Seq.toList
            let totalsShown = totals |> Seq.skip ((Seq.length totals) - showMaxBars) |> Seq.toList
            let maxConfirmed = totals |> Seq.tryLast |> Option.map (fun dp -> dp.ConfirmedToDate) |> Option.defaultValue None
            let lastChange = totals |> Seq.filter (fun dp -> dp.ConfirmedToDate = maxConfirmed) |> Seq.head
            let activeCases = totalsShown |> Seq.tryLast |> Option.map (fun dp -> dp.ActiveCases) |> Option.defaultValue None
            let maxActive = totalsShown |> Seq.map (fun dp -> dp.ActiveCases) |> Seq.max
            let dayBefore = totalsShown |> Seq.skip (Seq.length totalsShown - 2) |> Seq.tryHead |> Option.map (fun dp -> dp.ConfirmedToDate) |> Option.defaultValue None
            let newCases =
                match dayBefore, maxConfirmed with
                | Some before, Some last -> if last > before then Some (last - before) else None
                | None, Some last -> Some last
                | _ -> None
            let doublingTime =
                if activeCases.IsSome && activeCases.Value >= 5
                then
                    dp
                    |> Seq.map (fun dp -> {| Date = dp.Date ; Value = dp.ActiveCases |})
                    |> Seq.toList
                    |> Utils.findDoublingTime
                else None
            { Key = municipalityKey
              Name = (Utils.Dictionaries.municipalities.TryFind municipalityKey) |> Option.map (fun municipality -> municipality.Name)
              RegionKey = (dp |> Seq.last).RegionKey
              DoublingTime = doublingTime
              NewCases = newCases
              ActiveCases = activeCases
              MaxActiveCases = maxActive
              MaxConfirmedCases = maxConfirmed
              LastConfirmedCase = lastChange.Date
              DaysSinceLastCase = DateTime.Today.Subtract(lastChange.Date).Days
              TotalsForDate = totalsShown
            })

    let state =
        { Municipalities = municipalities
          Regions = regions
          ShowAll = false
          SearchQuery = ""
          FilterByRegion = ""
          View = LastConfirmedCase }

    state, Cmd.none



let ViewQueryParam = [
    ("active-cases", View.ActiveCases);
    ("total-confirmed-cases", View.TotalConfirmedCases)
    ("last-confirmed-case", View.LastConfirmedCase)
    ("time-to-double", View.DoublingTime)] |> Map.ofList

let incorporateQueryParams (queryParams: QueryParams.State) (state: State, commands: Cmd<Msg>): State * Cmd<Msg>=
       let state = match queryParams.MunicipalitiesChartSearch with
                   | Some (search: string) -> { state with SearchQuery = search }
                   | _ -> state

       let state = match queryParams.MunicipalitiesChartSort with
                   | Some (sort : string) ->
                       match sort.ToLower() |> ViewQueryParam.TryFind with
                       | Some View.DoublingTime -> match Highcharts.showDoublingTimeFeatures with
                                                   | true -> {state with View=DoublingTime}
                                                   | _ -> state
                       | Some v -> {state with View=v}
                       | _ -> state
                   | _ -> state

       let state = match queryParams.MunicipalitiesChartRegion with
                   | Some (region: string) when List.exists (fun (e:Region) -> e.Key.Equals(region.ToLower())) state.Regions -> { state with FilterByRegion = region.ToLower() }
                   | _ -> state

       state, commands


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
    | ViewChanged view ->
        { state with View = view }, Cmd.none
    | QueryParamsUpdated queryParams -> (state, Cmd.none) |> incorporateQueryParams queryParams


let stateToQueryParams (state: State) (queryParams: QueryParams.State)
    = { queryParams with MunicipalitiesChartRegion = match state.FilterByRegion with
                                                     | "" -> None
                                                     | s -> Some s
                         MunicipalitiesChartSearch = match state.SearchQuery with
                                                     | "" -> None
                                                     | s -> Some s
                         MunicipalitiesChartSort = Map.tryFindKey (fun k v -> v = state.View) ViewQueryParam
                                                     }

let renderMunicipality (state : State) (municipality : Municipality) =

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
        let maxValue = if state.View = TotalConfirmedCases then municipality.MaxConfirmedCases else municipality.MaxActiveCases
        match maxValue with
        | None -> Seq.empty
        | Some maxValue ->
            seq {
                for dp in municipality.TotalsForDate do
                    match dp.ConfirmedToDate with
                    | None ->
                        yield Html.div [
                            prop.className "bar bar--empty"
                        ]
                    | Some confirmedToDate ->
                        yield Html.div [
                            prop.className "bar-wrapper"
                            prop.children [
                                let activeCases = dp.ActiveCases |> Option.defaultValue 0
                                let deceasedToDate = dp.DeceasedToDate |> Option.defaultValue 0
                                let recoveredToDate = confirmedToDate - deceasedToDate - activeCases
                                let aHeight = Math.Ceiling(float activeCases * float barMaxHeight / float maxValue)
                                let dHeight = Math.Ceiling(float deceasedToDate * float barMaxHeight / float maxValue)
                                let rHeight = confirmedToDate * barMaxHeight / maxValue - int dHeight - int aHeight
                                Html.div [
                                    prop.className "bar"
                                    prop.children [
                                        if state.View = TotalConfirmedCases then
                                            Html.div [
                                                prop.style [ style.height (int dHeight) ]
                                                prop.className "bar--deceased" ]
                                            Html.div [
                                                prop.style [ style.height rHeight ]
                                                prop.className "bar--recovered" ]
                                        Html.div [
                                            prop.style [ style.height (int aHeight) ]
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
                                            if (activeCases > 0) then
                                                prop.className "active"
                                                prop.children [
                                                    Html.span [ prop.text (I18N.t "charts.municipalities.active") ]
                                                    Html.b [ prop.text (I18N.NumberFormat.formatNumber(activeCases)) ] ] ]
                                        Html.div [
                                            if (recoveredToDate > 0) then
                                                prop.className "recovered"
                                                prop.children [
                                                    Html.span [ prop.text (I18N.t "charts.municipalities.recovered") ]
                                                    Html.b [ prop.text (I18N.NumberFormat.formatNumber(recoveredToDate)) ] ] ]
                                        Html.div [
                                            if (deceasedToDate > 0) then
                                                prop.className "deceased"
                                                prop.children [
                                                    Html.span [ prop.text (I18N.t "charts.municipalities.deceased") ]
                                                    Html.b [ prop.text (I18N.NumberFormat.formatNumber(deceasedToDate)) ] ] ]
                                        Html.div [
                                            prop.className "confirmed"
                                            prop.children [
                                                Html.span [ prop.text (I18N.t "charts.municipalities.all") ]
                                                Html.b [ prop.text (I18N.NumberFormat.formatNumber(confirmedToDate)) ] ] ]
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
                                prop.text (I18N.NumberFormat.formatNumber(municipality.ActiveCases |> Option.defaultValue 0)) ]
                            Html.div [
                                prop.className "total-and-new"
                                prop.children [
                                    Html.div [
                                        prop.className "total"
                                        prop.text (I18N.NumberFormat.formatNumber(municipality.MaxConfirmedCases |> Option.defaultValue 0)) ]
                                    if municipality.NewCases.IsSome then
                                        Html.div [
                                            prop.className "new"
                                            prop.text (sprintf "(+%s)" (I18N.NumberFormat.formatNumber(municipality.NewCases |> Option.defaultValue 0))) ]
                                ]
                            ]
                            Html.div [
                                prop.className "date"
                                prop.text (I18N.tOptions "days.date" {| date = municipality.LastConfirmedCase.Date |})]
                        ]
                    ]
                ]
            ]
            if Highcharts.showDoublingTimeFeatures then
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
        | Some s1, Some s2 -> String.Compare(s1, s2)

    let compareActiveCases m1 m2 =
        if m1.ActiveCases < m2.ActiveCases then 1
        else if m1.ActiveCases > m2.ActiveCases then -1
        else compareStringOption m1.Name m2.Name

    let compareMaxCases m1 m2 =
        if m1.MaxConfirmedCases < m2.MaxConfirmedCases then 1
        else if m1.MaxConfirmedCases > m2.MaxConfirmedCases then -1
        else compareStringOption m1.Name m2.Name

    let sortedMunicipalities =
        match state.View with
        | ActiveCases ->
            dataFilteredByRegion
            |> Seq.sortWith compareActiveCases
        | TotalConfirmedCases ->
            dataFilteredByRegion
            |> Seq.sortWith compareMaxCases
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
                else if m1.NewCases < m2.NewCases then 1
                else if m1.NewCases > m2.NewCases then -1
                else compareActiveCases m1 m2)

    let truncatedData, displayShowAllButton =
        if state.ShowAll then sortedMunicipalities, true
        else if Seq.length sortedMunicipalities <= collapsedMunicipalityCount then sortedMunicipalities, false
        else Seq.take collapsedMunicipalityCount sortedMunicipalities, true

    (truncatedData |> Seq.map (fun municipality -> renderMunicipality state municipality), displayShowAllButton)

let renderShowMore showAll dispatch =

    let scrollToElement (e: MouseEvent) =
            e.preventDefault ()

            dispatch ToggleShowAll

            let element =
                document.getElementById "municipalities-chart"

            let offset = -100.

            let position =
                element.getBoundingClientRect().top
                + window.pageYOffset
                + offset

            if showAll then
                window.scrollTo
                    ({| top = position
                        behavior = "auto" |}
                     |> unbox) // behavior = smooth | auto

    Html.div [
        prop.className "show-all"
        prop.children [
            Html.div [
                Html.a [
                    prop.className "btn btn-primary"
                    prop.text (if showAll then I18N.t "charts.municipalities.showLess" else I18N.t "charts.municipalities.showAll")
                    prop.onClick scrollToElement
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
        prop.onChange (SearchInputChanged >> dispatch)
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
        prop.onChange (RegionFilterChanged >> dispatch)
    ]

let renderView (currentView : View) dispatch =

    let renderSelector (view : View) (label : string) =
        let defaultProps =
            [ prop.text label
              Utils.classes [
                  (true, "chart-display-property-selector__item")
                  (view = currentView, "selected") ] ]
        if view = currentView
        then Html.div defaultProps
        else Html.div ((prop.onClick (fun _ -> ViewChanged view |> dispatch)) :: defaultProps)

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            Html.text (I18N.t "charts.common.sortBy")
            if Highcharts.showDoublingTimeFeatures then
                renderSelector View.DoublingTime (I18N.t "charts.municipalities.viewDoublingTime")
            renderSelector View.LastConfirmedCase (I18N.t "charts.municipalities.viewLast")
            renderSelector View.ActiveCases (I18N.t "charts.municipalities.viewActive")
            renderSelector View.TotalConfirmedCases (I18N.t "charts.municipalities.viewTotal")
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
                renderView state.View dispatch
            ]
            Html.div [
                prop.className "municipalities"
                prop.children renderedMunicipalities ]
            (if showMore then renderShowMore state.ShowAll dispatch else Html.none)
            Html.div [
                prop.className "credits"
                prop.children [
                    Html.a [
                        prop.href "https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19"
                        prop.text (sprintf "%s: %s, %s"
                            (I18N.t "charts.common.dataSource")
                            (I18N.t "charts.common.dsNIJZ")
                            (I18N.t "charts.common.dsMZ"))
                    ]
                ]
            ]
        ]
    ]

    // trigger event for iframe resize
    let evt = document.createEvent("event")
    evt.initEvent("chartLoaded", true, true)
    document.dispatchEvent(evt) |> ignore

    element

let chart =
    React.functionComponent (fun (props: {| data: RegionsData |}) ->
        let state, dispatch = QueryParams.useElmishWithQueryParams (props.data |> init) update stateToQueryParams Msg.QueryParamsUpdated
        React.useMemo((fun () -> render state dispatch), [|state|])
        )
