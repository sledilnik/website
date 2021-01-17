module Data.Stats

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let url = "https://api.sledilnik.org/api/stats"

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
        |}
    }

    member this.ToDomain : StatsDataPoint =
        { DayFromStart = this.dayFromStart
          Date = System.DateTime(this.year, this.month, this.day)
          Phase = this.phase
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
              Active = this.cases.active }
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
            { Administered = { ToDate = this.vaccination.administered.toDate; Today = this.vaccination.administered.today } }
        }

type private TransferStatsData = TransferStatsDataPoint list

let parseStatsData responseData =
    let transferStatsData =
        responseData
        |> Json.parseNativeAs<TransferStatsData>

    transferStatsData
    |> List.map (fun transferDataPoint -> transferDataPoint.ToDomain)

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
