using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace NeonCityRumbleAsyncServer
{
    public class Player
    {
        public string name;
        public Guid id;

        public Room connectedRoom;

        byte[] recBuffer = new byte[512];
        string inMsg;

        public Socket TcpSocket;
        public List<byte> TcpSendBuffer = new List<byte>();

        public EndPoint udpEndPoint = null;
        public List<byte> UdpSendBuffer = new List<byte>();

        public Stopwatch stopwatch;

        public Player(Socket connectingSocket)
        {
            try
            {
                //connect the tcp client
                TcpSocket = connectingSocket;

                //begin recieving from it
                TcpSocket.BeginReceive(recBuffer, 0, recBuffer.Length, 0, new AsyncCallback(JoinOrCreateRoomCallback), TcpSocket);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        ~Player()
        {
            try
            {
                udpEndPoint = null; 

                TcpSocket.Shutdown(SocketShutdown.Both);
                TcpSocket.Close();
            }
            catch (SocketException se)
            {
                if(se.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void JoinOrCreateRoomCallback(IAsyncResult result)
        {
            try
            {
                //process connecting to the correct room
                Socket client = (Socket)result.AsyncState;
                int rec = client.EndReceive(result);
                if(rec > 0)
                {
                    byte[] data = new byte[rec];
                    Array.Copy(recBuffer, data, rec);

                    inMsg = Encoding.ASCII.GetString(data);
                    string[] splitData = inMsg.Split('$');

                    //get the name
                    name = splitData[1];

                    //if the player claims not to have a guid, create one for them
                    if (splitData[2] == "0")
                    {
                        //generate a new guid 
                        id = Guid.NewGuid();
                        //handle telling the player it connected wrong
                        string reply = "9$" + id.ToString();
                        byte[] replyBuffer = Encoding.ASCII.GetBytes(reply);
                        replyBuffer = ServerHelperFunctions.AddLenghtToFront(replyBuffer);

                        TcpSocket.BeginSend(replyBuffer, 0, replyBuffer.Length, 0, new AsyncCallback(ReplySendCallBack), TcpSocket);
                    }
                    //otherwise just read the one they already have
                    else
                    {
                        id = Guid.Parse(splitData[3]);
                    }

                    if (splitData[0] == "0")
                    {
                        //create a new room
                        Room newRoom = NeonCityRumbleServer.CreateNewRoom();
                        newRoom.JoinRoom(this);
                    }
                    else if (splitData[0] == "1")
                    {
                        //join an existing room
                        string codeToJoin = splitData[4];

                        Room joiningRoom;
                        NeonCityRumbleServer.FindRoomByCode(codeToJoin, out joiningRoom);
                        if (joiningRoom != null)
                        {
                            joiningRoom.JoinRoom(this);
                        }
                        else
                        {
                            //handle telling the player it connected wrong
                            string reply = "-1";
                            byte[] replyBuffer = Encoding.ASCII.GetBytes(reply);
                            replyBuffer = ServerHelperFunctions.AddLenghtToFront(replyBuffer);

                            TcpSocket.BeginSend(replyBuffer, 0, replyBuffer.Length, 0, new AsyncCallback(ReplySendCallBack), TcpSocket);

                            //return since we didn't connect to the room properly, probably also want to handle removing the player from the list in the core server I think but we'll see
                            return;
                        }
                    }

                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    //update all of the connected players
                    connectedRoom.UpdateAllPlayers();
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        private void ReplySendCallBack(IAsyncResult result)
        {
            try
            {
                Socket client = (Socket)result.AsyncState;
                client.EndSend(result);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        public void RecieveUDPMessage(EndPoint ep, byte[] data)
        {
            udpEndPoint = ep;
            connectedRoom.UdpMessageRecieved(this, data);
        }
    }
}