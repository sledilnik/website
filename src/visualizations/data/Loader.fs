module DataLoader

open Fable.Core
open Fable.SimpleHttp
open Fable.SimpleJson

type DownloadedData<'T> = {
    ETag: string
    Data: 'T
}

let inline downloadData<'T>
    (url: string)
    (method: HttpMethod)
    : Async<Result<DownloadedData<'T>, string>> =
    async {
        let! response =
            Http.request url |> Http.method method |> Http.send

        let code = response.statusCode

        return
            match code with
            | 200 ->
                let json = response.responseText

                let etag =
                    if response.responseHeaders.ContainsKey "ETag" then
                        response.responseHeaders.["ETag"]
                    else
                        let xxx = sprintf "%s=%A" url response.responseHeaders
                        JS.console.log xxx
                        ""
                let parsedData = json |> Json.parseNativeAs<'T>
                Ok { Data = parsedData; ETag = etag }
            | _ ->
                Error (sprintf "got http %d while fetching %s" code url)
    }

let inline makeDataLoader<'T>(url) =
    let mutable cached = None

    let getOrFetch () = async {
        match cached with
        | Some x -> return x
        | None ->
            let! result = downloadData<'T> url GET
            cached <- Some result
            return result
    }

    getOrFetch
