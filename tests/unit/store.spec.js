import { assert } from 'chai'
import { lastChange } from 'store'

// mocha.fullTrace()
assert.includeStack = true

function testLastChange(data, expected) {
  const actual = lastChange(data, 'value')

  // console.log(actual)
  // console.log(expected)

  assert.deepEqual(actual, expected)
}

describe('vuex store helpers', function () {
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
          diff: 1
        }
      }

      testLastChange(fakedata, expected)
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
          diff: -1
        }
      }

      testLastChange(fakedata, expected)
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
          firstDate: undefined,
          value: 6,
          diff: 1,
          percentDiff: 50
        },
        dayBefore: undefined
      }

      testLastChange(fakedata, expected)
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
          firstDate: undefined,
          value: 3,
          diff: 1
        }
      }

      testLastChange(fakedata, expected)
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
          firstDate: undefined,
          value: 3,
          diff: 1,
          percentDiff: 50
        },
        dayBefore: {
          date: new Date('2020-01-02'),
          value: 2,
          diff: 1
        }
      }

      testLastChange(fakedata, expected)
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
          diff: -3
        }
      }

      testLastChange(fakedata, expected)
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
          diff: 3,
          percentDiff: 8.5
        },
        dayBefore: {
          date: new Date('2020-01-03'),
          firstDate: undefined,
          value: 35,
          diff: 12
        }
      }

      testLastChange(fakedata, expected)
    });


    it('should work with actual data sample (missing values) #2', function () {

      const fakedata = [
        { date: '2020-01-01', value: 20 },
        { date: '2020-01-02', value: 23 },
        { date: '2020-01-03', value: 33 },
        { date: '2020-01-04', value: undefined },
        { date: '2020-01-05', value: 33 },
        { date: '2020-01-06', value: undefined },
      ]
      const expected = {
        lastDay: {
          date: new Date('2020-01-05'),
          firstDate: undefined,
          value: 33,
          diff: 3,
          percentDiff: 8.5
        },
        dayBefore: {
          date: new Date('2020-01-03'),
          firstDate: undefined,
          value: 35,
          diff: 12
        }
      }

      testLastChange(fakedata, expected)
    });

  });
});
