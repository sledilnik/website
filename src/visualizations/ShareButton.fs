module ShareButton

open Feliz
open Browser

let dropdown (graphName: string) =
    React.functionComponent (fun () ->
        let (dropdown, setDropdown) = React.useState (false)

        let graphUrl =
            "https://covid-19.sledilnik.org/"
            + localStorage.getItem ("i18nextLng")
            + "/stats%23"
            + graphName

        Html.div
            [ Html.div
                [ prop.style [ if dropdown then style.display.block else style.display.none ]
                  prop.className "share-dropdown-wrapper"
                  prop.children
                      [ Html.a
                          [ prop.className "share-link"
                            prop.target "_blank"
                            prop.href
                                ("https://facebook.com/sharer/sharer.php?u="
                                 + graphUrl)
                            prop.children
                                [ Html.img
                                    [ prop.className "share-icon"
                                      prop.src "/images/facebook-f.svg"
                                      prop.alt (I18N.t "charts.common.facebook") ]
                                  Html.span [ prop.text (I18N.t "charts.common.facebook") ] ] ]
                        Html.a
                            [ prop.className "share-link"
                              prop.target "_blank"
                              prop.href ("https://twitter.com/share?url=" + graphUrl)
                              prop.children
                                  [ Html.img
                                      [ prop.className "share-icon"
                                        prop.src "/images/twitter.svg"
                                        prop.alt (I18N.t "charts.common.twitter") ]
                                    Html.span [ prop.text (I18N.t "charts.common.twitter") ] ] ]
                        Html.a
                            [ prop.className "share-link"
                              prop.children
                                  [ Html.img
                                      [ prop.className "share-icon"
                                        prop.src "/images/code.svg"
                                        prop.alt (I18N.t "charts.common.webpage") ]
                                    Html.span [ prop.text (I18N.t "charts.common.webpage") ] ] ] ] ]
              Html.div
                  [ prop.className "share-button-wrapper"
                    prop.children
                        [ Html.img
                            [ prop.className "share-button-icon"
                              prop.src "/images/share-icon.svg"
                              prop.alt (I18N.t "charts.common.share") ]
                          Html.span
                              [ prop.className "share-button-caption"
                                prop.text (I18N.t "charts.common.share") ] ]
                    prop.onClick (fun _ -> setDropdown (not dropdown)) ] ])
