using System;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Core;

namespace EdgeReference
{
  public class ProxyGenerator
  {
    private static ConcurrentDictionary<string, string> generatedProxies = new ConcurrentDictionary<string, string>();

    private Dictionary<string, List<MemberInfo>> generatedMembers;

    private Action<string, string> classGenerated;

    private JavaScriptEmitter emitter;

    protected ProxyGenerator ()
    {
    }

    /// <summary>
    /// Generates a contract.
    /// </summary>
    /// <param name="typeNameWithNamespace">Type name with namespace.</param>
    /// <param name="assemblyPath">Assembly path.</param>
    public static void Generate(
      string typeNameWithNamespace,
      string assemblyPath,
      Action<string, string> classGeneratedCallback)
    {

      ProxyGenerator generator = new ProxyGenerator ();
      Assembly owningAssembly = Assembly.ReflectionOnlyLoadFrom (assemblyPath);
      Type type = owningAssembly.GetType (typeNameWithNamespace);

      generator.classGenerated = classGeneratedCallback;
      generator.Generate(type);
    }

    private void Generate(Type target)
    {
      // TODO: file name should be the full name, with '.' replaced with '-'.
      // this.javaScriptClassName = target.Name;
      // this.javaScriptFullName = target.FullName.Replace ('.', '-');
      this.emitter = new JavaScriptEmitter();

      PropertyInfo[] staticProperties = RetrieveProperties(target, BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

			PropertyInfo[] instanceProperties = RetrieveProperties(target, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

			MethodInfo[] staticMethods = this.RetrieveMethods (target, BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

			MethodInfo[] instanceMethods = this.RetrieveMethods(target, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

      this.emitter.AppendBasicRequires(target);

      // Get all non-value, non-string property types.
      this.emitter.AppendRequires(
        staticProperties.Concat(instanceProperties)
        .Distinct()
        .Where(info => ReflectionUtils.IsReferenceType(info.PropertyType))
        .Select(info => info.PropertyType));

      // Get all non-value, non-string types used by methods.
      IEnumerable<Type> requireMethods = 
        staticMethods.Concat(instanceMethods)
        .Distinct()
        .SelectMany((info) => {
          IEnumerable<Type> result = info.GetParameters()

          .Where((param) => {
            return ReflectionUtils.IsReferenceType(param.ParameterType);
          })
          .Select((param) => {
            return param.ParameterType;
          });

            if (ReflectionUtils.IsReferenceType(info.ReturnType)) {
            result = result.Concat(new Type[] { info.ReturnType });
          }

          return result;
        });

      this.emitter.AppendRequires(requireMethods);

      // TODO: Reference component

      this.emitter.AppendClassDefinition(target);

      // TODO: Constructors - call super, pass params	
      foreach (PropertyInfo info in staticProperties) { 
        this.emitter.AppendProperty(info, true);
      };

      foreach (PropertyInfo info in instanceProperties) {
        this.emitter.AppendProperty(info, false);
      };
									
      foreach (MethodInfo info in staticMethods) {
        this.emitter.AppendFunction(info, true);
      };
									
      foreach (MethodInfo info in instanceMethods) {
        this.emitter.AppendFunction(info, false);
      };

      this.emitter.AppendClassTermination();
      this.OnClassGenerated (target, this.emitter.ToString ());

      // TODO: After generation, look at base classes
    }

    private PropertyInfo[] RetrieveProperties(Type target, BindingFlags flags)
    {
      PropertyInfo[] result = target.GetProperties (flags);

      // Alphabetical order
      result = result.OrderBy((member) => member.Name).ToArray();
      return result;
    }

    private MethodInfo[] RetrieveMethods(Type target, BindingFlags flags)
    {
      MethodInfo[] result = target.GetMethods(flags);

      // Alphabetical order
      result = result.OrderBy((method) => method.Name).ToArray();
      return result;
    }

    private void OnClassGenerated(Type classType, string generatedJavaScript)
    {
      string name = classType.FullName;

      if (
        generatedProxies.TryAdd(name, generatedJavaScript) 
          && this.classGenerated != null) {

        this.classGenerated (name, generatedJavaScript);
      }
    }
  }
}


