module ``OurWorldInData tests``

open Elmish
open Fable.SimpleHttp

open EdelweissData.Base.Identifiers
open EdelweissData.Base.Routes
open EdelweissData.Base.Queries
open EdelweissData.Base.Values

open Xunit
open Swensen.Unquote

//
//let mutable loadedData = None
//
//let onLoaded data =
//    loadedData <- Some data

//[<Fact>]
//let ``Can load data from OurWorldInData``() =
//    let loadTask = Data.OurWorldInData.load ["Slovenia"; "Sweden"] onLoaded
//    let x = Cmd.OfAsync.start loadTask
//    let y = Async.AwaitIAsyncResult x
//
//    let c = 1 + 1
//    test <@ loadedData <> None @>

//[<Fact>]
//let ``Can load data from OurWorldInData 2``() =
//    let statusCode, response =
//        Http.post Data.OurWorldInData.dataUrl
//            (Data.OurWorldInData.createQuery ["Slovenia"; "Sweden"]
//             |> EdelweissData.Thoth.Queries.DataQuery.toString)
//        |> Async.RunSynchronously
//
//    test <@ statusCode = 200 @>
