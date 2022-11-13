using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace MessagePipe
{
    public sealed class MessagePipeClient : IDisposable
    {
        private static class NativeImport
        {
            [DllImport("MessagePipe.Native.dll", EntryPoint = "pipe_client_create", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Create(int pipeDirection);
            
            [DllImport("MessagePipe.Native.dll", EntryPoint = "pipe_client_remove", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Remove(IntPtr pipeClient);
            
            [DllImport("MessagePipe.Native.dll", EntryPoint = "pipe_client_open", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Connect(IntPtr pipeClient, [MarshalAs(UnmanagedType.LPWStr)] string name);
            
            [DllImport("MessagePipe.Native.dll", EntryPoint = "pipe_client_close", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Disconnect(IntPtr pipeClient);
            
            [DllImport("MessagePipe.Native.dll", EntryPoint = "pipe_client_write", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Send(IntPtr pipeClient, IntPtr data, int length);
            
            [DllImport("MessagePipe.Native.dll", EntryPoint = "pipe_client_read", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Receive(IntPtr pipeClient, IntPtr data, int length);
            
            [DllImport("MessagePipe.Native.dll", EntryPoint = "pipe_client_is_connected", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool IsConnected(IntPtr pipeClient);
        }
        
        private IntPtr _instance;
        private string _pipeName;
        
        private PipeDirection _direction;

        public MessagePipeClient(string pipeName, PipeDirection direction)
        {
            _pipeName = pipeName;
            _direction = direction;
            
            _instance = NativeImport.Create((int)_direction);
        }

        public void Dispose()
        {
            NativeImport.Remove(_instance);
            _instance = IntPtr.Zero;
        }

        public void Connect()
        {
            var error = NativeImport.Connect(_instance, _pipeName);
            if (error != 0)
            {
                if (error == 2)
                {
                    throw new PipeServerNotReadyException();
                }
                
                throw new Exception("Failed to connect to pipe: (" + error + ")");
            }
        }

        public bool IsConnected()
        {
            return NativeImport.IsConnected(_instance);
        }

        public void Disconnect()
        {
            NativeImport.Disconnect(_instance);
        }

        public int Receive(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length == 0) return 0;

            var size = Marshal.SizeOf(buffer[0]) * count;
            var pBuffer = Marshal.AllocHGlobal(size);
            
            try
            {
                var bytesRead = NativeImport.Receive(_instance, pBuffer, count);

                if (bytesRead <= 0)
                {
                    var error = bytesRead;
                    if (error < 0)
                    {
                        throw new Exception($"read failed: {error}");
                    }

                    return 0;
                }
                else
                {
                    Marshal.Copy(pBuffer, buffer, offset, bytesRead);
                    return bytesRead;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        public void Send(byte[] buffer, int offset, int count)
        {
            var size = Marshal.SizeOf(buffer[0]) * count;
            var pBuffer = Marshal.AllocHGlobal(size);
            
            try
            {
                Marshal.Copy(buffer, offset, pBuffer, count);

                var result = NativeImport.Send(_instance, pBuffer, count);
                if (result < 0)
                {
                    throw new Exception("write failed: "+ result);
                }                
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }
        }
    }
}