using System;
using System.Collections.Generic;

namespace CoreDroid.Contract
{
	[ServiceContract]
	public abstract class OperationService<T, I> where T : OperationServiceThread where I : OperationInfo
	{
		private Dictionary<int, T> operations = new Dictionary<int, T> ();
		
		protected T Create (I info)
		{
			this.operations.Add (info.ID, Activator.CreateInstance (typeof(T), info) as T);
			
			return this.operations [info.ID];
		}
		
		[ServiceMember]
		public void Stop (int id)
		{
			this.operations [id].Stop ();
		}
		
		[ServiceMember]
		public I GetInfo (int id)
		{
			return (I)this.operations [id].Info;
		}
	}
}

