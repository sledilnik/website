[<RequireQualifiedAccess>]
module MunicipalitiesChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Types

let barMaxHeight = 50
let showMaxBars = 30
let collapsedMnicipalityCount = 24

let excludedMunicipalities = Set.ofList ["kraj" ; "tujina"]

type State =
    { Data : RegionsData
      Regions : Region list
      ShowAll : bool
      SearchQuery : string
      FilterByRegion : string }

type Msg =
    | ToggleShowAll
    | SearchInputChanged of string
    | RegionFilterChanged of string

let regionTotal (region : Region) : int =
    region.Municipalities
    |> List.map (fun city -> city.PositiveTests)
    |> List.choose id
    |> List.sum

let init (data : RegionsData) : State * Cmd<Msg> =
    let lastDataPoint = List.last data
    let regions =
        lastDataPoint.Regions
        |> List.filter (fun region -> Set.contains region.Name Utils.Dictionaries.excludedRegions |> not)
        |> List.sortByDescending (fun region -> regionTotal region)

    { Data = data ; Regions = regions ; ShowAll = false ; SearchQuery = "" ; FilterByRegion = "" }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleShowAll ->
        { state with ShowAll = not state.ShowAll }, Cmd.none
    | SearchInputChanged query ->
        { state with SearchQuery = query }, Cmd.none
    | RegionFilterChanged region ->
        { state with FilterByRegion = region }, Cmd.none

let renderMunicipality
        (key : {| Municipality: string; Region: string |})
        (data : {| Date: System.DateTime; Municipality: string; PositiveTests: int option; Region: string |} seq) =

    let trimmedData = Seq.skip ((Seq.length data) - showMaxBars) data

    let maxValue =
        try
            trimmedData
            |> Seq.map (fun d -> d.PositiveTests)
            |> Seq.filter Option.isSome
            |> Seq.max
        with
            | _ -> None

    let reversedDoublingTimeValues =
        trimmedData
        |> Seq.mapi (fun i p -> i, p.PositiveTests)
        |> Seq.choose (fun (i, p) ->
            match p with
            | None -> None
            | Some v -> Some {| Day = i ; PositiveTests = v |})
        |> Seq.rev
        |> Array.ofSeq

    let doublingTime =
        data
        |> Seq.map (fun dp -> {| Date = dp.Date ; Value = dp.PositiveTests |})
        |> Seq.toList
        |> Utils.findDoublingTime

    let renderedDoublingTime =
        match doublingTime with
        | None -> Html.none
        | Some value ->
            // printfn "%s - %f" key.Municipality value
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
                for i, d in trimmedData |> Seq.mapi (fun i d -> i, d) do
                    match d.PositiveTests with
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
        match lastDataPoint.PositiveTests with
        | None -> ""
        | Some v -> v.ToString()

    Html.div [
        prop.className "municipality"
        prop.children [
            Html.div [
                prop.className "name"
                prop.text (
                    match Utils.Dictionaries.municipalities.TryFind key.Municipality with
                    | None -> key.Municipality
                    | Some municipality -> municipality.Name)
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

    let pivotedData = seq {
        for dataPoint in state.Data do
            for region in dataPoint.Regions do
                for municipality in region.Municipalities do
                    if not (Set.contains municipality.Name excludedMunicipalities)  then
                        yield {| Date = dataPoint.Date
                                 Region = region.Name
                                 Municipality = municipality.Name
                                 PositiveTests = municipality.PositiveTests |} }

    let sortedData =
        pivotedData
        |> Seq.groupBy (fun d -> {| Region = d.Region ; Municipality = d.Municipality |})
        |> Seq.sortWith (fun (_, data1) (_, data2) ->
            let last1, last2 = (Seq.last data1), (Seq.last data2)
            match last1.PositiveTests, last2.PositiveTests with
            | None, None -> System.String.Compare(last1.Municipality, last2.Municipality)
            | Some v, None -> -1
            | None, Some v -> 1
            | Some v1, Some v2 ->
                if v1 > v2 then -1
                else if v1 < v2 then 1
                else System.String.Compare(last1.Municipality, last2.Municipality))

    let dataFilteredByQuery =
        let query = state.SearchQuery.Trim().ToLower() |> Utils.transliterateCSZ
        if  query = ""
        then sortedData
        else
            sortedData
            |> Seq.filter (fun (item, _) ->
               let name =
                match Utils.Dictionaries.municipalities.TryFind item.Municipality with
                | None -> item.Municipality
                | Some municipality -> municipality.Name
               (name.ToLower() |> Utils.transliterateCSZ).Contains(query))

    let dataFikteredByRegion =
        dataFilteredByQuery
        |> Seq.filter (fun (item, _) ->
            if state.FilterByRegion = ""
            then true
            else item.Region = state.FilterByRegion
        )

    let truncatedData, displayShowAllButton =
        if state.ShowAll = true
        then dataFikteredByRegion, true
        else if Seq.length dataFikteredByRegion <= collapsedMnicipalityCount then dataFikteredByRegion, false
        else Seq.take collapsedMnicipalityCount dataFikteredByRegion, true

    truncatedData |> Seq.map (fun (key, data) -> renderMunicipality key data), displayShowAllButton

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
                match Utils.Dictionaries.regions.TryFind region.Name with
                | None -> region.Name
                | Some value -> value.Name
            yield Html.option [
                prop.text label
                prop.value region.Name
            ]
    }

    Html.select [
        prop.value selected
        prop.className "form-control form-control-sm filters__region"
        prop.children renderedRegions
        prop.onChange (fun (value : string) -> RegionFilterChanged value |> dispatch)
    ]

let render (state : State) dispatch =
    let renderedMunicipalities, showMore = renderMunicipalities state dispatch

    Html.div [
        prop.children [
            Html.div [
                prop.className "filters"
                prop.children [
                    renderRegionSelector state.Regions state.FilterByRegion dispatch
                    renderSearch state.SearchQuery dispatch
                ]
            ]
            Html.div [
                prop.className "municipalities"
                prop.children renderedMunicipalities ]
            (if showMore then renderShowMore state.ShowAll dispatch else Html.none)
        ]
    ]

let municipalitiesChart (props : {| data : RegionsData |}) =
    React.elmishComponent("MunicipalitiesChart", init props.data, update, render)
