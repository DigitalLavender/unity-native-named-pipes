using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MessagePipe.Game
{
    public static class NativeProcess
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public uint cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            internal IntPtr hProcess;
            internal IntPtr hThread;
            internal int dwProcessId;
            internal int dwThreadId;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcess(
            // [MarshalAs(UnmanagedType.LPTStr)] 
            string lpApplicationName,
            [In] string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            // [MarshalAs(UnmanagedType.LPTStr)] 
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);
        
        private enum ProcessCreationFlags : uint
        {
            NONE = 0,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SECURE_PROCESS = 0x00400000,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        public static StringBuilder BuildCommandLine(string startInfoFileName, string startInfoArguments)
        {
            var stringBuilder = new StringBuilder();
            var str = startInfoFileName.Trim();
            var flag = str.StartsWith("\"", StringComparison.Ordinal) && str.EndsWith("\"", StringComparison.Ordinal);
            if (!flag)
            {
                stringBuilder.Append("\"");
            }

            stringBuilder.Append(str);
            if (!flag)
            {
                stringBuilder.Append("\"");
            }

            if (!string.IsNullOrEmpty(startInfoArguments))
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(startInfoArguments);
            }

            return stringBuilder;
        }

        public static int Start(ProcessStartInfo processStartInfo)
        {
            var flags = processStartInfo.CreateNoWindow ? ProcessCreationFlags.CREATE_NO_WINDOW : ProcessCreationFlags.NONE;
            var stringBuilder = BuildCommandLine(processStartInfo.FileName, processStartInfo.Arguments);

            var lpStartupInfo = new STARTUPINFO
            {
                cb = (uint) Marshal.SizeOf<STARTUPINFO>()
            };

            var lpProcessInformation = new PROCESS_INFORMATION();

            // var flag = 0;
            // if (processStartInfo.CreateNoWindow)
            // {
            //     flag |= 134217728;
            // }

            var lpEnv = IntPtr.Zero;

            var lpCurrentDirectory = processStartInfo.WorkingDirectory;
            if (lpCurrentDirectory == string.Empty)
            {
                lpCurrentDirectory = Environment.CurrentDirectory;
            }

            var process = CreateProcess(null, 
                stringBuilder.ToString(), 
                IntPtr.Zero,
                IntPtr.Zero, 
                true, 
                flags, 
                lpEnv, 
                lpCurrentDirectory, 
                ref lpStartupInfo, 
                ref lpProcessInformation);
            if (process)
            {
                return lpProcessInformation.dwProcessId;
            }

            throw new Win32Exception();
        }
    }
}