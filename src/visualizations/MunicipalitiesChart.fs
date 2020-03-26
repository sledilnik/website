
[<RequireQualifiedAccess>]
module MunicipalitiesChart

open Elmish

open Feliz
open Feliz.ElmishComponents

open Types

type State =
    { Data : RegionsData
      Regions : Region list }

type Msg = unit

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

    { Data = data ; Regions = regions }, Cmd.none

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    state, Cmd.none

let beautifyMunicipalityName (name : string) =
    (String.mapi (fun i c -> if i = 0 then System.Char.ToUpper c else c) name).Replace("_", " ")

let excludeMunicipalities = Set.ofList ["kraj"]

let renderMunicipalities (state : State) dispatch =
    let barMaxHeight = 50
    let showMaxBars = 20

    let pivotedData = seq {
        for dataPoint in state.Data do
            for region in dataPoint.Regions do
                for municipality in region.Municipalities do
                    if not (Set.contains municipality.Name excludeMunicipalities)  then
                        yield {| Date = dataPoint.Date
                                 Region = region.Name
                                 Municipality = municipality.Name
                                 PositiveTests = municipality.PositiveTests |} }

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
    |> Seq.map (fun (key, data) ->
        let trimmedData = Seq.skip ((Seq.length data) - showMaxBars) data

        let maxValue =
            try
                trimmedData
                |> Seq.map (fun d -> d.PositiveTests)
                |> Seq.filter Option.isSome
                |> Seq.max
            with
                | _ -> None

        let bars =
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
                                                prop.text (sprintf "%d-%d-%d" d.Date.Year d.Date.Month d.Date.Day) ]
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
                    prop.text (beautifyMunicipalityName key.Municipality)
                ]
                Html.div [
                    prop.className "positive-tests"
                    prop.children [
                        Html.div [
                            prop.className "bars"
                            prop.children bars
                        ]
                        Html.div [
                            prop.className "total-and-date"
                            prop.children [
                                Html.div [
                                    prop.className "total"
                                    prop.text totalPositiveTests ]
                                Html.div [
                                    prop.className "date"
                                    prop.text (sprintf "%d-%d-%d" lastDataPoint.Date.Year lastDataPoint.Date.Month lastDataPoint.Date.Day) ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    )

let render (state : State) dispatch =
    Html.div [
        prop.className "municipalities"
        prop.children (renderMunicipalities state dispatch)
    ]

type Props = {
    data : RegionsData
}

let municipalitiesChart (props : Props) =
    React.elmishComponent("MunicipalitiesChart", init props.data, update, render)
