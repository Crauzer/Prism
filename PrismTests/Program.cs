using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism;

namespace PrismTests
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MemoryEditor me = new MemoryEditor("TestCA"))
            {
                me.WriteInt32(1, new IntPtr(0x00E143D4));
            }
        }
    }
}
