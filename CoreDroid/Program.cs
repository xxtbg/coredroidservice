using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreDroid
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketService.Start(Convert.ToInt32(args[1]));
        }
    }
}
