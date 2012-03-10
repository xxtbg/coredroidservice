using System;
using CoreDroid.Contract;

namespace CoreDroid
{
	public abstract class OperationServiceProxy:ServiceProxy
	{
		public OperationInfo Remove (int id)
		{
			return this.Call ("Remove", id) as OperationInfo;
		}

		public OperationInfo GetInfo (int id)
		{
			return this.Call ("GetInfo", id) as OperationInfo;
		}

		public void CleanUp ()
		{
			this.Call ("CleanUp");
		}
	}
}