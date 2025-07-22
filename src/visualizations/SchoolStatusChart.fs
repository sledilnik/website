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

type FilterType =
    | ShowActive
    | ShowAll
  with
    static member All = [ ShowActive; ShowAll ]
    static member Default = ShowAll
    member this.GetName =
        match this with
        | ShowActive  -> chartText "showActive"
        | ShowAll -> chartText "showAll"

let filterChangesDate =
    DateTime.Today.AddDays(-6.)
let filterActiveDate =
    DateTime.Today.AddDays(-28.)

type State =
    { SchoolStatusMap : RemoteData<SchoolStatusMap, string>
      SchoolStatus : RemoteData<SchoolStatus option, string>
      SelectedSchool : School option
      FilterType : FilterType }

type Msg =
    | ConsumeSchoolStatusMapData of Result<SchoolStatusMap, string>
    | ConsumeSchoolStatusData of Result<SchoolStatus option, string>
    | ConsumeServerError of exn
    | ChangedSchoolSelected of string
    | SchoolSelected of School
    | FilterTypeChanged of FilterType
    | ResetSearch


type Query (query : obj) =
    member this.Query = query
    member this.SchoolId =
        match query?("schoolId") with
        | Some (id : string) -> Some id
        | _ -> None

let defaultCmd = Cmd.OfAsync.either loadData filterChangesDate ConsumeSchoolStatusMapData ConsumeServerError

let init (queryObj: obj): State * Cmd<Msg> =
    let query = Query(queryObj)

    let defaultState =
        { SchoolStatusMap = Loading
          SchoolStatus = NotAsked
          SelectedSchool = None
          FilterType = FilterType.Default }

    match query.SchoolId with
    | Some id ->
        match Utils.Dictionaries.schools.TryFind(id) with
        | Some school ->
            let state =
                { defaultState with
                    SchoolStatus = Loading
                    SelectedSchool = Some school
                    FilterType = FilterType.Default }
            state, Cmd.OfAsync.either loadSchoolData school.Key ConsumeSchoolStatusData ConsumeServerError
        | _ -> defaultState, defaultCmd
    | _ -> defaultState, defaultCmd

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | ConsumeSchoolStatusMapData (Ok data) ->
        { state with SchoolStatusMap = Success data }, Cmd.none
    | ConsumeSchoolStatusMapData (Error err) ->
        { state with SchoolStatusMap = Failure err }, Cmd.none
    | ConsumeSchoolStatusData (Ok data) ->
        { state with SchoolStatus = Success data }, Cmd.none
    | ConsumeSchoolStatusData (Error err) ->
        { state with SchoolStatus = Failure err }, Cmd.none
    | ConsumeServerError ex ->
        { state with SchoolStatus = Failure ex.Message }, Cmd.none
    | FilterTypeChanged ft ->
        { state with FilterType = ft  }, Cmd.none
    | ResetSearch ->
        { state with SchoolStatusMap = Loading
                     SchoolStatus = NotAsked
                     SelectedSchool = None
                     FilterType = FilterType.Default }, defaultCmd
    | ChangedSchoolSelected id ->
        let cmd =
            match Utils.Dictionaries.schools.TryFind(id) with
            | Some school -> Cmd.ofMsg (SchoolSelected school)
            | None -> Cmd.none
        state, cmd
    | SchoolSelected school ->
        let cmd = Cmd.OfAsync.either loadSchoolData school.Key ConsumeSchoolStatusData ConsumeServerError
        let newState = {
            state with SchoolStatus = Loading
                       SelectedSchool = Some school
                       FilterType = FilterType.Default }
        newState, cmd

let renderChartOptions state schoolStatus =

    let allSeries =

        let personType pType schoolType =
            match pType, schoolType with
            | "E", _ -> (I18N.tt "schoolDict" pType)
            | _, "PV" -> chartText "kid"
            | _, "OS" | _, "OSPP" -> chartText "pupil"
            | _, "SS" | _, "DD" -> chartText "student-hs"
            | _, _ -> chartText "kid"

        let absenceText (absences : SchoolAbsence array) =
            // absences
            // |> Array.mapi (fun i abs ->
            //                 sprintf "- %s: %s<br>"
            //                     (I18N.tt "schoolDict" abs.personClass)
            //                     (I18N.tt "schoolDict" abs.reason))
            // |> String.Concat

            absences
            |> Array.groupBy (fun abs -> abs.reason)
            |> Array.map (fun (reason, absList) ->
                            sprintf "- %s: %d<br>"
                                (I18N.tt "schoolDict" reason)
                                absList.Length)
            |> String.Concat


        let filterByDate (fromDate: DateTime) (toDate:DateTime) =
            match state.FilterType with
            | ShowActive -> toDate.CompareTo(filterActiveDate) >= 0
            | _ -> true

        let absenceData color pType startIdx =
            schoolStatus.absences
            |> Array.filter (fun abs -> abs.personType = pType)
            |> Array.filter (fun abs -> filterByDate abs.DateAbsentFrom abs.DateAbsentTo)
            |> Array.groupBy (fun abs -> (abs.JsDate12hAbsentFrom, abs.JsDate12hAbsentTo))
            |> Array.mapi (fun i ((f,t), v) ->
                        {|
                            x =  f
                            x2 = t
                            y = startIdx + i + 1
                            color = color
                            label = (personType pType v.[0].schoolType)
                                     + if v.Length > 1
                                       then sprintf " (%d)" v.Length
                                       else ""
                            text = absenceText v
                        |} |> pojo )

        let regimeData color startIdx =
            schoolStatus.regimes
            |> Array.filter (fun reg -> filterByDate reg.DateChangedFrom reg.DateChangedTo)
            |> Array.mapi (fun i reg ->
                        {|
                            x =  reg.JsDate12hChangedFrom
                            x2 = reg.JsDate12hChangedTo
                            y = startIdx + i + 1
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
            match state.FilterType with
            | ShowActive -> filterActiveDate |> jsTime12h
            | _ ->
                min
                     (filterActiveDate |> jsTime12h)
                     (schoolStatus.absences |> Array.map (fun v -> v.JsDate12hAbsentFrom)
                      |> Array.append (schoolStatus.regimes |> Array.map (fun v -> v.JsDate12hChangedFrom))
                      |> Array.reduce min)
        let endTime = // DateTime.Today |> jsTime12h     // We only show historic data
                     (schoolStatus.absences |> Array.map (fun v -> v.JsDate12hAbsentFrom)
                      |> Array.append (schoolStatus.regimes |> Array.map (fun v -> v.JsDate12hChangedFrom))
                      |> Array.reduce max)


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
                   showInLegend = true
                   data = regData |} |> pojo
            yield
                {| name = personStr + chartText "absence"
                   color = "#bda506"
                   pointWidth = 15
                   dataLabels = {| format = "{point.label}" |}
                   showInLegend = true
                   data = attData |} |> pojo
            yield
                {| name = chartText "employee" + chartText "absence"
                   color = "#dba51d"
                   pointWidth = 15
                   dataLabels = {| format = "{point.label}" |}
                   showInLegend = true
                   data = empData |} |> pojo
            yield
                {| showInLegend = false
                   opacity = 0
                   data = [| {| x = startTime
                                x2 = endTime
                                y = -1 |} |] |} |> pojo

            yield addContainmentMeasuresFlags startTime (Some endTime) |> pojo
        }

    let baseOptions =
        basicChart Linear "covid19-school-status"

    let xAxis = baseOptions.xAxis
                |> Array.map(fun xAxis ->
                    {| xAxis with
                        plotLines = xAxis.plotLines
                                    |> Array.append [| {| value=jsTime <| DateTime.Today
                                                          dashStyle = "Dot"
                                                          width = 2
                                                          color = "red" |} |> pojo |] |} )
    {| baseOptions with
           chart = pojo {| ``type`` = "xrange"; animation = false |}
           // xAxis = xAxis   // Do not show current date, as chart has only historic data
           yAxis = [| {| title = {| text = null |} |> pojo
                         labels = {| enabled = false |} |> pojo |} |]
           series = Seq.toArray allSeries
           plotOptions = pojo {| series = {| dataLabels = {| enabled = true; inside = true |} |} |}
           tooltip = pojo {| shared = false; split = false
                             pointFormat = "{point.text}"
                             xDateFormat = "<b>" + chartText "date" + "</b>" |}
           legend = pojo {| enabled = true ; layout = "horizontal" |}
           credits = chartCreditsMIZS
    |}

let renderChangedSchools (state: State) dispatch =

    let renderChanges regionsSchools dispatch =
        regionsSchools
        |> List.map (fun (id, status) -> Utils.Dictionaries.schools.TryFind(id), status)
        |> List.choose (fun (school, status) ->
                        match school with
                        | Some a -> Some (a, status)
                        | None -> None)
        |> List.sortBy (fun (school,status) -> school.Name)
        |> List.sortByDescending (fun (school,status) -> status.regimes.Length + status.absences.Length)
        |> List.map (fun (school, status) ->
                        Html.option [
                            prop.text (sprintf "%s (%d)" school.Name (status.regimes.Length + status.absences.Length))
                            prop.value school.Key
                        ] )

    let renderRegionChanges (schoolStatusMap: SchoolStatusMap) dispatch =
        let regionSummary reg nrSchools  =
            Html.option [
                prop.text (sprintf "%s (%d)" (I18N.tt "region" reg) nrSchools)
                prop.value ""
            ]

        schoolStatusMap
        |> Map.toList
        |> List.groupBy (fun (id, _) ->
            match Utils.Dictionaries.schools.TryFind(id) with
            | Some school -> school.Region
            | _ -> "")
        |> List.sortByDescending (fun (k,v) -> v.Length)
        |> List.map (fun (reg, regionSchools) ->
            if reg.Length > 0 then
                Html.select [
                    prop.value ""
                    prop.className "form-control form-control-sm changes__region"
                    prop.children (regionSummary reg regionSchools.Length :: renderChanges regionSchools dispatch)
                    prop.onChange (ChangedSchoolSelected >> dispatch)
                ]
            else Html.none)


    match state.SchoolStatusMap with
    | NotAsked -> Html.none
    | Loading -> Html.none
    | Failure err -> Html.text err
    | Success data ->
        Html.div [
            prop.className "changes"
            prop.children (
                Html.h4 (chartText "regionsWithChanges"):: renderRegionChanges data dispatch)
        ]


let renderSchool (state: State) dispatch =
    match state.SchoolStatus with
    | NotAsked ->
        renderChangedSchools state dispatch
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
                    renderChartOptions state schoolStatus |> Highcharts.chart
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
                Utils.Dictionaries.schoolsList
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
            AutoSuggest<School>.focusInputOnSuggestionClick false
        ]
    )

let renderFilterTypes (activeFilterType: FilterType) dispatch =
    let renderFilterSelector (filterType : FilterType) dispatch =
        let active = filterType = activeFilterType

        Html.div [
            prop.text filterType.GetName
            prop.onClick (fun _ -> FilterTypeChanged filterType |> dispatch )
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children (
            FilterType.All
            |> List.map (fun ft -> renderFilterSelector ft dispatch) ) ]

let renderSchoolControls (school: School) dispatch =
    Html.div [
        prop.className "name"
        prop.children [

            Html.h3 school.Name
            Html.img [
                prop.className "reset-search"
                prop.src "/images/close-dd.svg"
                prop.alt ("X")
                prop.onClick (fun _ -> ResetSearch |> dispatch )
            ]
        ]
    ]

let render (state: State) dispatch =
    Html.div [
        prop.children [
            Utils.renderChartTopControls [
                autoSuggestSchoolInput {| dispatch = dispatch |}
                (match state.SelectedSchool with
                 | None -> Html.none
                 | Some school -> renderSchoolControls school dispatch)
                // (match state.SelectedSchool with                        // Only historical data, no selector
                // | None -> Html.none
                // | Some school -> renderFilterTypes state.FilterType dispatch)
            ]
            Html.div [
                prop.className "school"
                prop.children (renderSchool state dispatch)
            ]
            Html.div [
                prop.className "disclaimer"
                prop.children [
                    Html.text (chartText "disclaimer")
                ]
            ]
        ]
    ]

let schoolStatusChart (props: {| query: obj |}) =
    React.elmishComponent ("SchoolStatusChart", init props.query, update, render)
