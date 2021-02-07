[<RequireQualifiedAccess>]
module SchoolStatusChart

open System
open Elmish
open Browser
open Fable.Core.JsInterop

open Feliz
open Feliz.ElmishComponents

open Types

open Data.SchoolStatus

type State =
    { SchoolStatus: SchoolStatusMap
      Error: string option
      SearchQuery: string }

type Msg =
    | ConsumeSchoolStatusData of Result<SchoolStatusMap, string>
    | ConsumeServerError of exn
    | SearchInputChanged of string

let init (queryObj: obj): State * Cmd<Msg> =
    let state =
        { SchoolStatus = Map.empty
          Error = None
          SearchQuery = "" }

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
    | SearchInputChanged query ->
        let cmd =
            Cmd.OfAsync.either getOrFetch () ConsumeSchoolStatusData ConsumeServerError

        { state with
              SchoolStatus = Map.empty
              SearchQuery = query },
        cmd


let renderSearch (query: string) dispatch =
    Html.input [ prop.className "form-control form-control-sm filters__query"
                 prop.type'.text
                 prop.placeholder (I18N.t "charts.schoolStatus.search")
                 prop.valueOrDefault query
                 prop.onChange (SearchInputChanged >> dispatch) ]

let renderAbsences (absences: SchoolAbsence array) =
    absences 
    |> Array.toSeq
    |> Seq.sortByDescending (fun absence -> absence.AbsentToDate)
    |> Seq.map (fun absence ->
                let abs =
                    sprintf
                        "ABSENCE: %s - %s: %s %s %s"
                        (I18N.tOptions "charts.schoolStatus.date" {| date = absence.AbsentFromDate |})
                        (I18N.tOptions "charts.schoolStatus.date" {| date = absence.AbsentToDate |})
                        (I18N.tt "schoolDict" absence.personType)
                        (I18N.tt "schoolDict" absence.personClass)
                        (I18N.tt "schoolDict" absence.reason)

                Html.div [ prop.className "absence"
                           prop.text abs ] )

let renderRegimes (regimes: SchoolRegime array) =
    regimes 
    |> Array.toSeq
    |> Seq.sortByDescending (fun regime -> regime.ChangedToDate)
    |> Seq.map (fun regime ->
                    let reg =
                        sprintf
                            "REGIME: %s - %s: %s %d %s %s"
                            (I18N.tOptions "charts.schoolStatus.date" {| date = regime.ChangedFromDate |})
                            (I18N.tOptions "charts.schoolStatus.date" {| date = regime.ChangedToDate |})
                            (I18N.tt "schoolDict" regime.personClass)
                            regime.attendees
                            (I18N.tt "schoolDict" regime.regime)
                            (I18N.tt "schoolDict" regime.reason)

                    Html.div [ prop.className "regime"
                               prop.text reg ] )
   

let renderSchool (state: State) (schoolId: string) (schoolStatus: SchoolStatus) =
    Html.div [ prop.className "school"
               prop.children [ Html.h3 [ let schoolName =
                                             match Utils.Dictionaries.schools.TryFind(schoolId) with
                                             | Some s -> s.Name
                                             | _ -> schoolId

                                         prop.className "name"
                                         prop.text schoolName ]
                               Html.div [ prop.className "absences"
                                          prop.children (renderAbsences schoolStatus.absences) ]
                               Html.div [ prop.className "regimes"
                                          prop.children (renderRegimes schoolStatus.regimes) ] ] ]

let renderSchools (state: State) =
    (state.SchoolStatus
     |> Seq.map (fun school -> renderSchool state school.Key school.Value))

let render (state: State) dispatch =
    let element =
        Html.div [ prop.children [ Utils.renderChartTopControls [ Html.div [ prop.className "filters"
                                                                             prop.children [ renderSearch
                                                                                                 state.SearchQuery
                                                                                                 dispatch ] ] ]
                                   Html.div [ prop.className "shools"
                                              prop.children (renderSchools state) ]
                                   Html.div [ prop.className "credits"
                                              prop.children [ Html.a [ prop.href
                                                                           "https://www.gov.si/drzavni-organi/ministrstva/ministrstvo-za-izobrazevanje-znanost-in-sport/"
                                                                       prop.text (
                                                                           sprintf
                                                                               "%s: %s"
                                                                               (I18N.t "charts.common.dataSource")
                                                                               (I18N.tOptions
                                                                                   ("charts.common.dsMIZS")
                                                                                   {| context =
                                                                                          localStorage.getItem (
                                                                                              "contextCountry"
                                                                                          ) |})
                                                                       ) ] ] ] ] ]

    // trigger event for iframe resize
    let evt = document.createEvent ("event")
    evt.initEvent ("chartLoaded", true, true)
    document.dispatchEvent (evt) |> ignore

    element

let schoolStatusChart (props: {| query: obj |}) =
    React.elmishComponent ("SchoolStatusChart", init props.query, update, render)
