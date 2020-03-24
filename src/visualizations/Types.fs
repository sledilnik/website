module Types

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

type StatsDataPoint =
    { Date : System.DateTime
      Tests : int option
      TotalTests : int option
      PositiveTests : int option
      TotalPositiveTests : int option
      Hospitalized : int option
      HospitalizedIcu : int option
      Deaths : int option
      TotalDeaths : int option
      AgeGroups : AgeGroups }

type StatsData = StatsDataPoint list

type City =
    { Name : string
      PositiveTests : int option }

type Region =
    { Name : string
      Cities : City list }

type RegionsDataPoint =
    { Date : System.DateTime
      Regions : Region list }

type RegionsData = RegionsDataPoint list

type Data =
    { StatsData : StatsData
      RegionsData : RegionsData }

type State =
    { Data : RemoteData<Data, string> }

type Msg =
    | DataLoaded of RemoteData<Data, string>
