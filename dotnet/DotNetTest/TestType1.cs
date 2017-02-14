using System;

namespace DotNetTest
{
	public class TestType1
	{
		public TestType1()
		{
		}

		public string Name { get; set; }

		public TestType2 Child { get; private set; }
	}
}

