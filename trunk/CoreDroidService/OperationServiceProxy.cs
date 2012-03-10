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
using CoreDroid.Contract;

namespace CoreDroid
{
    public abstract class OperationServiceProxy:ServiceProxy
    {
        public OperationServiceProxy() : base() { }

        public OperationInfo Remove(int id)
        {
            return this.Call("Remove", id) as OperationInfo;
        }

        public OperationInfo GetInfo(int id)
        {
            return this.Call("GetInfo", id) as OperationInfo;
        }

        public void CleanUp()
        {
            this.Call("CleanUp");
        }
    }
}