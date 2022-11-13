using System;
using System.Diagnostics;
using System.IO;
using MessagePipe.Common;

namespace MessagePipe.Client.Unmanaged
{
    public static class UnmanagedClient
    {
        public static void SafeMain(string[] args)
        {
            var guid = Guid.NewGuid();

            var inPipeName = Constants.DefaultS2C + guid;
            var outPipeName = Constants.DefaultC2S + guid;
            
            var arguments = new PipeArgument.Builder()
                .ServerToClient(inPipeName)
                .ClientToServer(outPipeName)
                .Build();

            Console.WriteLine("[In  Pipe] " + inPipeName);
            Console.WriteLine("[Out Pipe] " + outPipeName);
            Console.WriteLine("arguments: " + arguments);

            var serverPath = Path.GetFullPath("MessagePipe.Server.Managed.exe");
            Console.WriteLine($"{File.Exists(serverPath)}: {serverPath}");

            var processStartInfo = new ProcessStartInfo(serverPath)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = arguments
            };
            
            var serverProcess = NativeProcess.Start(processStartInfo);

            var messageClient = new MessageClient(inPipeName, outPipeName);
            messageClient.Connect();
            messageClient.WaitForExit();
        }
    }
}