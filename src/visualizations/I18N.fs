module I18N

open Fable.Core.JsInterop

type Ii18n =
    abstract t : string -> string
    abstract t : string * obj -> string

let i18n : Ii18n = importDefault "i18n"

let t key = i18n.t key

let tOptions key options = i18n.t (key, options)

let tt section key =
    t (section + "." + key)

let chartText chartTextsGroup textId =
    t ("charts." + chartTextsGroup + textId)
