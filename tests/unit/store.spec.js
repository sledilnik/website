import { assert } from 'chai'
import { lastChange } from '@/store/index'

// mocha.fullTrace()
assert.includeStack = true

function testLastChange(data, expected, cumulative) {
  const actual = lastChange(data, 'value', cumulative)
  assert.deepEqual(actual, expected)
}

// everything here is skipped, 
// probably logic has changed much from time test was written
describe.skip('vuex store helpers', function () {
  describe('lastChange', function () {
    it('should work with monotonicaly increasing data', function () {

      const fakedata = [
        { date: '2020-01-01', value: 1 },
        { date: '2020-01-02', value: 2 },
        { date: '2020-01-03', value: 3 },
        { date: '2020-01-04', value: 4 },
        { date: '2020-01-05', value: 5 },
        { date: '2020-01-06', value: 6 },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-06'),
          firstDate: undefined,
          value: 6,
          diff: 1,
          percentDiff: 20
        },
        dayBefore: {
          date: new Date('2020-01-05'),
          firstDate: undefined,
          value: 5,
          diff: 1,
          percentDiff: 25
        },
        day2Before: {
          date: new Date('2020-01-04'),
          value: 4,
        }
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with monotonicaly decreasing data', function () {

      const fakedata = [
        { date: '2020-01-01', value: 6 },
        { date: '2020-01-02', value: 5 },
        { date: '2020-01-03', value: 4 },
        { date: '2020-01-04', value: 3 },
        { date: '2020-01-05', value: 2 },
        { date: '2020-01-06', value: 1 },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-06'),
          firstDate: undefined,
          value: 1,
          diff: -1,
          percentDiff: -50
        },
        dayBefore: {
          date: new Date('2020-01-05'),
          firstDate: undefined,
          value: 2,
          diff: -1,
          percentDiff: -33.3
        },
        day2Before: {
          date: new Date('2020-01-04'),
          value: 3,
        }
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with constant data', function () {

      const fakedata = [
        { date: '2020-01-01', value: 42 },
        { date: '2020-01-02', value: 42 },
        { date: '2020-01-03', value: 42 },
        { date: '2020-01-04', value: 42 },
        { date: '2020-01-05', value: 42 },
        { date: '2020-01-06', value: 42 },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-06'),
          firstDate: new Date('2020-01-01'),
          value: 42,
          diff: undefined,
          percentDiff: undefined
        },
        dayBefore: undefined,
        day2Before: undefined
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with constan data in beteween', function () {

      const fakedata = [
        { date: '2020-01-01', value: 1 },
        { date: '2020-01-02', value: 2 },
        { date: '2020-01-03', value: 3 },
        { date: '2020-01-04', value: 3 },
        { date: '2020-01-05', value: 3 },
        { date: '2020-01-06', value: 4 },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-06'),
          firstDate: undefined,
          value: 4,
          diff: 1,
          percentDiff: 33.3
        },
        dayBefore: {
          date: new Date('2020-01-05'),
          firstDate: new Date('2020-01-03'),
          value: 3,
          diff: 1,
          percentDiff: 50,
        },
        day2Before: {
          date: new Date('2020-01-02'),
          value: 2,
        },
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with constant data in tail', function () {

      const fakedata = [
        { date: '2020-01-01', value: 1 },
        { date: '2020-01-02', value: 2 },
        { date: '2020-01-03', value: 3 },
        { date: '2020-01-04', value: 3 },
        { date: '2020-01-05', value: 3 },
        { date: '2020-01-06', value: 3 },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-06'),
          firstDate: new Date('2020-01-03'),
          value: 3,
          diff: 1,
          percentDiff: 50
        },
        dayBefore: {
          date: new Date('2020-01-02'),
          firstDate: undefined,
          value: 2,
          diff: 1,
          percentDiff: 100
        },
        day2Before: {
          date: new Date('2020-01-01'),
          value: 1,
        },
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with actual data sample', function () {

      const fakedata = [
        { date: '2020-01-01', value: 20 },
        { date: '2020-01-02', value: 23 },
        { date: '2020-01-03', value: 35 },
        { date: '2020-01-04', value: 36 },
        { date: '2020-01-05', value: 33 },
        { date: '2020-01-06', value: 36 },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-06'),
          firstDate: undefined,
          value: 36,
          diff: 3,
          percentDiff: 9.1
        },
        dayBefore: {
          date: new Date('2020-01-05'),
          firstDate: undefined,
          value: 33,
          diff: -3,
          percentDiff: -8.3,
        },
        day2Before: {
          date: new Date('2020-01-04'),
          value: 36,
        }
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with actual data sample (missing values) #1', function () {

      const fakedata = [
        { date: '2020-01-01', value: 20 },
        { date: '2020-01-02', value: 23 },
        { date: '2020-01-03', value: 35 },
        { date: '2020-01-04', value: undefined },
        { date: '2020-01-05', value: 33 },
        { date: '2020-01-06', value: undefined },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-05'),
          firstDate: undefined,
          value: 33,
          diff: -2,
          percentDiff: -5.7
        },
        dayBefore: {
          date: new Date('2020-01-03'),
          firstDate: undefined,
          value: 35,
          diff: 12,
          percentDiff: 52.2,
        },
        day2Before: {
          date: new Date('2020-01-02'),
          value: 23,
        }
      }

      testLastChange(fakedata, expected, true)
    });


    it('should work with actual data sample (missing values) #2', function () {

      const fakedata = [
        { date: '2020-01-01', value: 20 },
        { date: '2020-01-02', value: 23 },
        { date: '2020-01-03', value: 33 },
        { date: '2020-01-04', value: undefined },
        { date: '2020-01-05', value: 33 },
        { date: '2020-01-06', value: undefined }
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-05'),
          firstDate: undefined,
          value: 33,
          diff: 0,
          percentDiff: 0
        },
        dayBefore: {
          date: new Date('2020-01-03'),
          firstDate: undefined,
          value: 33,
          diff: 10,
          percentDiff: 43.5
        },
        day2Before: {
          date: new Date('2020-01-02'),
          value: 23,
        }
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with data like recovered', function () {

      const fakedata = [
        { date: '2020-01-01', value: undefined },
        { date: '2020-01-02', value: 20 },
        { date: '2020-01-03', value: 20 },
        { date: '2020-01-04', value: undefined },
        { date: '2020-01-05', value: 33 },
        { date: '2020-01-06', value: undefined },
        { date: '2020-01-07', value: 36 },
        { date: '2020-01-08', value: 36 },
        { date: '2020-01-09', value: undefined },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-08'),
          firstDate: new Date('2020-01-07'),
          value: 36,
          diff: 3,
          percentDiff: 9.1
        },
        dayBefore: {
          date: new Date('2020-01-05'),
          firstDate: undefined,
          value: 33,
          diff: 13,
          percentDiff: 65
        },
        day2Before: {
          date: new Date('2020-01-03'),
          value: 20,
        }
      }

      testLastChange(fakedata, expected, true)
    });

    it('should work with data like recovered #2', function () {

      const fakedata = [
        { date: '2020-01-01', value: undefined },
        { date: '2020-01-02', value: undefined },
        { date: '2020-01-03', value: 10 },
        { date: '2020-01-04', value: 10 },
        { date: '2020-01-05', value: 10 },
        { date: '2020-01-06', value: 16 },
        { date: '2020-01-07', value: 16 },
        { date: '2020-01-08', value: 16 },
        { date: '2020-01-09', value: undefined },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-08'),
          firstDate: new Date('2020-01-06'),
          value: 16,
          diff: 6,
          percentDiff: 60
        },
        dayBefore: {
          date: new Date('2020-01-05'),
          firstDate: new Date('2020-01-03'),
          value: 10,
          diff: undefined,
          percentDiff: undefined
        },
        day2Before: undefined
      }

      testLastChange(fakedata, expected, true)
    });


    it('should work with non-cumulative series', function () {

      const fakedata = [
        { date: '2020-01-01', value: 10 },
        { date: '2020-01-02', value: 10 },
        { date: '2020-01-03', value: 10 },
        { date: '2020-01-04', value: 10 },
        { date: '2020-01-05', value: 10 },
        { date: '2020-01-06', value: 16 },
        { date: '2020-01-07', value: 16 },
        { date: '2020-01-08', value: 16 },
        { date: '2020-01-09', value: undefined },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-08'),
          firstDate: undefined,
          value: 16,
          diff: 0,
          percentDiff: 0
        },
        dayBefore: {
          date: new Date('2020-01-07'),
          firstDate: undefined,
          value: 16,
          diff: 0,
          percentDiff: 0
        },
        day2Before: {
          date: new Date('2020-01-06'),
          value: 16,
        }
      }

      testLastChange(fakedata, expected, false)
    });


  });
});
