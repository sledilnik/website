module Data.WeeklyStats

open Fable.SimpleHttp
open Fable.SimpleJson
open Types

let url = "https://api.sledilnik.org/api/stats-weekly"

type private TransferLocation =
    { family : int option
      work : int option
      school : int option
      hospital : int option
      otherhc : int option
      rh : int option
      prison : int option
      transport : int option
      shop : int option
      restaurant : int option
      sport : int option
      gathering_private : int option
      gathering_organized : int option
      other : int option
      unknown : int option
    }

    member this.ToDomain : InfectionLocation =
        { Family = this.family
          Work = this.work
          School = this.school
          Hospital = this.hospital
          OtherHealthcare = this.otherhc
          RetirementHome = this.rh
          Prison = this.prison
          Transport = this.transport
          Shop = this.shop
          Restaurant = this.restaurant
          Sport = this.sport
          GatheringPrivate = this.gathering_private
          GatheringOrganized = this.gathering_organized
          Other = this.other
          Unknown = this.unknown }


type private TransferSource =
    { quarantine : int option
      local : int option
      import : int option
      ``import-related`` : int option
      unknown : int option
    }

    member this.ToDomain : InfectionSource =
        { FromQuarantine = this.quarantine
          Local = this.local
          Import = this.import
          ImportRelated = this.``import-related``
          Unknown = this.unknown }

let private mapKeysToUpperCase (map: Map<string, 'T>) =
    Map.toSeq map
    |> Seq.map (fun (k, v) -> (k.ToUpper(), v))
    |> Map.ofSeq

type private TransferWStatsDataPoint =
    {
        week : string
        year : int
        month : int
        day : int
        ``to`` :
            {|
                year : int
                month : int
                day : int
            |}
        confirmed : int option
        investigated : int option
        healthcare : int option
        healthcareMale : int option
        healthcareFemale : int option
        ``rh-occupant`` : int option
        sentTo :
              {|
                  quarantine : int option
              |}
        locations : TransferLocation
        source : TransferSource
        from : Map<string,int option>
    }

    member this.ToDomain : WeeklyStatsDataPoint =
        {
          Week = this.week
          Date = System.DateTime(this.year, this.month, this.day)
          DateTo = System.DateTime(this.``to``.year, this.``to``.month, this.``to``.day)
          ConfirmedCases = this.confirmed
          InvestigatedCases = this.investigated
          HealthcareCases = this.healthcare
          HealthcareMaleCases = this.healthcareMale
          HealthcareFemaleCases = this.healthcareFemale
          RetirementHomeOccupantCases = this.``rh-occupant``
          SentToQuarantine = this.sentTo.quarantine
          Location = this.locations.ToDomain
          Source = this.source.ToDomain
          ImportedFrom = this.from |> mapKeysToUpperCase
        }

type private TransferWStatsData = TransferWStatsDataPoint[]

let parseWStatsData responseData =
    let transferWStatsData =
        responseData
        |> Json.parseNativeAs<TransferWStatsData>

    transferWStatsData
    |> Array.map (fun transferDataPoint -> transferDataPoint.ToDomain)

let countryTotals (weeklyStats: seq<WeeklyStatsDataPoint>) =
    let sum (a: int option) (b: int option) =
        match a, b with
        | None, Some b_ -> b_
        | Some a_, Some b_ -> a_ + b_
        | Some a_, None -> a_
        | _ -> 0

    let sumOfMaps (a: Map<string, int>) (b: Map<string, int option>) =
        Map.fold (fun a_ key value -> Map.add key (sum (a.TryFind key) value) a_) a b

    weeklyStats
    |> Seq.map (fun ws -> ws.ImportedFrom)
    |> Seq.fold sumOfMaps Map.empty
    |> Map.filter (fun _ v -> v > 0)
    |> Map.toArray
    |> Array.sortByDescending (fun (_, v) -> v)

let load =
    async {
        let! (statusCode, response) = Http.get url

        if statusCode <> 200 then
            return WeeklyStatsDataLoaded (sprintf "Napaka pri nalaganju statističnih podatkov: %d" statusCode |> Failure)
        else
            try
                let data = parseWStatsData response
                return WeeklyStatsDataLoaded (Success data)
            with
            | ex -> return WeeklyStatsDataLoaded (sprintf "Napaka pri branju statističnih podatkov: %s" (ex.Message.Substring(0, 1000)) |> Failure)
    }
