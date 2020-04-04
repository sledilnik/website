module Data.Stats

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let url = "https://covid19.rthand.com/api/stats"

type TransferAgeGroup =
    { ageFrom : int option
      ageTo : int option
      allToDate : int option
      femaleToDate : int option
      maleToDate : int option }

    member this.ToDomain : AgeGroup =
        { AgeFrom = this.ageFrom
          AgeTo = this.ageTo
          TestedPositiveMale = this.maleToDate
          TestedPositiveFemale = this.femaleToDate
          TestedPositiveAll = this.allToDate }

type private TransferStatsDataPoint =
    { dayFromStart : int
      year : int
      month : int
      day : int
      phase : string
      performedTestsToDate : int option
      performedTests : int option
      positiveTestsToDate : int option
      positiveTests : int option
      cases :
        {| confirmedToday : int option
           confirmedToDate : int option
           closedToDate : int option
           activeToDate : int option
           hs : {| employeeConfirmedToDate: int option |}
           rh : {| employeeConfirmedToDate: int option; occupantConfirmedToDate: int option |}
        |}
      statePerTreatment :
        {| inHospital : int option
           inHospitalToDate : int option
           inICU : int option
           critical : int option
           deceasedToDate : int option
           deceased : int option
           outOfHospitalToDate : int option
           outOfHospital : int option
           recoveredToDate : int option |}
      statePerAgeToDate : TransferAgeGroup list
    }

    member this.ToDomain : StatsDataPoint =
        { DayFromStart = this.dayFromStart
          Date = System.DateTime(this.year, this.month, this.day)
          Phase = this.phase
          PerformedTests = this.performedTests
          PerformedTestsToDate = this.performedTestsToDate
          PositiveTests = this.positiveTests
          PositiveTestsToDate = this.positiveTestsToDate
          Cases =
            { ConfirmedToday = this.cases.confirmedToday
              ConfirmedToDate = this.cases.confirmedToDate
              ClosedToDate = this.cases.closedToDate
              ActiveToDate = this.cases.activeToDate }
          StatePerTreatment =
            { InHospital = this.statePerTreatment.inHospital
              InHospitalToDate = this.statePerTreatment.inHospitalToDate
              InICU = this.statePerTreatment.inICU
              Critical = this.statePerTreatment.critical
              DeceasedToDate = this.statePerTreatment.deceasedToDate
              Deceased = this.statePerTreatment.deceased
              OutOfHospitalToDate = this.statePerTreatment.outOfHospitalToDate
              OutOfHospital = this.statePerTreatment.outOfHospital
              RecoveredToDate = this.statePerTreatment.recoveredToDate }
          StatePerAgeToDate = this.statePerAgeToDate |> List.map (fun item -> item.ToDomain)
          HospitalEmployeePositiveTestsToDate = this.cases.hs.employeeConfirmedToDate
          RestHomeEmployeePositiveTestsToDate = this.cases.rh.employeeConfirmedToDate
          RestHomeOccupantPositiveTestsToDate = this.cases.rh.occupantConfirmedToDate
        }

type private TransferStatsData = TransferStatsDataPoint list

let parseStatsData responseData =
    let transferStatsData =
        responseData
        |> SimpleJson.parse
        |> Json.convertFromJsonAs<TransferStatsData>

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
                | ex -> return StatsDataLoaded (sprintf "Napaka pri branju statističnih podatkov: %s" ex.Message |> Failure)
    }
