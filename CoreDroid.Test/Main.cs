using System;
using System.Linq;
using System.IO;
using System.Reflection;
using CoreDroid.Contract;
using DiskDroid.FileSystem;
using DiskDroid.FileSystem.Contract;
using System.Collections.Generic;
using System.Threading;

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
				MoveFileOperationService copyService = client.GetService<MoveFileOperationService> ();
				DirectoryItemInfo tmpDir = (DirectoryItemInfo)service.Get ("/home/ralph/tmp");
				DirectoryItemInfo testDir = (DirectoryItemInfo)service.Get ("/home/ghost.old/Downloads");
				int opID = copyService.Start (testDir, tmpDir, false);
				
				bool finished = false;
				bool overwriteAll = false;
				while (!finished) {
					CopyFileOperationInfo info = (CopyFileOperationInfo)copyService.GetInfo (opID);
					if (info != null) {
						Console.Clear ();
							
						if (info.Actual != null)
							Console.WriteLine (info.Actual.Path);
							
						if (info.ActualProgress != null) {
							Console.WriteLine ("Progress:\t" + (info.ActualProgress != null ? info.ActualProgress.Current + "\t/" + info.ActualProgress.Max : string.Empty));
							Console.WriteLine ("Overall:\t" + info.OverallProgress.Current + "\t/" + info.OverallProgress.Max);
						}
						
						if (info.IsRunning) {
							if (info.ConflictItem != null) {
								char key;
								
								if (info.ConflictOverwriteable) {
									if (!overwriteAll) {
										Console.WriteLine ("Conflict: " + info.Actual.Path + " vs. " + info.ConflictItem.Path);
										Console.Write ("(k)eep/(o)verwrite/(a)all/(c)ancel: ");
										key = Console.ReadKey ().KeyChar;
										Console.WriteLine ();
									} else {
										key = 'o';
									}
								} else {
									Console.WriteLine ("Conflict(not overwriteable): " + info.Actual.Path + " vs. " + info.ConflictItem.Path);
									Console.Write ("(k)eep/(c)ancel: ");
									key = Console.ReadKey ().KeyChar;
									Console.WriteLine ();
								}
								
								switch (key) {
								case 'k':
									copyService.ResolveConflict (opID, false);
									break;
								case 'o':
									copyService.ResolveConflict (opID, true);
									break;
								case 'a':
									overwriteAll = true;
									copyService.ResolveConflict (opID, true);
									break;
								case 'c':
									copyService.Remove (opID);
									finished = true;
									break;
								}
							} else {
								Thread.Sleep (50);	
							}
						} else {
							if (info.Exception != null) {
								Console.WriteLine ("EXCEPTION: " + info.Exception.ExceptionTypeName + ", " + info.Exception.ExceptionTypeName);
								Console.WriteLine ("Message:");
								Console.WriteLine (info.Exception.ExceptionMessage);
								Console.WriteLine ("StackTrace:");
								Console.WriteLine (info.Exception.ExceptionStackTrace);
							}
							
							finished = true;
						}
					} else {
						finished = true;
					}
				}
				
				FileSystemItemInfo targetItem = service.GetContents (tmpDir).Where (i => i.Name == "Downloads").FirstOrDefault ();
				
				if (targetItem != null)
					Console.WriteLine (targetItem.Path);
				else
					Console.WriteLine ("something went wrong");
				
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
