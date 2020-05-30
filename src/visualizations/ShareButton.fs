module ShareButton

open Feliz
open Browser
open Types

let dropdown (viz: Visualization) =
    React.functionComponent (fun () ->
        let (dropdown, setDropdown) = React.useState (false)
        let (modal, setModal) = React.useState (false)

        let graphUrl =
            "https://covid-19.sledilnik.org/"
            + localStorage.getItem ("i18nextLng")
            + "/stats%23"
            + viz.ClassName

        let embedUrl =
            "https://covid-19.sledilnik.org/"
            + "embed.html#/"
            + localStorage.getItem ("i18nextLng")
            + "/chart/"
            + viz.VisualizationType.ToString()

        Html.div
            [ prop.className "share-component-wrapper"
              prop.children
                  [ Html.div
                      [ prop.style [ if dropdown then style.display.block else style.display.none ]
                        prop.className "share-dropdown-wrapper"
                        prop.children
                            [ Html.a
                                [ prop.className "share-link"
                                  prop.target "_blank"
                                  prop.href
                                      // TODO: make facebook sharing better by creating an app here:
                                      // https://developers.facebook.com/docs/sharing/reference/share-dialog
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
                                    prop.href
                                        // https://developer.twitter.com/en/docs/twitter-for-websites/tweet-button/guides/web-intent
                                        ("https://twitter.com/intent/tweet/?"
                                         + "text="
                                         + I18N.t viz.Label
                                         + "&url="
                                         + graphUrl)
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
                                          Html.span [ prop.text (I18N.t "charts.common.webpage") ] ]
                                    prop.onClick (fun _ -> setModal (not modal)) ] ] ]

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
                          prop.onClick (fun _ -> setDropdown (not dropdown)) ]

                    Html.div
                        [ prop.className "embed-menu"
                          prop.style [ if modal then style.display.block else style.display.none ]
                          prop.children
                              [ Html.h2 [ prop.text (I18N.t "embedMaker.title") ]
                                Html.p [ prop.text (I18N.t "embedMaker.description") ]
                                Html.textarea
                                    [ prop.text
                                        ("<iframe src=\""
                                         + embedUrl
                                         + "\" frameborder=\"0\" width=\"1140\" height=\"780\"></iframe>") ]
                                Html.button
                                    [ prop.text (I18N.t "charts.common.close")
                                      prop.className "btn btn-primary btn-sm"
                                      prop.onClick (fun _ -> setModal (not modal)) ] ] ] ] ])

// TODO:
// - fix translation with links
// - implement copy method
// - put these width/height values below into iframe
//         "MetricsComparison": {
//             dimensions: [1140, 780]
//         },
//         "Cases": {
//           dimensions: [1140, 630]
//         },
//         "Patients": {
//           dimensions: [1140, 720]
//         },
//         "Ratios": {
//           dimensions: [1140, 720]
//         },
//         "HCenters": {
//           dimensions: [1140, 720]
//         },
//         "Tests": {
//           dimensions: [1140, 720]
//         },
//         "Infections": {
//           dimensions: [1140, 720]
//         },
//         "Spread": {
//           dimensions: [1140, 630]
//         },
//         "Regions": {
//           dimensions: [1140, 720]
//         },
//         "Map": {
//           dimensions: [1140, 820]
//         },
//         "Municipalities": {
//           dimensions: [1140, 1150]
//         },
//         "AgeGroups": {
//           dimensions: [1140, 720]
//         },
// //        "Hospitals": {
// //          dimensions: [1140, 1300]
// //        },
//         "Countries": {
//           dimensions: [1140, 740]
//         },
