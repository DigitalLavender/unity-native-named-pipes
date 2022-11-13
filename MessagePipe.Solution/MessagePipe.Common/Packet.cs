using System;
using System.IO;

namespace MessagePipe.Common
{
    public class Packet
    {
        private static void ReadBytes(Stream stream, byte[] buffer)
        {
            int readLen;
            for (var offset = 0; offset < buffer.Length; offset += readLen)
            {
                readLen = stream.Read(buffer, offset, buffer.Length - offset);
            }
        }
        
        public static Packet Get(Stream stream)
        {
            const int len = sizeof(int);
            var buffer = new byte[len];
            
            ReadBytes(stream, buffer);
            var value = BitConverter.ToInt32(buffer, 0);
            
            return new Packet(value);
        }

        public static Packet Create(int ii)
        {
            return new Packet(ii);
        }
        
        public void Send(Stream stream)
        {
            const int len = sizeof(int);
            var bytes = BitConverter.GetBytes(_i);
            stream.Write(bytes, 0, len);
        }

        private Packet(int value)
        {
            this._i = value;
        }

        public override string ToString()
        {
            return $"Packet({_i:0000})";
        }

        private readonly int _i;

        public void DoSomething(string speaker = "")
        {
            if (!string.IsNullOrEmpty(speaker))
            {
                Console.Write($"[Speaker] {speaker} :: ");
            }
            
            Console.Write($"packet received! - {_i}\n");
        }
    }
}