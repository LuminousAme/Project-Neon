using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NeonCityRumbleAsyncServer
{
    public static class NeonCityRumbleServer
    {
        //static Dictionary<string, Room> allRooms = new Dictionary<string, Room>();
        static Random rand = new Random();

        static public IPAddress serverIP;

        static public int tcpPort = 11111;
        static Socket TcpServer;

        static public int udpPort = 11112;
        static Socket UdpServer;
        static EndPoint UdpRemoteEP;
        static byte[] UdpRecBuffer = new byte[1024];

        static List<Player> allplayers = new List<Player>();
        static List<Room> allRooms = new List<Room>();

        static void Main(string[] args)
        {
            try
            {
                Console.Title = "Neon City Rumble Server";

                //tcp setup
                TcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverIP = ServerHelperFunctions.FindIP4V(false);
                Console.WriteLine("Server IP: " + serverIP.ToString());
                TcpServer.Bind(new IPEndPoint(serverIP, tcpPort));
                TcpServer.Listen(10);

                //Async IO
                TcpServer.BeginAccept(new AsyncCallback(InitalConnectCallback), null);

                //udp setup
                UdpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                UdpServer.Bind(new IPEndPoint(IPAddress.Any, udpPort));
                //no need to listen because it's a udp port, we do want to do a recieve callback and stuff though

                //we do however we do need to process incoming udp data
                IPEndPoint udpEndpoint = new IPEndPoint(IPAddress.Any, 0);
                UdpRemoteEP = (EndPoint)udpEndpoint;

                UdpServer.BeginReceiveFrom(UdpRecBuffer, 0, UdpRecBuffer.Length, 0, ref UdpRemoteEP, new AsyncCallback(UdpRecieveCallBack), UdpServer);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }

            Console.ReadLine();
        }

        private static void InitalConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket newClient = TcpServer.EndAccept(result);
                newClient.SetKeepAliveValues(1500, 500); //keep it alive for 1500ms and give it 500ms to reconnect if it mommentarily "disconnects"

                //make a new player out of the socket
                allplayers.Add(new Player(newClient));

                TcpServer.BeginAccept(new AsyncCallback(InitalConnectCallback), null);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        public static Room CreateNewRoom()
        {
            string newCode = "";
            do
            {
                newCode = "";
                for (int i = 0; i < 4; i++)
                {
                    int val = rand.Next(0, 26);
                    char letter = Convert.ToChar(65 + val);
                    newCode += letter;
                }
            }
            while (allRooms.Any(r => r.RoomCode == newCode));

            Room room = new Room(newCode);
            allRooms.Add(room);
            return room;
        }

        public static void FindRoomByCode(string code, out Room desiredRoom)
        {
            if(allRooms.Exists(r => r.RoomCode == code))
            {
                desiredRoom = allRooms.Find(r => r.RoomCode == code);
            }
            else
            {
                desiredRoom = null;
            }
        }

        private static void UdpRecieveCallBack(IAsyncResult result)
        {
            try
            {
                int rec = UdpServer.EndReceiveFrom(result, ref UdpRemoteEP);

                if (rec > 0)
                {
                    byte[] data = new byte[rec];
                    Array.Copy(UdpRecBuffer, data, rec);

                    string recMsg = Encoding.ASCII.GetString(data);
                    string[] splitRecMsg = recMsg.Split('$');

                    //Console.WriteLine("Udp message recieved: " + recMsg);

                    Guid sendingPlayer;

                    if (Guid.TryParse(splitRecMsg[0], out sendingPlayer))
                    {
                        if (allplayers.Exists(p => p.id == sendingPlayer))
                        {
                            allplayers.Find(p => p.id == sendingPlayer).RecieveUDPMessage(UdpRemoteEP, data);
                        }
                    }
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }

            UdpServer.BeginReceiveFrom(UdpRecBuffer, 0, UdpRecBuffer.Length, 0, ref UdpRemoteEP, new AsyncCallback(UdpRecieveCallBack), UdpServer);
        }

        public static void UdpSendMessage(EndPoint targetEP, byte[] data)
        {
            try
            {
                UdpServer.BeginSendTo(data, 0, data.Length, 0, targetEP, new AsyncCallback(UdpSendCallBack), UdpServer);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        private static void UdpSendCallBack(IAsyncResult result)
        {
            try
            {
                UdpServer.EndSendTo(result);
            }
            catch (SocketException se)
            {
                if(se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        public static void RemovePlayer(Player player)
        {
            if(allplayers.Contains(player)) allplayers.Remove(player);
        }
    }
}
