import Vue from "vue"
import marked from "marked"

Vue.directive("htmlMd", {
    bind(el, binding) {
        el.innerHTML = marked(binding.value || '')
    }
})
