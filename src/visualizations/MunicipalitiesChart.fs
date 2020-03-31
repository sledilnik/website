
[<RequireQualifiedAccess>]
module MunicipalitiesChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Types

let barMaxHeight = 50
let showMaxBars = 20
let collapsedMnicipalityCount = 24
let doublingTimeInterval = 7

type State =
    { Data : RegionsData
      Regions : Region list
      ShowAll : bool
      SearchQuery : string }

type Msg =
    | ToggleShowAll
    | SearchInputChanged of string

let regionTotal (region : Region) : int =
    region.Municipalities
    |> List.map (fun city -> city.PositiveTests)
    |> List.choose id
    |> List.sum

let init (data : RegionsData) : State * Cmd<Msg> =
    let lastDataPoint = List.last data
    let regions =
        lastDataPoint.Regions
        |> List.sortByDescending (fun region -> regionTotal region)

    { Data = data ; Regions = regions ; ShowAll = false ; SearchQuery = "" }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ToggleShowAll ->
        { state with ShowAll = not state.ShowAll }, Cmd.none
    | SearchInputChanged query ->
        { state with SearchQuery = query }, Cmd.none

let excludeMunicipalities = Set.ofList ["kraj"]

let calculateDoublingTime (v1 : {| Day : int ; PositiveTests : int |}) (v2 : {| Day : int ; PositiveTests : int |}) =
    let v1, v2, dt = float v1.PositiveTests, float v2.PositiveTests, float (v2.Day - v1.Day)
    if v1 = v2 then None
    else
        let value = log10 2.0 / log10 ((v2 / v1) ** (1.0 / dt))
        if value < 0.0 then None
        else Some value

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
        match reversedDoublingTimeValues.Length with
        | 0 | 1 -> None
        | length ->
            if length >= doublingTimeInterval then
                calculateDoublingTime reversedDoublingTimeValues.[doublingTimeInterval - 1] reversedDoublingTimeValues.[0]
            else
                calculateDoublingTime reversedDoublingTimeValues.[length - 1] reversedDoublingTimeValues.[0]

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
                    if not (Set.contains municipality.Name excludeMunicipalities)  then
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

    let filteredData =
        let query = state.SearchQuery.Trim().ToLower()
        if  query = ""
        then sortedData
        else
            sortedData
            |> Seq.filter (fun (item, _) ->
               let name =
                match Utils.Dictionaries.municipalities.TryFind item.Municipality with
                | None -> item.Municipality
                | Some municipality -> municipality.Name
               name.ToLower().Contains(query))

    let truncatedData, displayShowAllButton =
        if state.ShowAll = true
        then filteredData, true
        else if Seq.length filteredData <= collapsedMnicipalityCount then filteredData, false
        else Seq.take collapsedMnicipalityCount filteredData, true

    truncatedData |> Seq.map (fun (key, data) -> renderMunicipality key data), displayShowAllButton

let renderShowMore showAll dispatch =
    Html.div [
        prop.className "show-all"
        prop.children [
            Html.div [
                Html.button [
                    prop.className "btn btn-primary btn-sm"
                    prop.text (if showAll then "Prikaži manj občin" else "Prikaži več občin")
                    prop.onClick (fun _ -> dispatch ToggleShowAll)
                ]
            ]
        ]
    ]

let render (state : State) dispatch =
    let renderedMunicipalities, showMore = renderMunicipalities state dispatch

    Html.div [
        prop.children [
            Html.div [
                Html.input [
                    prop.className "form-control"
                    prop.type' .text
                    prop.placeholder "Išči po občinah"
                    prop.valueOrDefault state.SearchQuery
                    prop.onChange (fun query -> SearchInputChanged query |> dispatch)
                ]
            ]
            Html.div [
                prop.className "municipalities"
                prop.children renderedMunicipalities ]
            (if showMore then renderShowMore state.ShowAll dispatch else Html.none)
        ]
    ]

type Props = {
    data : RegionsData
}

let municipalitiesChart (props : Props) =
    React.elmishComponent("MunicipalitiesChart", init props.data, update, render)
