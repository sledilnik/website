module DataAnalysis.DatedTypes

open System


type DateTuple<'T> = DateTime * 'T
type DateTupleList<'T> = DateTuple<'T> list
type DatedArray<'T> = {
    StartDate: DateTime
    Data: 'T[]
}

let mapDateTuplesListToArray (dateTupleList: DateTupleList<'T>)
    : DatedArray<'T> =
    // find the first and the last dates in the list
    let (firstDate, _) = dateTupleList |> List.minBy(fun (date, _) -> date.Date)
    let (lastDate, _) = dateTupleList |> List.maxBy(fun (date, _) -> date.Date)

    // from that, we can know the size of the resulting array
    let arraySize = (Days.between firstDate lastDate) + 1

    // create an empty array
    let array: 'T[] = Array.zeroCreate arraySize

    // now fill the array with the values from the original list
    dateTupleList
    |> List.iter(fun (date, itemData) ->
        let daysSince = Days.between firstDate date
        array.[daysSince] <- itemData
        )

    { StartDate = firstDate; Data = array }

let trimDatedArray trimPredicate (datedArray: DatedArray<'T>) =
    let trimmedLeading =
        datedArray.Data
        |> Array.skipWhile trimPredicate

    // we need to increment the start date by the count of trimmed leading days
    let daysToIncrement =
        (datedArray.Data |> Array.length) - (trimmedLeading |> Array.length)

    let newStartDate = datedArray.StartDate |> Days.add daysToIncrement

    let trimmed =
        trimmedLeading
        |> Array.rev
        |> Array.skipWhile trimPredicate
        |> Array.rev

    { StartDate = newStartDate; Data = trimmed }

let mapDatedArrayItems mapping datedArray =
    let startDate = datedArray.StartDate
    let originalArray = datedArray.Data
    { StartDate = startDate; Data = originalArray |> Array.map mapping }

let mapDatedArray mapping datedArray =
    let startDate = datedArray.StartDate
    let originalArray = datedArray.Data
    { StartDate = startDate; Data = originalArray |> mapping }
