[<RequireQualifiedAccess>]
module HospitalsChart

open System
open Elmish
open Feliz
open Feliz.ElmishComponents
open Browser

open Types
open Data.Patients
open Data.Hospitals
open Highcharts


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
    RangeSelectionButtonIndex: int
  } with
    static member initial =
        {
            scaleType = Linear
            facData = [||]
            patientsData = [||]
            error = None
            facilities = []
            scope = Totals
            RangeSelectionButtonIndex = 0
        }
    static member switchBreakdown breakdown state = { state with scope = breakdown }

type Msg =
    | ConsumeHospitalsData of Result<FacilityAssets [], string>
    | ConsumePatientsData of Result<PatientsStats [], string>
    | ConsumeServerError of exn
    | ScaleTypeChanged of ScaleType
    | SwitchBreakdown of Scope
    | RangeSelectionChanged of int

let init () : State * Cmd<Msg> =
    let cmd = Cmd.OfAsync.either Data.Hospitals.getOrFetch () ConsumeHospitalsData ConsumeServerError
    let cmd2 = Cmd.OfAsync.either Data.Patients.getOrFetch () ConsumePatientsData ConsumeServerError
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
    | RangeSelectionChanged buttonIndex ->
        { state with RangeSelectionButtonIndex = buttonIndex }, Cmd.none

let getAllScopes state = seq {
    yield Totals, I18N.t "charts.hospitals.allHospitals"
    yield Projection, I18N.t "charts.hospitals.projection"
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
        fun (fa: FacilityAssets) -> fa.JsDate12h, (fa.overall |> Assets.getValue ctype atype)
    | Facility fcode ->
        fun fa ->
            let value =
                fa.perHospital
                |> Map.tryFind fcode
                |> Option.bind (Assets.getValue ctype atype)
                |> zeroToNone
            fa.JsDate12h, value

let extractPatientDataPoint scope cType : (PatientsStats -> (JsTimestamp * int option)) =
    let extractTotalsCount : TotalPatientStats -> int option =
        match cType with
        | Beds -> fun ps -> ps.inHospital.today
        | Icus -> fun ps -> ps.icu.today
        | Vents -> fun _ -> failwithf "no vents in data"
    let extractFacilityCount : FacilityPatientStats -> int option =
        match cType with
        | Beds -> fun ps -> ps.inHospital.today
        | Icus -> fun ps -> ps.icu.today
        | Vents -> fun _ -> failwithf "no vents in data"

    match scope with
    | Totals ->
        fun (fa: PatientsStats) -> fa.JsDate12h, (fa.total |> extractTotalsCount)
    | Projection ->
        fun (fa: PatientsStats) -> fa.JsDate12h, (fa.total |> extractTotalsCount)
    | Facility fcode ->
        fun (fa: PatientsStats) ->
            let value =
                fa.facilities
                |> Map.tryFind fcode
                |> Option.bind extractFacilityCount
                |> zeroToNone
            fa.JsDate12h, value


let renderChartOptions (state : State) dispatch =

    let projectDays = 40

    let yAxes = [|
        {|
            index = 0
            height = "55%"; top = "0%"
            offset = 0
            ``type`` = if state.scaleType=Linear then "linear" else "logarithmic"
            min = if state.scaleType=Linear then None else Some 1.0
            //floor = if scaleType=Linear then None else Some 1.0
            opposite = true // right side
            title = pojo {| text = I18N.t "charts.hospitals.bedsShort" |} // "oseb" |}
            //showFirstLabel = false
            tickInterval = if state.scaleType=Linear then None else Some 0.25
            gridZIndex = -1
            visible = true
            plotLines=[| {| value=0; color="black"; |} |]
        |}
        {|
            index = 1
            height = "40%"; top = "60%"
            offset = 0
            ``type`` = if state.scaleType=Linear then "linear" else "logarithmic"
            min = if state.scaleType=Linear then None else Some 1.0
            //floor = if scaleType=Linear then None else Some 1.0
            opposite = true // right side
            title = pojo {| text = I18N.t "charts.hospitals.bedsICUShort" |} // "oseb" |}
            //showFirstLabel = false
            tickInterval = if state.scaleType=Linear then None else Some 0.25
            gridZIndex = -1
            visible = true
            plotLines=[| {| value=0; color="black"; |} |]
        |}
    |]
    let getYAxis = function
        | Beds -> 0
        | Icus
        | Vents -> 1


    let extendFacilitiesData (data: ((JsTimestamp*option<int>)[])) =
        match data with
        | [||] -> data
        | _ ->
            let startDate, point = data.[data.Length-1]
            //printfn "xy %A" (startDate, point)
            match point with
            | None -> data
            | Some 0 -> data
            | _ ->
                let extra = [| for i in 1..projectDays+1 -> startDate + 86400000.0*float i, point |]
                Array.append data extra

    let renderFacilitiesSeries (scope: Scope) (aType:AssetType) (cType: CountType) scaleBy color dash name =
        let renderPoint =
            match scaleBy with
            | 1.0 -> extractFacilityDataPoint scope aType cType
            | k ->
                let scale = fun (ts,x) -> ts, x |> Option.map (fun n -> float n * k |> int)
                extractFacilityDataPoint scope aType cType >> scale

        {|
            //visible = state.activeSeries |> Set.contains series
            ``type``="line"
            color = color
            name = name
            dashStyle = dash |> DashStyle.toString
            showInLegend = true
            data =
                state.facData
                |> Seq.map renderPoint
                |> Seq.skipWhile (function
                    | _, None -> true
                    | _, Some 0 -> true
                    | _ -> false)
                |> Array.ofSeq
                |> if scope=Projection then extendFacilitiesData else id
            yAxis = getYAxis aType
            options = pojo {| dataLabels = false |}
        |}
        |> pojo


    let renderPatientsProjection (scope: Scope) (aType:AssetType) color dash growthFactor limit name =
        let startDate, point =
            match state.patientsData with
            | [||] -> DateTime.Now |> jsTime, None
            | data -> data.[data.Length-1] |> extractPatientDataPoint scope aType
        {|
            ``type``="line"
            color = color
            name = name
            showInLegend = false
            dashStyle = dash |> DashStyle.toString
            //lineWidth = "1"
            data =
                [| for i in 1..projectDays+1 do
                    let k = Math.Pow(growthFactor,float i)
                    match point with
                    | Some 0
                    | None -> ()
                    | Some n ->
                        let value = k * float n |> int
                        if value < limit then
                            yield startDate + 86400000.0*float i, point |> Option.map (fun n -> k * float n |> int)
                |]
            yAxis = getYAxis aType
        |}
        |> pojo

    let renderPatientsSeries (scope: Scope) (aType) color dash name =
        {|
            ``type``="spline"
            color = color
            name = name
            dashStyle = dash |> DashStyle.toString
            data =
                state.patientsData
                |> Seq.map (extractPatientDataPoint scope aType)
                |> Seq.skipWhile (snd >> Option.isNone)
                |> Array.ofSeq
            yAxis = getYAxis aType
        |}
        |> pojo

    let growthFactor nDays =
        Math.Exp(Math.Log 2.0 / float nDays)

    let series = [|
        let gf7, gf14, gf21 = growthFactor 7, growthFactor 14, growthFactor 21

        let clr = "#444"
        if state.scope = Projection then
            yield renderFacilitiesSeries state.scope Beds Max 1.0 clr Dash (I18N.t "charts.hospitals.bedsMax")
        else
            yield pojo {| showInLegend = false; data=[||] |}

        yield renderFacilitiesSeries state.scope Beds Total 1.0 clr Solid (I18N.t "charts.hospitals.bedsAll")
        yield renderFacilitiesSeries state.scope Beds Total 0.7 "#777" Dash (I18N.t "charts.hospitals.beds70")
        //yield renderFacilitiesSeries state.scope Beds Free    clr ShortDot "Postelje, proste"
        //yield renderFacilitiesSeries state.scope Beds Occupied clr Solid "Postelje, zasedene"
        yield renderPatientsSeries state.scope Beds clr Solid (I18N.t "charts.hospitals.bedsFull")

        let clr = "#c44"
        //yield renderFacilitiesSeries state.scope Icus Max      clr Dash "Intenzivne, maksimalno"
        yield renderFacilitiesSeries state.scope Icus Total 1.0 clr Solid (I18N.t "charts.hospitals.bedsICUAll")
        yield renderFacilitiesSeries state.scope Icus Total 0.7 "#c88" Dash (I18N.t "charts.hospitals.bedsICU70")
        //yield renderFacilitiesSeries state.scope Icus Occupied clr Solid "Intenzivne, zasedene"
        yield renderPatientsSeries state.scope Icus clr Solid (I18N.t "charts.hospitals.bedsICUFull")
        if state.scope = Projection then
            let clr = "#888"
            yield renderPatientsProjection state.scope Beds clr ShortDash gf7 1100  (I18N.t "charts.hospitals.projection7")
            yield renderPatientsProjection state.scope Beds clr ShortDash gf14 1100 (I18N.t "charts.hospitals.projection14")
            yield renderPatientsProjection state.scope Beds clr ShortDash gf21 1100 (I18N.t "charts.hospitals.projection21")

            let clr = "#c88"
            yield renderPatientsProjection state.scope Icus clr ShortDash gf7 130  (I18N.t "charts.hospitals.projection7")
            yield renderPatientsProjection state.scope Icus clr ShortDash gf14 130 (I18N.t "charts.hospitals.projection14")
            yield renderPatientsProjection state.scope Icus clr ShortDash gf21 130 (I18N.t "charts.hospitals.projection21")


        //let clr = "#4ad"
        //yield renderFacilitiesSeries state.scope Vents Total    clr Dash "Respiratorji, vsi"
        //yield renderFacilitiesSeries state.scope Vents Occupied clr Solid "Respiratorji, v uporabi"
    |]

    let onRangeSelectorButtonClick(buttonIndex: int) =
        let res (_ : Event) =
            RangeSelectionChanged buttonIndex |> dispatch
            true
        res

    let baseOptions =
        Highcharts.basicChartOptions
            state.scaleType "hospitals-chart"
            state.RangeSelectionButtonIndex onRangeSelectorButtonClick
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
                backgroundColor = "rgba(255,255,255,0.5)"
            |}
        xAxis = baseOptions.xAxis |> Array.map (fun xAxis ->
            if false //state.scope = Projection
            then
                {| xAxis with
                    plotLines=[| {| value=jsTime <| DateTime.Now; label=None |} |]
                    plotBands=[|
                        {| ``from``=jsTime <| DateTime(2020,2,29);
                           ``to``=jsTime <| DateTime.Now
                           color="transparent"
                           label={| align="center"; text=I18N.t "charts.hospitals.data" |}
                        |}
                        {| ``from``=jsTime <| DateTime.Today;
                           ``to``=jsTime <| DateTime(2020,3,20) + TimeSpan.FromDays (float projectDays)
                           color="transparent"
                           label={| align="center"; text=(I18N.t "charts.hospitals.projection") |}
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

let renderChartContainer state dispatch =
    Html.div [
        prop.style [ style.height 480 ]
        prop.className "highcharts-wrapper"
        prop.children [
            renderChartOptions state dispatch
            |> Highcharts.chartFromWindow
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
        yield Html.th [
            prop.className "text-left"
            prop.text facilityName
        ]

        let numericCell (pt: int option) =
            Html.td [ prop.text (pt |> Option.map string |> Option.defaultValue "") ]

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
                            Html.th [ prop.text (I18N.t "charts.hospitals.bedsShort"); prop.colSpan 4 ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.bedsICUShort"); prop.colSpan 4 ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.ventilators"); prop.colSpan 4 ]
                        ]
                    ]
                    Html.tableRow [
                        prop.children [
                            Html.th []
                            // postelje
                            Html.th [ prop.text (I18N.t "charts.hospitals.empty") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.full") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.all") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.max") ]
                            // icu
                            Html.th [ prop.text (I18N.t "charts.hospitals.empty") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.full") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.all") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.max") ]
                            // vents
                            Html.th [ prop.text (I18N.t "charts.hospitals.empty") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.full") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.all") ]
                            Html.th [ prop.text (I18N.t "charts.hospitals.max") ]
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
        Utils.classes
            [(true, "btn btn-sm metric-selector")
             (state.scope = scope, "metric-selector--selected") ]
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
    | _, None ->
        Html.div [
            Utils.renderChartTopControlRight
                (Utils.renderScaleSelector
                    state.scaleType (ScaleTypeChanged >> dispatch))

            renderChartContainer state dispatch
            //Html.div [ prop.style [ style.height 10 ] ]
            renderBreakdownSelectors state dispatch
            Html.div [
                prop.style [ style.overflow.scroll ]
                prop.children [
                    renderTable state dispatch
                ]
            ]
        ]

let hospitalsChart () =
    React.elmishComponent("HospitalsChart", init (), update, render)
