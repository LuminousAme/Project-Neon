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

    private Socket client;

    private IPEndPoint localEP;

    private IPAddress thisPlayerIp;

    private byte[] sendBuffer = new byte[512];
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

        localEP = new IPEndPoint(ip, 11111);

        //create
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //setting it up to handle ungraceful disconnecting, checks every second, and waits half a second before trying again if it failed
        client.SetKeepAliveValues(1000, 500);
        client.Blocking = true;

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
            client.Connect(localEP);
            Debug.Log("Client Connected to IP: " + client.RemoteEndPoint.ToString());

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
            client.Send(sendBuffer);

            int recv = client.Receive(recieveBuffer);

            string data = Encoding.ASCII.GetString(recieveBuffer, 0, recv);
            Debug.Log(data);
            string[] splitData = data.Split('$');
            roomCode = splitData[1];
            thisClientId = Guid.Parse(splitData[2]);

            client.Blocking = false;

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
                connection = IsConnected(client);
                if (!connection)
                {
                    //if not connected release the socket
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    isStarted = false;
                    return;
                }
            }
            try
            {
                int recv = client.Receive(recieveBuffer);
                string data = Encoding.ASCII.GetString(recieveBuffer, 0, recv);
                Debug.Log(data);
                string[] splitData = data.Split('$');

                if (int.Parse(splitData[0]) == 0)
                {
                    List <Player> newPlayerList = new List<Player>();
                    for (int i = 1; i < splitData.Length; i = i + 2)
                    {
                        Guid newID = Guid.Parse(splitData[i+1]);
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
                            newplayer.id = Guid.Parse(splitData[i+1]);
                            players.Add(newplayer);
                        }
                    }

                    //if a player has disconnected, remove them from the list of players

                    if(newPlayerList.Count != players.Count)
                    {
                        List<Player> noLongerHere = new List<Player>();

                        foreach (Player player in players)
                        {
                            if(!newPlayerList.Exists(p => p.id == player.id)) {
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
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Debug.Log(e.ToString());
            }
        }
    }

    public void Disconnect()
    {
        client.Shutdown(SocketShutdown.Both);
        client.Close();
        isStarted = false;
        players.Clear();
    }


    public void EnterGame(int type)
    {
        StartClient(type);
        connection = IsConnected(client);
    }

    public void SendMessageToOtherPlayers(string msg)
    {
        string toSend = "2$" + PlayerPrefs.GetString("DisplayName") + "$" + msg;
        sendBuffer = Encoding.ASCII.GetBytes(toSend);

        client.Send(sendBuffer);
    }

    public void SetReady(bool ready)
    {
        Player thisPlayer = players.Find(p => p.id == thisClientId);
        thisPlayer.ready = ready;

        string toSend = "3$" + thisClientId + "$";
        toSend += (ready) ? "1" : "0";
        sendBuffer = Encoding.ASCII.GetBytes(toSend);

        client.Send(sendBuffer);
    }

    public void LaunchGameForAll()
    {
        string toSend = "4";
        sendBuffer = Encoding.ASCII.GetBytes(toSend);

        client.Send(sendBuffer);
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

    public void SendPosRotUpdate(Vector3 pos, Vector3 vel, Quaternion rot, Vector3 angularVel)
    {
        //block copy the data to send to the server, so it can then send it to all of the other clients
        float[] toSend = { pos.x, pos.y, pos.z, vel.x, vel.y, vel.z, rot.x, rot.y, rot.z, rot.w, angularVel.x, angularVel.y, angularVel.z};
        Buffer.BlockCopy(toSend, 0, sendBuffer, 0, sizeof(float) * toSend.Length); //should be 13 floats
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