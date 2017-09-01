using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Prism
{
    public class MemoryEditor : IDisposable
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessRights dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);
        [DllImport("Kernel32")]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        public static extern bool GetThreadContext(IntPtr hThread, ref ThreadContext lpContext);
        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, MemoryAllocation flAllocationType, Protection flProtect);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, MemoryAllocation freeType);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, uint dwAddress, int nSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        public IntPtr Handle { get; private set; }
        public IntPtr MainModuleAddress { get; private set; }
        public int MainModuleSize { get; private set; }
        public Process Process { get; private set; }
        public bool IsActive { get; private set; }

        public MemoryEditor(string processName)
        {
            this.IsActive = OpenProcess(processName);
        }

        public MemoryEditor(string processName, bool inheritHandle)
        {
            this.IsActive = OpenProcess(processName, inheritHandle);
        }

        public MemoryEditor(string processName, bool inheritHandle, ProcessAccessRights access)
        {
            this.IsActive = OpenProcess(processName, inheritHandle, access);
        }

        public bool OpenProcess(string processName)
        {
            return OpenProcess(processName, false, ProcessAccessRights.PROCESS_ALL_ACCESS);
        }

        public bool OpenProcess(string processName, bool inheritHandle)
        {
            return OpenProcess(processName, inheritHandle, ProcessAccessRights.PROCESS_ALL_ACCESS);
        }

        public bool OpenProcess(string processName, bool inheritHandle, ProcessAccessRights access)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length != 0)
            {
                this.Process = processes[0];

                try
                {
                    this.Handle = OpenProcess(access, inheritHandle, this.Process.Id);
                    this.MainModuleAddress = this.Process.MainModule.BaseAddress;
                    this.MainModuleSize = this.Process.MainModule.ModuleMemorySize;
                    this.IsActive = true;
                    return true;
                }
                catch (Exception)
                {
                    throw new Exception("Opening process: " + processes[0].ProcessName + " failed");
                }
            }
            else
            {
                throw new Exception("Specified process not found");
            }
        }

        public byte ReadByte(IntPtr address)
        {
            return ReadBytes(address, sizeof(byte))[0];
        }

        public short ReadInt16(IntPtr address)
        {
            return BitConverter.ToInt16(ReadBytes(address, sizeof(short)), 0);
        }

        public ushort ReadUInt16(IntPtr address)
        {
            return BitConverter.ToUInt16(ReadBytes(address, sizeof(ushort)), 0);
        }

        public int ReadInt32(IntPtr address)
        {
            return BitConverter.ToInt32(ReadBytes(address, sizeof(int)), 0);
        }

        public uint ReadUInt32(IntPtr address)
        {
            return BitConverter.ToUInt32(ReadBytes(address, sizeof(uint)), 0);
        }

        public long ReadInt64(IntPtr address)
        {
            return BitConverter.ToInt64(ReadBytes(address, sizeof(long)), 0);
        }

        public ulong ReadUInt64(IntPtr address)
        {
            return BitConverter.ToUInt64(ReadBytes(address, sizeof(ulong)), 0);
        }

        public IntPtr ReadIntPtr(IntPtr address)
        {
            return (IntPtr)BitConverter.ToInt32(ReadBytes(address, sizeof(uint)), 0);
        }

        public double ReadDouble(IntPtr address)
        {
            return BitConverter.ToDouble(ReadBytes(address, sizeof(double)), 0);
        }

        public float ReadFloat(IntPtr address)
        {
            return BitConverter.ToSingle(ReadBytes(address, sizeof(float)), 0);
        }

        public T ReadStruct<T>(IntPtr address) where T : struct
        {
            T structure = new T();

            int structSize = Marshal.SizeOf(structure);
            byte[] structBuffer = ReadBytes(address, structSize);
            IntPtr structPointer = Marshal.AllocHGlobal(structSize);

            Marshal.Copy(structBuffer, 0, structPointer, structSize);

            structure = (T)Marshal.PtrToStructure(structPointer, structure.GetType());
            Marshal.FreeHGlobal(structPointer);

            return structure;
        }

        public byte[] ReadBytes(IntPtr address, int size)
        {
            int byteCount = 0;
            byte[] buffer = new byte[size];

            bool success = ReadProcessMemory((int)Handle, (int)address, buffer, buffer.Length, ref byteCount);

            return success ? buffer : buffer = null;
        }

        public bool WriteByte(byte value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteInt16(short value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteUInt16(ushort value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteInt32(int value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteUInt32(uint value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteInt64(long value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteUInt64(ulong value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteIntPtr(IntPtr value, IntPtr address)
        {
            return WriteInt32(value.ToInt32(), address);
        }

        public bool WriteDouble(double value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteFloat(float value, IntPtr address)
        {
            return WriteBytes(BitConverter.GetBytes(value), address);
        }

        public bool WriteStruct<T>(T value, IntPtr address) where T : struct
        {
            int structSize = Marshal.SizeOf(value);
            byte[] structBuffer = new byte[structSize];

            IntPtr structPointer = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(value, structPointer, true);
            Marshal.Copy(structPointer, structBuffer, 0, structSize);
            Marshal.FreeHGlobal(structPointer);

            return WriteBytes(structBuffer, address);
        }

        public bool WriteBytes(byte[] buffer, IntPtr address)
        {
            int byteCount = 0;
            return WriteProcessMemory((int)this.Handle, (int)address, buffer, buffer.Length, ref byteCount);
        }

        public void Dispose()
        {
            CloseHandle(this.Handle);
            this.IsActive = false;
            this.Process = null;
            this.MainModuleAddress = IntPtr.Zero;
            this.MainModuleSize = 0;
        }
    }
}
