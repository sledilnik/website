import Vue from 'vue'
import i18next from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import VueI18Next from '@panter/vue-i18next'
import en from './locales/en.json'
import sl from './locales/sl.json'

Vue.use(VueI18Next)

const detectionOptions = {
  order: [
    'path',
    'cookie',
    'navigator',
    'localStorage',
    'subdomain',
    'queryString',
    'htmlTag',
  ],
  lookupFromPathIndex: 0,
}

i18next.use(LanguageDetector).init({
  lng: 'sl',
  fallbackLng: 'sl',
  resources: {
    sl: { translation: sl },
    en: { translation: en },
  },
  detection: detectionOptions,
  debug: true,
})

const i18n = new VueI18Next(i18next)

export default i18n
