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

type PersonTypeCount =
    { RhOccupant : int option
      Other : int option }

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
      DeceasedPerType : PersonTypeCount
      HospitalEmployeePositiveTestsToDate : int option
      RestHomeEmployeePositiveTestsToDate : int option
      RestHomeOccupantPositiveTestsToDate : int option
      UnclassifiedPositiveTestsToDate : int option
    }

type StatsData = StatsDataPoint list

type InfectionLocation =
    { Family : int option
      Work : int option
      School : int option
      Hospital : int option
      OtherHealthcare : int option
      RetirementHome : int option
      Prison : int option
      Transport : int option
      Shop : int option
      Restaurant : int option
      Sport : int option
      GatheringPrivate : int option
      GatheringOrganized : int option
      Other : int option
      Unknown : int option
    }

type InfectionSource =
    { FromQuarantine : int option
      Local : int option
      Import : int option
      ImportRelated : int option
      Unknown : int option
    }

type WeeklyStatsDataPoint =
    { Week : string
      Date : System.DateTime
      DateTo : System.DateTime
      ConfirmedCases : int option
      InvestigatedCases : int option
      HealthcareCases : int option
      RetirementHomeOccupantCases : int option
      SentToQuarantine : int option
      Location : InfectionLocation
      Source : InfectionSource
      ImportedFrom : Map<string, int option>
    }

type WeeklyStatsData = WeeklyStatsDataPoint[]

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
    | DailyComparison
    | Patients
    | CarePatients
    | Ratios
    | HCenters
    | Hospitals
    | HcCases
    | Tests
    | Cases
    | Spread
    | Regions
    | Regions100k
    | Sources
    | Municipalities
    | AgeGroups
    | AgeGroupsTimeline
    | Map
    | RegionMap
    | EuropeMap
    | WorldMap
    | Infections
    | CountriesCasesPer100k
    | CountriesActiveCasesPer100k
    | CountriesNewDeathsPer100k
    | CountriesTotalDeathsPer100k
    | PhaseDiagram
    | Deceased
    | ExcessDeaths
    | MetricsCorrelation

type RenderingMode =
    | Normal
    | Embedded of VisualizationType option

type State =
    {
      ApiEndpoint : string
      Page : string
      Query : obj // URL query parameters
      StatsData : RemoteData<StatsData, string>
      WeeklyStatsData : RemoteData<WeeklyStatsData, string>
      RegionsData : RemoteData<RegionsData, string>
      RenderingMode : RenderingMode }

type Visualization = {
    VisualizationType: VisualizationType
    ClassName: string
    ChartTextsGroup: string
    Explicit: bool
    Renderer: State -> Fable.React.ReactElement
}

type Msg =
    | StatsDataRequested
    | StatsDataLoaded of RemoteData<StatsData, string>
    | WeeklyStatsDataRequested
    | WeeklyStatsDataLoaded of RemoteData<WeeklyStatsData, string>
    | RegionsDataRequest
    | RegionsDataLoaded of RemoteData<RegionsData, string>
