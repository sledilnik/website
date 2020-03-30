module Data.Hospitals

open System
open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let url = "https://covid19.rthand.com/api/hospitals"

type CountType =
    | Total
    | Max
    | Occupied
    | Free
    | MaxFree
  with
    static member seriesInfo = function
        | Total     -> "", "Skupaj"
        | Max       -> "", ""
        | Occupied  -> "", ""
        | Free      -> "", ""
        | MaxFree   -> "", ""

type AssetCounts = {
    total: int option
    max: int option
    occupied: int option
    free: int option
    maxFree: int option
  } with
    static member getValue count (counts: AssetCounts) =
        match count with
        | Total -> counts.total
        | Max -> counts.max
        | Occupied -> counts.occupied
        | Free -> counts.free
        | MaxFree -> counts.maxFree

type AssetType =
    | Beds
    | Icus
    | Vents
  with
    static member seriesInfo = function
        | Beds  -> "", "", "Postelje"
        | Icus   -> "", "", "ICU"
        | Vents -> "", "", "Respiratorji"

type Assets = {
    beds: AssetCounts
    icu: AssetCounts
    vents: AssetCounts
  } with
    static member getValue (count: CountType) asset assets =
        match asset with
        | Beds  -> assets.beds  |> AssetCounts.getValue count
        | Icus   -> assets.icu   |> AssetCounts.getValue count
        | Vents -> assets.vents |> AssetCounts.getValue count



type FacilityCode = string // ukclj, sbce, ukg, ...

/// return (seriesName * color) based on facility name
let facilitySeriesInfo : FacilityCode -> (string option * string) = function
    | "bse"   -> None          , "B Sežana"
    | "bto"   -> None          , "B Topolšica"
    | "sbbr"  -> None          , "SB Brežice"
    | "sbce"  -> Some "#70a471", "SB Celje"
    | "sbje"  -> None          , "SB Jesenice"
    | "sbiz"  -> None          , "SB Izola"
    | "sbms"  -> None          , "SB Murska Sobota"
    | "sbng"  -> None          , "SB Nova Gorica"
    | "sbnm"  -> None          , "SB Novo mesto"
    | "sbpt"  -> None          , "SB Ptuj"
    | "sbsg"  -> None          , "SB Slovenj Gradec"
    | "sbtr"  -> None          , "SB Trbovlje"
    | "ukclj" -> Some "#10829a", "UKC Ljubljana"
    | "ukcmb" -> Some "#003f5c", "UKC Maribor"
    | "ukg"   -> Some "#7B7226", "UK Golnik"
    | other   -> None          , other


type FacilityAssets = {
    year: int
    month: int
    day: int
    overall: Assets
    perHospital: Map<FacilityCode, Assets>
  } with
    member ps.Date = new DateTime(ps.year, ps.month, ps.day)
    member ps.JsDate = new DateTime(ps.year, ps.month, ps.day) |> Highcharts.Helpers.jsTime

let getSortedFacilityCodes (data: FacilityAssets []) =
    match data with
    | [||] -> []
    | [| _ |] -> []
    | data ->
        // TODO: in future we'll need more
        seq { // take few samples
            data.[data.Length/2]
            data.[data.Length-2]
            data.[data.Length-1]
        }
        |> Seq.collect (fun assets ->
            assets.perHospital
            |> Map.toSeq
            |> Seq.map (fun (facility, stats) ->
                let cnt =
                    match stats.beds.total, stats.icu.total with
                    | Some b, Some i -> Some (b+i)
                    | Some b, None -> Some b
                    | None, Some i -> Some i
                    | None, None -> None
                facility,cnt)) // hospital name
        |> Seq.fold (fun hospitals (hospital,cnt) -> hospitals |> Map.add hospital cnt) Map.empty // all
        |> Map.toList
        |> List.sortBy (fun (fc,cnt) ->
            printfn "hospital %s %A" fc cnt
            (cnt |> Option.defaultValue 0 |> ( * ) -1), (if fc.Length = 3 then fc else "x"+fc))
        |> List.map fst


let fetch () = async {
    let! code,json = Http.get url
    return
        match code with
        | 200 ->
            json
            |> SimpleJson.parse
            |> Json.convertFromJsonAs<FacilityAssets []>
            |> Ok
        | err ->
            Error (sprintf "got http %d while fetching %s" err url)
}


