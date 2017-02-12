using System;
using System.Collections.Generic;

namespace EdgeReference
{
	public class FunctionDefinition : ProxyDefinition
	{
		public FunctionDefinition ()
		{
		}

		public string Name {
			get;
			set;
		}

		public ProxyDefinition ReturnType {
			get;
			set;
		}

		public Dictionary<string, ProxyDefinition> Arguments {
			get;
			private set;
		}


	}
}

