using System;
using System.IO;
using System.Reflection;
using CoreDroid.Contract;
using DiskDroid.FileSystem;
using DiskDroid.FileSystem.Contract;
using System.Collections.Generic;

namespace CoreDroid.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				SocketClient client = new SocketClient (10000);//Convert.ToInt32 (Console.ReadLine ()));
			
				//client.LoadMono (File.OpenRead (Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "CoreDroid.Extensions.dll")));
				//client.LoadMono (File.OpenRead (Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "CoreDroid.Test.Contract.dll")));
				//client.LoadMono (File.OpenRead (Path.Combine (Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), "CoreDroid.Test.Plugin.dll")));
				DirectoryService service = client.GetService<DirectoryService> ();
				
				// testing copy service
				CopyFileOperationService copyService = client.GetService<CopyFileOperationService> ();
				DirectoryItemInfo tmpDir = (DirectoryItemInfo)service.Get ("/home/ralph/tmp");
				DirectoryItemInfo testDir = (DirectoryItemInfo)service.Get ("/home/ralph/Downloads");
				int opID = copyService.Start (testDir, tmpDir);
				
				bool finished = false;
				while (!finished) {
					CopyFileOperationInfo info = (CopyFileOperationInfo)copyService.GetInfo (opID);
					if (info != null) {
						if (info.IsRunning) {
							Console.WriteLine ("<" + info.Actual.Path + "> " + info.ActualProgress.Current + "\t/" + info.ActualProgress.Max);
						} else {
							if (info.Exception != null) {
								Console.WriteLine ("EXCEPTION: " + info.Exception.ExceptionTypeName + ", " + info.Exception.ExceptionTypeName);
								Console.WriteLine ("Message:");
								Console.WriteLine (info.Exception.ExceptionMessage);
								Console.WriteLine ("StackTrace:");
								Console.WriteLine (info.Exception.ExceptionStackTrace);
							}
							////////////////// TODO: DER POSTWORKER SOLL DAS ERGEBNISSOBJEKT IN DIE INFO HINEIN SCHREIBEN
							finished = true;
						}
					} else {
						finished = true;
					}
				}
				
				/*
				 DirectoryItemInfo directory = service.Get ("/") as DirectoryItemInfo;
				 IEnumerable<FileSystemItemInfo> contents = service.GetContents (directory);
			
				foreach (FileSystemItemInfo content in contents)
					Console.WriteLine (content.Path);*/
				
				service.Close ();
			} catch (ServiceException ex) {
				Console.WriteLine ("ServiceException");
				Console.WriteLine ("\tAssemblyName: " + ex.OriginalAssemblyName);
				Console.WriteLine ("\tTypeName: " + ex.OriginalTypeName);
				Console.WriteLine ("\tMessage: " + ex.OriginalMessage);
				Console.WriteLine ("\tStackTrace: " + ex.OriginalStackTrace);
			}
		}
	}
}
