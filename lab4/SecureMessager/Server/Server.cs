using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerData;

namespace Server
{
    internal static class Server
    {
        private static Socket _listener;
        private static List<ClientData> _clients;
        private static readonly Guid ServerId = Guid.NewGuid();

        private static void Main(string[] args)
        {
            Console.WriteLine($"Starting server on {Packet.GetIpAddress()}");

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clients = new List<ClientData>();

            var ipEndpoint = new IPEndPoint(IPAddress.Parse(Packet.GetIpAddress()), 4242);
            _listener.Bind(ipEndpoint);

            var listenThread = new Thread(ListenThread);
            listenThread.Start();
        }

        private static void ListenThread()
        {
            while(true)
            {
                _listener.Listen(0);
                var client = new ClientData(_listener.Accept());
                _clients.Add(client);
                var packet = new Packet(PacketType.ClientId, ServerId, client.Id, null);
                client.ClientSocket.Send(packet.ToBytes());
            }
        }

        public static void DataIn(object cSocket)
        {
            var clientSocket = cSocket as Socket;

            try
            {
                while(true)
                {
                    if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));
                    
                    var buffer = new byte[clientSocket.SendBufferSize];
                    var readBytes = clientSocket.Receive(buffer);

                    if(readBytes > 0)
                    {
                        ManageData(new Packet(buffer));
                    }
                }
            }
            catch(SocketException)
            {
                Console.WriteLine($"A client has disconnected.");
            }
        }

        private static void ManageData(Packet p)
        {
            switch (p.Type)
            {
                case PacketType.Registration:
                    break;
                case PacketType.Chat:
                    var cl = _clients.FirstOrDefault(c => c.Id == p.ReceiverId);
                    cl?.ClientSocket.Send(p.ToBytes());
                    break;
                case PacketType.Broadcast:
                    foreach (var client in _clients.Where(client => client.Id != p.SenderId))
                    {
                        client.ClientSocket.Send(p.ToBytes());
                    }
                    break;
                case PacketType.ClientId:
                    break;
                case PacketType.GetParticipants:
                    cl = _clients.FirstOrDefault(c => c.Id == p.ReceiverId);
                    cl?.ClientSocket.Send(p.ToBytes());
                    break;
                case PacketType.KeyExchange:
                    cl = _clients.FirstOrDefault(c => c.Id == p.ReceiverId);
                    cl?.ClientSocket.Send(p.ToBytes());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}