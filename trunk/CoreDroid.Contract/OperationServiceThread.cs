using System;
using System.Threading;

namespace CoreDroid.Contract
{
	public abstract class OperationServiceThread : IDisposable
	{
		public OperationInfo Info { get; private set; }

		public Thread Thread { get; private set; }
		
		protected abstract ThreadStart Worker { get; }
		
		public OperationServiceThread (OperationInfo info)
		{
			this.Info = info;
		}
		
		public void Start ()
		{
			this.Thread = new Thread (this.Worker);
			this.Thread.Start ();
		}
		
		public void Stop ()
		{
			if (this.Thread != null)
				this.Thread.Abort ();
		}
		
		public void Dispose ()
		{
			this.Stop ();
		}
	}
}

