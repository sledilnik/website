module Data.DailyDeaths

open FsToolkit.ErrorHandling

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
    DeceasedAgeGroupFrom1tto31 : int option
    DeceasedAgeGroupFrom3tto41 : int option
    DeceasedAgeGroupFrom4tto51 : int option
    DeceasedAgeGroupFrom5tto61 : int option
    DeceasedAgeGroupFrom6tto71 : int option
    DeceasedAgeGroupOver72 : int option
} with
    member this.sum =
        [ this.DeceasedAgeGroupFrom0to3
          this.DeceasedAgeGroupFrom4to18
          this.DeceasedAgeGroupFrom1tto31
          this.DeceasedAgeGroupFrom3tto41
          this.DeceasedAgeGroupFrom4tto51
          this.DeceasedAgeGroupFrom5tto61
          this.DeceasedAgeGroupFrom6tto71
          this.DeceasedAgeGroupOver72 ]
        |> List.sumBy (fun x -> Option.defaultValue 0 x)

type DataPoint = {
    Date : System.DateTime
    Deceased : int
    DeceasedMale : AgeGroupDataPoint
    DeceasedFemale : AgeGroupDataPoint
}

let apiUrl = "https://api.sledilnik.org/api/age-daily-deaths-slovenia"

let loadData () =
    asyncResult {
        let! data = DataLoader.makeDataLoader<SourceDataPoint list> apiUrl ()

        return data
        |> List.map (fun dp ->
            let date = System.DateTime(dp.year, dp.month, dp.day)

            let deceasedSex data = {
                DeceasedAgeGroupFrom0to3 = data.``0-3``
                DeceasedAgeGroupFrom4to18 = data.``4-18``
                DeceasedAgeGroupFrom1tto31 = data.``19-31``
                DeceasedAgeGroupFrom3tto41 = data.``32-41``
                DeceasedAgeGroupFrom4tto51 = data.``42-51``
                DeceasedAgeGroupFrom5tto61 = data.``52-61``
                DeceasedAgeGroupFrom6tto71 = data.``62-71``
                DeceasedAgeGroupOver72 = data.``72+``
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
