using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.WinExtern
{
    public struct ExcpRecord
    {
        public readonly uint ExceptionCode;
        public readonly uint ExceptionFlags;
        public readonly IntPtr ExceptionRecord;
        public readonly IntPtr ExceptionAddress;
        public readonly uint NumberParameters;
        public unsafe fixed uint ExceptionInformation[15];
    }
}
