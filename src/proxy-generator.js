const edge = require('edge');
const fs = require('fs');
const path = require('path');

const proxyGenerator = edge.func(function() {
  /*
    #r "./dotnet/bin/Debug/EdgeReference.dll"

    using System;
    using System.Threading.Tasks;
    using EdgeReference;

    public class Startup {
      private class Result
      {
        public string name;
        public string script;
      }

      public async Task<dynamic> Invoke(dynamic input) {
        Action<string, string> completion =
          new Action<string, string>((name, script) => {
            Result output = new Result();
            output.name = name;
            output.script = script;

            input.callback(output);
          });

        Console.WriteLine("1");

        ProxyGenerator.Generate(
          input.typeFullName,
          input.assemblyLocation,
          completion);
        Console.WriteLine("2");

        return null;
      }
    }
  */
});

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
