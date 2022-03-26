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
        static Socket server, udpSend, udpRecieve;

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
                sendBuffer = Encoding.ASCII.GetBytes("1$" + newCode + "$" + creator.id.ToString());
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
                    sendBuffer = Encoding.ASCII.GetBytes("1$" + code + "$" + joiningPlayer.id.ToString());
                    joiningPlayer.socket.Send(sendBuffer);

                    string sendData = "0";
                    for (int i = 0; i < allRooms[code].connectedPlayers.Count; i++)
                    {
                        sendData += "$" + allRooms[code].connectedPlayers[i].name + "$" + allRooms[code].connectedPlayers[i].id.ToString();
                    }

                    sendBuffer = Encoding.ASCII.GetBytes(sendData);
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

        //check if a socket is connected
        static bool IsConnected(Socket s)
        {
            //taken from here https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c/2661876#2661876
            try
            {
                return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
            } catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock) return true;
                else return false;
            }
        }

        //control how long a socket can stay alive
        static void SetKeepAliveValues(Socket s, int keepAliveTime, int keepAliveInterval)
        {
            int size = sizeof(uint);
            byte[] values = new byte[size * 3];

            BitConverter.GetBytes((uint)(1)).CopyTo(values, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(values, size);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(values, size * 2);

            byte[] outvalues = BitConverter.GetBytes(0);

            s.IOControl(IOControlCode.KeepAliveValues, values, outvalues);
        }

        static void Main(string[] args)
        {
            //setup sockets
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

                IPEndPoint udpLocalEP = new IPEndPoint(ip, 11112);
                udpRecieve = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                udpRecieve.Bind(udpLocalEP);
                udpRecieve.Blocking = false;

                udpSend = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
            }

            //start a stopwatch
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
                    //setting it up to handle ungraceful disconnecting, checks every second, and waits half a second before trying again if it failed
                    SetKeepAliveValues(newConnection, 1000, 500);
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

                //tcp update sent to all clients every 1000 seconds
                if(stopwatch.ElapsedMilliseconds >= 1000)
                {
                    //Console.WriteLine("Elapsed: " + stopwatch.ElapsedMilliseconds);
   
                    stopwatch.Restart();
                    try
                    {
                        List<string> toDelete = new List<string>();
                        foreach(var roomdata in allRooms)
                        {
                            Room room = roomdata.Value;
                            Console.WriteLine(room.connectedPlayers.Count);

                            if(room.connectedPlayers.Count == 0)
                            {
                                toDelete.Add(roomdata.Key);
                                continue;
                            }

                            List<int> toRemove = new List<int>();
                            for(int i = 0; i < room.connectedPlayers.Count; i++)
                            {
                                if(!IsConnected(room.connectedPlayers[i].socket))
                                {
                                    toRemove.Add(i);
                                }
                            }

                            foreach (int index in toRemove) LeaveRoom(roomdata.Key, room.connectedPlayers[index]);

                            string sendData = "0";
                            for(int i = 0; i < room.connectedPlayers.Count; i++)
                            {
                                sendData += "$" + room.connectedPlayers[i].name + "$" + room.connectedPlayers[i].id.ToString();
                            }

                            sendBuffer = Encoding.ASCII.GetBytes(sendData);

                            for (int i = 0; i < room.connectedPlayers.Count; i++)
                            {
                                room.connectedPlayers[i].socket.Send(sendBuffer);
                            }
                        }

                        //delete any empty rooms
                        foreach (var room in toDelete) allRooms.Remove(room);
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
                    }
                }


                //listening for data from each of the clients and forwarding it to other clients
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
                                for(int j = 0; j < room.connectedPlayers.Count; j++)
                                {
                                    if (j == i) continue;

                                    Buffer.BlockCopy(recieveBuffer, 0, sendBuffer, 0, rec);
                                    room.connectedPlayers[j].socket.Send(sendBuffer);
                                }
                            }

                            int udpRec = udpRecieve.ReceiveFrom(recieveBuffer, ref player.udpEndpoint);
                            if(udpRec > 0)
                            {
                                for (int j = 0; j < room.connectedPlayers.Count; j++)
                                {
                                    if (j == i) continue;

                                    Buffer.BlockCopy(recieveBuffer, 0, sendBuffer, 0, udpRec);
                                    udpSend.SendTo(sendBuffer, room.connectedPlayers[j].udpAltEndpoint);
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
        public Guid id;

        public EndPoint udpEndpoint;
        public EndPoint udpAltEndpoint;

        public Player(IPAddress ip, string name, Socket socket, IPEndPoint clientEndPoint, EndPoint remoteEndPoint)
        {
            this.ip = ip;
            this.name = name;
            this.socket = socket;
            this.clientEndPoint = clientEndPoint;
            this.remoteEndPoint = remoteEndPoint;
            this.id = Guid.NewGuid();

            IPEndPoint tempIpEndPoint = new IPEndPoint(ip, 0);
            udpEndpoint = (EndPoint)tempIpEndPoint;

            IPEndPoint tempAltIPEndPoint = new IPEndPoint(ip, 11112);
            udpAltEndpoint = (EndPoint)tempAltIPEndPoint;
        }
    }
}