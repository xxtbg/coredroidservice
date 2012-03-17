using System;
using System.Threading;
using CoreDroid.Contract;
using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem
{
	public class FileProgressWatcher
	{
		private DirectoryService directoryService = new DirectoryService ();
		private string path;
		private ProgressInfo info;
		private Thread thread;
		private bool stopped = false;
		
		public FileProgressWatcher (string path, ProgressInfo info)
		{
			this.path = path;
			this.info = info;
			this.thread = new Thread (new ThreadStart (this.Worker));
			this.thread.Start ();
		}
		
		public void Stop ()
		{
			this.stopped = true;
		}
		
		private void Worker ()
		{
			while (!this.stopped) {
				try {
					FileItemInfo item = this.directoryService.Get (this.path, 0) as FileItemInfo;
					
					if (item != null) {
						this.info.Current = item.Size;
					} else {
						this.info.Current = 0;
					}
				} catch {
					this.info.Current = 0;
				}
				
				Thread.Sleep (50);
			}
		}
	}
}

