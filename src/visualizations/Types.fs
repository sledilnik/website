module Types

type ScaleType =
    | Linear
    | Logarithmic

type RemoteData<'data, 'error> =
    | NotAsked
    | Loading
    | Failure of 'error
    | Success of 'data

type TodayToDate =
    { ToDate : int option
      Today : int option }

type TestGroup =
    { Performed : TodayToDate
      Positive : TodayToDate }

type Tests =
    { Performed : TodayToDate
      Positive : TodayToDate
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

type Vaccination =
    { Administered : TodayToDate
      Administered2nd : TodayToDate
      Used : TodayToDate
      Delivered : TodayToDate }

type Treatment =
    { InHospital : int option
      InHospitalToDate : int option
      InICU : int option
      Critical : int option
      DeceasedToDate : int option
      Deceased : int option
      OutOfHospitalToDate : int option
      OutOfHospital : int option }

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
      All : int option } with

    static member colorOfAgeGroup ageGroupIndex =
        let colors =
            [| "#FFEEBA"; "#FFDA6B";"#E9B825";"#AEEFDB";"#52C4A2";"#33AB87"
               "#189A73";"#F4B2E0";"#D559B0";"#B01C83" |]
        colors.[ageGroupIndex]

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
      Vaccination : Vaccination
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
      HealthcareMaleCases : int option
      HealthcareFemaleCases : int option
      RetirementHomeOccupantCases : int option
      SentToQuarantine : int option
      Location : InfectionLocation
      Source : InfectionSource
      ImportedFrom : Map<string, int option>
    }

type WeeklyStatsData = WeeklyStatsDataPoint[]

type AreaCases =
    { Name : string
      ActiveCases : int option
      ConfirmedToDate : int option
      DeceasedToDate : int option }

type RegionMunicipalities =
    { Name : string
      Municipalities : AreaCases list }

type MunicipalitiesDataPoint =
    { Date : System.DateTime
      Regions : RegionMunicipalities list }

type MunicipalitiesData = MunicipalitiesDataPoint list

type RegionsDataPoint =
    { Date : System.DateTime
      Regions : AreaCases list }

type RegionsData = RegionsDataPoint list


type VisualizationType =
    | MetricsComparison
    | DailyComparison
    | Patients
    | IcuPatients
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
    | Schools
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
    | WeeklyDemographics

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
      MunicipalitiesData : RemoteData<MunicipalitiesData, string>
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
    | MunicipalitiesDataRequest
    | MunicipalitiesDataLoaded of RemoteData<MunicipalitiesData, string>
