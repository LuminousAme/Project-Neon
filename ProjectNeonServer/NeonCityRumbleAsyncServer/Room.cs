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
    public class Room
    {
        public string RoomCode;
        List<Player> playersInThisRoom = new List<Player>();
        byte[] recBuffer = new byte[1024];

        List<Player> toRemove = new List<Player>();

        //create a mutex for the room
        private Mutex mutex;

        public Room(string code)
        {
            mutex = new Mutex();
            RoomCode = code;

            Thread sendThread = new Thread(new ThreadStart(SendLoop));
            sendThread.Name = RoomCode + "'s Send Thread";
            sendThread.Start();
        }

        ~Room()
        {
            mutex.Dispose();
        }

        public void JoinRoom(Player joiningPlayer)
        {
            try
            {
                playersInThisRoom.Add(joiningPlayer);
                joiningPlayer.connectedRoom = this;

                string replyMsg = "5$" + RoomCode;
                byte[] toSendReply = Encoding.ASCII.GetBytes(replyMsg);
                toSendReply = ServerHelperFunctions.AddLenghtToFront(toSendReply);

                joiningPlayer.TcpSocket.BeginSend(toSendReply, 0, toSendReply.Length, 0, new AsyncCallback(TcpSendCallBack), joiningPlayer.TcpSocket);

                joiningPlayer.TcpSocket.BeginReceive(recBuffer, 0, recBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), joiningPlayer.TcpSocket);
            }
            catch
            {
                //ignore any excpetions
            }
        }

        private void TcpRecieveCallBack(IAsyncResult result)
        {
            if (mutex.WaitOne())
            {
                try
                {
                    Socket client = (Socket)result.AsyncState;
                    int rec = client.EndReceive(result);
                    if (rec > 0)
                    {
                        byte[] data = new byte[rec];
                        Array.Copy(recBuffer, data, rec);

                        string recMsg = Encoding.ASCII.GetString(data);
                        string[] splitRecMsg = recMsg.Split('$');
                        //check if it's a disconnect message
                        if (splitRecMsg[2] == "-2")
                        {
                            //if it is, identify the player
                            Player disconnectingPlayer = playersInThisRoom.Find(p => p.TcpSocket == client);

                            if (disconnectingPlayer != null)
                            {
                                playersInThisRoom.Remove(disconnectingPlayer);
                                NeonCityRumbleServer.RemovePlayer(disconnectingPlayer);
                                UpdateAllPlayers();
                            }
                        }
                        //if it's not send the message to all of the other players
                        else
                        {
                            foreach (Player player in playersInThisRoom)
                            {
                                if (player.TcpSocket == client && splitRecMsg[2] != "8")
                                {
                                    if (splitRecMsg.Length > 4 && splitRecMsg[2] == "7") player.ready = (splitRecMsg[4] == "0") ? false : true;

                                    continue;
                                }

                                player.TcpSendBuffer.AddRange(data);
                            }
                            client.BeginReceive(recBuffer, 0, recBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), client);
                        }
                    }
                    else
                    {
                        client.BeginReceive(recBuffer, 0, recBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), client);
                    }
                }
                catch
                {
                    //ignore any excpetions
                }

                mutex.ReleaseMutex();
            }
        }

        private void TcpSendCallBack(IAsyncResult result)
        {
            Socket client = (Socket)result.AsyncState;
            client.EndSend(result);
        }

        public void UdpMessageRecieved(Player sendingPlayer, byte[] data)
        {
            if (mutex.WaitOne())
            {
                foreach (Player player in playersInThisRoom)
                {
                    if (player == sendingPlayer) continue;

                    player.UdpSendBuffer.AddRange(data);
                }

                mutex.ReleaseMutex();
            }
        }

        private void SendLoop()
        {
            while (true)
            {
                if (mutex.WaitOne())
                {
                    try
                    {
                        //check for disconnects
                        {
                            //if any of the players have disconnected add to the list of players to remove
                            List<Player> toRemove = new List<Player>();
                            for (int i = 0; i < playersInThisRoom.Count; i++)
                            {
                                if (!playersInThisRoom[i].TcpSocket.IsConnected())
                                {
                                    toRemove.Add(playersInThisRoom[i]);
                                }
                            }

                            //go over all of the players to remove, and remove them from both list
                            //when this scope exits there shouldn't be any remaining references to the player object
                            foreach (Player player in toRemove)
                            {
                                playersInThisRoom.Remove(player);
                                NeonCityRumbleServer.RemovePlayer(player);
                            }

                            if (toRemove.Count > 0)
                            {
                                UpdateAllPlayers();
                            }
                        }

                        //tcp
                        {
                            foreach (Player player in playersInThisRoom)
                            {
                                //if it's been a second since the last send, make sure to send a ping message
                                if (player.stopwatch.ElapsedMilliseconds >= 1000)
                                {
                                    string pingStr = "4";
                                    byte[] pingBuffer = Encoding.ASCII.GetBytes(pingStr);
                                    pingBuffer = ServerHelperFunctions.AddLenghtToFront(pingBuffer);
                                    player.TcpSendBuffer.AddRange(pingBuffer);
                                }

                                //if there's anything to send, then send it
                                if (player.TcpSendBuffer.Count > 0)
                                {
                                    player.TcpSocket.BeginSend(player.TcpSendBuffer.ToArray(), 0, player.TcpSendBuffer.Count, 0, new AsyncCallback(TcpSendCallBack), player.TcpSocket);
                                    player.TcpSendBuffer.Clear();
                                    //and restart the timer since the last time a message was sent
                                    player.stopwatch.Restart();
                                }
                            }
                        }

                        //udp
                        {
                            foreach (Player player in playersInThisRoom)
                            {
                                if (player.udpEndPoint != null && player.UdpSendBuffer.Count > 0)
                                {
                                    NeonCityRumbleServer.UdpSendMessage(player.udpEndPoint, player.UdpSendBuffer.ToArray());
                                    player.UdpSendBuffer.Clear();
                                }
                            }
                        }

                    }
                    catch
                    {
                        //ignore any excpetions
                    }

                    mutex.ReleaseMutex();

                    Thread.Sleep(50); //sleep for 50ms, so it sends messages 20 times a second
                }
            }
        }

        //send the updated list of all connected players to each of the clients in this room
        public void UpdateAllPlayers()
        {
            try
            {
                string toSendMsg = "3";

                foreach (Player player in playersInThisRoom)
                {
                    string name = player.name;
                    string id = player.id.ToString();
                    toSendMsg += "$" + player.name + "$" + player.id.ToString() + "$";
                    toSendMsg += (player.ready) ? "1" : "0";
                }

                byte[] toSendBuffer = Encoding.ASCII.GetBytes(toSendMsg);
                toSendBuffer = ServerHelperFunctions.AddLenghtToFront(toSendBuffer);

                foreach (Player player in playersInThisRoom)
                {
                    player.TcpSocket.BeginSend(toSendBuffer, 0, toSendBuffer.Length, 0, new AsyncCallback(TcpSendCallBack), player.TcpSocket);
                }
            }
            catch
            {
                //ignore any excpetions
            }
        }
    }
}