const generator = require('../src/proxy-generator.js');
const path = require('path');

describe('generator', () => {
  describe('#generate()', () => {
    it('should successfully generate a proxy for a .NET assembly', (done) => {      
      generator.generateProxy(
        'DotNetTests.TestType1',
        path.join(__dirname, 'DotNetTests.dll'),
        path.resolve(__dirname, '../junk'),
        done);

    });
  });
});
