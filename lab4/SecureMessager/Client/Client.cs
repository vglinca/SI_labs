using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Security;
using ServerData;
// ReSharper disable MemberCanBePrivate.Global

namespace Client
{
    internal static class Client
    {
        public const int Port = 4242;
        public static string Name;
        public static Guid Id;
        public static Socket MasterSocket;
        public static readonly Dictionary<string, Guid> Recipients = new Dictionary<string, Guid>();
        public static readonly Dictionary<Guid, Pair> PublicKeys = new Dictionary<Guid, Pair>();
        public static RSAUtils RsaCrypto;

        private static void Main(string[] args)
        {
            Console.Write("Enter your name: ");
            Name = Console.ReadLine()?.Trim();
            Id = Guid.NewGuid();
            
            var connected = false;

            while (!connected)
            {
                Console.Clear();
                Console.Write($"Enter host ip: ");
                var ip = Console.ReadLine()?.Trim();

                MasterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ipEndpoint = new IPEndPoint(IPAddress.Parse(ip), Port);
                
                RsaCrypto = new RSAUtils();
                RsaCrypto.KeyGen();

                Console.WriteLine($"Keys: public: ({RsaCrypto.PublicKey.X} {RsaCrypto.PublicKey.N}) secret: ({RsaCrypto.SecretKey.X} {RsaCrypto.SecretKey.N})");

                try
                {
                    MasterSocket.Connect(ipEndpoint);
                    connected = true;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Could not connect to host.");
                    connected = false;
                    Thread.Sleep(2000);
                }
            }

            var th = new Thread(DataIn);
            th.Start();

            while (true)
            {
                Console.Write($"{DateTime.Now}::>");
                var input = Console.ReadLine()?.Trim();
                
                Console.Write("Send message to: ");
                var recipient = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrWhiteSpace(recipient)) throw new ArgumentNullException(nameof(recipient));

                Recipients.TryGetValue(recipient, out var recId);
                PublicKeys.TryGetValue(recId, out var recipientPublicKey);
                
                if (recipientPublicKey == null)
                {
                    var p = new Packet(PacketType.KeyExchange, Id, recId, Name)
                    {
                        PublicKey = {[0] = RsaCrypto.PublicKey.X, [1] = RsaCrypto.PublicKey.N}
                    };

                    Console.WriteLine($"{Name}: Sending public key ({RsaCrypto.PublicKey.X} {RsaCrypto.PublicKey.N}) to {recId}");
                    
                    MasterSocket.Send(p.ToBytes());
                }
                else
                {
                    var data = RsaCrypto.Encrypt($"[{Name}]: {input}", recipientPublicKey.X, recipientPublicKey.N);
                    var packet = new Packet(PacketType.Chat, Id, recId, Name) {Data = data};
                    MasterSocket.Send(packet.ToBytes());
                }
            }
        }
        
        public static void DataIn()
        {
            try
            {
                while (true)
                {
                    var buffer = new byte[MasterSocket.SendBufferSize];
                    var readBytes = MasterSocket.Receive(buffer);
                    if (readBytes > 0)
                    {
                        ManageData(new Packet(buffer));
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server has disconnected.");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        private static void ManageData(Packet p)
        {
            switch (p.Type)
            {
                case PacketType.Registration:
                    Console.WriteLine($"Received a packet for registration. Responding...");
                    Console.WriteLine();
                    break;
                case PacketType.Chat:
                    var c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    var message = RsaCrypto.Decrypt(p.Data);
                    Console.WriteLine($"{message}");
                    Console.ForegroundColor = c;
                    break;
                case PacketType.Broadcast:
                    c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{p.SenderId} with name {p.Name} has connected.");
                    Console.ForegroundColor = c;
                    Recipients.Add(p.Name, p.SenderId);
                    var packet = new Packet(PacketType.GetParticipants, Id, p.SenderId, Name);
                    MasterSocket.Send(packet.ToBytes());
                    break;
                case PacketType.ClientId:
                    var idAsString = p.ReceiverId.ToString();
                    Guid.TryParse(idAsString, out Id);
                    c = Console.ForegroundColor; 
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Your id is {Id}");
                    Console.ForegroundColor = c;
                    packet = new Packet(PacketType.Broadcast, Id, null, Name);
                    MasterSocket.Send(packet.ToBytes());
                    break;
                case PacketType.GetParticipants:
                    Recipients.TryAdd(p.Name, p.SenderId);
                    break;
                case PacketType.KeyExchange:
                    //Console.WriteLine($"Receive public key: ({p.PublicKey[0]} {p.PublicKey[1]})");
                    PublicKeys.TryAdd(p.SenderId, new Pair(p.PublicKey[0], p.PublicKey[1]));
                    packet = new Packet(PacketType.KeyExchange, Id, p.SenderId, Name);
                    packet.PublicKey[0] = RsaCrypto.PublicKey.X;
                    packet.PublicKey[1] = RsaCrypto.PublicKey.N;
                    MasterSocket.Send(packet.ToBytes());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
