using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProjectNeonServer
{
    static class ProjectNeonServer
    {
        static Dictionary<string, Room> allRooms = new Dictionary<string, Room>();
        static Random rand = new Random();
        static byte[] recieveBuffer = new byte[512];
        static int rec = 0;
        static byte[] sendBuffer = new byte[1024];
        static Socket server;

        static void CreateNewRoom(Player creator)
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
            while (allRooms.ContainsKey(newCode));

            Room room = new Room(creator);
            allRooms.Add(newCode, room);

            try
            {
                sendBuffer = Encoding.ASCII.GetBytes("1$" + newCode);
                creator.socket.Send(sendBuffer);
            }
            catch (SocketException e)
            {
                if(e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
            }
        }

        static bool JoinRoom(string code, Player joiningPlayer)
        {
            if(allRooms.ContainsKey(code))
            {
                allRooms[code].connectedPlayers.Add(joiningPlayer);

                try
                {
                    sendBuffer = Encoding.ASCII.GetBytes("1$" + code);
                    joiningPlayer.socket.Send(sendBuffer);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
                }

                return true;
            }

            return false;
        }

        static void LeaveRoom(string code, Player leavingPlayer)
        {
            if(allRooms.ContainsKey(code))
            {
                allRooms[code].connectedPlayers.Remove(leavingPlayer);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ip = null;

                for (int i = 0; i < hostInfo.AddressList.Length; i++)
                {
                    //check for IPv4 address
                    if (hostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                        ip = hostInfo.AddressList[i];
                }
                IPEndPoint localEP = new IPEndPoint(ip, 11111);

                Console.WriteLine("ip address: " + ip.ToString());

                server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                server.Blocking = false;

                server.Bind(localEP);
                server.Listen(10);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                Thread.Sleep(50);

                //accept new clients
                try
                {
                    server.Listen(10);
                    Socket newConnection = server.Accept();
                    IPEndPoint newIPEndPoint = (IPEndPoint)newConnection.RemoteEndPoint;
                    EndPoint endPoint = (EndPoint)newIPEndPoint;
                    Console.WriteLine("connected: " + newIPEndPoint.Address.ToString());
                    rec = newConnection.Receive(recieveBuffer);
                    string data = Encoding.ASCII.GetString(recieveBuffer, 0, rec);
                    Console.Write(data);
                    string[] splitData = data.Split('$');

                    IPAddress newIp = IPAddress.Parse(splitData[1]);

                    Player newPlayer = new Player(newIp, splitData[2], newConnection, newIPEndPoint, endPoint);
                    if(int.Parse(splitData[0]) == 0)
                    {
                        CreateNewRoom(newPlayer);
                    }
                    else
                    {
                        JoinRoom(splitData[3], newPlayer);
                    }

                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
                }

                //send to existing clients
                if(stopwatch.ElapsedMilliseconds >= 1000)
                {
                    //Console.WriteLine("Elapsed: " + stopwatch.ElapsedMilliseconds);
   
                    stopwatch.Restart();
                    try
                    {
                        foreach(var roomdata in allRooms)
                        {
                            Room room = roomdata.Value;
                            Console.WriteLine(room.connectedPlayers.Count);

                            List<int> toRemove = new List<int>();
                            for(int i = 0; i < room.connectedPlayers.Count; i++)
                            {
                                if(!room.connectedPlayers[i].socket.Connected)
                                {
                                    toRemove.Add(i);
                                }
                            }

                            foreach (int index in toRemove) LeaveRoom(roomdata.Key, room.connectedPlayers[index]);

                            string sendData = "0";
                            for(int i = 0; i < room.connectedPlayers.Count; i++)
                            {
                                sendData += "$" + room.connectedPlayers[i].name;
                            }

                            sendBuffer = Encoding.ASCII.GetBytes(sendData);

                            for (int i = 0; i < room.connectedPlayers.Count; i++)
                            {
                                room.connectedPlayers[i].socket.Send(sendBuffer);
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
                    }
                }

                foreach(var roomdata in allRooms)
                {
                    Room room = roomdata.Value;
                    for(int i = 0; i < room.connectedPlayers.Count; i++)
                    {
                        try
                        {
                            Player player = room.connectedPlayers[i];
                            int rec = player.socket.Receive(recieveBuffer);
                            if(rec > 0)
                            {
                                string data = Encoding.ASCII.GetString(recieveBuffer, 0, rec);
                                for(int j = 0; j < room.connectedPlayers.Count; j++)
                                {
                                    if (j == i) continue;

                                    sendBuffer = Encoding.ASCII.GetBytes(data);
                                    room.connectedPlayers[j].socket.Send(sendBuffer);
                                }
                            }
                        }
                        catch (SocketException e)
                        {
                            if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
                        }
                    }
                }

            }
        }
    }

    class Room
    {
        public List<Player> connectedPlayers = new List<Player>();

        public Room(Player firstPlayer)
        {
            connectedPlayers.Add(firstPlayer);
        }
    }

    class Player
    {
        public IPAddress ip;
        public IPEndPoint clientEndPoint;
        public EndPoint remoteEndPoint;
        public Socket socket;
        public string name;

        public Player(IPAddress ip, string name, Socket socket, IPEndPoint clientEndPoint, EndPoint remoteEndPoint)
        {
            this.ip = ip;
            this.name = name;
            this.socket = socket;
            this.clientEndPoint = clientEndPoint;
            this.remoteEndPoint = remoteEndPoint;
        }
    }
}