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
        static Socket TcpServer;
        static Socket UDPServer;
        static EndPoint remoteClient;

        static List<Player> allplayers = new List<Player>();
        static List<Room> allRooms = new List<Room>();

        static void Main(string[] args)
        {
            TcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = ServerHelperFunctions.FindIP4V(false);
            TcpServer.Bind(new IPEndPoint(ip, 11111));
            TcpServer.Listen(10);

            //Async IO
            TcpServer.BeginAccept(new AsyncCallback(InitalConnectCallback), null);
        }

        private static void InitalConnectCallback(IAsyncResult result)
        {
            Socket newClient = TcpServer.EndAccept(result);
            newClient.SetKeepAliveValues(1500, 500); //keep it alive for 1500ms and give it 500ms to reconnect if it mommentarily "disconnects"

            //make a new player out of the socket
            allplayers.Add(new Player(newClient));

            TcpServer.BeginAccept(new AsyncCallback(InitalConnectCallback), null) ;
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
    }
}
