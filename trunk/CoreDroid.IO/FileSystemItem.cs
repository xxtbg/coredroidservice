using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Unix;
using Mono.Unix.Native;
using ProtoBuf;

namespace CoreDroid.IO
{
    [ProtoContract]
    public class FileSystemItem
    {
        private UnixFileSystemInfo info;

        public FileSystemItem Parent { get; private set; }
        public string Path { private get; set; }

        public string Name { get { return System.IO.Path.GetFileName(this.Path); } }
        public string Extension { get { return System.IO.Path.GetExtension(this.Path); } }

        public long Size { get; private set; }

        public FileSystemItem() : this("/", null) { }

        public FileSystemItem(string path, FileSystemItem parent)
        {
            this.Path = path;
            this.Parent = parent;
            this.info = new UnixFileInfo(this.Path);
            this.Reload();
        }

        public void Reload()
        {
            this.info.Refresh();
        }

        void Child_OperationStarting(object sender, OperationStartingEventArgs e)
        {
            this.SendOperationStarting(sender, e);
        }

        void Child_OperationFinished(object sender, EventArgs e)
        {
            this.SendOperationFinished(sender, e);
        }

        public void NewDirectory(string directoryName)
        {
            if (this.CanWrite)
                FileSystem.Mkdir(System.IO.Path.Combine(this.Path, directoryName));
            else
                throw (new OperationExecutionPermissionException());
        }

        public void CopyCheck(FileSystemItem target)
        {
            this.CopyCheck(target, false);
        }

        public void Copy(FileSystemItem target)
        {
            this.Copy(target, false);
        }

        public void MoveCheck(FileSystemItem target)
        {
            this.CopyCheck(target, true);
        }

        public void Move(FileSystemItem target)
        {
            this.Copy(target, true);
        }

        private void CopyCheck(FileSystemItem target, bool deleteOrig)
        {
            if (!(target == null || target.Type == FileSystemType.Directory))
                throw (new NoDirectoryTargetException());

            if ((this.Parent != null && this.Parent == target) || this == target)
                throw (new TargetSameLocationException());

            this.CopyCheckInternal(target, deleteOrig);
        }

        private void CopyCheckInternal(FileSystemItem target, bool deleteOrig)
        {
            string newPath = target != null ? System.IO.Path.Combine(target.Path, this.Name) : null;

            if (this.CanRead && (!deleteOrig || this.CanWrite) && (target == null || target.CanWrite))
            {
                if (this.Type == FileSystemType.Directory)
                {
                    FileSystemItem childTarget = target != null ? GetOrNull(newPath) : null;
                    foreach (FileSystemItem child in this.Children)
                        child.CopyCheck(childTarget, deleteOrig);
                }
            }
            else
                throw (new OperationExecutionPermissionException());
        }

        private void Copy(FileSystemItem target, bool deleteOrig)
        {
            this.CopyInternal(target, deleteOrig);

            if (deleteOrig && this.Parent != null)
                this.Parent.Reload();
        }

        private void CopyInternal(FileSystemItem target, bool deleteOrig)
        {
            string newPath = System.IO.Path.Combine(target.Path, this.Name);

            CheckOperationCanceled();

            this.SendOperationStarting(newPath);

            if (this.Type == FileSystemType.Directory)
            {
                if (GetOrNull(newPath) == null)
                {
                    FileSystem.Mkdir(newPath);
                }

                foreach (FileSystemItem item in this.Children)
                    item.CopyInternal(Get(newPath), deleteOrig);
            }
            else if (this.Type == FileSystemType.File)
            {
                if (deleteOrig)
                    FileSystem.Mv(this.Path, target.Path);
                else
                    FileSystem.Cp(this.Path, target.Path);
            }
            else if (this.Type == FileSystemType.SymbolicLink && this.SymbolicLinkTarget != null)
            {
                FileSystem.Ln(newPath, this.SymbolicLinkTarget.Path);
            }

            if ((deleteOrig && this.Type == FileSystemType.Directory) || this.Type == FileSystemType.SymbolicLink)
            {
                this.DeleteInternal();
            }

            this.SendOperationFinished();
        }

        private string GenerateFileMode()
        {
            string fileMode = "0";
            fileMode += ((short)this.OwnerFlags).ToString();
            fileMode += ((short)this.GroupFlags).ToString();
            fileMode += ((short)this.OtherFlags).ToString();

            return fileMode;
        }

        public void DeleteCheck()
        {
            if (this.CanWrite)
            {
                if (this.Type == FileSystemType.Directory)
                {
                    foreach (FileSystemItem child in this.Children)
                        child.DeleteCheck();
                }
            }
            else
                throw (new OperationExecutionPermissionException());
        }

        public void Delete()
        {
            this.DeleteInternal();

            if (this.Parent != null)
                this.Parent.Reload();
        }

        private void DeleteInternal()
        {
            this.SendOperationStarting();

            CheckOperationCanceled();

            if (this.Type == FileSystemType.Directory)
            {
                foreach (FileSystemItem item in this.Children)
                    item.DeleteInternal();
            }

            if (this.Type == FileSystemType.Directory)
                FileSystem.Rmdir(this.Path);
            else
                FileSystem.Rm(this.Path);

            cache.Remove(this.Path);

            this.SendOperationFinished();
        }

        public void RenameCheck()
        {
            if (!this.CanWrite)
                throw (new OperationExecutionPermissionException());
        }

        public void Rename(string newName)
        {
            this.RenameInternal(newName);

            if (this.Parent != null)
                this.Parent.Reload();
        }

        private void RenameInternal(string newName)
        {
            this.SendOperationStarting();

            FileSystem.Mv(this.Path, System.IO.Path.Combine(this.Parent.Path, newName));

            this.SendOperationFinished();
        }

        private static FileSystemFlags GetPermissionFlags(string perm)
        {
            FileSystemFlags flags = 0;

            if (perm[0] == 'r') flags = FileSystemFlags.Read;
            if (perm[1] == 'w') flags = flags | FileSystemFlags.Write;
            if (perm[2] == 'x') flags = flags | FileSystemFlags.Execute;

            return flags;
        }

        private void SendOperationStarting()
        {
            this.SendOperationStarting(null);
        }

        private void SendOperationStarting(string targetPath)
        {
            this.SendOperationStarting(this, new OperationStartingEventArgs(targetPath));
        }

        private void SendOperationStarting(object sender, OperationStartingEventArgs e)
        {
            if (this.OperationStarting != null)
                this.OperationStarting(sender, e);
        }

        public delegate void OperationStartingHandler(object sender, OperationStartingEventArgs e);
        public event OperationStartingHandler OperationStarting;

        private void SendOperationFinished()
        {
            this.SendOperationFinished(this, EventArgs.Empty);
        }

        private void SendOperationFinished(object sender, EventArgs e)
        {
            if (this.OperationFinished != null)
                this.OperationFinished(sender, e);
        }
        public event EventHandler OperationFinished;
    }

    public class OperationStartingEventArgs : EventArgs
    {
        public string TargetPath { get; private set; }

        public OperationStartingEventArgs(string targetPath)
        {
            this.TargetPath = targetPath;
        }
    }

    public enum FileSystemType
    {
        Directory,
        File,
        SymbolicLink
    }

    [Flags]
    public enum FileSystemFlags : short
    {
        Read = 0x01,
        Write = 0x02,
        Execute = 0x04
    }

    public class OperationExecutionPermissionException : Exception { public OperationExecutionPermissionException() : base() { } }
    public class NoDirectoryTargetException : Exception { public NoDirectoryTargetException() : base() { } }
    public class TargetSameLocationException : Exception { public TargetSameLocationException() : base() { } }
}