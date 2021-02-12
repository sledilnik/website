module Components.AutoSuggest

open Fable.Core.JsInterop
open Feliz

type AutoSuggest<'Suggestion> =
    static member inline input props = Interop.reactApi.createElement (import "default" "react-autosuggest", createObj !!props)

    static member inline inputProps (props : obj) = prop.custom ("inputProps", props)

    static member inline suggestions (suggestions : 'Suggestion array) = prop.custom ("suggestions", suggestions)

    static member inline getSuggestionValue (getValue : 'Suggestion -> string) = prop.custom ("getSuggestionValue", getValue)

    static member inline renderSuggestion (render : 'Suggestion -> ReactElement) = prop.custom ("renderSuggestion", render)

    static member inline onSuggestionsFetchRequested (getSuggestions : string -> unit) = prop.custom ("onSuggestionsFetchRequested", getSuggestions)

    static member inline onSuggestionsClearRequested (clearSuggestions : unit -> unit) = prop.custom ("onSuggestionsClearRequested", clearSuggestions)

    static member inline onSuggestionSelected (handler : System.Func<obj, {| suggestion : 'Suggestion ; suggestionValue : string ; suggestionIndex : int ; sectionIndex : int option ; method : string |}, unit>) = prop.custom ("onSuggestionSelected", handler)

    static member inline focusInputOnSuggestionClick (enabled : bool) = prop.custom ("focusInputOnSuggestionClick", enabled)
