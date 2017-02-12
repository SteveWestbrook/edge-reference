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
		public const string EdgeReferenceTypeName = "EdgeReference";

		public const string EdgeReferenceModuleName = "edge-reference";

		private const int DefaultIndentWidth = 2;

		private static readonly string MemberSeparator = Environment.NewLine + Environment.NewLine;

		private static ConcurrentDictionary<string, string> generatedProxies = new ConcurrentDictionary<string, string>();

		private Dictionary<string, List<MemberInfo>> generatedMembers;

		private Action<string, string> classGenerated;

		private StringBuilder buffer;

		private int currentIndentWidth;

		private string incrementalIndent;

		/// <summary>
		/// The name of the JavaScript class.
		/// </summary>
		private string javaScriptClassName;

		/// <summary>
		/// The source type's full name with &quot.&quot; replaced with 
		/// &quot-&quot;.  Intended for use as a file name.
		/// </summary>
		private string javaScriptFullName;

		protected ProxyGenerator ()
		{
			this.IndentWidth = DefaultIndentWidth;
		}

		public int IndentWidth 
		{
			get 
			{
				return this.incrementalIndent.Length;
			}

			set
			{
				this.incrementalIndent = new string (' ', value);
			}
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
			this.buffer = new StringBuilder ();

			GenerateRequires (target);

			PropertyInfo[] staticProperties = RetrieveProperties(target, BindingFlags.Static | BindingFlags.Public);
			PropertyInfo[] instanceProperties = RetrieveProperties(target, BindingFlags.Instance | BindingFlags.Public);

			MethodInfo[] staticMethods = this.RetrieveMethods (target, BindingFlags.Static | BindingFlags.Public);
			MethodInfo[] instanceMethods = this.RetrieveMethods(target, BindingFlags.Instance | BindingFlags.Public);

			this.OnClassGenerated (target, this.buffer.ToString ());

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

		#region JavaScript Generation

		#region Class-level

		/// <summary>
		/// Generates JavaScript require statements for the file.
		/// </summary>
		/// <param name="target">Target.</param>
		private void GenerateRequires(Type target) 
		{
			AppendReference (EdgeReferenceTypeName, EdgeReferenceModuleName);

			if (target.BaseType != null) {
				AppendReference(
					target.Name,
					target.FullName.Replace ('.', '-'));
			}

		}

		private void AppendReference(string name, string file)
		{
			buffer.AppendFormat (
				CultureInfo.InvariantCulture,
				"const {0} = require('{1}');",
				name,
				file);

			buffer.AppendLine ();
		}

		private void GenerateClassDefinition(Type target) 
		{
			string name = target.Name;
			const string ClassDefinitionTemplate = @"{0}class {1}{2} \{";

			string extendsStatement = string.Empty;

			if (target.BaseType != typeof(object)) {
				extendsStatement = string.Concat (
					" extends", 
					target.BaseType.Name);
			}

			this.currentIndentWidth += this.IndentWidth;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Generates and appends a JavaScript function to the ProxyGenerator's internal buffer.
		/// </summary>
		/// <param name="source">Information about the method to be generated.</param>
		/// <param name="isStatic">If set to <c>true</c>, the member is static.</param>
		private void GenerateFunction(MethodInfo source, bool isStatic)
		{
			const string MethodTemplate = @"{0}{1}{2}() \{
{0}{3}return {4}.{5}('{2}');
{0}\}";
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
{0}{3}return {4}.Get{5}('{2}');
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
{0}{3}{4}.Set{5}('{2}', value);
{0}\}";

			string baseIndent = new string (' ', this.currentIndentWidth);
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


