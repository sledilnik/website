module Data.WeeklyStats

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let url = "https://api.sledilnik.org/api/stats-weekly"

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
        sentTo : 
              {|
                  quarantine : int option
              |}
        source : TransferSource
        from : Map<string,int option>
    }

    member this.ToDomain : WeeklyStatsDataPoint =
        { 
          Week = this.week
          Date = System.DateTime(this.year, this.month, this.day)
          DateTo = System.DateTime(this.``to``.year, this.``to``.month, this.``to``.day)
          ConfirmedCases = this.confirmed
          SentToQuarantine = this.sentTo.quarantine
          Source = this.source.ToDomain
          ImportedFrom = this.from
        }

type private TransferWStatsData = TransferWStatsDataPoint list

let parseWStatsData responseData =
    let transferWStatsData =
        responseData
        |> Json.parseNativeAs<TransferWStatsData>

    transferWStatsData
    |> List.map (fun transferDataPoint -> transferDataPoint.ToDomain)

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
