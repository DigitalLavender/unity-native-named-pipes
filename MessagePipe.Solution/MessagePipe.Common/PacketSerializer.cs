using System;
using System.IO;

namespace MessagePipe.Common
{
    public static class PacketSerializer
    {
        private const int HeaderSize = sizeof(int);
        private const int PacketIdSize = sizeof(int);
        
        public static Packet Deserialize(Stream stream)
        {
            // read header
            var headerBuffer = new byte[HeaderSize];
            ReadBytes(stream, headerBuffer);
            var contentSize = BitConverter.ToInt32(headerBuffer, 0);
            
            // read id
            var idBuffer = new byte[PacketIdSize];
            ReadBytes(stream, idBuffer);
            var id = BitConverter.ToInt32(idBuffer, 0);
            
            // read body
            var bodyBuffer = new byte[contentSize - PacketIdSize];
            ReadBytes(stream, bodyBuffer);

            var packetType = TypeContracts.GetPacketType(id);
            // var message = (Message)Activator.CreateInstance(messageType);

            using (var ms = new MemoryStream(bodyBuffer))
            {
                var packet = TypeContracts.CreateObject(packetType, ms);
                return packet;
            }
        }
        
        public static void Serialize(Packet packet, Stream stream)
        {
            // write header
            // write size
            // write body
        }

        private static void ReadBytes(Stream stream, byte[] buffer)
        {
            int readLen;
            for (var offset = 0; offset < buffer.Length; offset += readLen)
            {
                readLen = stream.Read(buffer, offset, buffer.Length - offset);
            }
        }
    }
}