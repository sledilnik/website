module DataLoader

open Fable.SimpleHttp
open Fable.SimpleJson

let formatError url error =
    sprintf "Napaka pri nalaganju podatkov iz %s: %A" url error

let inline makeDataLoader<'T> url =
    let fetch () = async {
        let! response_code, json = Http.get url
        return
            match response_code with
            | 200 ->
                let data = Json.tryParseNativeAs<'T> json
                match data with
                | Error err ->
                    Error (formatError url err)
                | Ok data ->
                    Ok data
            | _ ->
                Error (formatError url response_code)
    }

    Utils.memoize fetch
