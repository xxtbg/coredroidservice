using System;
using System.Threading;

namespace CoreDroid.Contract
{
	public class OperationServiceThread
	{
		public OperationInfo Info { get; private set; }

		public Thread Thread { get; private set; }
		
		public OperationServiceThread (OperationInfo info, ThreadStart worker)
		{
			this.Info = info;
			this.Thread = new Thread (worker);
		}
		
		public void Start ()
		{
			this.Thread.Start ();
		}
		
		public void Stop ()
		{
			this.Thread.Abort ();
		}
	}
}

