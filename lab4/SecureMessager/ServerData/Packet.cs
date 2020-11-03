using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Security;

namespace ServerData
{
    [Serializable]
    public class Packet
    {
        public List<int> Data;
        public int PacketInt;
        public bool PacketBool;
        public Guid SenderId;
        public Guid? ReceiverId;
        public PacketType Type;
        public string Name;
        public long[] PublicKey;

        public Packet(PacketType type, Guid senderId, Guid? receiverId, string name)
        {
            Data = new List<int>();
            SenderId = senderId;
            ReceiverId = receiverId;
            Type = type;
            Name = name;
            PublicKey = new long[2];
        }

        public Packet(byte[] packetBytes)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream(packetBytes);
            var p = bf.Deserialize(ms) as Packet;
            ms.Close();
            Data = p.Data;
            PacketInt = p.PacketInt;
            PacketBool = p.PacketBool;
            SenderId = p.SenderId;
            ReceiverId = p.ReceiverId;
            Name = p.Name;
            Type = p.Type;
            PublicKey = p.PublicKey;
        }

        public byte[] ToBytes()
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();

            bf.Serialize(ms, this);
            var bytes = ms.ToArray();
            ms.Close();

            return bytes;
        }

        public static string GetIpAddress()
        {
            var ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var ip in ips)
            {
                if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            
            return "127.0.0.1";
        }
    }
}