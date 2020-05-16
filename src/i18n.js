import Vue from 'vue'
import i18next from 'i18next'
import VueI18Next from '@panter/vue-i18next'
import en from './locales/en.json'
import sl from './locales/sl.json'

// TODO: import LanguageDetector from 'i18next-browser-languagedetector'

Vue.use(VueI18Next)

i18next.init({
  lng: 'sl',
  fallbackLng: 'sl',
  resources: {
    sl: { translation: sl },
    en: { translation: en },
  },
})

const i18n = new VueI18Next(i18next)

export default i18n
