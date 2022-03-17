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
    private Socket server;

    private IPEndPoint remoteEP;
    private IPEndPoint localEP;

    private IPAddress thisPlayerIp;

    private byte[] sendBuffer = new byte[512];
    private byte[] recieveBuffer = new byte[1024];

    private bool connection;

    private List<Player> players = new List<Player>();
    public List<Player> GetPlayers() => players;

    //starts client
    private void StartClient(int type)
    {
        //get ip
        //IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        //IPAddress ip = host.AddressList[1];// user's ipv4
        IPAddress ip = IPAddress.Parse(ipText);// server's ip

        localEP = new IPEndPoint(ip, 11111);

        //create
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
        return socket.Connected;
    }

    private void Start()
    {
        if (instance != null) Destroy(this.gameObject);
        instance = this;
        DontDestroyOnLoad(this.gameObject);


    }

    // Update is called once per frame
    private void Update()
    {
        if (isStarted)
        {
            //check for connection
            connection = IsConnected(client);
            if (!connection)
            {
                //if not connected release the socket
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            else
            {
                try
                {
                    int recv = client.Receive(recieveBuffer);
                    string data = Encoding.ASCII.GetString(recieveBuffer, 0, recv);
                    Debug.Log(data);
                    string[] splitData = data.Split('$');

                    if(int.Parse(splitData[0]) == 0)
                    {
                        for(int i = 1; i < splitData.Length; i++)
                        {
                            bool exists = false;
                            foreach(Player player in players)
                            {
                                if (player.name == splitData[i]) exists = true;
                            }

                            if(!exists)
                            {
                                Player newplayer = new Player();
                                newplayer.name = splitData[i];
                                players.Add(newplayer);
                            }
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
                        Player player = players.Find(p => p.name == splitData[1]);
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
        Player thisPlayer = players.Find(p => p.name == PlayerPrefs.GetString("DisplayName"));
        thisPlayer.ready = ready;

        string toSend = "3$" + PlayerPrefs.GetString("DisplayName") + "$";
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
        foreach(Player player in players)
        {
            if (!player.ready)
            {
                ready = false;
                break;
            }
        }

        return ready;
    }
}

public class Player {
    public string name = "";
    public bool ready = false;
}