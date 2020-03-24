module Recharts

open Feliz
open Feliz.Recharts

type Feliz.Recharts.yAxis with
    static member label value = Interop.mkAttr "label" (unbox value)

type Feliz.Recharts.line with
    static member isAnimationActive (value : bool) = Interop.mkAttr "isAnimationActive" value
