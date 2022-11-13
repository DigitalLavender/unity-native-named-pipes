using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace MessagePipe.Client.Unmanaged
{
    public class NativeProcess
    {
        public class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            internal SafeLocalMemHandle()
                : base(true)
            {
            }

            internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
                : base(ownsHandle)
            {
                this.SetHandle(existingHandle);
            }

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
            internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
                string StringSecurityDescriptor,
                int StringSDRevision,
                out SafeLocalMemHandle pSecurityDescriptor,
                IntPtr SecurityDescriptorSize);

            [DllImport("kernel32.dll")]
            private static extern IntPtr LocalFree(IntPtr hMem);

            protected override bool ReleaseHandle() => SafeLocalMemHandle.LocalFree(this.handle) == IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class SECURITY_ATTRIBUTES
        {
            public int nLength = 12;
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class STARTUPINFO
        {
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
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
            public IntPtr lpReserved2 = IntPtr.Zero;
            public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);

            public STARTUPINFO() => this.cb = Marshal.SizeOf((object)this);

            public void Dispose()
            {
                if (this.hStdInput != null && !this.hStdInput.IsInvalid)
                {
                    this.hStdInput.Close();
                    this.hStdInput = (SafeFileHandle)null;
                }

                if (this.hStdOutput != null && !this.hStdOutput.IsInvalid)
                {
                    this.hStdOutput.Close();
                    this.hStdOutput = (SafeFileHandle)null;
                }

                if (this.hStdError == null || this.hStdError.IsInvalid)
                    return;
                this.hStdError.Close();
                this.hStdError = (SafeFileHandle)null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        private static extern bool CreateProcess(
            [MarshalAs(UnmanagedType.LPTStr)] string lpApplicationName,
            StringBuilder lpCommandLine,
            SECURITY_ATTRIBUTES lpProcessAttributes,
            SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory,
            STARTUPINFO lpStartupInfo,
            PROCESS_INFORMATION lpProcessInformation);

        private static StringBuilder BuildCommandLine(string startInfoFileName, string startInfoArguments)
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

        public static bool Start(ProcessStartInfo processStartInfo)
        {
            var stringBuilder = BuildCommandLine(processStartInfo.FileName, processStartInfo.Arguments);

            var lpStartupInfo = new STARTUPINFO();
            var lpProcessInformation = new PROCESS_INFORMATION();

            var flag = 0;
            if (processStartInfo.CreateNoWindow)
            {
                flag |= 134217728;
            }

            var lpEnv = (IntPtr)0;

            var lpCurrentDirectory = processStartInfo.WorkingDirectory;
            if (lpCurrentDirectory == string.Empty)
            {
                lpCurrentDirectory = Environment.CurrentDirectory;
            }

            var process = CreateProcess((string)null, stringBuilder, (SECURITY_ATTRIBUTES)null,
                (SECURITY_ATTRIBUTES)null, true, flag, lpEnv, lpCurrentDirectory, lpStartupInfo, lpProcessInformation);

            return process;
        }
    }
}