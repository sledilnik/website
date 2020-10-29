module Components.Slider

open Fable.Core
open Fable.React
open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
type Props =
    | Min of int
    | Max of int
    | Value of int
    | DefaultValue of int
    | OnChange of (int -> unit)

let inline Slider (props : Props list) (children : ReactElement list) : ReactElement =
    ofImport "default" "rc-slider" (keyValueList CaseRules.LowerFirst props) children
