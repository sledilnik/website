module Types

type ScaleType =
    | Linear
    | Logarithmic

type RemoteData<'data, 'error> =
    | NotAsked
    | Loading
    | Failure of 'error
    | Success of 'data

type TestMeasure =
    { ToDate : int option
      Today : int option }

type TestGroup =
    { Performed : TestMeasure
      Positive : TestMeasure }

type Tests =
    { Performed : TestMeasure
      Positive : TestMeasure
      Regular : TestGroup
      NsApr20 : TestGroup
    }

type Cases =
    { ConfirmedToday : int option
      ConfirmedToDate : int option
      RecoveredToDate : int option
      ClosedToDate : int option
      Active : int option
    }

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

type AgeGroupKey = {
    AgeFrom : int option
    AgeTo : int option
    } with

    member this.Label =
        match this.AgeFrom, this.AgeTo with
        | None, None -> ""
        | None, Some b -> sprintf "0-%d" b
        | Some a, Some b -> sprintf "%d-%d" a b
        | Some a, None -> sprintf "%d+" a

type AgeGroup =
    { GroupKey : AgeGroupKey
      Male : int option
      Female : int option
      All : int option }

type AgeGroupsList = AgeGroup list

type StatsDataPoint =
    { DayFromStart : int
      Date : System.DateTime
      Phase : string
      Tests : Tests
      Cases : Cases
      StatePerTreatment : Treatment
      StatePerAgeToDate : AgeGroupsList
      DeceasedPerAgeToDate : AgeGroupsList
      HospitalEmployeePositiveTestsToDate : int option
      RestHomeEmployeePositiveTestsToDate : int option
      RestHomeOccupantPositiveTestsToDate : int option
      UnclassifiedPositiveTestsToDate : int option
    }

type StatsData = StatsDataPoint list

type Municipality =
    { Name : string
      ActiveCases : int option
      ConfirmedToDate : int option
      DeceasedToDate : int option }

type Region =
    { Name : string
      Municipalities : Municipality list }

type RegionsDataPoint =
    { Date : System.DateTime
      Regions : Region list }

type RegionsData = RegionsDataPoint list

type VisualizationType =
    | MetricsComparison
    | Patients
    | Ratios
    | HCenters
    | Hospitals
    | Tests
    | Cases
    | Spread
    | Regions
    | Municipalities
    | AgeGroups
    | Map
    | Infections
    | Countries

type RenderingMode =
    | Normal
    | Embedded of VisualizationType option

type State =
    { Query : obj // URL query parameters
      StatsData : RemoteData<StatsData, string>
      RegionsData : RemoteData<RegionsData, string>
      RenderingMode : RenderingMode }

type Visualization = {
    VisualizationType: VisualizationType
    ClassName: string
    Label: string
    Explicit: bool
    Renderer: State -> Fable.React.ReactElement
}

type Msg =
    | StatsDataRequested
    | StatsDataLoaded of RemoteData<StatsData, string>
    | RegionsDataRequest
    | RegionsDataLoaded of RemoteData<RegionsData, string>

