module Recharts

open Feliz
open Feliz.Recharts
open Fable.Core

[<StringEnum>]
type ScaleType =
    | Auto
    | Linear
    | Pow
    | Sqrt
    | Log
    | Identity
    | Time
    | Band
    | Point
    | Ordinal
    | Quantile
    | Quantize
    | Utc
    | Sequential
    | Threshold

type Feliz.Recharts.yAxis with
    static member label value = Interop.mkAttr "label" (unbox value)
    static member scale (value : ScaleType) = Interop.mkAttr "scale" value

type Feliz.Recharts.line with
    static member isAnimationActive (value : bool) = Interop.mkAttr "isAnimationActive" value
    static member inline dataKey (f: 'a -> int option) = Interop.mkAttr "dataKey" f

type Feliz.Recharts.bar with
    static member isAnimationActive (value : bool) = Interop.mkAttr "isAnimationActive" value
    static member inline dataKey (f: 'a -> int option) = Interop.mkAttr "dataKey" f
