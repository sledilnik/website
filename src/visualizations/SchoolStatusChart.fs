[<RequireQualifiedAccess>]
module SchoolStatusChart

open System
open Elmish
open Browser
open Fable.Core.JsInterop

open Feliz
open Feliz.ElmishComponents

open Types
open Highcharts
open Components.AutoSuggest
open Utils.Dictionaries
open Data.SchoolStatus

type School = Utils.Dictionaries.School

let chartText = I18N.chartText "schoolStatus"

type State =
    { SchoolStatus : RemoteData<SchoolStatus option, string>
      SelectedSchool : School option
      RangeSelectionButtonIndex : int }

type Msg =
    | ConsumeSchoolStatusData of Result<SchoolStatus option, string>
    | ConsumeServerError of exn
    | SchoolSelected of School
    | RangeSelectionChanged of int

let init (queryObj: obj): State * Cmd<Msg> =
    let state =
        { SchoolStatus = NotAsked
          SelectedSchool = None
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    // trigger event for iframe resize
    let evt = document.createEvent ("event")
    evt.initEvent ("chartLoaded", true, true)
    document.dispatchEvent (evt) |> ignore

    match msg with
    | ConsumeSchoolStatusData (Ok data) ->
        { state with SchoolStatus = Success data }, Cmd.none
    | ConsumeSchoolStatusData (Error err) ->
        { state with SchoolStatus = Failure err }, Cmd.none
    | ConsumeServerError ex ->
        { state with SchoolStatus = Failure ex.Message }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none
    | SchoolSelected school ->
        let cmd = Cmd.OfAsync.either loadData school.Key ConsumeSchoolStatusData ConsumeServerError
        let newState = {
            state with SchoolStatus = Loading
                       SelectedSchool = Some school }
        newState, cmd

let renderChartOptions state schoolStatus dispatch =

    let allSeries =

        let personType pType schoolType =
            match pType, schoolType with
            | "E", _ -> (I18N.tt "schoolDict" pType)
            | _, "PV" -> chartText "kid"
            | _, "OS" | _, "OSPP" -> chartText "pupil"
            | _, "SS" | _, "DD" -> chartText "student-hs"
            | _, _ -> chartText "kid"

        let absenceText (absences : SchoolAbsence array) =
            absences
            |> Array.mapi (fun i abs ->
                            sprintf "- %s: %s<br>"
                                (I18N.tt "schoolDict" abs.personClass)
                                (I18N.tt "schoolDict" abs.reason))
            |> String.Concat

        let absenceData color pType startIdx =
            schoolStatus.absences
            |> Array.filter (fun abs -> abs.personType = pType)
            |> Array.groupBy (fun abs -> (abs.JsDate12hAbsentFrom, abs.JsDate12hAbsentTo))
            |> Array.mapi (fun i (d, v) ->
                        {|
                            x =  d |> fst
                            x2 = d |> snd
                            y = startIdx + i
                            color = color
                            label = (personType pType v.[0].schoolType)
                                     + if v.Length > 1
                                       then sprintf " (%d)" v.Length
                                       else ""
                            text = absenceText v
                        |} |> pojo )

        let regimeData color startIdx =
            schoolStatus.regimes
            |> Array.mapi (fun i reg ->
                        {|
                            x =  reg.JsDate12hChangedFrom
                            x2 = reg.JsDate12hChangedTo
                            y = startIdx + i
                            color = color
                            label = (I18N.tt "schoolDict" reg.personClass)
                                     + if reg.attendees > 1
                                       then sprintf " (%d)" reg.attendees
                                       else ""
                            text = sprintf "%s: %s<br>- %s<br>- %s<br>%s: %d"
                                    (chartText "unit")
                                    (I18N.tt "schoolDict" reg.personClass)
                                    (I18N.tt "schoolDict" reg.regime)
                                    (I18N.tt "schoolDict" reg.reason)
                                    (chartText "persons")
                                    reg.attendees
                        |} |> pojo )

        let empData = absenceData "#dba51d" "E" 0
        let attData = absenceData "#bda506" "A" empData.Length
        let regData = regimeData "#f4b2e0" (empData.Length+attData.Length)

        let min x y = if x < y then x else y
        let startTime = 
             schoolStatus.absences |> Array.map (fun v -> v.JsDate12hAbsentFrom) 
             |> Array.append (schoolStatus.regimes |> Array.map (fun v -> v.JsDate12hChangedFrom)) 
             |> Array.reduce min
        let endTime = DateTime.Today |> jsTime12h 

        let personStr =
            match state.SelectedSchool with
            | Some school -> personType "A" school.Type
            | _ -> personType "A" ""

        seq {
            yield
                {| name = chartText "regimeChange"
                   color = "#f4b2e0"
                   pointWidth = 15
                   dataLabels = {| format = "{point.label}" |}
                   data = empData |} |> pojo
            yield
                {| name = personStr + chartText "absence"
                   color = "#bda506"
                   pointWidth = 15
                   dataLabels = {| format = "{point.label}" |}
                   data = attData |} |> pojo
            yield
                {| name = chartText "employee" + chartText "absence"
                   color = "#dba51d"
                   pointWidth = 15
                   dataLabels = {| format = "{point.label}" |}
                   data = regData |} |> pojo
            yield
                {| showInLegend = false
                   opacity = 0
                   data = [| {| x = startTime
                                x2 = endTime
                                y = -1 |} |] |} |> pojo

            yield addContainmentMeasuresFlags startTime (Some endTime) |> pojo
        }

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        basicChartOptions Linear "covid19-school-status"
            state.RangeSelectionButtonIndex
            onRangeSelectorButtonClick

    {| baseOptions with
           chart = pojo {| ``type`` = "xrange"; animation = false |}
           yAxis = [| {| title = {| text = null |}
                         labels = {| enabled = false |} |} |]
           series = Seq.toArray allSeries
           plotOptions = pojo {| series = {| dataLabels = {| enabled = true; inside = true |} |} |}
           tooltip = pojo {| shared = false; split = false
                             pointFormat = "{point.text}"
                             xDateFormat = "<b>" + chartText "date" + "</b>" |}
           legend = pojo {| enabled = true ; layout = "horizontal" |}
           credits = chartCreditsMIZS
    |}

let renderSchool (state: State) dispatch =
    match state.SchoolStatus with
    | NotAsked -> Html.none
    | Loading -> Html.none
    | Failure err -> Html.text err
    | Success data ->
        match data with
        | None ->
            Html.div [
                prop.className "no-data"
                prop.children [
                    Html.text (chartText "noData")
                ]
            ]
        | Some schoolStatus ->
            Html.div [
                prop.className "highcharts-wrapper"
                prop.style [ style.height 480 ]
                prop.children [
                    renderChartOptions state schoolStatus dispatch |> Highcharts.chart
                ]
            ]

let autoSuggestSchoolInput = React.functionComponent(fun (props : {| dispatch : Msg -> unit |}) ->
        let (query, setQuery) = React.useState("")
        let (suggestions, setSuggestions) = React.useState(Array.empty<School>)

        let tokenizeQuery (query : string) =
            query.Split(" ")
            |> Array.map (fun (token : string) -> token.Trim().ToLower() |> Utils.transliterateCSZ)
            |> Array.distinct
            |> Array.toList

        let schoolMatches (tokens : string list) (school : School) =
            tokens
            |> List.forall (fun t -> (school.Name.ToLower() |> Utils.transliterateCSZ).Contains(t))

        let filterSchools (query : string) =
            let tokens = tokenizeQuery query
            let maxTokenLen =
                tokens
                |> List.map (fun t -> t.Length)
                |> List.max
            if maxTokenLen <= 1 then
                Array.empty
            else
                Utils.Dictionaries.schools
                |> Array.filter (schoolMatches tokens)

        let inputProps = {|
            value = query
            placeholder = chartText "search"
            onChange = (fun (ev) -> setQuery ev?target?value)
        |}

        AutoSuggest<School>.input [
            AutoSuggest<School>.inputProps inputProps
            AutoSuggest<School>.suggestions suggestions
            AutoSuggest<School>.onSuggestionsFetchRequested (fun query -> filterSchools query?value |> setSuggestions)
            AutoSuggest<School>.onSuggestionsClearRequested (fun () -> setSuggestions Array.empty<School>)
            AutoSuggest<School>.getSuggestionValue (fun (school : School) -> school.Key)
            AutoSuggest<School>.renderSuggestion (fun (school : School) -> Html.text school.Name)
            AutoSuggest<School>.onSuggestionSelected (fun ev payload -> setQuery "" ; props.dispatch (SchoolSelected payload.suggestion))
        ]
    )

let render (state: State) dispatch =
    let element =
        Html.div [
            prop.children [
                Utils.renderChartTopControls [
                    Html.div [
                        prop.className "filters"
                        prop.children [
                            autoSuggestSchoolInput {| dispatch = dispatch |}
                            match state.SelectedSchool with
                            | None -> Html.none
                            | Some school -> Html.h3 school.Name
                        ] ] ]
                Html.div [
                    prop.className "school"
                    prop.children (renderSchool state dispatch)
                ] ] ]

    // trigger event for iframe resize
    let evt = document.createEvent ("event")
    evt.initEvent ("chartLoaded", true, true)
    document.dispatchEvent (evt) |> ignore

    element

let schoolStatusChart (props: {| query: obj |}) =
    React.elmishComponent ("SchoolStatusChart", init props.query, update, render)
