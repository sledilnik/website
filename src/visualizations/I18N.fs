module I18N

open Fable.Core
open Fable.Core.JsInterop

type Ii18n =
    abstract t : string -> string
    abstract t : string * obj -> string

let i18n : Ii18n = importDefault "i18n"

let t key = i18n.t key

let tOptions key options = i18n.t (key, options)

let tt section key =
    t (section + "." + key)

let dow dayOfWeek =
    t (sprintf "weekday.%d" dayOfWeek)

let chartText chartTextsGroup textId =
    t ("charts." + chartTextsGroup + "." + textId)


open Browser

[<Erase>]
type Number =
    | Int of int
    | Float of float

[<ImportMember("i18n")>]
let formatNumber (x : Number) : string = jsNative

// printfn "%s" (formatNumber (Int 123456))
// printfn "%s" (formatNumber (Float 123456.789))


type NumberFormat =
    abstract formatNumber : int -> string
    abstract formatNumber : float -> string

[<ImportAll("i18n")>]
let NumberFormat : NumberFormat = jsNative

// printfn "%s" (NumberFormat.formatNumber 123456)
// printfn "%s" (NumberFormat.formatNumber 123456.789)
