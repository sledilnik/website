import { assert } from 'chai'
import i18n from '../../src/i18n'

describe('Date formatting', () => {
    it('should correctly format dates', () => {
        assert.strictEqual(i18n.i18next.format(new Date(2020, 6, 3), '%Y-%m-%d'), '2020-07-03')
        assert.strictEqual(i18n.i18next.format(new Date(2020, 6, 3), '%y-%m-%e'), '20-07- 3')

        // Weeks
        assert.strictEqual(i18n.i18next.format(new Date(2020, 5, 1), 'Week %W'), 'Week 23')
    })
})
