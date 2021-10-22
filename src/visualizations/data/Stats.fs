module Data.Stats

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let url = "https://api.sledilnik.org/api/stats"

let todayFromToDate (a : int option) (b : int option) =
  match a, b with
  | Some aa, Some bb -> Some (bb - aa)
  | Some aa, None -> None
  | None, Some _ -> b
  | _ -> None

type TransferAgeGroup =
    { ageFrom : int option
      ageTo : int option
      allToDate : int option
      femaleToDate : int option
      maleToDate : int option }

    member this.ToDomain : AgeGroup =
        { GroupKey = { AgeFrom = this.ageFrom; AgeTo = this.ageTo }
          Male = this.maleToDate
          Female = this.femaleToDate
          All = this.allToDate }

type private TransferStatsDataPoint =
    { dayFromStart : int
      year : int
      month : int
      day : int
      phase : string
      deceased : int option
      deceasedToDate : int option
      tests :
        {|
            performed : {| toDate : int option; today : int option |}
            positive : {| toDate : int option; today : int option |}
            regular :
              {|
                performed : {| toDate : int option; today : int option |}
                positive : {| toDate : int option; today : int option |}
              |}
            nsApr20 :
              {|
                performed : {| toDate : int option; today : int option |}
                positive : {| toDate : int option; today : int option |}
              |}
        |}
      cases :
        {| confirmedToday : int option
           confirmedToDate : int option
           recoveredToDate : int option
           closedToDate : int option
           active : int option
           hs : {| employeeConfirmedToDate: int option |}
           rh : {| employeeConfirmedToDate: int option; occupantConfirmedToDate: int option |}
           unclassified : {| confirmedToDate: int option |}
           vaccinatedConfirmedToDate : int option
        |}
      statePerTreatment :
        {| inHospital : int option
           inHospitalToDate : int option
           inICU : int option
           critical : int option
           deceasedToDate : int option
           deceased : int option
           outOfHospitalToDate : int option
           outOfHospital : int option |}
      statePerAgeToDate : TransferAgeGroup list
      deceasedPerAgeToDate : TransferAgeGroup list
      deceasedPerType :
        {| rhOccupant : int option
           other : int option |}
      vaccination :
        {|
            administered : {| toDate : int option; today : int option |}
            administered2nd : {| toDate : int option; today : int option |}
            administered3rd : {| toDate : int option; today : int option |}
            used : {| toDate : int option |}
            delivered : {| toDate : int option |}
        |}
    }

    member this.ToDomain prevDP : StatsDataPoint =

        { DayFromStart = this.dayFromStart
          Date = System.DateTime(this.year, this.month, this.day)
          Phase = this.phase
          Deceased = this.deceased
          DeceasedToDate = this.deceasedToDate
          Tests =
            { Performed = { ToDate = this.tests.performed.toDate; Today = this.tests.performed.today }
              Positive = { ToDate = this.tests.positive.toDate; Today = this.tests.positive.today }
              Regular =
                { Performed = { ToDate = this.tests.regular.performed.toDate; Today = this.tests.regular.performed.today }
                  Positive = { ToDate = this.tests.regular.positive.toDate; Today = this.tests.regular.positive.today } }
              NsApr20 =
                { Performed = { ToDate = this.tests.nsApr20.performed.toDate; Today = this.tests.nsApr20.performed.today }
                  Positive = { ToDate = this.tests.nsApr20.positive.toDate; Today = this.tests.nsApr20.positive.today } }
            }
          Cases =
            { ConfirmedToday = this.cases.confirmedToday
              ConfirmedToDate = this.cases.confirmedToDate
              RecoveredToDate = this.cases.recoveredToDate
              ClosedToDate = this.cases.closedToDate
              Active = this.cases.active
              VaccinatedConfirmedToDate = this.cases.vaccinatedConfirmedToDate
            }
          StatePerTreatment =
            { InHospital = this.statePerTreatment.inHospital
              InHospitalToDate = this.statePerTreatment.inHospitalToDate
              InICU = this.statePerTreatment.inICU
              Critical = this.statePerTreatment.critical
              DeceasedToDate = this.statePerTreatment.deceasedToDate
              Deceased = this.statePerTreatment.deceased
              OutOfHospitalToDate = this.statePerTreatment.outOfHospitalToDate
              OutOfHospital = this.statePerTreatment.outOfHospital }
          StatePerAgeToDate = this.statePerAgeToDate |> List.map (fun item -> item.ToDomain)
          DeceasedPerAgeToDate = this.deceasedPerAgeToDate |> List.map (fun item -> item.ToDomain)
          DeceasedPerType =
            { RhOccupant = this.deceasedPerType.rhOccupant
              Other = this.deceasedPerType.other }
          HospitalEmployeePositiveTestsToDate = this.cases.hs.employeeConfirmedToDate
          RestHomeEmployeePositiveTestsToDate = this.cases.rh.employeeConfirmedToDate
          RestHomeOccupantPositiveTestsToDate = this.cases.rh.occupantConfirmedToDate
          UnclassifiedPositiveTestsToDate = this.cases.unclassified.confirmedToDate
          Vaccination =
            { Administered = { ToDate = this.vaccination.administered.toDate; Today = this.vaccination.administered.today }
              Administered2nd = { ToDate = this.vaccination.administered2nd.toDate; Today = this.vaccination.administered2nd.today }
              Administered3rd = { ToDate = this.vaccination.administered3rd.toDate; Today = this.vaccination.administered3rd.today }
              Used = {
                ToDate = this.vaccination.used.toDate
                Today = this.vaccination.used.toDate |> todayFromToDate prevDP.vaccination.used.toDate
              }
              Delivered = {
                ToDate = this.vaccination.delivered.toDate
                Today = this.vaccination.delivered.toDate |> todayFromToDate prevDP.vaccination.delivered.toDate } }
        }

type private TransferStatsData = TransferStatsDataPoint list

let parseStatsData responseData =
    let transferStatsData =
        responseData
        |> Json.parseNativeAs<TransferStatsData>

    transferStatsData
    |> List.pairwise
    |> List.map (fun (prevDP, transferDP) -> transferDP.ToDomain prevDP)

let load =
    async {
        let! (statusCode, response) = Http.get url

        if statusCode <> 200 then
            return StatsDataLoaded (sprintf "Napaka pri nalaganju statističnih podatkov: %d" statusCode |> Failure)
        else
            try
                let data = parseStatsData response
                return StatsDataLoaded (Success data)
            with
            | ex -> return StatsDataLoaded (sprintf "Napaka pri branju statističnih podatkov: %s" (ex.Message.Substring(0, 1000)) |> Failure)
    }
