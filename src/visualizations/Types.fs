module Types

type ScaleType =
    | Linear
    | Logarithmic

type RemoteData<'data, 'error> =
    | NotAsked
    | Loading
    | Failure of 'error
    | Success of 'data

type AgeGroup =
    { AgeFrom : int option
      AgeTo : int option
      TestedPositiveMale : int option
      TestedPositiveFemale : int option
      TestedPositiveAll : int option }

type AgeGroups = AgeGroup list

type Cases =
    { ConfirmedToday : int option
      ConfirmedToDate : int option
      ClosedToDate : int option
      ActiveToDate : int option }

type StatsDataPoint =
    { DayFromStart : int
      Date : System.DateTime
      Phase : string

      PerformedTests : int option
      PerformedTestsToDate : int option
      PositiveTests : int option
      PositiveTestsToDate : int option

      Hospitalized : int option
      HospitalizedToDate : int option
      HospitalizedIcu : int option
      Deaths : int option
      TotalDeaths : int option
      OutOfHospitalToDate : int option
      OutOfHospital : int option
      RecoveredToDate : int option
      AgeGroups : AgeGroups
      Cases : Cases }

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

type Data =
    { StatsData : StatsData
      RegionsData : RegionsData }

type Visualization =
    | MetricsComparison
    | Patients
    | Hospitals
    | Spread
    | Regions
    | Municipalities
    | AgeGroups

type RenderingMode =
    | Normal
    | Embeded of Visualization option

type State =
    { Data : RemoteData<Data, string>
      RenderingMode : RenderingMode }

type Msg =
    | DataLoaded of RemoteData<Data, string>
