import Vue from 'vue'
import Vuex from 'vuex'
import VueRouter from 'vue-router'
import HighchartsVue from 'highcharts-vue'
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight, faSpinner } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

// global vue filters
import './filters'

// this is common Vue initialization for index.js and embed.js (app entrypoints)

Vue.use(Vuex)
Vue.use(VueRouter)
Vue.use(BootstrapVue)
Vue.use(IconsPlugin)
Vue.use(HighchartsVue)

// fontawesome
library.add(faArrowCircleUp, faArrowCircleDown, faArrowCircleLeft, faArrowCircleRight, faSpinner)
Vue.component('font-awesome-icon', FontAwesomeIcon)

Vue.config.productionTip = false

// source: https://stackoverflow.com/questions/6215779/scroll-if-element-is-not-visible
function scrollToView(element){
    var offset = element.offset().top;
    if(!element.is(":visible")) {
        element.css({"visibility":"hidden"}).show();
        var offset = element.offset().top;
        element.css({"visibility":"", "display":""});
    }

    var visible_area_start = $(window).scrollTop();
    var visible_area_end = visible_area_start + window.innerHeight;

    if(offset < visible_area_start || offset > visible_area_end){
         // Not in view so scroll to it
         $('html,body').animate({scrollTop: offset - window.innerHeight/3}, 1000);
         return false;
    }
    return true;
}

function jumpToVisualization(visualizationId) {
    var visualizationRootEl = document.getElementById(visualizationId);
    scrollToView(visualizationRootEl);
    return true;
}