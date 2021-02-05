module AgeGroupsTimelineTests.``Working with dated arrays``

open System
open DataAnalysis.DatedTypes
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Can map date tuples to a dated list``() =
    let baseDate = DateTime(2020, 07, 11)

    let tuples = [
        (baseDate.AddDays 1., 20); (baseDate, 10); (baseDate.AddDays 3., 30)
    ]

    let expectedList = {
        StartDate = baseDate
        Data = [| 10; 20; 0; 30 |]
    }

    let datedList = mapDateTuplesListToArray tuples

    test <@ datedList = expectedList @>
