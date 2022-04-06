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
            playersInThisRoom.Add(joiningPlayer);
            joiningPlayer.TcpSocket.BeginReceive(recBuffer, 0, recBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), joiningPlayer.TcpSocket);
        }

        private void TcpRecieveCallBack(IAsyncResult result)
        {
            if (mutex.WaitOne())
            {
                Socket client = (Socket)result.AsyncState;
                int rec = client.EndReceive(result);
                byte[] data = new byte[rec];
                Array.Copy(recBuffer, data, rec);

                string recMsg = Encoding.ASCII.GetString(data);
                string[] splitRecMsg = recMsg.Split('$');
                //check if it's a disconnect message
                if (splitRecMsg[0] == "-2")
                {
                    //if it is, identify the player
                    Player disconnectingPlayer = playersInThisRoom.Find(p => p.TcpSocket == client);

                    if (disconnectingPlayer != null)
                    {
                        playersInThisRoom.Remove(disconnectingPlayer);
                        UpdateAllPlayers();
                    }
                }
                //if it's not send the message to all of the other players
                else
                {
                    foreach (Player player in playersInThisRoom)
                    {
                        if (player.TcpSocket == client) continue;

                        player.sendBuffer.AddRange(data);
                    }
                    client.BeginReceive(recBuffer, 0, recBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), client);
                }

                mutex.ReleaseMutex();
            }
        }

        private void TcpSendCallBack(IAsyncResult result)
        {
            Socket client = (Socket)result.AsyncState;
            client.EndSend(result);
        }

        private void SendLoop()
        {
            while (true)
            {
                if (mutex.WaitOne())
                {
                    foreach (Player player in playersInThisRoom)
                    {
                        //if it's been a second since the last send, make sure to send a ping message
                        if(player.stopwatch.ElapsedMilliseconds >= 1000)
                        {
                            string pingStr = "4";
                            byte[] pingBuffer = Encoding.ASCII.GetBytes(pingStr);
                            player.sendBuffer.AddRange(pingBuffer);
                        }

                        //if there's anything to send, then send it
                        if(player.sendBuffer.Count >= 0)
                        {
                            player.TcpSocket.BeginSend(player.sendBuffer.ToArray(), 0, player.sendBuffer.Count, 0, new AsyncCallback(TcpSendCallBack), player.TcpSocket);
                            //and restart the timer since the last time a message was sent
                            player.stopwatch.Restart();
                        }

                        player.sendBuffer.Clear();
                    }

                    mutex.ReleaseMutex();

                    Thread.Sleep(50); //sleep for 50ms, so it sends messages 20 times a second
                }
            }
        }

        //send the updated list of all connected players to each of the clients in this room
        public void UpdateAllPlayers()
        {
            string toSendMsg = "3";

            foreach (Player player in playersInThisRoom)
            {
                string name = player.name;
                string id = player.id.ToString();
                toSendMsg += "$" + player.id.ToString() + "$" + player.name;
            }

            byte[] toSendBuffer = Encoding.ASCII.GetBytes(toSendMsg);

            foreach (Player player in playersInThisRoom)
            {
                player.TcpSocket.BeginSend(toSendBuffer, 0, toSendBuffer.Length, 0, new AsyncCallback(TcpSendCallBack), player.TcpSocket);
            }
        }
    }
}