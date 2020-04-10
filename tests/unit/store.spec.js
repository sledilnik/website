/* eslint-disable */

import { assert } from 'chai'
import { lastChange } from 'store'

describe('Array', function() {
  describe('#indexOf()', function() {
    it('should return -1 when the value is not present', function() {

      const fakedata = [
        { date: '2020-01-01', a: 1 },
        { date: '2020-01-02', a: 2 },
        { date: '2020-01-03', a: 3 },
        { date: '2020-01-04', a: 2 },
        { date: '2020-01-05', a: 3 },
        { date: '2020-01-06', a: 3 },
      ]

      let x = lastChange(fakedata, 'a')

      console.log(x)

      assert.equal([1, 2, 3].indexOf(4), -1);
    });
  });
});
