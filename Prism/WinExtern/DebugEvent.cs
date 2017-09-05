using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Prism.WinExtern
{
    public struct DebugEvent
    {
        public readonly uint dwDebugEventCode;
        public readonly int dwProcessId;
        public readonly int dwThreadId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 86, ArraySubType = UnmanagedType.U1)]
        private readonly byte[] debugInfo;

        public unsafe ExceptionDebugInfo Exception
        {
            get
            {
                if (this.debugInfo == null)
                    return new ExceptionDebugInfo();
                fixed (byte* numPtr = this.debugInfo)
                    return *(ExceptionDebugInfo*)numPtr;
            }
        }

        public unsafe LoadDllDebugInfo LoadDll
        {
            get
            {
                if (this.debugInfo == null)
                    return new LoadDllDebugInfo();
                fixed (byte* numPtr = this.debugInfo)
                    return *(LoadDllDebugInfo*)numPtr;
            }
        }
    }
}
