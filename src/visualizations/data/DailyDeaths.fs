module Data.DailyDeaths

open FsToolkit.ErrorHandling

let apiUrl = "https://api.sledilnik.org/api/age-daily-deaths-slovenia"

type private SourceAgeGroupsDataPoint = {
    ``0-3`` : int option
    ``4-18`` : int option
    ``19-31`` : int option
    ``32-41`` : int option
    ``42-51`` : int option
    ``52-61`` : int option
    ``62-71`` : int option
    ``72+`` : int option
}

type private SourceDataPoint = {
    year : int
    month : int
    day : int
    male : SourceAgeGroupsDataPoint
    female : SourceAgeGroupsDataPoint
}

type AgeGroupDataPoint = {
    DeceasedAgeGroupFrom0to3 : int option
    DeceasedAgeGroupFrom4to18 : int option
    DeceasedAgeGroupFrom18to31 : int option
    DeceasedAgeGroupFrom32to41 : int option
    DeceasedAgeGroupFrom42to51 : int option
    DeceasedAgeGroupFrom52to61 : int option
    DeceasedAgeGroupFrom62to71 : int option
    DeceasedAgeGroup72AndMore : int option
} with
    member this.sum =
        [ this.DeceasedAgeGroupFrom0to3
          this.DeceasedAgeGroupFrom4to18
          this.DeceasedAgeGroupFrom18to31
          this.DeceasedAgeGroupFrom32to41
          this.DeceasedAgeGroupFrom42to51
          this.DeceasedAgeGroupFrom52to61
          this.DeceasedAgeGroupFrom62to71
          this.DeceasedAgeGroup72AndMore ]
        |> List.sumBy (fun x -> Option.defaultValue 0 x)

type DataPoint = {
    Date : System.DateTime
    Deceased : int
    DeceasedMale : AgeGroupDataPoint
    DeceasedFemale : AgeGroupDataPoint
}

let loadData () =
    asyncResult {
        let! data = DataLoader.makeDataLoader<SourceDataPoint list> apiUrl ()

        return data
        |> List.map (fun dp ->
            let date = System.DateTime(dp.year, dp.month, dp.day)

            let deceasedSex data = {
                DeceasedAgeGroupFrom0to3 = data.``0-3``
                DeceasedAgeGroupFrom4to18 = data.``4-18``
                DeceasedAgeGroupFrom18to31 = data.``19-31``
                DeceasedAgeGroupFrom32to41 = data.``32-41``
                DeceasedAgeGroupFrom42to51 = data.``42-51``
                DeceasedAgeGroupFrom52to61 = data.``52-61``
                DeceasedAgeGroupFrom62to71 = data.``62-71``
                DeceasedAgeGroup72AndMore = data.``72+``
            }

            let deceasedMale = deceasedSex dp.male
            let deceasedFemale = deceasedSex dp.female

            { Date = date
              Deceased = deceasedMale.sum + deceasedFemale.sum
              DeceasedMale = deceasedMale
              DeceasedFemale = deceasedFemale
            }
        )
    }
