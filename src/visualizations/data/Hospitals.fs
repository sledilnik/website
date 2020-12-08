module Data.Hospitals

open System

let url = "https://api.sledilnik.org/api/hospitals"

type CountType =
    | Total
    | Max
    | Occupied
    | Free
    | MaxFree

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

type FacilityAssets = {
    year: int
    month: int
    day: int
    overall: Assets
    perHospital: Map<FacilityCode, Assets>
  } with
    member ps.Date = DateTime(ps.year, ps.month, ps.day)
    member ps.JsDate12h = DateTime(ps.year, ps.month, ps.day)
                          |> Highcharts.Helpers.jsTime12h

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
                let quality =
                    seq {
                        stats.beds.total
                        stats.vents.total |> Option.map (fun vents -> if vents>0 then 1000 else 0)
                        //stats.icu.total
                    }
                    |> Seq.sumBy (Option.defaultValue -1)
                facility,quality)) // hospital name
        |> Seq.fold (fun hospitals (hospital,cnt) -> hospitals |> Map.add hospital cnt) Map.empty // all
        |> Map.toList
        |> List.sortBy (fun (fc,quality) ->
            //printfn "hospital %s %A" fc cnt
            -quality, (if fc.Length = 3 then fc else "x"+fc))
        |> List.map fst


let getOrFetch = DataLoader.makeDataLoader<FacilityAssets []> url
