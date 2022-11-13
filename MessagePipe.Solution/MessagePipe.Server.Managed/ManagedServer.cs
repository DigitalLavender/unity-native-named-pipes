using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using MessagePipe.Common;

namespace MessagePipe.Server.Managed
{
    public static class ManagedServer
    {
        public static void SafeMain(string[] args)
        {
            Setting.Version();

            var arguments = new PipeArgument.Parser(args);

            var outPipeName = arguments.GetString(Constants.PipeNameKeyS2C);
            var inPipeName = arguments.GetString(Constants.PipeNameKeyC2S);
            var ownerPid = arguments.GetInt(Constants.OwnerProcessId);


            // escapes if owner process is not alive.
            {
                if (string.IsNullOrEmpty(outPipeName) || string.IsNullOrEmpty(inPipeName))
                {
                    Console.WriteLine("pipe name is empty.");
                    Console.ReadKey();
                    return;
                }
                
                if (ownerPid < 1)
                {
                    Console.WriteLine("owner process id is invalid.");
                    Console.ReadKey();
                    return;
                }
            }

            Console.Title = $"PipeServer [{ownerPid:00000}]";
            Console.WriteLine("[In] " + inPipeName);
            Console.WriteLine("[Out] " + outPipeName);
            
            var isRunning = true;

            // receiver
            Task.Run(() =>
            {
                var stream = new NamedPipeServerStream(inPipeName, PipeDirection.In);
                stream.WaitForConnection();

                while (stream.IsConnected)
                {
                    try
                    {
                        var packet = Packet.Get(stream);
                        packet.DoSomething("SERVER");
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
                var stream = new NamedPipeServerStream(outPipeName, PipeDirection.Out);
                stream.WaitForConnection();

                while (isRunning)
                {
                    try
                    {
                        var message = Packet.Create(new Random().Next(10, 99));
                        message.Send(stream);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Sent Message!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    catch (Exception e)
                    {
                        isRunning = false;
                        Console.WriteLine(e);
                    }

                    Thread.Sleep(2000);
                }
            });

            // 부모 프로세스 감시
            Task.Run(() =>
            {
                while (isRunning)
                {
                    var process = Process.GetProcessById(ownerPid);
                    if (process.HasExited)
                    {
                        isRunning = false;

                        Console.WriteLine("OwnerProcess has exited.");
                    }
                    
                    Thread.Sleep(1000);
                }
            });

            while (isRunning)
            {
                Thread.Sleep(1);
            }
        }
    }
}