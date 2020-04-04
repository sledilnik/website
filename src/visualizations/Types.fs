module Types

type ScaleType =
    | Linear
    | Logarithmic

type RemoteData<'data, 'error> =
    | NotAsked
    | Loading
    | Failure of 'error
    | Success of 'data

type Cases =
    { ConfirmedToday : int option
      ConfirmedToDate : int option
      ClosedToDate : int option
      ActiveToDate : int option }

type Treatment =
    { InHospital : int option
      InHospitalToDate : int option
      InICU : int option
      Critical : int option
      DeceasedToDate : int option
      Deceased : int option
      OutOfHospitalToDate : int option
      OutOfHospital : int option
      RecoveredToDate : int option }

type AgeGroup =
    { AgeFrom : int option
      AgeTo : int option
      TestedPositiveMale : int option
      TestedPositiveFemale : int option
      TestedPositiveAll : int option }

type StatePerAgeToDate = AgeGroup list

type StatsDataPoint =
    { DayFromStart : int
      Date : System.DateTime
      Phase : string
      PerformedTests : int option
      PerformedTestsToDate : int option
      PositiveTests : int option
      PositiveTestsToDate : int option
      Cases : Cases
      StatePerTreatment : Treatment
      StatePerAgeToDate : StatePerAgeToDate
      HospitalEmployeePositiveTestsToDate : int option
      RestHomeEmployeePositiveTestsToDate : int option
      RestHomeOccupantPositiveTestsToDate : int option
    }

type StatsData = StatsDataPoint list

type Municipality =
    { Name : string
      PositiveTests : int option }

type Region =
    { Name : string
      Municipalities : Municipality list }

type RegionsDataPoint =
    { Date : System.DateTime
      Regions : Region list }

type RegionsData = RegionsDataPoint list

type Visualization =
    | MetricsComparison
    | Patients
    | Hospitals
    | Spread
    | Regions
    | Municipalities
    | AgeGroups
    | Map
    | Infections

type RenderingMode =
    | Normal
    | Embeded of Visualization option

type State =
    { StatsData : RemoteData<StatsData, string>
      RegionsData : RemoteData<RegionsData, string>
      RenderingMode : RenderingMode }

type Msg =
    | StatsDataRequested
    | StatsDataLoaded of RemoteData<StatsData, string>
    | RegionsDataRequest
    | RegionsDataLoaded of RemoteData<RegionsData, string>
