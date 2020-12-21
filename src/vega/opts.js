import i18n from '@/i18n'

let $t = (id) => i18n.i18next.t(id)

// https://github.com/vega/vega-embed#options
export default {
    actions: false,
    timeFormatLocale: {
        // https://github.com/d3/d3-time-format/blob/master/locale/de-DE.json
        // dateTime: "%A, der %e. %B %Y, %X",
        // date: "%d.%m.%Y",
        // time: "%H:%M:%S",
        periods: ['AM', 'PM'], //not used, but required
        days: [
            $t('weekday.0'), //"So",
            $t('weekday.1'), //"Mo",
            $t('weekday.2'), //"Di",
            $t('weekday.3'), //"Mi",
            $t('weekday.4'), //"Do",
            $t('weekday.5'), //"Fr",
            $t('weekday.6'), //"Sa"
        ],
        shortDays: [
            $t('weekdayShort.0'), //"So",
            $t('weekdayShort.1'), //"Mo",
            $t('weekdayShort.2'), //"Di",
            $t('weekdayShort.3'), //"Mi",
            $t('weekdayShort.4'), //"Do",
            $t('weekdayShort.5'), //"Fr",
            $t('weekdayShort.6'), //"Sa"
        ],
        months: [
            $t('month.0'), //"Jan",
            $t('month.1'), //"Feb",
            $t('month.2'), //"Mrz",
            $t('month.3'), //"Apr",
            $t('month.4'), //"Mai",
            $t('month.5'), //"Jun",
            $t('month.6'), //"Jul",
            $t('month.7'), //"Aug",
            $t('month.8'), //"Sep",
            $t('month.9'), //"Okt",
            $t('month.10'), //"Nov",
            $t('month.11'), //"Dez",
        ],
        shortMonths: [
            $t('shortMonth.0'), //"Jan",
            $t('shortMonth.1'), //"Feb",
            $t('shortMonth.2'), //"Mrz",
            $t('shortMonth.3'), //"Apr",
            $t('shortMonth.4'), //"Mai",
            $t('shortMonth.5'), //"Jun",
            $t('shortMonth.6'), //"Jul",
            $t('shortMonth.7'), //"Aug",
            $t('shortMonth.8'), //"Sep",
            $t('shortMonth.9'), //"Okt",
            $t('shortMonth.10'), //"Nov",
            $t('shortMonth.11'), //"Dez",
        ],
    },
    formatLocale: {
        decimal: i18n.i18next.separators.decimal,
        thousands: i18n.i18next.separators.group,
        grouping: [3],
    }, // https://github.com/d3/d3-format/blob/master/locale/en-US.json
}