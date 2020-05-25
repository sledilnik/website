import Vue from 'vue'
import i18next from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import VueI18Next from '@panter/vue-i18next'
import moment from 'moment'
import en from './locales/en.json'
import sl from './locales/sl.json'
import ru from './locales/ru.json'
import de from './locales/de.json'
import it from './locales/it.json'
import hr from './locales/hr.json'

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
  lookupLocalStorage: 'i18nextLng',
  lookupFromPathIndex: 0,
}

i18next.use(LanguageDetector).init({
  lng: 'sl',
  returnObjects: true,
  fallbackLng: 'sl',
  resources: {
    sl: { translation: sl },
    en: { translation: en },
    ru: { translation: ru },
    de: { translation: de },
    it: { translation: it },
    hr: { translation: hr },
  },
  detection: detectionOptions,
  interpolation: {
    format: function (value, format, lng) {
      if (value instanceof Date) {
        return moment(value).format(format)
      }
      return value
    },
  },
})

const i18n = new VueI18Next(i18next)

export default i18n
