using System;

namespace CoreDroid.Extensions
{
	public static class ExceptionExtensions
	{
		public static void WriteToConsole (this Exception ex)
		{
			Console.WriteLine (ex.GetType ().FullName);
			Console.WriteLine (ex.Message);
			if (ex.InnerException != null)
				ex.InnerException.WriteToConsole ();
			Console.WriteLine (ex.StackTrace);
		}
	}
}

