using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using System.Reflection;
using System.IO;

namespace CoreDroid
{
    public class RuntimeHelper
    {
        private static string cachePath = Android.App.Application.Context.CacheDir.AbsolutePath;
        private static string binPath = Path.Combine(cachePath, "bin");

        private static Dictionary<SocketClient, Java.Lang.Process> instances = new Dictionary<SocketClient,Java.Lang.Process>();

        private static bool? canRoot;
        public static bool CanRoot
        {
            get
            {
                if (!canRoot.HasValue)
                {
                    try
                    {
                        canRoot = !string.IsNullOrEmpty(Encoding.UTF8.GetString(Run("su -v")));
                    }
                    catch { canRoot = false; }
                }

                return canRoot.Value;
            }
        }

        private static byte[] Run(string command)
        {
            Java.Lang.Process shellProcess = null;
            Stream outS = null;
            Stream inS = null;
            Stream errS = null;

            try
            {
                shellProcess = Java.Lang.Runtime.GetRuntime().Exec(command);

                outS = shellProcess.OutputStream;
                inS = shellProcess.InputStream;
                errS = shellProcess.ErrorStream;

                shellProcess.WaitFor();

                byte[] stdOut = new byte[outS.Length];
                outS.Read(stdOut, 0, Convert.ToInt32(outS.Length));

                if (errS.Length > 0)
                {
                    byte[] stdErr = new byte[errS.Length];
                    errS.Read(stdErr, 0, Convert.ToInt32(errS.Length));

                    throw (new ExecutionEngineException(Encoding.UTF8.GetString(stdErr)));
                }
                else
                {
                    return stdOut;
                }
            }
            finally
            {
                if (shellProcess != null)
                    shellProcess.Destroy();

                if (outS != null)
                    outS.Dispose();

                if (inS != null)
                    inS.Dispose();

                if (errS != null)
                    errS.Dispose();
            }
        }

        public static void Extract()
        {
            AssetManager assets = Android.App.Application.Context.Assets;

            Assembly a = Assembly.GetEntryAssembly();
            string prefix = "coredroid";
            foreach (string name in assets.List(prefix))
            {
                string path = Path.Combine(binPath, name);

                if (!File.Exists(path))
                {
                    using (FileStream fs = File.OpenWrite(path))
                    {
                        using (Stream rs = assets.Open(string.Concat(prefix, "/", name)))
                        {
                            rs.CopyTo(fs);
                        }
                    }

                    Run("chmod 0777 " + path);
                }
            }
        }

        public static SocketClient StartInstance(bool asRoot)
        {
            Java.Lang.Process shellProcess = null;
            Stream outS = null;
            SocketClient client = null;

            try
            {
                shellProcess = Java.Lang.Runtime.GetRuntime().Exec((asRoot ? "su -c " : string.Empty) + Path.Combine(binPath, "monorun"));

                outS = shellProcess.OutputStream;

                int counter = 0;
                byte[] buf = new byte[5];
                while (counter < 5)
                {
                    counter += outS.Read(buf, counter, 5);
                }

                int port = Convert.ToInt32(Encoding.UTF8.GetString(buf));

                client = new SocketClient(port);

                instances.Add(client, shellProcess);

                return client;
            }
            finally
            {
                if (shellProcess != null)
                    shellProcess.Destroy();

                if (outS != null)
                    outS.Dispose();             
            }
        }

        public static void StopInstance(SocketClient client)
        {
            Java.Lang.Process shellProcess = instances[client];
            instances.Remove(client);

            client.Shutdown();
            client.Dispose();
            shellProcess.Destroy();
        }
    }
}