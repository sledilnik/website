module ShareButton

open Feliz
open Browser
open Types
open I18N

let dropdown (viz: Visualization) =
    React.functionComponent (fun () ->
        let (dropdown, setDropdown) = React.useState (false)
        let (modal, setModal) = React.useState (false)
        // TODO: needs refactoring eventually because we're just duplicating code from EmbedMakerPage.vue
        let (width, height) =
            match viz.VisualizationType with
            | MetricsComparison -> (1140, 720)
            | DailyComparison -> (1140, 720)
            | Cases -> (1140, 630)
            | Patients -> (1140, 720)
            | CarePatients -> (1140, 720)
            | Ratios -> (1140, 720)
            | HCenters -> (1140, 720)
            | Tests -> (1140, 720)
            | Infections -> (1140, 720)
            | Spread -> (1140, 630)
            | Regions -> (1140, 720)
            | Regions100k -> (1140, 720)
            | HcCases -> (1140, 720)
            | Sources -> (1140, 720)
            | Map -> (1140, 820)
            | RegionMap -> (1140, 820)
            | EuropeMap -> (1140, 780)
            | WorldMap -> (1140, 780)
            | Municipalities -> (1140, 1150)
            | AgeGroups -> (1140, 720)
            | AgeGroupsTimeline -> (1140, 720)
            | Hospitals -> (1140, 1130)
            | CountriesCasesPer1M -> (1140, 740)
            | CountriesActiveCasesPer1M -> (1140, 740)
            | CountriesNewDeathsPer1M -> (1140, 740)
            | CountriesTotalDeathsPer1M -> (1140, 740)
            | PhaseDiagram -> (1140, 720)

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

        // let copy (e: MouseEvent) =
        //     let element = e.target
        //     element.select ()
        //     element.setSelectionRange (0, 99999)
        //     document.execCommand ("copy")

        Html.div
            [ prop.className "share-component-wrapper"
              prop.children
                  [ Html.div
                      [ if dropdown
                        then prop.className "share-dropdown-wrapper show"
                        else prop.className "share-dropdown-wrapper hide"
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
                                         + chartText viz.ChartTextsGroup "title"
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

                              // TODO: add export to PNG
                              // https://www.highcharts.com/docs/export-module/client-side-export

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
                          // TODO: click outside the button should close the dropdown as well
                          prop.onClick (fun _ -> setDropdown (not dropdown)) ]


                    Html.div
                        [ if modal then prop.className "embed-menu show" else prop.className "embed-menu hide"
                          prop.children
                              [ Html.h2 [ prop.text (I18N.t "embedMaker.title") ]
                                Html.p
                                    // TODO: refactor https://www.i18next.com/translation-function/interpolation
                                    // check why this doesn't work: I18N.tOptions "embedMaker.description" {| interpolation = {| escapeValue = false |} }|}
                                    [ Html.span [ prop.text (I18N.t "embedMaker.descriptionPart1") ]
                                      Html.a
                                          [ prop.href
                                              "https://github.com/sledilnik/website/blob/master/examples/README.md"
                                            prop.text (I18N.t "embedMaker.descriptionPart2") ] ]
                                Html.textarea
                                    [ prop.title (I18N.t "embedMaker.copy")
                                      prop.className "form-control"
                                      prop.defaultValue
                                          ("<iframe src=\""
                                           + embedUrl
                                           + "\" frameborder=\"0\" width=\""
                                           + width.ToString()
                                           + "\" height=\""
                                           + height.ToString()
                                           + "\"></iframe>") ]
                                // TODO: implement copy method as it is in EmbedPageMake
                                // prop.onClick (fun _ -> copy) ]
                                Html.button
                                    [ prop.text (I18N.t "charts.common.close")
                                      prop.className "btn btn-primary btn-sm"
                                      prop.onClick (fun _ -> setModal (not modal)) ] ] ] ] ])
