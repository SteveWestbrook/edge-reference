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
		public const string EdgeTypeName = "Edge";

		public const string EdgeModuleName = "edge";

		public const string EdgeReferenceTypeName = "EdgeReference";

		public const string EdgeReferenceModuleName = "edge-reference";

		private const int DefaultIndentWidth = 2;

    /**
     * 0 - indent
     * 1 - static
     * 2 - name
     * 3 - additional indent
     * 4 - property body
     */
    private const string GetterTemplate = @"{0}{1}get {2}() \{
{3}
{0}\}";

    /**
     * 0 - indent
     * 1 - static
     * 2 - name
     * 3 - additional indent
     * 4 - property body
     */
    private const string SetterTemplate = @"{0}{1}set {2}(value) \{
{3}
{0}\}";


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

		private string CurrentIndent 
		{ 
			get 
			{
				return new string(' ', this.currentIndentWidth);
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

			PropertyInfo[] staticProperties = RetrieveProperties(target, BindingFlags.Static | BindingFlags.Public);

			PropertyInfo[] instanceProperties = RetrieveProperties(target, BindingFlags.Instance | BindingFlags.Public);

			MethodInfo[] staticMethods = this.RetrieveMethods (target, BindingFlags.Static | BindingFlags.Public);

			MethodInfo[] instanceMethods = this.RetrieveMethods(target, BindingFlags.Instance | BindingFlags.Public);

			this.AppendBasicRequires(target);

      // Get all non-value, non-string property types.
      this.AppendRequires(
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

      this.AppendRequires(requireMethods);

      // TODO: Reference component

			this.AppendClassDefinition(target);

      // TODO: Constructors - call super, pass params

			staticProperties.ForEach ((info) => { 
				this.AppendProperty(info, true);
			});

			instanceProperties.ForEach ((info) => {
				this.AppendProperty(info, false);
			});

			staticMethods.ForEach ((info) => {
				this.AppendFunction (info, true);
			});

			instanceMethods.ForEach ((info) => {
				this.AppendFunction (info, false);
			});

			this.GenerateClassTermination ();
			this.OnClassGenerated (target, this.buffer.ToString ());

			// TODO: After generation, look at base classes
		}

    private static bool IsReferenceType(Type type) {
      return !type.IsValueType && type != typeof(string);
    }

		/// <summary>
		/// Generates JavaScript require statements for the file.
		/// </summary>
		/// <param name="target">Target.</param>
		private void AppendBasicRequires(Type target) 
		{
			AppendRequire(EdgeTypeName, EdgeModuleName);

			if (target.BaseType != null) {
				AppendRequire(target.BaseType);
			}

			this.buffer.AppendLine ();
		}

    private void AppendRequires(IEnumerable<Type> referenceTypes) {
      referenceTypes.ForEach((type) => {
        this.AppendRequire(type);        
      });
    }

		private void AppendRequire(string name, string file)
		{
			buffer.AppendFormat (
				CultureInfo.InvariantCulture,
				"const {0} = require('{1}');",
				name,
				file);

			buffer.AppendLine ();
		}

    private void AppendRequire(Type type) {
      string name = type.Name;

      // TODO: look up name in collection of existing names.
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "const {0} = require('{1}');",
        name,
        ConvertFullName(type.FullName)));

      buffer.AppendLine();
    }

    private static string ConvertFullName(string fullName) {
      return type.FullName.replace('.', '-');
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

		private void AppendClassDefinition(Type target) 
		{
			string name = target.Name;
			const string ClassDefinitionTemplate = @"{0}class {1}{2} \{";

			string extendsStatement = string.Empty;

      // If this class inherits from something, so should the proxy.
      // TODO: Look up type names here
      string baseClass =
        (target.BaseType != typeof(object)) 
        ? target.BaseType.Name
        : EdgeReferenceTypeName;

      extendsStatement = string.Concat (" extends", baseClass);

			this.buffer.AppendFormat(
				CultureInfo.InvariantCulture,
				ClassDefinitionTemplate,
				this.CurrentIndent, 
				name,
				extendsStatement);

      this.buffer.AppendLine();
      this.buffer.AppendLine();

			// Add indent for future declarations
			this.currentIndentWidth += this.IndentWidth;
		}

		private void GenerateClassTermination()
		{
			// Outdent
			this.currentIndentWidth -= this.IndentWidth;

			buffer.Append(this.CurrentIndent);
			buffer.AppendLine("}");
		}

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
		private void AppendProperty(PropertyInfo source, bool isStatic)
		{
			string baseIndent = this.CurrentIndent;
			string staticModifier = isStatic ? "static " : string.Empty;
			string selfReference = isStatic ? this.javaScriptClassName : "this";
			string typeModifier = DetermineTypeModifier (source.PropertyType);

      string getterBody = GenerateGetterBody(
        source.Name,
        source.PropertyType,
        isStatic);

      string setterBody = GenerateSetterBody()
        source.Name,
        source.PropertyType,
        isStatic);

			Action<string> formatAccessor = (formatString, body) => {
				this.buffer.AppendFormat(
					CultureInfo.InvariantCulture,
					formatString,
					baseIndent,
					staticModifier,
					source.Name,
					body);
			};

			// used twice
			bool canWrite = source.CanWrite && source.GetSetMethod().IsPublic;

			// Note that public properties are defined as properties with a 
			// public getter OR setter - therefore make sure the accessor is 
			// public.
			if (source.CanRead && source.GetGetMethod().IsPublic) {
				formatAccessor(GetterTemplate, getterBody);

				if (canWrite) {
					buffer.AppendFormat(MemberSeparator);
				}
			}

			if (canWrite) {
				formatAccessor(SetterTemplate, setterBody);
			}
		}

    private string GenerateGetterBody(string name, Type type, bool isStatic) {
      string result;

      if (this.IsReferenceType(type)) {
        result = string.Format(
          CultureInfo.InvariantCulture,
          @"{0}{1}var returnId = Reference.{2}({3});
{0}{1}return new {4}(returnId);",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "this._referenceId",
          this.DetermineJavaScriptTypeName(type));
      } else {
        result = string.Format(
          CultureInfo.InvariantCulture,
          "return Reference.{0}({1});",
          name,
          isStatic ? string.Empty : "this._referenceId, ");
      }

      return result;
    }

    private string GenerateSetterBody(string name, Type type, bool isStatic) {
      string result;

      if (this.IsReferenceType(type)) {
        result = string.format(
          CultureInfo.InvariantCulture,
          "{0}{1}Reference.{2}({3}value._edgeId));",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "this._referenceId, ");
      } else {
        result = string.format(
          CultureInfo.InvariantCulture,
          "{0}{1}Reference.{2}({3}value));",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "this._referenceId, ");
      }

      return result;
    }

    private void AppendFunction(MethodInfo source, bool isStatic) {
      // Indent
			this.currentIndentWidth += this.IndentWidth;

      // TODO:
      // Append argument conversions
      // Generate argument references

      // Append call line
      const string FunctionCallLineTemplate =
        "{0}var result = Reference.{1}({2});";

      // Append return line
      const string ReturnLineTemplate = "{0}return new {1}(result);";
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ReturnLineTemplate,
        this.CurrentIndent,

        )
      this.buffer.AppendLine();

      // Outdent
			this.currentIndentWidth -= this.IndentWidth;
    }

    private string GenerateReturnLine(MethodInfo source) {
      
    }

    /// <summary>
    /// Looks up the JavaScript type name for the specified type.
    /// </summary>
    private string DetermineJavaScriptTypeName(Type type) {
      // TODO: Look up set of stored names here in case of naming conflict
      return type.Name;
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


