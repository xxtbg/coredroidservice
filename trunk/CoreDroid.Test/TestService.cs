using System;

namespace CoreDroid.Test
{
	public class TestService : ServiceProxy
	{
		public TestService ():base()
		{
		}
		
		public string[] List (string path)
		{
			return this.Call ("List", typeof(string[]), path) as string[];
		}
	}
}

