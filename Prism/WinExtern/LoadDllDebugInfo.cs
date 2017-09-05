using System;

namespace Prism.WinExtern
{
    public struct LoadDllDebugInfo
    {
        public readonly IntPtr hFile;
        public readonly IntPtr lpBaseOfDll;
        public readonly uint dwDebugInfoFileOffset;
        public readonly uint nDebugInfoSize;
        public readonly IntPtr lpImageName;
        public readonly ushort fUnicode;
    }
}
