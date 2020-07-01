module I18N

open Fable.Core.JsInterop

type Ii18n =
    abstract t : string -> string
    abstract t : string * obj -> string

let i18n : Ii18n = importDefault "i18n"

let t key = i18n.t (key, {| context = "SVN" |}) // default context, TODO: get it from .env: process.env.VUE_APP_LOCALE_CONTEXT

let tOptions key options = i18n.t (key, options)

let tt section key =
    t (section + "." + key)
