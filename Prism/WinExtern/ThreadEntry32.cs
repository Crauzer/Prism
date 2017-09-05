using System.Runtime.InteropServices;

namespace Prism.WinExtern
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct ThreadEntry32
    {
        internal uint dwSize;
        internal readonly uint cntUsage;
        internal readonly uint th32ThreadID;
        internal readonly uint th32OwnerProcessID;
        internal readonly uint tpBasePri;
        internal readonly uint tpDeltaPri;
        internal readonly uint dwFlags;
    }
}
