using System;
using System.Threading;
using CoreDroid.Contract;
using DiskDroid.FileSystem.Contract;

namespace DiskDroid.FileSystem.Contract
{
	public class FileProgressWatcher
	{
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
			FileItemInfo item = null;
			while (!this.stopped) {
				try {
					if (item == null)
						item = FileSystemItemInfo.Get (this.path) as FileItemInfo;
					else
						item.ReloadInfo ();
					
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

