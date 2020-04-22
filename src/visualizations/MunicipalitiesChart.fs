[<RequireQualifiedAccess>]
module MunicipalitiesChart

open Elmish
open Browser
open Fable.Core.JsInterop

open Feliz
open Feliz.ElmishComponents

open Types

let barMaxHeight = 50
let showMaxBars = 30
let collapsedMnicipalityCount = 24

let excludedMunicipalities = Set.ofList ["kraj" ; "tujina"]

type Region =
    { Key : string
      Name : string option }

type TotalPositiveTestsForDate =
    { Date : System.DateTime
      TotalPositiveTests : int option }

type Municipality =
    { Key : string
      Name : string option
      RegionKey : string
      DoublingTime : float option
      TotalPositiveTest : TotalPositiveTestsForDate seq }

type SortBy =
    | TotalPositiveTests
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
            | "total-positive-tests" -> Some TotalPositiveTests
            | "time-to-double" -> Some DoublingTime
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
        |> List.map (fun reg -> { Key = reg.Name ; Name = (Utils.Dictionaries.regions.TryFind reg.Name) |> Option.map (fun region -> region.Name) })
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
                                     TotalPositiveTests = municipality.PositiveTests |} }
        |> Seq.groupBy (fun dp -> dp.MunicipalityKey)
        |> Seq.map (fun (municipalityKey, dp) ->
            let doublingTime =
                dp
                |> Seq.map (fun dp -> {| Date = dp.Date ; Value = dp.TotalPositiveTests |})
                |> Seq.toList
                |> Utils.findDoublingTime
            { Key = municipalityKey
              Name = (Utils.Dictionaries.municipalities.TryFind municipalityKey) |> Option.map (fun municipality -> municipality.Name)
              RegionKey = (dp |> Seq.last).RegionKey
              DoublingTime = doublingTime
              TotalPositiveTest =
                dp
                |> Seq.map (fun dp -> { Date = dp.Date ; TotalPositiveTests = dp.TotalPositiveTests })
                |> Seq.sortBy (fun dp -> dp.Date)
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
            | None -> TotalPositiveTests
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

    let data = municipality.TotalPositiveTest

    let truncatedData = data |> Seq.skip ((Seq.length data) - showMaxBars)

    let maxValue =
        try
            truncatedData
            |> Seq.map (fun d -> d.TotalPositiveTests)
            |> Seq.filter Option.isSome
            |> Seq.max
        with
            | _ -> None

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
                        prop.text "Podvojitev v "
                    ]
                    Html.span [
                        prop.className "value"
                        prop.text (sprintf "%d %s" displayValue (Utils.daysMestnik displayValue))
                    ]
                ]
            ]

    let renderedBars =
        match maxValue with
        | None -> Seq.empty
        | Some maxValue ->
            seq {
                for i, d in truncatedData |> Seq.mapi (fun i d -> i, d) do
                    match d.TotalPositiveTests with
                    | None ->
                        yield Html.div [
                            prop.className "bar bar--empty"
                        ]
                    | Some positiveTests ->
                        yield Html.div [
                            prop.className "bar-wrapper"
                            prop.children [
                                Html.div [
                                    prop.className "bar"
                                    prop.style [ style.height (positiveTests * barMaxHeight / maxValue) ] ]
                                Html.div [
                                    prop.className "total-and-date total-and-date--hover"
                                    prop.children [
                                        Html.div [
                                            prop.className "total"
                                            prop.text positiveTests ]
                                        Html.div [
                                            prop.className "date"
                                            prop.text (sprintf "%d. %s" d.Date.Day (Utils.monthNameOfdate d.Date)) ]
                                    ]
                                ]
                            ]
                        ]
                }

    let lastDataPoint = Seq.last data

    let totalPositiveTests =
        match lastDataPoint.TotalPositiveTests with
        | None -> ""
        | Some v -> v.ToString()

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
                                prop.className "total"
                                prop.text totalPositiveTests ]
                            Html.div [
                                prop.className "date"
                                prop.text (sprintf "%d. %s" lastDataPoint.Date.Day (Utils.monthNameOfdate lastDataPoint.Date)) ]
                        ]
                    ]
                ]
            ]
            renderedDoublingTime
        ]
    ]

let renderMunicipalities (state : State) dispatch =

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
        | Some s1, None -> 1
        | None, Some s2 -> -1
        | Some s1, Some s2 -> System.String.Compare(s1, s2)

    let sortedMunicipalities =
        match state.SortBy with
        | TotalPositiveTests ->
            dataFilteredByRegion
            |> Seq.sortWith (fun m1 m2 ->
                let lastDataPoint municipality =
                    municipality.TotalPositiveTest
                    |> Seq.choose (fun dp ->
                        match dp.TotalPositiveTests with
                        | None -> None
                        | Some totalPositiveTests -> Some {| Date = dp.Date ; TotalPositiveTests = totalPositiveTests |})
                    |> Seq.sortBy (fun dp -> dp.Date)
                    |> Seq.tryLast
                let t1 = lastDataPoint m1 |> Option.map (fun m -> m.TotalPositiveTests) |> Option.defaultValue 0
                let t2 = lastDataPoint m2 |> Option.map (fun m -> m.TotalPositiveTests) |> Option.defaultValue 0
                if t1 > t2 then -1
                else if t1 < t2 then 1
                else
                    match m1.Name, m2.Name with
                    | None, None -> 0
                    | Some n1, None -> 1
                    | None, Some n2 -> -1
                    | Some n1, Some n2 -> System.String.Compare(n1, n2))
        | DoublingTime ->
            dataFilteredByRegion
            |> Seq.sortWith (fun m1 m2 ->
                match m1.DoublingTime, m2.DoublingTime with
                | None, None -> compareStringOption m1.Name m2.Name
                | Some d1, None -> -1
                | None, Some d2 -> 1
                | Some d1, Some d2 ->
                    if d1 > d2 then 1
                    else if d1 < d2 then -1
                    else compareStringOption m1.Name m2.Name)

    let truncatedData, displayShowAllButton =
        if state.ShowAll = true
        then sortedMunicipalities, true
        else if Seq.length sortedMunicipalities <= collapsedMnicipalityCount then sortedMunicipalities, false
        else Seq.take collapsedMnicipalityCount sortedMunicipalities, true

    (truncatedData |> Seq.map (fun municipality -> renderMunicipality municipality), displayShowAllButton)

let renderShowMore showAll dispatch =
    Html.div [
        prop.className "show-all"
        prop.children [
            Html.div [
                Html.button [
                    prop.className "btn btn-primary btn-sm"
                    prop.text (if showAll then "Prikaži manj občin" else "Prikaži vse občine")
                    prop.onClick (fun _ -> dispatch ToggleShowAll)
                ]
            ]
        ]
    ]

let renderSearch (query : string) dispatch =
    Html.input [
        prop.className "form-control form-control-sm filters__query"
        prop.type' .text
        prop.placeholder "Poišči občino"
        prop.valueOrDefault query
        prop.onChange (fun query -> SearchInputChanged query |> dispatch)
    ]

let renderRegionSelector (regions : Region list) (selected : string) dispatch =
    let renderedRegions = seq {
        yield Html.option [
            prop.text "Vse regije"
            prop.value ""
        ]

        for region in regions do
            let label =
                match region.Name with
                | None -> region.Key
                | Some name -> name
            yield Html.option [
                prop.text label
                prop.value region.Key
            ]
    }

    Html.select [
        prop.value selected
        prop.className "form-control form-control-sm filters__region"
        prop.children renderedRegions
        prop.onChange (fun (value : string) -> RegionFilterChanged value |> dispatch)
    ]

let renderSortBy (currenSortBy : SortBy) dispatch =

    let renderSelector (currentSortBy : SortBy) (sortBy : SortBy) (label : string) =
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
            Html.text "Razvrsti:"
            renderSelector currenSortBy SortBy.TotalPositiveTests "Absolutno"
            renderSelector currenSortBy SortBy.DoublingTime "Dnevih podvojitve"
        ]
    ]

let render (state : State) dispatch =
    let renderedMunicipalities, showMore = renderMunicipalities state dispatch

    let element = Html.div [
        prop.children [
            Html.div [
                prop.className "filter-and-sort"
                prop.children [
                    Html.div [
                        prop.className "filters"
                        prop.children [
                            renderRegionSelector state.Regions state.FilterByRegion dispatch
                            renderSearch state.SearchQuery dispatch
                        ]
                    ]
                    renderSortBy state.SortBy dispatch
                ]
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
