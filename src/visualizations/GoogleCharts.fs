module GoogleCharts

open Fable.Core
open Fable.Core.JsInterop
open Feliz

type [<StringEnum>] [<RequireQualifiedAccess>] ChartType =
    | [<CompiledName "GeoChart">] GeoChart

type [<StringEnum>] [<RequireQualifiedAccess>] Resolution =
    | Countries
    | Provinces
    | Metros

type Options =
    | Region of string
    | Resolution of Resolution
    | Legend of obj option

type Props =
    static member inline width (value : int) = Interop.mkAttr "width" value
    static member inline width (value : string) = Interop.mkAttr "width" value
    static member inline height (value : int) = Interop.mkAttr "height" value
    static member inline height (value : string) = Interop.mkAttr "height" value
    static member inline data (value : obj list) = Interop.mkAttr "data" (Array.ofList value)
    static member inline chartType (value : ChartType) = Interop.mkAttr "chartType" value
    static member inline options (value : Options list) = Interop.mkAttr "options" (keyValueList CaseRules.LowerFirst value)

let Chart (properties: IReactProperty list) : ReactElement =
    Interop.reactApi.createElement(import "Chart" "react-google-charts", createObj !!properties)
