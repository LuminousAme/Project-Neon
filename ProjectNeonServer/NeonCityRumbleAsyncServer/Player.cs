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

        public Socket TcpSocket;
        byte[] recBuffer = new byte[512];
        string inMsg;

        public List<byte> sendBuffer;

        public Stopwatch stopwatch;

        public Player(Socket connectingSocket)
        {
            TcpSocket = connectingSocket;

            TcpSocket.BeginReceive(recBuffer, 0, recBuffer.Length, 0, new AsyncCallback(JoinOrCreateRoomCallback), TcpSocket);
        }

        private void JoinOrCreateRoomCallback(IAsyncResult result)
        {
            //process connecting to the correct room
            Socket client = (Socket)result.AsyncState;
            int rec = client.EndReceive(result);
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
            else if (splitData[0] == "4")
            {
                //join an existing room
                string codeToJoin = splitData[1];

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

                    TcpSocket.BeginSend(replyBuffer, 0, replyBuffer.Length, 0, new AsyncCallback(ReplySendCallBack), null);

                    //return since we didn't connect to the room properly, probably also want to handle removing the player from the list in the core server I think but we'll see
                    return;
                }
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();

            //update all of the connected players
            connectedRoom.UpdateAllPlayers();
        }

        private void ReplySendCallBack(IAsyncResult result)
        {
            Socket client = (Socket)result.AsyncState;
            client.EndSend(result);
        }
    }
}