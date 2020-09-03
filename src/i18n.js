import Vue from 'vue'
import i18next from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import VueI18Next from '@panter/vue-i18next'
import moment from 'moment'
import en from './locales/en.json'
import sl from './locales/sl.json'
import hr from './locales/hr.json'
import de from './locales/de.json'
import it from './locales/it.json'

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
  lng: process.env.VUE_APP_DEFAULT_LANGUAGE,
  fallbackLng: ['en', 'sl', 'hr', 'de', 'it'],
  returnObjects: true,
  resources: {
    sl: { translation: sl },
    en: { translation: en },
    hr: { translation: hr },
    de: { translation: de },
    it: { translation: it },
  },
  detection: detectionOptions,
  interpolation: {
    format: function(value, format, lng) {
      if (value instanceof Date) {
        return moment(value).format(format)
      }
      return value
    },
  },
})

i18next.services.pluralResolver.addRule(
  // override plural rule from
  // https://github.com/i18next/i18next/blob/270904f6369ee9bbda059c3186fcea7baf9eb15d/src/PluralResolver.js#L62
  // to match the one in weblate
  'sl',
  {
    numbers: [0, 1, 2, 3],
    plurals: function plurals(n) {
      return Number(
        n % 100 == 1 ? 0 : n % 100 == 2 ? 1 : n % 100 == 3 || n % 100 == 4 ? 2 : 3
      )
    },
  }
)

moment.locale(process.env.VUE_APP_DEFAULT_LANGUAGE)

moment.updateLocale('hr', {
  months: [
    'siječnja',
    'veljače',
    'ožujka',
    'travnja',
    'svibnja',
    'lipnja',
    'srpnja',
    'kolovoza',
    'rujna',
    'listopada',
    'studenoga',
    'prosinca',
  ],
})

localStorage.setItem('contextCountry', process.env.VUE_APP_LOCALE_CONTEXT)

const i18n = new VueI18Next(i18next)

export default i18n
