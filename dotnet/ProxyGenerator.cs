using System;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

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
			generator.Generate (type);
		}

		private void Generate(Type target)
		{
			// TODO: file name should be the full name, with '.' replaced with '-'.
			this.javaScriptClassName = target.Name;
			this.javaScriptFullName = target.FullName.Replace ('.', '-');
      this.emitter = new JavaScriptEmitter();

			PropertyInfo[] staticProperties = RetrieveProperties(target, BindingFlags.Static | BindingFlags.Public);

			PropertyInfo[] instanceProperties = RetrieveProperties(target, BindingFlags.Instance | BindingFlags.Public);

			MethodInfo[] staticMethods = this.RetrieveMethods (target, BindingFlags.Static | BindingFlags.Public);

			MethodInfo[] instanceMethods = this.RetrieveMethods(target, BindingFlags.Instance | BindingFlags.Public);

			this.emitter.AppendBasicRequires(target);

      // Get all non-value, non-string property types.
      this.emitter.AppendRequires(
        staticProperties.Concat(instanceProperties)
        .Distinct()
        .Where(info => IsReferenceType(info))
        .Select(info => info.PropertyType));

      // Get all non-value, non-string types used by methods.
      IEnumerable<Type> requireMethods = 
        staticMethods.Concat(instanceMethods)
        .Distinct()
        .SelectMany((info) => {
          IEnumerable<Type> result = info.GetParameters()

          .Where((param) => {
            return IsReferenceType(param.ParameterType);
          })
          .Select((param) => {
            return param.ParameterType;
          });

          if (IsReferenceType(info.ReturnType)) {
            result = result.Concat(new Type[] { info.ReturnType });
          }

          return result;
        });

      this.emitter.AppendRequires(requireMethods);

      // TODO: Reference component

			this.emitter.AppendClassDefinition(target);

      // TODO: Constructors - call super, pass params

			staticProperties.ForEach((info) => { 
				this.emitter.AppendProperty(info, true);
			});

			instanceProperties.ForEach((info) => {
				this.emitter.AppendProperty(info, false);
			});

			staticMethods.ForEach((info) => {
				this.emitter.AppendFunction(info, true);
			});

			instanceMethods.ForEach((info) => {
				this.emitter.AppendFunction(info, false);
			});

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

//// OLD BELOW HERE

		#region JavaScript Generation

		#region Class-level

		#endregion

		#region Methods

		/// <summary>
		/// Generates and appends a JavaScript function to the ProxyGenerator's internal buffer.
		/// </summary>
		/// <param name="source">Information about the method to be generated.</param>
		/// <param name="isStatic">If set to <c>true</c>, the member is static.</param>
		private void GenerateFunction(MethodInfo source, bool isStatic)
		{
			/**
			 * 0 - Current indent
			 * 1 - static keyword if needed
			 * 2 - name
			 * 3 - argument list
			 * 4 - additional indent
			 * 5 - self reference
			 * 6 - 
			 */
			const string MethodTemplate = @"{0}{1}{2}({3}) \{
{0}{4}return {5}.call{6}('{2}', {3});
{0}\}";

			string staticModifier = isStatic ? "static " : string.Empty;
			string functionName = source.Name;

			buffer.AppendFormat(
				CultureInfo.InvariantCulture,
				MethodTemplate,
				CurrentIndent,
				staticModifier,
				functionName

// TODO
		}

		#endregion

		#region Properties

		/// <summary>
		/// Generates and appends a JavaScript property to the ProxyGenerator's internal buffer.
		/// </summary>
		/// <param name="source">
		/// Information about the property to be generated.
		/// </param>
		/// <param name="isStatic">
		/// If set to <c>true</c>, the member is static.
		/// </param>
		/// <remarks>
		/// The property info provided could be used to determine whether the member 
		/// is static; however a parameter is more convenient.
		/// </remarks>
		private void GenerateProperty(PropertyInfo source, bool isStatic)
		{
			/**
			 * 0 - indent
			 * 1 - static
			 * 2 - name
			 * 3 - additional indent
			 * 4 - self reference (this or the class name, if static)
			 * 5 - type modifier (''/String/UserDefinedType)
			 */
			const string GetterTemplate = @"{0}{1}get {2}() \{
{0}{3}return {4}.get{5}('{2}');
{0}\}";

			/**
			 * 0 - indent
			 * 1 - static
			 * 2 - name
			 * 3 - additional indent
			 * 4 - self reference (this or the class name, if static)
			 * 5 - type modifier (''/String/UserDefinedType)
			 */
			const string SetterTemplate = @"{0}{1}set {2}(value) \{
{0}{3}{4}.set{5}('{2}', value);
{0}\}";

			string baseIndent = this.CurrentIndent;
			string staticModifier = isStatic ? "static " : string.Empty;
			string selfReference = isStatic ? this.javaScriptClassName : "this";
			string typeModifier = DetermineTypeModifier (source.PropertyType);

			Action<string> formatAccessor = (formatString) => {
				this.buffer.AppendFormat(
					CultureInfo.InvariantCulture,
					formatString,
					baseIndent,
					staticModifier,
					source.Name,
					this.incrementalIndent,
					selfReference,
					typeModifier);
			};

			// used twice
			bool canWrite = source.CanWrite && source.GetSetMethod ().IsPublic;

			// Note that public properties are defined as properties with a 
			// public getter OR setter - therefore make sure the accessor is 
			// public.
			if (source.CanRead && source.GetGetMethod().IsPublic) {
				formatAccessor (GetterTemplate);

				if (canWrite) {
					buffer.AppendFormat(MemberSeparator);
				}
			}

			if (canWrite) {
				formatAccessor (SetterTemplate);
			}
		}

		private string DetermineTypeModifier(Type type)
		{
			const string StringTypeModifier = "String";
			const string UserDefinedTypeModifier = "UserDefined";

			// TODO: check String vs string
			if (type == typeof(string)) {
				return StringTypeModifier;
			} else if (type.IsValueType) {
				return string.Empty;
			} else {
				return UserDefinedTypeModifier;
			}
		}

		#endregion

		#endregion
	}
}


