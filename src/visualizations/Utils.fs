
[<RequireQualifiedAccess>]
module Utils

open Feliz

open Recharts

let zeroToNone value =
    match value with
    | Some 0 -> None
    | _ -> value

let formatChartAxixDate (date : System.DateTime) =
    sprintf "%d.%d." date.Date.Day date.Date.Month

let renderScaleSelector scaleType dispatch =
    let renderSelector (scaleType : ScaleType) (currentScaleType : ScaleType) (label : string) =
        let defaultProps =
            [ prop.text label
              prop.className [
                  true, "scale-type-selector__item"
                  scaleType = currentScaleType, "selected" ] ]
        if scaleType = currentScaleType
        then Html.div defaultProps
        else Html.div ((prop.onClick (fun _ -> dispatch scaleType)) :: defaultProps)

    Html.div [
        prop.className "scale-type-selector"
        prop.children [
            Html.text "Skala na Y osi: "
            renderSelector Linear scaleType "linearna"
            renderSelector Log scaleType "logaritmična"
        ]
    ]
