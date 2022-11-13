using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using MessagePipe.Common;

namespace MessagePipe.Client.Managed
{
    public static class ManagedClient
    {
        public static void SafeMain(string[] args)
        {
            var guid = Guid.NewGuid();

            string inPipeName = Constants.DefaultS2C + guid;
            string outPipeName = Constants.DefaultC2S + guid;
            
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
                // CreateNoWindow = true,
                // UseShellExecute = false,
                Arguments = arguments
            };

            var serverProcess = Process.Start(processStartInfo);
            
            var isRunning = true;

            // receiver
            Task.Run(() =>
            {
                var stream = new NamedPipeClientStream(".", inPipeName, PipeDirection.In);
                stream.Connect();

                while (isRunning)
                {
                    try
                    {
                        var packet = Packet.Get(stream);
                        packet.DoSomething("CLIENT");
                    }
                    catch (Exception e)
                    {
                        isRunning = false;
                        Console.WriteLine(e);
                    }
                }
            });

            // sender
            Task.Run(() =>
            {
                var stream = new NamedPipeClientStream(".", outPipeName, PipeDirection.Out);
                stream.Connect();

                Thread.Sleep(1000);

                while (isRunning)
                {
                    try
                    {
                        var packet = Packet.Create(new Random().Next(1000, 9999));
                        packet.Send(stream);
                    }
                    catch (Exception e)
                    {
                        isRunning = false;
                        Console.WriteLine(e);
                    }

                    Thread.Sleep(2000);
                }
            });

            while (isRunning)
            {
                Thread.Sleep(1);
            }
        }
    }
}