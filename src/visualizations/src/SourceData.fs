module SourceData

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let private dataUrl = "https://covid19.rthand.com/api/data"

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

type private TransferDataPoint =
    { dayFromStart : int
      year : int
      month : int
      day : int
      phase : string
      performedTestsToDate : int option
      performedTests : int option
      positiveTestsToDate : int option
      positiveTests : int option
      statePerTreatment :
        {| inCare : int option
           inHospital : int option
           needsO2 : int option
           inICU : int option
           critical : int option
           deceased : int option
           deceasedToDate : int option
           outOfHospital : int option
           outOfHospitalToDate : int option |}
      statePerRegion :
        {| kp : int option
           foreign : int option
           sg : int option
           ms : int option
           ng : int option
           nm : int option
           po : int option
           unknown : int option
           kk : int option
           za : int option
           ce : int option
           kr : int option
           lj : int option
           mb : int option |}
      statePerAgeToDate :
        {| below16 : TransferAgeGroup
           from16to29 : TransferAgeGroup
           from30to49 : TransferAgeGroup
           from50to59 : TransferAgeGroup
           above60 : TransferAgeGroup |}
    }

    member this.ToDomain : DataPoint =
        { Date = System.DateTime(this.year, this.month, this.day)
          Tests = this.performedTests
          TotalTests = this.performedTestsToDate
          PositiveTests = this.positiveTests
          TotalPositiveTests = this.positiveTestsToDate
          Hospitalized = this.statePerTreatment.inHospital
          HospitalizedIcu = this.statePerTreatment.inICU
          Deaths = this.statePerTreatment.deceased
          TotalDeaths = this.statePerTreatment.deceasedToDate
          AgeGroups =
            { Below16 = this.statePerAgeToDate.below16.ToDomain
              From16to29 = this.statePerAgeToDate.from16to29.ToDomain
              From30to49 = this.statePerAgeToDate.from30to49.ToDomain
              From50to59 = this.statePerAgeToDate.from50to59.ToDomain
              Above60 = this.statePerAgeToDate.above60.ToDomain } }

type private TransferData = TransferDataPoint list

let loadData =
    async {
        let! (statusCode, response) = Http.get dataUrl
        if statusCode <> 200 then
            return DataLoaded (sprintf "Napaka pri nalaganju podatkov: %d" statusCode |> Failure)
        else
            try
                let transferData =
                    response
                    |> SimpleJson.parse
                    |> Json.convertFromJsonAs<TransferData>

                let data =
                    transferData
                    |> List.map (fun transferDataPoint -> transferDataPoint.ToDomain)

                return DataLoaded (Success data)
            with
                | ex -> return DataLoaded (sprintf "Napaka pri branju podatkov: %s" ex.Message |> Failure)
    }
