using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Linq;

//using PeertoPeer;
public class Client : MonoBehaviour
{
    public static Client instance = null;
    public string ipText = "127.0.0.1";
    public string roomCode = "";

    bool isStarted = false;

    private Socket TcpClient;
    private Socket UdpClient;

    private IPEndPoint remoteTcpEP;
    private IPEndPoint remoteUdpEP;
    private EndPoint remoteUdpAbstractEP;

    private IPAddress thisPlayerIp;

    private byte[] sendBuffer = new byte[1024];
    private byte[] recieveBuffer = new byte[1024];

    private bool connection;

    private List<Player> players = new List<Player>();
    public List<Player> GetPlayers() => players;
    private Guid thisClientId;
    public Guid GetThisClientID() => thisClientId;
    private float timeBetweenConnectionChecks = 1f, elapsedTime = 0f;

    //starts client
    private void StartClient(int type)
    {
        elapsedTime = 0f;
        //get ip
        //IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        //IPAddress ip = host.AddressList[1];// user's ipv4
        IPAddress ip = IPAddress.Parse(ipText);// server's ip

        remoteTcpEP = new IPEndPoint(ip, 11111);

        //create
        TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //setting it up to handle ungraceful disconnecting, checks every second, and waits half a second before trying again if it failed
        TcpClient.SetKeepAliveValues(1000, 500);
        TcpClient.Blocking = true;

        // Attempt a connection
        try
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());
            thisPlayerIp = null;

            for (int i = 0; i < hostInfo.AddressList.Length; i++)
            {
                //check for IPv4 address
                if (hostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    thisPlayerIp = hostInfo.AddressList[i];
            }

            Debug.Log("Connecting to server...");
            TcpClient.Connect(remoteTcpEP);
            Debug.Log("Client Connected to IP: " + TcpClient.RemoteEndPoint.ToString());

            string toSend = "";
            if (type == 0)
            {
                toSend += "0$" + thisPlayerIp.ToString() + "$" + PlayerPrefs.GetString("DisplayName");
            }
            else if (type == 1)
            {
                toSend += "1$" + thisPlayerIp.ToString() + "$" + PlayerPrefs.GetString("DisplayName") + "$" + roomCode;
            }

            sendBuffer = Encoding.ASCII.GetBytes(toSend);
            TcpClient.Send(sendBuffer);

            int recv = TcpClient.Receive(recieveBuffer);

            string data = Encoding.ASCII.GetString(recieveBuffer, 0, recv);
            string[] splitData = data.Split('$');
            roomCode = splitData[1];
            thisClientId = Guid.Parse(splitData[2]);

            TcpClient.Blocking = false;

            remoteUdpEP = new IPEndPoint(ip, 11112);
            remoteUdpAbstractEP = (EndPoint)remoteUdpEP;

            UdpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UdpClient.Blocking = true;

            //just send the id to start the connection lol
            sendBuffer = Encoding.ASCII.GetBytes(thisClientId.ToString());
            UdpClient.SendTo(sendBuffer, remoteUdpEP);
            int rec = UdpClient.ReceiveFrom(recieveBuffer, ref remoteUdpAbstractEP);
            Debug.Log("Server UDP Response: " + Encoding.ASCII.GetString(recieveBuffer, 0, rec));
            UdpClient.Blocking = false;

            isStarted = true;
        }
        catch (ArgumentNullException anexc)
        {
            Debug.Log("ArgumentNullException: " + anexc.ToString());
        }
        catch (SocketException se)
        {
            if (se.SocketErrorCode != SocketError.WouldBlock) Debug.Log("SocketException: " + se.ToString());
        }
    }

    public bool IsConnected(Socket socket)
    {
        return socket.IsConnected();
    }

    private void Start()
    {
        if (instance != null) Destroy(this.gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (isStarted)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeBetweenConnectionChecks)
            {
                //check for connection
                connection = IsConnected(TcpClient);
                if (!connection)
                {
                    //if not connected release the socket
                    TcpClient.Shutdown(SocketShutdown.Both);
                    TcpClient.Close();
                    isStarted = false;
                    return;
                }
            }
            //tcp
            try
            {
                int recv = TcpClient.Receive(recieveBuffer);
                string data = Encoding.ASCII.GetString(recieveBuffer, 0, recv);
                string[] splitData = data.Split('$');

                if (int.Parse(splitData[0]) == 0)
                {
                    List<Player> newPlayerList = new List<Player>();
                    for (int i = 1; i < splitData.Length; i = i + 2)
                    {
                        Guid newID = Guid.Parse(splitData[i + 1]);
                        newPlayerList.Add(new Player(splitData[i], newID));

                        bool exists = false;
                        foreach (Player player in players)
                        {
                            if (player.id == newID) exists = true;
                        }

                        if (!exists)
                        {
                            Player newplayer = new Player();
                            newplayer.name = splitData[i];
                            newplayer.id = Guid.Parse(splitData[i + 1]);
                            players.Add(newplayer);
                        }
                    }

                    //if a player has disconnected, remove them from the list of players

                    if (newPlayerList.Count != players.Count)
                    {
                        List<Player> noLongerHere = new List<Player>();

                        foreach (Player player in players)
                        {
                            if (!newPlayerList.Exists(p => p.id == player.id))
                            {
                                noLongerHere.Add(player);
                            }
                        }

                        foreach (var player in noLongerHere) players.Remove(player);
                    }
                }
                else if (int.Parse(splitData[0]) == 2)
                {
                    string username = splitData[1];
                    string msg = splitData[2];

                    ChatManager chat = FindObjectOfType<ChatManager>();
                    if (chat != null) chat.AddMessageToChat(username, msg);
                }
                else if (int.Parse(splitData[0]) == 3)
                {
                    Player player = players.Find(p => p.id == Guid.Parse(splitData[1]));
                    player.ready = (int.Parse(splitData[2]) == 0) ? false : true;
                }
                else if (int.Parse(splitData[0]) == 4)
                {
                    LobbyMenu lobby = FindObjectOfType<LobbyMenu>();
                    if (lobby != null) lobby.StartGame();
                }
                else if (int.Parse(splitData[0]) == 5)
                {
                    bool status = (int.Parse(splitData[2]) == 1);
                    Guid relevantPlayer = Guid.Parse(splitData[1]);

                    const int size = sizeof(float) * (3); //vec3, vec3, float, float
                    byte[] temp = new byte[size];
                    Buffer.BlockCopy(recieveBuffer, recv - size, temp, 0, size);

                    float[] floatarr = new float[3];
                    if (temp.Length == size)
                    {
                        Buffer.BlockCopy(temp, 0, floatarr, 0, temp.Length);

                        Vector3 newTarget = new Vector3(floatarr[0], floatarr[1], floatarr[2]);
                        //this is jank but will find the remote player to set the values
                        if (MatchManager.instance != null)
                        {
                            PlayerState player = MatchManager.instance.GetPlayers().Find(p => p.GetPlayerID() == relevantPlayer);
                            if (player != null)
                            {
                                RemotePlayer remotePlayer = player.GetComponent<RemotePlayer>();
                                if (remotePlayer != null) remotePlayer.SetGrappleStatus(status, newTarget);
                            }
                        }
                    }

                }
                else if (int.Parse(splitData[0]) == 6)
                {
                    Guid relevantPlayer = Guid.Parse(splitData[1]);
                    //this is jank but will find the remote player to set the values
                    if (MatchManager.instance != null)
                    {
                        PlayerState player = MatchManager.instance.GetPlayers().Find(p => p.GetPlayerID() == relevantPlayer);
                        if (player != null)
                        {
                            RemotePlayer remotePlayer = player.GetComponent<RemotePlayer>();
                            if (remotePlayer != null) remotePlayer.BeginQuickAttack();
                        }
                    }
                }
                else if (int.Parse(splitData[0]) == 7)
                {
                    Guid relevantPlayer = Guid.Parse(splitData[1]);
                    //this is jank but will find the remote player to set the values
                    if (MatchManager.instance != null)
                    {
                        PlayerState player = MatchManager.instance.GetPlayers().Find(p => p.GetPlayerID() == relevantPlayer);
                        if (player != null)
                        {
                            RemotePlayer remotePlayer = player.GetComponent<RemotePlayer>();
                            if (remotePlayer != null) remotePlayer.BeginRaiseHeavyAttack();
                        }
                    }
                }
                else if (int.Parse(splitData[0]) == 8)
                {
                    Guid relevantPlayer = Guid.Parse(splitData[1]);
                    //this is jank but will find the remote player to set the values
                    if (MatchManager.instance != null)
                    {
                        PlayerState player = MatchManager.instance.GetPlayers().Find(p => p.GetPlayerID() == relevantPlayer);
                        if (player != null)
                        {
                            RemotePlayer remotePlayer = player.GetComponent<RemotePlayer>();
                            if (remotePlayer != null) remotePlayer.BeginHeavyDown();
                        }
                    }
                }

            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Debug.Log(e.ToString());
            }

            //udp
            try
            {
                int recv = UdpClient.ReceiveFrom(recieveBuffer, ref remoteUdpAbstractEP);



                if (recv > 0)
                {
                    string data = Encoding.ASCII.GetString(recieveBuffer, 0, recv);
                    string[] splitData = data.Split('$');
                    //Debug.Log("Recieved on UDP: " + data); //know we're recieving correctly

                    if (splitData[1] == "0")
                    {
                        Guid targetPlayer = Guid.Parse(splitData[0]);

                        const int size = sizeof(float) * (3 + 3 + 1 + 1); //vec3, vec3, float, float
                        byte[] temp = new byte[size];
                        Buffer.BlockCopy(recieveBuffer, recv - size, temp, 0, size);

                        float[] floatarr = new float[3 + 3 + 1 + 1];
                        if(temp.Length == size)
                        {
                            Buffer.BlockCopy(temp, 0, floatarr, 0, temp.Length);


                            Vector3 newPos = new Vector3(floatarr[0], floatarr[1], floatarr[2]);
                            Vector3 newVel = new Vector3(floatarr[3], floatarr[4], floatarr[5]);
                            float newYaw = floatarr[6];
                            float newPitch = floatarr[7];
                            //this is jank but will find the remote player to set the values
                            if (MatchManager.instance != null)
                            {
                                PlayerState player = MatchManager.instance.GetPlayers().Find(p => p.GetPlayerID() == targetPlayer);
                                if (player != null)
                                {
                                    RemotePlayer remotePlayer = player.GetComponent<RemotePlayer>();
                                    if (remotePlayer != null) remotePlayer.SetData(newPos, newVel, newYaw, newPitch);
                                }
                            }
                        }   
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Debug.Log(e.ToString());
            }

            if(Input.GetKeyDown(KeyCode.F1))
            {
                try
                {
                    sendBuffer = Encoding.ASCII.GetBytes(thisClientId.ToString());
                    UdpClient.SendTo(sendBuffer, remoteUdpEP);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock) Debug.Log(e.ToString());
                }
            }
        }
    }

    public void Disconnect()
    {
        TcpClient.Shutdown(SocketShutdown.Both);
        TcpClient.Close();
        UdpClient.Shutdown(SocketShutdown.Both);
        UdpClient.Close();
        isStarted = false;
        players.Clear();
    }


    public void EnterGame(int type)
    {
        StartClient(type);
        connection = IsConnected(TcpClient);
    }

    public void SendMessageToOtherPlayers(string msg)
    {
        if(isStarted)
        {
            string toSend = "2$" + PlayerPrefs.GetString("DisplayName") + "$" + msg;
            sendBuffer = Encoding.ASCII.GetBytes(toSend);

            TcpClient.Send(sendBuffer);
        }
    }

    public void SetReady(bool ready)
    {
        if(isStarted)
        {
            Player thisPlayer = players.Find(p => p.id == thisClientId);
            thisPlayer.ready = ready;

            string toSend = "3$" + thisClientId.ToString() + "$";
            toSend += (ready) ? "1" : "0";
            sendBuffer = Encoding.ASCII.GetBytes(toSend);

            TcpClient.Send(sendBuffer);
        }
    }

    public void LaunchGameForAll()
    {
        if(isStarted)
        {
            string toSend = "4";
            sendBuffer = Encoding.ASCII.GetBytes(toSend);

            TcpClient.Send(sendBuffer);
        }
    }

    public bool GetAllPlayersReady()
    {
        bool ready = true;
        foreach (Player player in players)
        {
            if (!player.ready)
            {
                ready = false;
                break;
            }
        }

        return ready;
    }

    public void SendPosRotUpdate(Vector3 pos, Vector3 vel, float yaw, float pitch)
    {
        if(isStarted)
        {
            //block copy the data to send to the server, so it can then send it to all of the other clients
            float[] floatarr = { pos.x, pos.y, pos.z, vel.x, vel.y, vel.z, yaw, pitch };
            byte[] temp = new byte[sizeof(float) * floatarr.Length];
            Buffer.BlockCopy(floatarr, 0, temp, 0, sizeof(float) * floatarr.Length);  //should be 8 floats

            string toSend = thisClientId.ToString() + "$0$";

            //this is jank but works
            byte[] temp2 = Encoding.ASCII.GetBytes(toSend);
            byte[] temp3 = new byte[temp.Length + temp2.Length];
            Array.Copy(temp2, temp3, temp2.Length);
            Array.Copy(temp, 0, temp3, temp2.Length, temp.Length);
            sendBuffer = temp3;
            UdpClient.SendTo(sendBuffer, remoteUdpEP);
        }
    }

    public void SendDoQuickAttack()
    {
        if(isStarted)
        {
            string toSend = "6$" + thisClientId.ToString();
            sendBuffer = Encoding.ASCII.GetBytes(toSend);

            TcpClient.Send(sendBuffer);
        }
    }

    public void SendStartHeavyAttack()
    {
        if(isStarted)
        {
            string toSend = "7$" + thisClientId.ToString();
            sendBuffer = Encoding.ASCII.GetBytes(toSend);

            TcpClient.Send(sendBuffer);
        }
    }

    public void SendEndHeavyAttack()
    {
        if(isStarted)
        {
            string toSend = "8$" + thisClientId.ToString();
            sendBuffer = Encoding.ASCII.GetBytes(toSend);

            TcpClient.Send(sendBuffer);
        }
    }

    public void SendGrappleStatus(bool isActive, Vector3 target)
    {
        if(isStarted)
        {
            // block copy the data to send to the server, so it can then send it to all of the other clients
            float[] floatarr = { target.x, target.y, target.z };
            byte[] temp = new byte[sizeof(float) * floatarr.Length];
            Buffer.BlockCopy(floatarr, 0, temp, 0, sizeof(float) * floatarr.Length);  //should be 3 floats

            string toSend = "5$" + thisClientId.ToString() + "$";
            toSend += (isActive) ? "1$" : "0$";

            //[0] = 5, [1] = id, [2] = status, after that it is the vector

            //this is jank but works
            byte[] temp2 = Encoding.ASCII.GetBytes(toSend);
            byte[] temp3 = new byte[temp.Length + temp2.Length];
            Array.Copy(temp2, temp3, temp2.Length);
            Array.Copy(temp, 0, temp3, temp2.Length, temp.Length);
            sendBuffer = temp3;
            TcpClient.Send(sendBuffer);
        }
    }

    private void OnDestroy()
    {
        if(isStarted)
        {
            Disconnect();
        }
    }

    private void OnApplicationQuit()
    {
        if (isStarted) Disconnect();
    }
}

public class Player
{
    public string name = "";
    public bool ready = false;
    public Guid id;

    public Player()
    {
        name = "";
        ready = false;
    }

    public Player(string name, Guid id)
    {
        this.name = name;
        this.id = id;
        ready = false;
    }
}