using System;
using System.Net.Sockets;
using System.Threading;
using ServerData;

namespace Server
{
    public class ClientData
    {
        public Socket ClientSocket { get; }
        public Thread ClientThread { get; }
        public Guid Id { get; set; }

        public ClientData()
        {
            Id = Guid.NewGuid();
            ClientThread = new Thread(Server.DataIn);
            ClientThread.Start(ClientSocket);
            // SendRegistrationPacket();
        }

        public ClientData(Socket clientSocket)
        {
            ClientSocket = clientSocket;
            Id = Guid.NewGuid();
            ClientThread = new Thread(Server.DataIn);
            ClientThread.Start(ClientSocket);
            // SendRegistrationPacket();
        }

        private void SendRegistrationPacket()
        {
            var p = new Packet(PacketType.Registration, Id, null, null);
            ClientSocket.Send(p.ToBytes());
        }
    }
}