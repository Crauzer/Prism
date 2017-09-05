using System;

namespace Prism.WinExtern
{
    [Flags]
    public enum ThreadContextFlags : uint
    {
        i386 = 0x10000,
        i486 = 0x10000,
        Control = i386 | 0x01,
        Integer = i386 | 0x02,
        Segments = i386 | 0x04,
        FloatingPoint = i386 | 0x08,
        DebugRegisters = i386 | 0x10,
        ExtendedRegisters = i386 | 0x20,
        Full = Control | Integer | Segments,
        All = Control | Integer | Segments | FloatingPoint | DebugRegisters | ExtendedRegisters
    }

    [Flags]
    public enum ThreadAccess : uint
    {
        Terminate = 1 << 0,
        SuspendResume = 1 << 1,
        GetContext = 1 << 3,
        SetContext = 1 << 4,
        SetInformation = 1 << 5,
        QueryInformation = 1 << 6,
        SetThreadToken = 1 << 7,
        Impersonate = 1 << 8,
        DirectImpersonation = 1 << 9,
        SetLimitedInformation = 1 << 10,
        QueryLimitedInformation = 1 << 11,
        Synchronize = 1 << 20,
        All = 0x1F03FF
    }

    public enum MemoryAllocation
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum Protection : uint
    {
        NoAccess = 1 << 0,
        ReadOnly = 1 << 1,
        ReadWrite = 1 << 2,
        WriteCopy = 1 << 3,
        Execute = 1 << 4,
        ExecuteRead = 1 << 5,
        ExecuteReadWrite = 1 << 6,
        ExecuteWriteCopy = 1 << 7,
        Guard = 1 << 8,
        NoCache = 1 << 9,
        WriteCombine = 1 << 10
    }

    public enum ProcessAccessRights : int
    {
        PROCESS_ALL_ACCESS = 0x1F0FFF,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_TERMINATE = 0x0001,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        SYNCHRONIZE = 0x00100000
    }

    [Flags]
    public enum HookRegister
    {
        None = 0,
        DR0 = 1,
        DR1 = 2,
        DR2 = 4,
        DR3 = 8,
    }
}
