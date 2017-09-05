using Prism.WinExtern;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Prism
{
    public class ProcessHook : IDisposable
    {
        public List<Hook> Hooks { get; private set; } = new List<Hook>();
        private readonly List<IntPtr> OpenThreadHandles = new List<IntPtr>();
        private int _hookCount;
        private Thread _workerThread;
        private bool _removeHook;
        private bool _isDebugActive;
        private readonly Hook _hook;
        private bool _disposed;

        public ProcessHook(Process gameProcess, HookRegister register, IntPtr hookLocation, ProcessHook.HandleHookCallback callback)
        {
            Hook hookItem = new Hook()
            {
                Callback = callback,
                Location = hookLocation,
                Register = register
            };
            this._hook = hookItem;
            this.Hooks.Add(hookItem);
            ++this._hookCount;
            if (this._hookCount == 0 || this._workerThread == null)
            {
                this._workerThread = new Thread(() => InstallHardwareHook(gameProcess))
                {
                    IsBackground = true
                };
                this._workerThread.Start();
            }
            RemoveThreadHook(hookItem);
        }

        ~ProcessHook()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            --this._hookCount;
            AddThreadHook(this._hook);

            if (this._hookCount == 0)
            {
                this._removeHook = true;
            }

            this.Hooks.Remove(this._hook);
            this._disposed = true;
        }

        private void OpenAllThreads(Process proc)
        {
            ThreadEntry32 threadEntry = new ThreadEntry32
            {
                dwSize = 28U
            };
            IntPtr toolhelp32Snapshot = Win32.CreateToolhelp32Snapshot(4, 0U);

            if (!Win32.Thread32First(toolhelp32Snapshot, ref threadEntry) || !Win32.Thread32Next(toolhelp32Snapshot, out threadEntry))
            {
                return;
            }

            do
            {
                if (threadEntry.th32OwnerProcessID == proc.Id)
                {
                    this.OpenThreadHandles.Add(Win32.OpenThread((ThreadAccess)2097151U, false, threadEntry.th32ThreadID));
                }
            }
            while (Win32.Thread32Next(toolhelp32Snapshot, out threadEntry));
        }

        private void SetDebugRegisters(HookRegister register, IntPtr hookLocation, ref ThreadContext threadContext)
        {

            switch (register)
            {
                case HookRegister.DR0:
                    threadContext.Dr0 = (uint)(int)hookLocation;
                    threadContext.Dr7 |= 1U;
                    break;
                case HookRegister.DR1:
                    threadContext.Dr1 = (uint)(int)hookLocation;
                    threadContext.Dr7 |= 4U;
                    break;
                case HookRegister.DR2:
                    threadContext.Dr2 = (uint)(int)hookLocation;
                    threadContext.Dr7 |= 16U;
                    break;
                case HookRegister.DR3:
                    threadContext.Dr3 = (uint)(int)hookLocation;
                    threadContext.Dr7 |= 64U;
                    break;
            }
            threadContext.Dr6 = 0U;
        }

        private void RemoveDebugRegisters(HookRegister register, IntPtr hookLocation, ref ThreadContext threadContext)
        {
            uint num = 0;
            switch (register)
            {
                case HookRegister.DR0:
                    num = 1U;
                    threadContext.Dr0 = 0U;
                    break;
                case HookRegister.DR1:
                    num = 4U;
                    threadContext.Dr1 = 0U;
                    break;
                case HookRegister.DR2:
                    num = 16U;
                    threadContext.Dr2 = 0U;
                    break;
                case HookRegister.DR3:
                    num = 64U;
                    threadContext.Dr3 = 0U;
                    break;
            }
            threadContext.Dr7 &= ~num;
        }

        private void AddThreadHook(Hook item)
        {
            ThreadContext threadContext = new ThreadContext
            {
                ThreadContextFlags = (ThreadContextFlags)65559
            };

            foreach (IntPtr openThreadHandle in this.OpenThreadHandles)
            {
                Win32.SuspendThread(openThreadHandle);
                Win32.GetThreadContext(openThreadHandle, ref threadContext);
                SetDebugRegisters(item.Register, item.Location, ref threadContext);
                item.Hooked = true;
                Win32.SetThreadContext(openThreadHandle, ref threadContext);
                Win32.ResumeThread(openThreadHandle);
            }
        }

        private void RemoveThreadHook(Hook item)
        {
            ThreadContext threadContext = new ThreadContext
            {
                ThreadContextFlags = (ThreadContextFlags)65559
            };

            foreach (IntPtr openThreadHandle in this.OpenThreadHandles)
            {
                Win32.SuspendThread(openThreadHandle);
                Win32.GetThreadContext(openThreadHandle, ref threadContext);
                RemoveDebugRegisters(item.Register, item.Location, ref threadContext);
                item.Hooked = false;
                Win32.SetThreadContext(openThreadHandle, ref threadContext);
                Win32.ResumeThread(openThreadHandle);
            }
        }

        public void InstallHardwareHook(Process process)
        {
            Win32.OpenProcess((ProcessAccessRights)2097151U, false, process.Id);
            OpenAllThreads(process);

            if (!_isDebugActive && !Win32.DebugActiveProcess((uint)process.Id))
            {
                throw new Exception("Failed to attach hook");
            }

            this._isDebugActive = true;
            Win32.DebugSetProcessKillOnExit(0U);

            try
            {
                while (!_removeHook)
                {
                    if (this.Hooks.Any(i => !i.Hooked))
                    {
                        foreach (Hook hook in this.Hooks)
                        {
                            if (hook.Hooked == false)
                            {
                                AddThreadHook(hook);
                            }
                        }
                    }

                    ThreadContext threadContext = new ThreadContext
                    {
                        ThreadContextFlags = (ThreadContextFlags)65559U
                    };
                    DebugEvent lpDebugEvent;

                    if (Win32.WaitForDebugEvent(out lpDebugEvent, uint.MaxValue))
                    {
                        if ((int)lpDebugEvent.Exception.ExceptionRecord.ExceptionCode != -2147483644)
                        {
                            Win32.ContinueDebugEvent((uint)lpDebugEvent.dwProcessId, (uint)lpDebugEvent.dwThreadId, 2147549185U);
                        }
                        else
                        {
                            IntPtr handle = Win32.OpenThread((ThreadAccess)2097151U, false, (uint)lpDebugEvent.dwThreadId);
                            Win32.GetThreadContext(handle, ref threadContext);
                            threadContext.EFlags |= 65600U;

                            try
                            {
                                Hook hookItem = this.Hooks.FirstOrDefault(hook => (int)hook.Location == (int)threadContext.Eip);
                                if (hookItem != null)
                                {
                                    hookItem.Callback(ref threadContext);
                                }
                            }
                            catch
                            {
                            }

                            Win32.SetThreadContext(handle, ref threadContext);
                            Win32.CloseHandle(handle);
                            Win32.ContinueDebugEvent((uint)lpDebugEvent.dwProcessId, (uint)lpDebugEvent.dwThreadId, 65538U);
                        }
                    }
                }
            }
            finally
            {
                foreach (Hook hook in this.Hooks)
                {
                    if (hook.Hooked)
                    {
                        AddThreadHook(hook);
                    }
                }
            }
        }

        public delegate void HandleHookCallback(ref ThreadContext threadContext);

        public class Hook
        {
            public HandleHookCallback Callback;
            public bool Hooked;
            public IntPtr Location;
            public HookRegister Register;
        }
    }
}
