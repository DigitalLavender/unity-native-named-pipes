using System;
using System.IO;
using System.IO.Pipes;

namespace MessagePipe
{
    public class MessagePipeClientStream : System.IO.Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position
        {
            get => 0;
            set { }
        }

        private MessagePipeClient _client;
        private readonly string _pipeName;
        private bool _isDisposed;
        
        public bool IsConnected => _client?.IsConnected() ?? false;

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_client.IsConnected())
            {
                throw new Exception("Pipe is not connected");
            }

            var received = _client.Receive(buffer, offset, count);

            if (received < 1)
            {
                return 0;
            }
            
            return received;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_client.IsConnected()) throw new Exception("Pipe is not connected");

            _client.Send(buffer, offset, count);
        }
        
        public MessagePipeClientStream(string server, string pipeName, PipeDirection direction)
        {
            _pipeName = $@"\\{server}\pipe\{pipeName}";
            _client = new MessagePipeClient(_pipeName, direction);
            
            PipeLogger.Verbose("Created new NamedPipeClientStream '{0}' => '{1}'", pipeName, _pipeName);
        }
        
        ~MessagePipeClientStream()
        {
            Dispose(false);
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_isDisposed)
            {
                Disconnect();
                
                _client.Dispose();
                _client = null;
                _isDisposed = true;
            }
        }
        
        public override int ReadByte()
        {
            byte[] buffer = new byte[1];
            return this.Read(buffer, 0, 1) == 0 ? -1 : (int)buffer[0];
        }

        public override void WriteByte(byte value)
        {
            this.Write(new byte[1], 0, 1);
        }

        public void Connect()
        {
            _client.Connect();
        }

        public void Disconnect()
        {
            _client?.Disconnect();
        }
    }
}