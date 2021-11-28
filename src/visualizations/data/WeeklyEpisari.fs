module Data.WeeklyEpisari

open Fable.SimpleHttp
open Fable.SimpleJson
open Types

let url = "https://api.sledilnik.org/api/episari-weekly"


type TransferHAgeGroup =
    { ageFrom : int option
      ageTo : int option
      covidIn : int option
      vaccinatedIn : int option
      icuIn : int option
      deceased : int option }

    member this.ToDomain : HospitalAgeGroup =
        { GroupKey = { AgeFrom = this.ageFrom; AgeTo = this.ageTo }
          CovidIn = this.covidIn
          VaccinatedIn = this.vaccinatedIn
          IcuIn = this.icuIn
          Deceased = this.deceased }

type private TransferWEpisariDataPoint =
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
        source : string option
        missing : string option
        sariIn : int option
        testedIn : int option
        covidIn : int option
        covidInNotSari : int option
        covidInVaccinated : int option
        covidInVaccinatedUnknown : int option
        covidInNotVaccinated : int option
        covidIcuIn : int option
        covidDiscoveredInHospital : int option
        covidAcquiredInHospital : int option
        covidOut : int option
        covidDeceased : int option
        perAge : TransferHAgeGroup list
    }

    member this.ToDomain : WeeklyEpisariDataPoint =
        {
          Week = this.week
          Date = System.DateTime(this.year, this.month, this.day)
          DateTo = System.DateTime(this.``to``.year, this.``to``.month, this.``to``.day)
          Source = this.source
          Missing = this.missing
          SariIn = this.sariIn
          TestedIn = this.testedIn
          CovidIn = this.covidIn
          CovidInNotSari = this.covidInNotSari
          CovidInVaccinated = this.covidInVaccinated
          CovidInVaccinatedUnknown = this.covidInVaccinatedUnknown
          CovidInNotVaccinated = this.covidInNotVaccinated
          CovidIcuIn = this.covidIcuIn
          CovidDiscoveredInHospital = this.covidDiscoveredInHospital
          CovidAcquiredInHospital = this.covidAcquiredInHospital
          CovidOut = this.covidOut
          CovidDeceased = this.covidDeceased
          PerAge =
            this.perAge
            |> List.map (fun item -> item.ToDomain)
            |> List.sortWith (fun a1 a2 ->
                match a1.GroupKey.AgeFrom, a1.GroupKey.AgeTo, a2.GroupKey.AgeFrom, a2.GroupKey.AgeTo with
                | Some a1f, Some a1t, Some a2f, Some a2t    -> if a1t < a2f then -1 else 1  // fully specified
                | Some a1f, None, Some a2f, _               -> if a1f < a2f then -1 else 1  // X+
                | None, None, _, _                          -> 1                            // no range -> mean value (put last)
                | _, _, _, _                                -> -1                           // keep order
            )
        }

type private TransferWEpisariData = TransferWEpisariDataPoint[]

let parseWEpisariData responseData =
    let transferWEpisariData =
        responseData
        |> Json.parseNativeAs<TransferWEpisariData>

    transferWEpisariData
    |> Array.map (fun transferDataPoint -> transferDataPoint.ToDomain)

let load =
    async {
        let! (statusCode, response) = Http.get url

        if statusCode <> 200 then
            return WeeklyEpisariDataLoaded (sprintf "Napaka pri nalaganju statističnih podatkov: %d" statusCode |> Failure)
        else
            try
                let data = parseWEpisariData response
                return WeeklyEpisariDataLoaded (Success data)
            with
            | ex -> return WeeklyEpisariDataLoaded (sprintf "Napaka pri branju statističnih podatkov: %s" (ex.Message.Substring(0, 1000)) |> Failure)
    }
