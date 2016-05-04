using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SerieHandler
{
    public class ProcessHandler
    {
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern int SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess,
            bool bInheritHandle,
            uint dwThreadId
        );

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }
        

        public static void FreezeThreads(Process pProc)
        {
            if (pProc.ProcessName == "")
                return;

            foreach (ProcessThread pT in pProc.Threads)
            {
                IntPtr ptrOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (ptrOpenThread == null)
                    break;

                SuspendThread(ptrOpenThread);
            }
        }


        public static void UnfreezeThreads(Process pProc)
        {
            if (pProc.ProcessName == "")
                return;
                        
            foreach (ProcessThread pT in pProc.Threads)
            {
                IntPtr ptrOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (ptrOpenThread == null)
                    break;

                ResumeThread(ptrOpenThread);
            }
        }
    }
}
