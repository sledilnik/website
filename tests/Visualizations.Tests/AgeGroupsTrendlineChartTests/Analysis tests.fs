module AgeGroupsTrendlineChartTests.``Analysis tests``

open System.Collections.Generic
open Types
open Xunit
open Swensen.Unquote

//type AgeGroup =
//    { GroupKey : AgeGroupKey
//      Male : int option
//      Female : int option
//      All : int option }
//
//type AgeGroupsList = AgeGroup list

//type StatsDataPoint =
//    { DayFromStart : int
//      Date : System.DateTime
//      Phase : string
//      Tests : Tests
//      Cases : Cases
//      StatePerTreatment : Treatment
//      StatePerAgeToDate : AgeGroupsList
//      DeceasedPerAgeToDate : AgeGroupsList
//      HospitalEmployeePositiveTestsToDate : int option
//      RestHomeEmployeePositiveTestsToDate : int option
//      RestHomeOccupantPositiveTestsToDate : int option
//      UnclassifiedPositiveTestsToDate : int option
//    }

let calcDailyCasesByAge
    (prevDay: AgeGroupsList option)
    (currentDay: AgeGroupsList): AgeGroupsList =

    let calcIntOptionDiff value1 value2 =
        match value1, value2 with
        | Some value1, Some value2 -> value2 - value1 |> Some
        | Some _, None -> None
        | None, Some value2 -> Some value2
        | None, None -> None

    let calcAgeGroupDiff
        (prevDay: IDictionary<AgeGroupKey, AgeGroup>) ageGroup =
        match prevDay.TryGetValue ageGroup.GroupKey with
        | true, prevDayGroup ->
            { GroupKey = ageGroup.GroupKey
              Male = calcIntOptionDiff prevDayGroup.Male ageGroup.Male
              Female = calcIntOptionDiff prevDayGroup.Female ageGroup.Female
              All = calcIntOptionDiff prevDayGroup.All ageGroup.All
            }
        | false, _ -> ageGroup

    match prevDay with
    | None -> currentDay
    | Some prevDay ->
        // create a dictionary of prevDay age groups
        let prevDayGroups =
            prevDay
            |> List.map (fun ageGroup -> (ageGroup.GroupKey, ageGroup))
            |> dict

        currentDay
        // for each age group of currentDay find the group in prevDay
        // and calculate the difference
        |> List.map (fun ageGroup -> calcAgeGroupDiff prevDayGroups ageGroup)

let buildAgeGroups(): AgeGroupsList = []

let group
    groupIndex (male: int option) (female: int option): AgeGroup =
    let sum =
        match male, female with
        | Some male, Some female -> male + female |> Some
        | Some male, None -> Some male
        | None, Some female -> Some female
        | None, None -> None

    { GroupKey =
        { AgeFrom = groupIndex * 10 |> Some
          AgeTo = groupIndex * 10 + 1 |> Some }
      Male = male; Female = female; All = sum
    }

let withGroup group groups = group :: groups

[<Fact>]
let ``Can calculate data for first day``() =
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 1 (Some 10) (Some 15))

    let resultGroup = calcDailyCasesByAge None currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 1 (Some 10) (Some 15) @>

[<Fact>]
let ``Can calculate data when both days have same groups``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> withGroup (group 1 (Some 5) (Some 10))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))

    let resultGroup = calcDailyCasesByAge prevDay currentDay

    test <@ resultGroup.Length = 2 @>
    test <@ resultGroup.[0] = group 1 (Some 5) (Some 5) @>
    test <@ resultGroup.[1] = group 2 (Some 1) (Some 5) @>

[<Fact>]
let ``Can calculate data when prev day is missing some groups``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 1 (Some 5) (Some 10))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))
        |> withGroup (group 1 (Some 10) (Some 15))

    let resultGroup = calcDailyCasesByAge prevDay currentDay

    test <@ resultGroup.Length = 2 @>
    test <@ resultGroup.[0] = group 1 (Some 5) (Some 5) @>
    test <@ resultGroup.[1] = group 2 (Some 1) (Some 20) @>

[<Fact>]
let ``Can calculate data when current day is missing some groups``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 0) (Some 15))
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))

    let resultGroup = calcDailyCasesByAge prevDay currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 2 (Some 1) (Some 5) @>

[<Fact>]
let ``Can calculate data when prev day group has missing numbers``() =
    let prevDay =
        buildAgeGroups()
        |> withGroup (group 2 None None)
        |> Some
    let currentDay =
        buildAgeGroups()
        |> withGroup (group 2 (Some 1) (Some 20))

    let resultGroup = calcDailyCasesByAge prevDay currentDay

    test <@ resultGroup.Length = 1 @>
    test <@ resultGroup.[0] = group 2 (Some 1) (Some 20) @>
