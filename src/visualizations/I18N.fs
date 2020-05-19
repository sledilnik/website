module I18N

open Fable.Core.JsInterop

type Ii18n =
    abstract t : string -> string

let i18n : Ii18n = importDefault "i18n"

let t key = i18n.t key
