using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using MessagePipe.Common;

namespace MessagePipe.Game
{
    public class MessageClient : IDisposable
    {
        private MessagePipeClientStream _receiveStream;
        private MessagePipeClientStream _sendStream;
        
        private CancellationTokenSource _receiveCancellationTokenSource;
        private Task _receiveTask;

        public event Action<Packet> OnMessageReceived = packet => { };
        
        public MessageClient(string inPipeName, string outPipeName)
        {
            _receiveStream = new MessagePipeClientStream(".", inPipeName, PipeDirection.In);
            _sendStream = new MessagePipeClientStream(".", outPipeName, PipeDirection.Out);

            _receiveCancellationTokenSource = new CancellationTokenSource();
        }

        public void Connect(int retryCount = 30, int intervalInMs = 100)
        {
            var isConnected = false;
            for (var i = 0; i < retryCount; i++)
            {
                try
                {
                    _receiveStream.Connect();
                    _sendStream.Connect();

                    _receiveTask = Task.Run(Receive, _receiveCancellationTokenSource.Token);

                    isConnected = true;
                    break;
                }
                catch (Exception e)
                {
                    if (e is PipeServerNotReadyException)
                    {
                        Thread.Sleep(intervalInMs);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (isConnected)
            {
                Console.WriteLine("Connected!");
            }
            else
            {
                _receiveCancellationTokenSource?.Cancel();
                
                _receiveStream?.Dispose();
                _sendStream?.Dispose();

                _receiveCancellationTokenSource = null;
                _receiveStream = null;
                _sendStream = null;

                throw new Exception("Connection failed.");
            }
        }

        public void WaitForExit()
        {
            while (_receiveTask != null && !_receiveTask.IsCompleted)
            {
                Thread.Sleep(1);
            }
        }
        
        private void Receive()
        {
            while (!_receiveStream.IsConnected)
            {
                Thread.Sleep(1);
            }

            while (_receiveStream != null)
            {
                var packet = Packet.Get(_receiveStream);
                OnMessageReceived(packet);
            }
        }

        public void Send(Packet message)
        {
            if (_sendStream.IsConnected)
            {
                message.Send(_sendStream);
            }
            else
            {
                
            }
        }

        public void Dispose()
        {
            _receiveCancellationTokenSource?.Cancel();
            _receiveCancellationTokenSource?.Dispose();

            _receiveStream?.Dispose();
            _sendStream?.Dispose();
        }
    }
}