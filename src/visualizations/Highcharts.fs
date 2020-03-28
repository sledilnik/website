module Highcharts

open System
open Fable.Core
open Fable.React

[<Import("renderChart", from="./_highcharts")>]
let chart: obj -> ReactElement = jsNative
