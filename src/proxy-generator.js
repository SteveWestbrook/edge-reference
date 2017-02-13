const edge = require('edge');
const fs = require('fs');
const path = require('path');

const proxyGenerator = edge.func(function() {
  /*

    // using EdgeReference;
    // using System.Threading.Tasks;

    public class Startup() {
      public async Task<object> Invoke(object input) {
        Console.WriteLine("huh");
      }
    }
  */
});

        // return ProxyGenerator.Generate(
          // input.typeFullName,
          // input.assemblyLocation,
          // (Action<string, string>)input.callback
        // );

module.exports = {
  generate: generateProxy
};

function generateProxy(typeName, assemblyPath, targetDirectory, callback) {
  var parameters = {
    typeFullName: typeName,
    assemblyLocation: assemblyPath,
    callback: (fullName, generatedScript) => {

      console.log('Generated proxy for %s.', fullName);

      var writePath = path.join(targetDirectory, fullName.replace('.', '-'));
      var stream = fs.createWriteStream(writePath);

      stream.on('open', () => {
        stream.write(generatedScript, 'utf8', () => {
          stream.end();
          console.log('Finished writing to %s.', writePath);
        });
      });

      stream.on('error', (err) => {
        console.error('Failed to write to %s:', writePath);
        console.error(err);
      });
    }
  }

  proxyGenerator(parameters, (err) => {
    if (err) {
      console.error(err);
    } else {
      console.log('Proxy generation complete for %s.', typeName);
    }

    if (callback) {
      callback(err);
    }
  });
}
