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

open Data.SchoolStatus

let chartText = I18N.chartText "schoolStatus"

type State =
    { SchoolStatus: SchoolStatusMap
      Error: string option
      SelectedSchool: string
      RangeSelectionButtonIndex: int }

type Msg =
    | ConsumeSchoolStatusData of Result<SchoolStatusMap, string>
    | ConsumeServerError of exn
    | SchoolsFilterChanged of string
    | RangeSelectionChanged of int


let init (queryObj: obj): State * Cmd<Msg> =
    let state =
        { SchoolStatus = Map.empty
          Error = None
          SelectedSchool = ""
          RangeSelectionButtonIndex = 0 }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    // trigger event for iframe resize
    let evt = document.createEvent ("event")
    evt.initEvent ("chartLoaded", true, true)
    document.dispatchEvent (evt) |> ignore

    match msg with
    | ConsumeSchoolStatusData (Ok data) -> { state with SchoolStatus = data }, Cmd.none
    | ConsumeSchoolStatusData (Error err) -> { state with Error = Some err }, Cmd.none
    | ConsumeServerError ex -> { state with Error = Some ex.Message }, Cmd.none
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none
    | SchoolsFilterChanged schoolId ->
        let cmd =
            Cmd.OfAsync.either loadData schoolId ConsumeSchoolStatusData ConsumeServerError

        { state with
              SchoolStatus = Map.empty
              SelectedSchool = schoolId },
        cmd


let renderChart schoolStatus state dispatch =
 
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

        [| 
            {| pointWidth = 15
               dataLabels = {| format = "{point.label}" |}
               data = empData |} 
            {| pointWidth = 15
               dataLabels = {| format = "{point.label}" |}
               data = attData |} 
            {| pointWidth = 15
               dataLabels = {| format = "{point.label}" |}
               data = regData |} 
        |]

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
           series = allSeries
           plotOptions = pojo {| series = {| dataLabels = {| enabled = true; inside = true |} |} |} 
           tooltip = pojo {| shared = false; split = false
                             pointFormat = "{point.text}"
                             xDateFormat = "<b>" + chartText "date" + "</b>" |}
           credits = chartCreditsMIZS           
    |}


let renderSchool (state: State) (schoolId: string) (schoolStatus: SchoolStatus) dispatch =
    Html.div [ prop.className "school"
               prop.children [ Html.div [ prop.style [ style.height 480 ]
                                          prop.className "highcharts-wrapper"
                                          prop.children [ renderChart schoolStatus state dispatch
                                                           |> Highcharts.chart ] ] ] ]

let renderSchools (state: State) dispatch =
    (state.SchoolStatus
     |> Seq.map (fun school -> renderSchool state school.Key school.Value dispatch))

let renderSchoolSelector state dispatch =
    let renderedSchools =
        Utils.Dictionaries.schools 
        |> Map.toSeq
        |> Seq.map (fun school -> school |> snd)
        |> Seq.sortBy (fun sData -> sData.Name)
        |> Seq.map (fun sData ->
                        Html.option [
                            prop.text sData.Name
                            prop.value sData.Key
                        ] )

    Html.select [
        prop.value state.SelectedSchool
        prop.className "form-control form-control-sm filters__school"
        prop.children renderedSchools
        prop.onChange (SchoolsFilterChanged >> dispatch)
    ]

let render (state: State) dispatch =
    let element =
        Html.div [ prop.children [ Utils.renderChartTopControls [ Html.div [ prop.className "filters"
                                                                             prop.children [ renderSchoolSelector 
                                                                                                state dispatch ] ] ]
                                   Html.div [ prop.className "schools"
                                              prop.children (renderSchools state dispatch) ] ] ]

    // trigger event for iframe resize
    let evt = document.createEvent ("event")
    evt.initEvent ("chartLoaded", true, true)
    document.dispatchEvent (evt) |> ignore

    element

let schoolStatusChart (props: {| query: obj |}) =
    React.elmishComponent ("SchoolStatusChart", init props.query, update, render)
