module ShareButton

open Feliz

let dd = React.functionComponent(fun () ->
    let (dropdown, setDropdown) = React.useState(false)
    Html.div [
        Html.div  [
            prop.style [
              if dropdown then
                  style.display.block
              else 
                  style.display.none
            ]
            prop.className "share-dropdown-wrapper"
            prop.children
              [
                Html.a [
                  prop.className "share-link"
                  prop.children
                    [ Html.img
                        [ prop.className "share-icon"
                          prop.src "/images/facebook-f.svg"
                          prop.alt "Facebook"
                        ]
                      Html.span
                        [ 
                          prop.text "Facebook"
                        ]
                    ]
                ]
                Html.a [
                  prop.className "share-link"
                  prop.children
                    [ Html.img
                        [ prop.className "share-icon"
                          prop.src "/images/twitter.svg"
                          prop.alt "Twitter"
                        ]
                      Html.span
                        [ 
                          prop.text "Twitter"
                        ]
                    ]
                ]
                Html.a [
                  prop.className "share-link"
                  prop.children
                    [ Html.img
                        [ prop.className "share-icon"
                          prop.src "/images/code.svg"
                          prop.alt "Spletno stran"
                        ]
                      Html.span
                        [ 
                          prop.text "Spletno stran"
                        ]
                    ]
                ]
              ]
        ]
        Html.div [
            prop.className "share-button-wrapper"
            prop.children 
                [
                  Html.img
                      [ prop.className "share-button-icon"
                        prop.src "/images/share-icon.svg"
                        prop.alt "Share"
                      ]
                  Html.span
                      [ prop.className "share-button-caption"
                        prop.text "Deli graf na"
                      ]
                ]
            prop.onClick (fun _ -> setDropdown(not dropdown))
        ]
    ])