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
        public static string Name;
        public static Guid Id;
        public static Socket MasterSocket;
        public static Dictionary<string, Guid> Recepients = new Dictionary<string, Guid>();
        public static RSAUtils RsaCrypto;
        public static Pair[] Keys;
        public static List<int> Chipher = new List<int>();
        public static List<int> Decrypted = new List<int>();

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
                var ipEndpoint = new IPEndPoint(IPAddress.Parse(ip), 4242);
                
                RsaCrypto = new RSAUtils();
                Keys = RsaCrypto.KeyGen();

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

                
                
                Console.Write("Send this message to: ");
                var recipient = Console.ReadLine()?.Trim();
                Recepients.TryGetValue(recipient, out var recId);

                Console.WriteLine($"Recipient id id {recId}");
                var packet = new Packet(PacketType.Chat, Id, recId, Name);
                packet.Data.Add(Name);
                packet.Data.Add(input);
                MasterSocket.Send(packet.ToBytes());
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
                    Console.WriteLine($"{p.Data[0]} : {p.Data[1]}");
                    Console.ForegroundColor = c;
                    break;
                case PacketType.Broadcast:
                    c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{p.SenderId} with name {p.Name} has connected.");
                    Console.ForegroundColor = c;
                    Recepients.Add(p.Name, p.SenderId);
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
                    Recepients.TryAdd(p.Name, p.SenderId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
