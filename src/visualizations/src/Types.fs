module Types

type RemoteData<'data, 'error> =
    | NotAsked
    | Loading
    | Failure of 'error
    | Success of 'data

type DataPoint =
    { Date : System.DateTime
      Tests : int option
      TotalTests : int option
      Cases : int option
      TotalCases : int option
      Hospitalized : int option
      HospitalizedIcu : int option
      Deaths : int option
      TotalDeaths : int option }

type Data = DataPoint list

type Metric =
    { Color : string
      Visible : bool
      Label : string }

type Metrics =
    { Tests : Metric
      TotalTests : Metric
      Cases : Metric
      TotalCases : Metric
      Hospitalized : Metric
      HospitalizedIcu : Metric
      Deaths : Metric
      TotalDeaths : Metric }

type MetricMsg =
    | Tests
    | TotalTests
    | Cases
    | TotalCases
    | Hospitalized
    | HospitalizedIcu
    | Deaths
    | TotalDeaths

type State =
    { Data : RemoteData<Data, string>
      Metrics : Metrics }

type Msg =
    | DataLoaded of RemoteData<Data, string>
    | ToggleMetricVisible of MetricMsg
