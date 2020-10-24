module Main

open Feliz
open Browser.Dom

let Visualizations
        (elementId: string,
         page: string,
         query: obj,
         apiEndpoint: string,
         visualization: string option) =
    ReactDOM.render(App.app { query = query ; visualization = visualization ; page = page ; apiEndpoint = apiEndpoint }, document.getElementById elementId)
