using System;
using System.Collections.Generic;

namespace EdgeReference
{
	public class ProxyDefinition
	{
		public ProxyDefinition ()
		{
		}

		public string TypeName {
			get;
			set;
		}

		public Dictionary<string, ProxyDefinition> Properties {
			get;
			private set;
		}

		public Dictionary<string, FunctionDefinition> Functions {
			get;
			private set;
		}

	}
}

