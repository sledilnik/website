[<RequireQualifiedAccess>]
module HospitalsChart

open System
open Fable.Core
open Elmish
open Feliz
open Feliz.ElmishComponents

open Types
open Data.Patients
open Data.Hospitals
open Highcharts

(*
[<Emit """require("./fsapps.scss")""">]
let importScss : unit = jsNative
importScss |> ignore
*)


type Scope =
    | Totals
    | Projection
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

let getAllScopes state = seq {
    yield Totals, "Vse bolnice"
    yield Projection, "Projekcija"
    for fcode in state.facilities do
        let _, name = fcode |> facilitySeriesInfo
        yield Facility fcode, name
}


let zeroToNone value =
    match value with
    | None -> None
    | Some x ->
        if x = 0 then None
        else Some x

let extractFacilityDataPoint (scope: Scope) (atype:AssetType) (ctype: CountType) : (FacilityAssets -> (JsTimestamp * int option)) =
    match scope with
    | Totals
    | Projection ->
        fun (fa: FacilityAssets) -> fa.JsDate, (fa.overall |> Assets.getValue ctype atype)
    | Facility fcode ->
        fun fa ->
            let value =
                fa.perHospital
                |> Map.tryFind fcode
                |> Option.bind (Assets.getValue ctype atype)
                |> zeroToNone
            fa.JsDate, value

let extractPatientDataPoint scope cType : (PatientsStats -> (JsTimestamp * int option)) =
    let extractTotalsCount : TotalPatientStats -> int option =
        match cType with
        | Beds -> fun ps -> ps.inHospital.today
        | Icus -> fun ps -> ps.icu.today
        | Vents -> fun ps -> ps.needsO2.today // failwithf "no vents in data"
    let extractFacilityCount : PatientsByFacilityStats -> int option =
        match cType with
        | Beds -> fun ps -> ps.inHospital.today
        | Icus -> fun ps -> ps.icu.today
        | Vents -> fun ps -> ps.needsO2.today

    match scope with
    | Totals ->
        fun (fa: PatientsStats) -> fa.JsDate, (fa.total |> extractTotalsCount)
    | Projection ->
        fun (fa: PatientsStats) -> fa.JsDate, (fa.total |> extractTotalsCount)
    | Facility fcode ->
        fun (fa: PatientsStats) ->
            let value =
                fa.facilities
                |> Map.tryFind fcode
                |> Option.bind extractFacilityCount
                |> zeroToNone
            fa.JsDate, value


let renderChartOptions (state : State) =

    let startDate =
        if state.scope = Projection then DateTime(2020,03,11)
        else DateTime(2020,03,21)

    let projectDays = 30

    let yAxes = [|
        {|
            index = 0
            height = "55%"; top = "0%"
            ``type`` = if state.scaleType=Linear then "linear" else "logarithmic"
            min = if state.scaleType=Linear then None else Some 1.0
            //floor = if scaleType=Linear then None else Some 1.0
            opposite = true // right side
            title = pojo {| text = "Postelje" |} // "oseb" |}
            //showFirstLabel = false
            tickInterval = if state.scaleType=Linear then None else Some 0.25
            gridZIndex = -1
            visible = true
            plotLines=[| {| value=0; label=None |} |]
        |}
        {|
            index = 1
            height = "40%"; top = "60%"
            ``type`` = if state.scaleType=Linear then "linear" else "logarithmic"
            min = if state.scaleType=Linear then None else Some 1.0
            //floor = if scaleType=Linear then None else Some 1.0
            opposite = true // right side
            title = pojo {| text = "ICU, Respiratorji" |} // "oseb" |}
            //showFirstLabel = false
            tickInterval = if state.scaleType=Linear then None else Some 0.25
            gridZIndex = -1
            visible = true
            plotLines=[| {| value=0; label=None |} |]
        |}
    |]
    let getYAxis = function
        | Beds -> 0
        | Icus
        | Vents -> 1


    let extendFacilitiesData (scope: Scope) (aType:AssetType) (cType: CountType) =
        let startDate, point =
            match state.facData with
            | [||] -> DateTime.Now |> jsTime, None
            | data -> data.[data.Length-1] |> extractFacilityDataPoint scope aType cType
        match point with
        | None -> [||]
        | Some 0 -> [||]
        | _ ->
            [| for i in 1..projectDays+1 do
                    yield startDate + 86400000.0*float i, point |]

    let renderFacilitiesSeries (scope: Scope) (aType:AssetType) (cType: CountType) color dash name =
        let renderPoint = extractFacilityDataPoint scope aType cType
        {|
            //visible = state.activeSeries |> Set.contains series
            ``type``="line"
            color = color
            name = name
            dashStyle = dash |> DashStyle.value
            data =
                state.facData
                |> Seq.map renderPoint
                |> Seq.skipWhile (function
                    | _, None -> true
                    | _, Some 0 -> true
                    | _ -> false)
                |> Array.ofSeq
                |> fun data ->
                    if scope=Projection then Array.append data (extendFacilitiesData scope aType cType)
                    else data
            yAxis = getYAxis aType
            options = pojo {| dataLabels = false |}
        |}
        |> pojo


    let extendPatientsData (scope: Scope) (aType:AssetType) =
        let startDate, point =
            match state.patientsData with
            | [||] -> DateTime.Now |> jsTime, None
            | data -> data.[data.Length-1] |> extractPatientDataPoint scope aType
        let growth = 1.06
        [| for i in 1..projectDays+1 do
                let k = Math.Pow(growth,float i)
                yield startDate + 86400000.0*float i, point |> Option.map (fun n -> k * float n |> int) |]

    let renderPatientsSeries (scope: Scope) (aType) color dash name =
        let renderPoint = extractPatientDataPoint scope aType
        {|
            //visible = state.activeSeries |> Set.contains series
            ``type``="spline"
            color = color
            name = name
            dashStyle = dash |> DashStyle.value
            data =
                state.patientsData
                |> Seq.skipWhile (fun pt -> pt.Date < startDate)
                |> Seq.map renderPoint
                |> Seq.skipWhile (snd >> Option.isNone)
                |> Array.ofSeq
                |> fun data ->
                    if scope=Projection then Array.append data (extendPatientsData scope aType)
                    else data
            yAxis = getYAxis aType
        |}
        |> pojo


    let series = [|
        let clr = "#444"
        //yield renderFacilitiesSeries state.scope Beds Max      clr Dash "Postelje, maksimalno"
        yield renderFacilitiesSeries state.scope Beds Total    clr LongDash "Postelje, vse"
        //yield renderFacilitiesSeries state.scope Beds Free    clr ShortDot "Postelje, proste"
        //yield renderFacilitiesSeries state.scope Beds Occupied clr Solid "Postelje, zasedene"
        yield renderPatientsSeries state.scope Beds clr Solid "Postelje, polne"

        let clr = "#c44"
        yield renderFacilitiesSeries state.scope Icus Max      clr LongDash "Intenzivne, maksimalno"
        yield renderFacilitiesSeries state.scope Icus Total    clr ShortDot "Intenzivne, vse"
        //yield renderFacilitiesSeries state.scope Icus Occupied clr Solid "Intenzivne, zasedene"
        yield renderPatientsSeries state.scope Icus clr Solid "Intenzivne, polne"

        let clr = "#4ad"
        yield renderFacilitiesSeries state.scope Vents Total    clr Dash "Respiratorji, vsi"
        yield renderFacilitiesSeries state.scope Vents Occupied clr Solid "Respiratorji, v uporabi"
    |]

    let baseOptions = Highcharts.basicChartOptions state.scaleType "hospitals-chart"
    {| baseOptions with
        yAxis = yAxes
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
        tooltip = pojo {| shared=true |}
        xAxis = baseOptions.xAxis |> Array.map (fun xAxis ->
            if state.scope = Projection
            then
                {| xAxis with
                    plotLines=[| {| value=jsTime <| DateTime.Now; label=None |} |]
                    plotBands=[|
                        {| ``from``=jsTime <| DateTime(2020,2,29);
                           ``to``=jsTime <| DateTime.Now
                           color="transparent"
                           label={| align="center"; text="Podatki" |}
                        |}
                        {| ``from``=jsTime <| DateTime.Today;
                           ``to``=jsTime <| DateTime(2020,3,20) + TimeSpan.FromDays (float projectDays)
                           color="transparent"
                           label={| align="center"; text="Projekcija" |}
                        |}
                    |]
                |}
            else {| xAxis with plotLines=[||]; plotBands=[||] |}
        )
        plotOptions = pojo
                {|
                    spline = pojo {| dataLabels = pojo {| enabled = true |} |}
                    line = pojo {| dataLabels = pojo {| enabled = false |}; marker = pojo {| enabled = false |} |}
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

let renderTable (state: State) dispatch =

    let getFacilityDp (breakdown: Scope) (atype:AssetType) (ctype: CountType) =
        let renderPoint = extractFacilityDataPoint breakdown atype ctype
        match state.facData with
        | [||]
        | [| _ |] -> None
        | data ->
            seq {
                for i = data.Length-1 downto data.Length / 2 do
                    yield data.[i]
            }
            |> Seq.map (renderPoint >> snd)
            |> Seq.skipWhile Option.isNone
            |> Seq.take 1
            |> Seq.tryExactlyOne
            |> Option.flatten

    let getPatientsDp (breakdown: Scope) (atype:AssetType) =
        let renderPoint = extractPatientDataPoint breakdown atype
        match state.patientsData with
        | [||]
        | [| _ |] -> None
        | data ->
            seq {
                yield data.[data.Length-1]
                yield data.[data.Length-2]
            }
            |> Seq.map (renderPoint >> snd)
            |> Seq.skipWhile Option.isNone
            |> Seq.take 1
            |> Seq.tryExactlyOne
            |> Option.flatten

    let renderFacilityCells scope (facilityName: string) = [
        let isCurrent = state.scope = scope
        yield Html.th [
            prop.className "text-left"
            prop.text facilityName
        ]

        let numericCell (pt: int option) =
            Html.td [ prop.text (pt |> Option.map string |> Option.defaultValue "") ]
        let getFree cur total = Option.map2 (-) total cur

        // postelje
        let cur = getPatientsDp scope Beds
        let total = getFacilityDp scope Beds Total
        //let free = getFree cur total
        let free = getFacilityDp scope Beds Free
        yield free |> numericCell
        yield cur |> numericCell
        yield total |> numericCell
        yield getFacilityDp scope Beds Max |> numericCell
        // icu
        let cur = getPatientsDp scope Icus
        let total = getFacilityDp scope Icus Total
        //let free = getFree cur total
        let free = getFacilityDp scope Icus Free
        yield free |> numericCell
        yield cur |> numericCell
        yield total |> numericCell
        yield getFacilityDp scope Icus Max |> numericCell
        // resp
        //let cur = getPatientsDp scope Vents
        let cur = getFacilityDp scope Vents Occupied
        let total = getFacilityDp scope Vents Total
        //let free = getFree cur total
        let free = getFacilityDp scope Vents Free
        yield free |> numericCell
        yield cur |> numericCell
        yield total |> numericCell
        yield Html.td []
    ]

    Html.table [
        prop.className "facilities-navigate b-table-sticky-header b-table table-striped table-hover table-bordered text-lg-right"
        prop.style [ style.width (length.percent 100.0); style.fontSize 16 ]
        prop.children [
            Html.thead [
                prop.children [
                    Html.tableRow [
                        prop.className "text-center"
                        prop.children [
                            Html.th []
                            Html.th [ prop.text "Postelje"; prop.colSpan 4 ]
                            Html.th [ prop.text "Intenzivne Postelje"; prop.colSpan 4 ]
                            Html.th [ prop.text "Respiratorji"; prop.colSpan 4 ]
                        ]
                    ]
                    Html.tableRow [
                        prop.children [
                            Html.th []
                            // postelje
                            Html.th [ prop.text "Proste" ]
                            Html.th [ prop.text "Polne" ]
                            Html.th [ prop.text "Vse" ]
                            Html.th [ prop.text "Max" ]
                            // icu
                            Html.th [ prop.text "Proste" ]
                            Html.th [ prop.text "Polne" ]
                            Html.th [ prop.text "Vse" ]
                            Html.th [ prop.text "Max" ]
                            // vents
                            Html.th [ prop.text "Prosti" ]
                            Html.th [ prop.text "V uporabi" ]
                            Html.th [ prop.text "Vsi" ]
                            Html.th [ prop.text "Max" ]
                        ]
                    ]
                ]
            ]
            Html.tbody [
                prop.children [
                    for scope, facilityName in getAllScopes state do
                        if scope <> Projection then
                            yield Html.tableRow [
                                yield prop.children (renderFacilityCells scope facilityName)
                                if scope = state.scope then
                                    yield prop.className "current highlight" // bg-light"
                                    yield prop.style [
                                        style.fontWeight.bold;
                                        style.cursor.defaultCursor
                                        style.backgroundColor "#ccc"
                                    ]
                                else
                                    yield prop.onClick (fun _ -> SwitchBreakdown scope |> dispatch)
                                    yield prop.style [ style.cursor.pointer ]
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
    Html.div [
        prop.className "metrics-selectors"
        prop.children (
            getAllScopes state
            |> Seq.map (fun (scope,name) ->
                renderScopeSelector state scope name (fun _ -> SwitchBreakdown scope |> dispatch)
            ) ) ]

let render (state : State) dispatch =
    match state.facData, state.error with
    | [||], None -> Html.div [ Utils.renderLoading ]
    | _, Some err -> Html.div [ Utils.renderErrorLoading err ]
    | data, None ->
        Html.div [
            Utils.renderScaleSelector state.scaleType (ScaleTypeChanged >> dispatch)
            renderChartContainer state
            //Html.div [ prop.style [ style.height 10 ] ]
            renderBreakdownSelectors state dispatch
            renderTable state dispatch
        ]

let hospitalsChart () =
    React.elmishComponent("HospitalsChart", init (), update, render)
