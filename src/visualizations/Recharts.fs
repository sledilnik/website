module Recharts

open Feliz
open Feliz.Recharts

type Feliz.Recharts.yAxis with
    static member label value = Interop.mkAttr "label" (unbox value)
