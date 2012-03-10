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
		public I Remove (int id)
		{
			this.operations [id].Stop ();
			I info = this.GetInfo (id);
			this.operations [id].Dispose ();
			this.operations.Remove (id);
			
			return info;
		}
		
		[ServiceMember]
		public I GetInfo (int id)
		{
			return (I)this.operations [id].Info;
		}
		
		[ServiceMember]
		public void CleanUp ()
		{
			foreach (int id in this.operations.Keys) {
				if (this.operations [id].Info.IsFinished || this.operations [id].Info.Exception != null)
					this.Remove (id);
			}
		}
	}
}

