module SourceData

open Fable.SimpleHttp
open Fable.SimpleJson

open Types

let private dataUrl = "https://covid19.rthand.com/api/data"

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
    //   statePerAgeToDate :
    //     {| age0to15 :
    //         {| allToDate : int option
    //            femaleToDate : int option
    //            maleToDate : int option |}
    //        age16to29 :
    //         {| allToDate : int option
    //            femaleToDate : int option
    //            maleToDate : int option |}
    //        age30to49 :
    //         {| allToDate : int option
    //            femaleToDate : int option
    //            maleToDate : int option |}
    //        age50to59 :
    //         {| allToDate : int option
    //            femaleToDate : int option
    //            maleToDate : int option |}
    //        ageAbove60 :
    //         {| allToDate : int option
    //            femaleToDate : int option
    //            maleToDate : int option |} |}
    }

    member this.ToDomain : DataPoint =
        { Date = System.DateTime(this.year, this.month, this.day)
          Tests = this.performedTests
          TotalTests = this.performedTestsToDate
          Cases = this.positiveTests
          TotalCases = this.positiveTestsToDate
          Hospitalized = this.statePerTreatment.inHospital
          HospitalizedIcu = this.statePerTreatment.inICU
          Deaths = this.statePerTreatment.deceased
          TotalDeaths = this.statePerTreatment.deceasedToDate }

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
