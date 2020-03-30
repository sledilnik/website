[<RequireQualifiedAccess>]
module HospitalsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents

open Types
open Data.Patients
open Data.Hospitals
open Highcharts

type Scope =
    | Totals
    | Facility of FacilityCode

type State = {
    scaleType : ScaleType
    facData : FacilityAssets []
    patientsData: PatientsStats []
    error: string option
    facilities: FacilityCode list
    scope: Scope
  } with
    static member initial =
        {
            scaleType = Linear
            facData = [||]
            patientsData = [||]
            error = None
            facilities = []
            scope = Totals
        }
    static member switchBreakdown breakdown state = { state with scope = breakdown }

type Msg =
    | ConsumeHospitalsData of Result<FacilityAssets [], string>
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ScaleTypeChanged of ScaleType
    | SwitchBreakdown of Scope

let init () : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either Data.Hospitals.fetch () ConsumeHospitalsData ConsumeServerError
    let cmd2 = Cmd.OfAsync.either Data.Patients.fetch () ConsumePatientsData ConsumeServerError
    State.initial, (cmd @ cmd2)

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ConsumeHospitalsData (Ok data) ->
        { state with
            facData = data
            facilities = data |> Data.Hospitals.getSortedFacilityCodes
        } |> State.switchBreakdown state.scope, Cmd.none
    | ConsumeHospitalsData (Error err) ->
        { state with error = Some err }, Cmd.none

    | ConsumePatientsData (Ok data) ->
        { state with
            patientsData = data
        } |> State.switchBreakdown state.scope, Cmd.none
    | ConsumePatientsData (Error err) ->
        { state with error = Some err }, Cmd.none

    | ConsumeServerError ex ->
        { state with error = Some ex.Message }, Cmd.none
    | ScaleTypeChanged scaleType ->
        { state with scaleType = scaleType }, Cmd.none
    | SwitchBreakdown breakdown ->
        (state |> State.switchBreakdown breakdown), Cmd.none

let renderChartOptions (state : State) =

    let startDate = DateTime(2020,03,10)

    let zeroToNone value =
        match value with
        | None -> None
        | Some x ->
            if x = 0 then None
            else Some x

    let renderFacilitiesSeries (breakdown: Scope) (atype:AssetType) (ctype: CountType) color dash name =
        let renderPoint : (FacilityAssets -> (JsTimestamp * int option)) =
            match breakdown with
            | Totals ->
                fun (fa: FacilityAssets) -> fa.JsDate, (fa.overall |> Assets.getValue ctype atype)
            | Facility fcode ->
                fun fa ->
                    let value =
                        fa.perHospital
                        |> Map.tryFind fcode
                        |> Option.bind (Assets.getValue ctype atype)
                        |> zeroToNone
                    fa.JsDate, value

        {|
            //visible = state.activeSeries |> Set.contains series
            color = color
            name = name
            dashStyle = dash |> DashStyle.value
            data =
                state.facData
                |> Seq.map renderPoint
                |> Seq.skipWhile (snd >> Option.isNone)
                |> Array.ofSeq
        |}
        |> pojo

    let renderPatientsSeries (breakdown: Scope) (countType) color dash name =
        let extractTotalsCount : TotalPatientStats -> int option =
            match countType with
            | Beds -> fun ps -> ps.inHospital.today
            | Icus -> fun ps -> ps.icu.today
            | Vents -> failwithf "no vents in data"
        let extractFacilityCount : PatientsByFacilityStats -> int option =
            match countType with
            | Beds -> fun ps -> ps.inHospital.today
            | Icus -> fun ps -> ps.icu.today
            | Vents -> failwithf "no vents in data"
        let renderPoint : (PatientsStats -> (JsTimestamp * int option)) =
            match breakdown with
            | Totals ->
                fun (fa: PatientsStats) -> fa.JsDate, (fa.total |> extractTotalsCount)
            | Facility fcode ->
                fun (fa: PatientsStats) ->
                    let value =
                        fa.facilities
                        |> Map.tryFind fcode
                        |> Option.bind extractFacilityCount
                        |> zeroToNone
                    fa.JsDate, value

        {|
            //visible = state.activeSeries |> Set.contains series
            color = color
            name = name
            dashStyle = dash |> DashStyle.value
            data =
                state.patientsData
                |> Seq.map renderPoint
                |> Seq.skipWhile (snd >> Option.isNone)
                |> Array.ofSeq
        |}
        |> pojo


    let series = [|
        let clr = "#444"
        yield renderFacilitiesSeries state.scope Beds Max      clr Dash "Postelje, maksimum"
        yield renderFacilitiesSeries state.scope Beds Total    clr ShortDot "Postelje, vse"
        //yield renderFacilitiesSeries state.scope Beds Occupied clr Solid "Postelje, zasedene"
        yield renderPatientsSeries state.scope Beds clr Solid "Postelje, zasedene"

        let clr = "#c44"
        yield renderFacilitiesSeries state.scope Icus Max      clr Dash "Intenzivne, maksimum"
        yield renderFacilitiesSeries state.scope Icus Total    clr ShortDot "Intenzivne, vse"
        //yield renderFacilitiesSeries state.scope Icus Occupied clr Solid "Intenzivne, zasedene"
        yield renderPatientsSeries state.scope Icus clr Solid "Intenzivne, zasedene"

        let clr = "#4ad"
        yield renderFacilitiesSeries state.scope Vents Total    clr ShortDot "Respiratorji, vsi"
        yield renderFacilitiesSeries state.scope Vents Occupied clr Solid "Respiratorji, zasedeni"
    |]

    {| Highcharts.basicChartOptions state.scaleType "hospitals-chart" with
        series = series
        legend = pojo
            {|
                enabled = Some true
                title = ""
                align = "left"
                verticalAlign = "top"
                borderColor = "#ddd"
                borderWidth = 1
                //labelFormatter = string //fun series -> series.name
                layout = "vertical"
                floating = true
                x = 20
                y = 30
                backgroundColor = "#FFF"
            |}
    |}

let renderChartContainer state =
    Html.div [
        prop.style [ style.height 450 ] //; style.width 500; ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state
            |> Highcharts.chart
        ]
    ]

let renderTable state =


    Html.table [
        prop.children [
            Html.tableHeader [
                prop.children [
                    Html.tableRow [
                        prop.children [
                            Html.tableCell [ prop.text "test" ]
                            Html.tableCell []
                        ]
                    ]
                ]
            ]
            Html.tableBody [
                prop.children [
                    Html.tableRow [
                        prop.children [
                            Html.tableCell [ prop.text "test" ]
                            Html.tableCell []
                        ]
                    ]
                ]
            ]
        ]
    ]


let renderScopeSelector state scope (name:string) onClick =
    Html.div [
        prop.onClick onClick
        prop.className [ true, "btn btn-sm metric-selector"; state.scope = scope, "metric-selector--selected" ]
        prop.text name
    ]

let renderBreakdownSelectors state dispatch =
    let allScopes = seq {
        yield Totals, "Vse bolnice"
        for fcode in state.facilities do
            let _, name = fcode |> facilitySeriesInfo
            yield Facility fcode, name
    }

    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            allScopes
            |> Seq.map (fun (scope,name) ->
                renderScopeSelector state scope name (fun _ -> SwitchBreakdown scope |> dispatch)
            ) ) ]

let render (state : State) dispatch =
    match state.facData, state.error with
    | [||], None -> Html.div [ prop.text "loading" ]
    | _, Some err -> Html.div [ prop.text err ]
    | data, None ->
        Html.div [
            Utils.renderScaleSelector state.scaleType (ScaleTypeChanged >> dispatch)
            renderChartContainer state
            renderBreakdownSelectors state dispatch
            renderTable state
        ]

let hospitalsChart () =
    React.elmishComponent("HospitalsChart", init (), update, render)
