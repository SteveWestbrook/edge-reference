const generator = require('../src/proxy-generator.js');
const path = require('path');

describe('generator', () => {
  describe('#generate()', () => {
    it('should successfully generate a proxy for a .NET assembly', (done) => {      
      generator.generate(
        'DotNetTest.TestType1',
        path.join(__dirname, 'DotNetTest.dll'),
        path.resolve(__dirname, '../junk'),
        done);

    });
  });
});
