using System;
using System.Runtime.Serialization;
using CoreDroid.Contract;
using DiskDroid.FileSystem.Contract;
using System.Collections.Generic;

namespace DiskDroid.FileSystem
{
	[ServiceContract]
	public class FileOperationService
	{
		private Dictionary<int, Tuple<OperationInfo, Thread>> operations = new Dictionary<int, Tuple<OperationInfo, Thread>> ();
		
		[ServiceMember]
		public void Start ()
		{
		}
		
		[ServiceMember]
		public void Stop ()
		{
		}
		
		[ServiceMember]
		public OperationInfo GetProgress (int id)
		{
		}
	}
}

