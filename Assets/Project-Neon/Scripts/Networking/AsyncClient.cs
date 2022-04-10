using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class AsyncClient : MonoBehaviour
{
    public static AsyncClient instance = null;
    public string ipText = "127.0.0.1";
    public string roomCode = "";
    private int type = 0;

    bool isStarted = false;

    private Socket TcpClient;
    private Socket UdpClient;

    private EndPoint udpRemoteEP;
    private IPAddress serverIp;

    private byte[] TcpRecBuffer = new byte[1024];
    private byte[] UdpRecBuffer = new byte[1024];
    private List<byte> TcpSendQueue = new List<byte>();

    private List<Player> players = new List<Player>();
    public List<Player> GetPlayers() => players;
    private Guid thisClientId;
    public Guid GetThisClientID() => thisClientId;
    private string thisClientName = "";
    private float timeBetweenConnectionChecks = 1f, elapsedTime = 0f;
    private float tcpSendTime = 0.05f, elapsedSendTime = 0f;

    [SerializeField] private GameObject hitParticlePrefab;
    [SerializeField] private SoundEffect hitSFX;
    [SerializeField] private AudioSource hitAudioSource;

    List<Packet> TcpPacketsToBeProcessed = new List<Packet>();
    List<Packet> UdpPacketsToBeProcessed = new List<Packet>();

    private void Start()
    {
        if (instance != null) Destroy(this.gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Update()
    {
        if (isStarted)
        {
            if (TcpPacketsToBeProcessed.Count > 0)
            {
                ProcessTcpPackets(TcpPacketsToBeProcessed);
                TcpPacketsToBeProcessed.Clear();
            }

            if (UdpPacketsToBeProcessed.Count > 0)
            {
                ProcessUdpPackets(UdpPacketsToBeProcessed);
                UdpPacketsToBeProcessed.Clear();
            }


            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeBetweenConnectionChecks)
            {
                //check for connection
                if (!TcpClient.IsConnected())
                {
                    //if not connected release the socket
                    //Disconnect();
                    return;
                }
            }

            elapsedSendTime += Time.deltaTime;

            //if it's time to send another tcp packet
            if (elapsedSendTime >= tcpSendTime)
            {
                elapsedSendTime = 0f;
                //and there's a packet to send, send it
                if (TcpSendQueue.Count > 0)
                {
                    try
                    {
                        byte[] data = TcpSendQueue.ToArray();
                        TcpClient.BeginSend(data, 0, data.Length, 0, new AsyncCallback(TcpSendCallBack), TcpClient);
                        TcpSendQueue.Clear();
                    }
                    catch
                    {
                        //ignore any excpetions
                    }
                }
            }
        }
    }

    public void EnterGame(int type)
    {
        StartClient(type);
    }

    public void SendTcpPacket(Packet packet)
    {
        byte[] data = packet.PackFront();
        //Debug.Log("Tcp: " + Encoding.ASCII.GetString(data));
        TcpSendQueue.AddRange(data);
    }

    public void SendUdpPacket(Packet packet)
    {
        try
        {
            byte[] data = packet.PackFront();
            //Debug.Log("Udp: " + Encoding.ASCII.GetString(data));
            UdpClient.BeginSendTo(data, 0, data.Length, 0, udpRemoteEP, new AsyncCallback(UdpSendCallBack), UdpClient);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            //ignore any excpetions
        }
    }

    private void StartClient(int type)
    {
        thisClientName = PlayerPrefs.GetString("DisplayName");
        this.type = type;
        elapsedTime = 0f;
        serverIp = IPAddress.Parse(ipText);

        TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        TcpClient.SetKeepAliveValues(1500, 500);
        TcpClient.Blocking = true;

        try
        {
            TcpClient.BeginConnect(serverIp, 11111, new AsyncCallback(TcpConnectCallBack), TcpClient);
        }
        catch
        {
            //ignore any exceptions
        }
    }

    void TcpConnectCallBack(IAsyncResult result)
    {
        try
        {
            TcpClient.EndConnect(result);

            string connectMessage = "";
            if (type == 0)
            {
                //need to edit this for various guids later
                connectMessage = "0$" + thisClientName + "$0$noguid$noroomcode";
            }
            else if (type == 1)
            {
                connectMessage = "1$" + thisClientName + "$0$noguid$" + roomCode;
            }

            byte[] toSend = Encoding.ASCII.GetBytes(connectMessage);

            TcpClient.BeginSend(toSend, 0, toSend.Length, 0, new AsyncCallback(TcpSendCallBack), TcpClient);

            TcpClient.BeginReceive(TcpRecBuffer, 0, TcpRecBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), TcpClient);

            //now that all the tcp is set up, time to set up the udp system
            IPEndPoint udpEndPoint = new IPEndPoint(serverIp, 11112);
            udpRemoteEP = (EndPoint)udpEndPoint;

            UdpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string udpConnectMessage = "Hello I have connected to the server!";
            toSend = Encoding.ASCII.GetBytes(udpConnectMessage);
            UdpClient.BeginSendTo(toSend, 0, toSend.Length, 0, udpRemoteEP, new AsyncCallback(UdpSendCallBack), UdpClient);
            UdpClient.BeginReceiveFrom(UdpRecBuffer, 0, UdpRecBuffer.Length, 0, ref udpRemoteEP, new AsyncCallback(UdpRecieveCallBack), UdpClient);

            elapsedSendTime = 0f;
            isStarted = true;
        }
        catch
        {
            //ignore any excpetions
        }
    }

    void TcpSendCallBack(IAsyncResult result)
    {
        try
        {
            Socket client = (Socket)result.AsyncState;
            client.EndSend(result);
        }
        catch
        {
            //ignore any excpetions
        }
    }

    void TcpRecieveCallBack(IAsyncResult result)
    {
        try
        {
            int rec = TcpClient.EndReceive(result);
            if (rec > 0)
            {
                byte[] data = new byte[rec];
                Array.Copy(TcpRecBuffer, data, rec);

                string datastr = Encoding.ASCII.GetString(data);
                Debug.Log("Tcp: " + datastr);

                Packet recievedPacket = new Packet(data);
                TcpPacketsToBeProcessed.AddRange(recievedPacket.UnPackFront());
            }

            TcpClient.BeginReceive(TcpRecBuffer, 0, TcpRecBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), TcpClient);
        }
        catch
        {
            //ignore any exceptions
        }
    }

    void UdpSendCallBack(IAsyncResult result)
    {
        try
        {
            UdpClient.EndSendTo(result);
        }
        catch
        {
            //ignore any exceptions
        }
    }

    void UdpRecieveCallBack(IAsyncResult result)
    {
        try
        {
            int rec = UdpClient.EndReceiveFrom(result, ref udpRemoteEP);

            if (rec > 0)
            {
                byte[] data = new byte[rec];
                Array.Copy(UdpRecBuffer, data, rec);

                string datastr = Encoding.ASCII.GetString(data);
                //Debug.Log("Udp: " + datastr);

                Packet recievedPacket = new Packet(data);

                UdpPacketsToBeProcessed.AddRange(recievedPacket.UnPackFront());
            }

            UdpClient.BeginReceiveFrom(UdpRecBuffer, 0, UdpRecBuffer.Length, 0, ref udpRemoteEP, new AsyncCallback(UdpRecieveCallBack), UdpClient);
        }
        catch
        {
            //ignore any exceptions
        }
    }

    void ProcessTcpPackets(List<Packet> packets)
    {
        foreach (Packet packet in packets)
        {
            byte[] data = packet.GetDataRaw();
            string datastr = Encoding.ASCII.GetString(data);

            Debug.Log("Packet Str: " + datastr);

            //otherwise we need to figure out what data the packet contains and how to use it
            string[] splitData = datastr.Split('$');

            //set the id
            if (splitData[0] == "9")
            {
                Guid.TryParse(splitData[1], out thisClientId);
            }

            //update the player list
            if (splitData[0] == "3")
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

                    foreach (var player in noLongerHere)
                    {
                        //handle removing players mid game here

                        //send a message about the disconnect
                        ChatManager chat = FindObjectOfType<ChatManager>();
                        if (chat != null) chat.AddMessageToChat("Server", $"{player.name} has disconnected");

                        players.Remove(player);
                    }
                }
            }

            //roomcode
            else if (splitData[0] == "5")
            {
                roomCode = splitData[1];
            }

            //chat message
            else if (splitData[0] == "6")
            {
                string username = splitData[1];
                string msg = splitData[2];

                ChatManager chat = FindObjectOfType<ChatManager>();
                if (chat != null) chat.AddMessageToChat(username, msg);
            }

            //ready or not
            else if (splitData[0] == "7")
            {
                Player player = players.Find(p => p.id == Guid.Parse(splitData[1]));
                player.ready = (splitData[2] == "0") ? false : true;
            }

            //launch the game
            else if (splitData[0] == "8")
            {
                LobbyMenu lobby = FindObjectOfType<LobbyMenu>();
                if (lobby != null) lobby.StartGame();
            }

            //graple status
            else if (splitData[0] == "10")
            {
                bool status = (splitData[2] == "1");
                Guid relevantPlayer = Guid.Parse(splitData[1]);

                const int size = sizeof(float) * (3); //vec3, vec3, float, float
                byte[] temp = new byte[size];
                Buffer.BlockCopy(data, data.Length - size, temp, 0, size);

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

            //quick attack animation
            else if (splitData[0] == "11")
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

            //heavy attack up
            else if (splitData[0] == "12")
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

            //heavy attack down
            else if (splitData[0] == "13")
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

            //update bounty
            else if (splitData[0] == "14")
            {
                Guid relevantPlayer = Guid.Parse(splitData[1]);

                const int size = sizeof(int) * 2; //2 ints
                byte[] temp = new byte[size];
                Buffer.BlockCopy(data, data.Length - size, temp, 0, size);

                int[] intarr = new int[2];
                if (temp.Length == size)
                {
                    Buffer.BlockCopy(temp, 0, intarr, 0, temp.Length);

                    //this is jank but will find the remote player to set the values
                    if (MatchManager.instance != null)
                    {
                        PlayerState player = MatchManager.instance.GetPlayers().Find(p => p.GetPlayerID() == relevantPlayer);
                        if (player != null)
                        {
                            player.RemoteUpdateBounty(intarr[0], intarr[1]);
                        }
                    }
                }
            }

            //hit 
            else if (splitData[0] == "15")
            {
                Guid relevantPlayer = Guid.Parse(splitData[1]);

                const int size = 5 * sizeof(float); //5 floats
                byte[] temp = new byte[size];
                Buffer.BlockCopy(data, data.Length - size, temp, 0, size);

                float[] floatarr = new float[5];
                if (temp.Length == size)
                {
                    Buffer.BlockCopy(temp, 0, floatarr, 0, temp.Length);

                    //this is jank but will find the remote player to set the values
                    if (MatchManager.instance != null)
                    {
                        PlayerState player = MatchManager.instance.GetPlayers().Find(p => p.GetPlayerID() == relevantPlayer);
                        if (player != null)
                        {
                            player.RemoteUpdateHP(floatarr[0]);
                        }

                        //play the particle effect where the hit happened
                        GameObject newObj = Instantiate(hitParticlePrefab, new Vector3(floatarr[1], floatarr[2], floatarr[3]), Quaternion.identity);
                        newObj.transform.localScale *= floatarr[4];
                        Destroy(newObj, 0.5f);

                        //play the sound effect where the hit happened
                        if (hitSFX != null && hitAudioSource != null)
                        {
                            hitAudioSource.transform.position = new Vector3(floatarr[1], floatarr[2], floatarr[3]);
                            hitSFX.Play(hitAudioSource);
                        }
                    }
                }
            }
        }
    }

    void ProcessUdpPackets(List<Packet> packets)
    {
        foreach (Packet packet in packets)
        {
            //otherwise we need to figure out what data the packet contains and how to use it
            byte[] data = packet.GetDataRaw();
            string datastr = Encoding.ASCII.GetString(data);

            string[] splitData = datastr.Split('$');

            if (splitData.Length > 2)
            {
                if(splitData[1] == "0")
                {
                    Guid targetPlayer = Guid.Parse(splitData[0]);
                    
                    const int size = sizeof(float) * (9); //vec3, vec3, float, float, float
                    byte[] temp = new byte[size];
                    Buffer.BlockCopy(data, data.Length - size, temp, 0, size);

                    float[] floatarr = new float[9];
                    if (temp.Length == size)
                    {
                        Buffer.BlockCopy(temp, 0, floatarr, 0, temp.Length);

                        Vector3 newPos = new Vector3(floatarr[0], floatarr[1], floatarr[2]);
                        Vector3 newVel = new Vector3(floatarr[3], floatarr[4], floatarr[5]);
                        float newYaw = floatarr[6];
                        float newPitch = floatarr[7];
                        float timeStamp = floatarr[8];
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
    }

    public void SendMessageToOtherPlayers(string msg)
    {
        if (isStarted)
        {
            string toSend = "6$" + thisClientName + "$" + msg;

            byte[] newBuffer = Encoding.ASCII.GetBytes(toSend);

            Packet messagePacket = new Packet();
            messagePacket.AddBytes(newBuffer);
            SendTcpPacket(messagePacket);
        }
    }

    public void SetReady(bool ready)
    {
        if (isStarted)
        {
            Player thisPlayer = players.Find(p => p.id == thisClientId);
            thisPlayer.ready = ready;

            string toSend = "7$" + thisClientId.ToString() + "$";
            toSend += (ready) ? "1" : "0";
            byte[] newBuffer = Encoding.ASCII.GetBytes(toSend);

            Packet readyPacket = new Packet();
            readyPacket.AddBytes(newBuffer);
            SendTcpPacket(readyPacket);
        }
    }

    public void LaunchGameForAll()
    {
        if (isStarted)
        {
            string toSend = "8";

            byte[] newBuffer = Encoding.ASCII.GetBytes(toSend);

            Packet readyPacket = new Packet();
            readyPacket.AddBytes(newBuffer);
            SendTcpPacket(readyPacket);
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

    public void SendDoQuickAttack()
    {
        if (isStarted)
        {
            string toSend = "11$" + thisClientId.ToString();

            byte[] newBuffer = Encoding.ASCII.GetBytes(toSend);

            Packet qucikAttackPacket = new Packet();
            qucikAttackPacket.AddBytes(newBuffer);
            SendTcpPacket(qucikAttackPacket);
        }
    }

    public void SendStartHeavyAttack()
    {
        if (isStarted)
        {
            string toSend = "12$" + thisClientId.ToString();

            byte[] newBuffer = Encoding.ASCII.GetBytes(toSend);

            Packet heavyAttackPacket = new Packet();
            heavyAttackPacket.AddBytes(newBuffer);
            SendTcpPacket(heavyAttackPacket);
        }
    }

    public void SendEndHeavyAttack()
    {
        if (isStarted)
        {
            string toSend = "13$" + thisClientId.ToString();

            byte[] newBuffer = Encoding.ASCII.GetBytes(toSend);

            Packet heavyAttackPacket = new Packet();
            heavyAttackPacket.AddBytes(newBuffer);
            SendTcpPacket(heavyAttackPacket);
        }
    }

    public void SendGrappleStatus(bool isActive, Vector3 target)
    {
        if (isStarted)
        {
            // block copy the data to send to the server, so it can then send it to all of the other clients
            float[] floatarr = { target.x, target.y, target.z };
            byte[] temp = new byte[sizeof(float) * floatarr.Length];
            Buffer.BlockCopy(floatarr, 0, temp, 0, sizeof(float) * floatarr.Length);  //should be 3 floats

            string toSend = "10$" + thisClientId.ToString() + "$";
            toSend += (isActive) ? "1$" : "0$";

            //[0] = 5, [1] = id, [2] = status, after that it is the vector

            //this is jank but works
            byte[] temp2 = Encoding.ASCII.GetBytes(toSend);
            byte[] temp3 = new byte[temp.Length + temp2.Length];
            Array.Copy(temp2, temp3, temp2.Length);
            Array.Copy(temp, 0, temp3, temp2.Length, temp.Length);

            Packet grappleStatusPacket = new Packet();
            grappleStatusPacket.AddBytes(temp3);
            SendTcpPacket(grappleStatusPacket);
        }
    }

    public void UpdateScore(Guid player, int kills, int damageDealt)
    {
        if (isStarted)
        {
            int[] intarr = { kills, damageDealt };
            byte[] temp = new byte[sizeof(int) * intarr.Length];
            Buffer.BlockCopy(intarr, 0, temp, 0, sizeof(int) * intarr.Length);  //should be 2 ints

            string toSend = "14$" + player.ToString() + "$";

            //this is jank but works
            byte[] temp2 = Encoding.ASCII.GetBytes(toSend);
            byte[] temp3 = new byte[temp.Length + temp2.Length];
            Array.Copy(temp2, temp3, temp2.Length);
            Array.Copy(temp, 0, temp3, temp2.Length, temp.Length);

            Packet bountyUpdatePacket = new Packet();
            bountyUpdatePacket.AddBytes(temp3);
            SendTcpPacket(bountyUpdatePacket);
        }
    }

    public void UpdateHP(Guid player, float newHp, Vector3 particlePos, float particleScale)
    {
        if (isStarted)
        {
            //sending two hp to help resolve if multiple players got a hit on the same player at the same time lol
            float[] floatarr = { newHp, particlePos.x, particlePos.y, particlePos.z, particleScale };
            byte[] temp = new byte[sizeof(float) * floatarr.Length];
            Buffer.BlockCopy(floatarr, 0, temp, 0, sizeof(float) * floatarr.Length); //should be 2 floats

            string toSend = "15$" + player.ToString() + "$";

            //this is jank but works
            byte[] temp2 = Encoding.ASCII.GetBytes(toSend);
            byte[] temp3 = new byte[temp.Length + temp2.Length];
            Array.Copy(temp2, temp3, temp2.Length);
            Array.Copy(temp, 0, temp3, temp2.Length, temp.Length);

            Packet hpUpdatePacket = new Packet();
            hpUpdatePacket.AddBytes(temp3);
            SendTcpPacket(hpUpdatePacket);
        }
    }

    public void SendPosRotUpdate(Vector3 pos, Vector3 vel, float yaw, float pitch)
    {
        if (isStarted && MatchManager.instance != null)
        {
            float timeStamp = MatchManager.instance.GetTimeRemaining();

            //block copy the data to send to the server, so it can then send it to all of the other clients
            float[] floatarr = { pos.x, pos.y, pos.z, vel.x, vel.y, vel.z, yaw, pitch, timeStamp };
            byte[] temp = new byte[sizeof(float) * floatarr.Length];
            Buffer.BlockCopy(floatarr, 0, temp, 0, sizeof(float) * floatarr.Length);  //should be 9 floats

            string toSend = thisClientId.ToString() + "$0$";

            //this is jank but works
            byte[] temp2 = Encoding.ASCII.GetBytes(toSend);
            byte[] temp3 = new byte[temp.Length + temp2.Length];
            Array.Copy(temp2, temp3, temp2.Length);
            Array.Copy(temp, 0, temp3, temp2.Length, temp.Length);

            Packet posRotUpdatePacket = new Packet();
            posRotUpdatePacket.AddBytes(temp3);
            SendUdpPacket(posRotUpdatePacket);
        }
    }

    public void Disconnect()
    {
        Debug.Log("shutdown");
        TcpClient.Shutdown(SocketShutdown.Both);
        TcpClient.Close();
        UdpClient.Shutdown(SocketShutdown.Both);
        UdpClient.Close();
        isStarted = false;
        players.Clear();
    }

    private void OnDestroy()
    {
        if (isStarted) Disconnect();
    }

    private void OnApplicationQuit()
    {
        if (isStarted) Disconnect();
    }

    public static IPAddress FindIP4V(bool useLocalHost = false)
    {
        //if it's set to use the local host just return that
        if (useLocalHost) return IPAddress.Parse("127.0.0.1");

        //if not search for the ipv4 and return it
        IPHostEntry hostinfo = Dns.GetHostEntry(Dns.GetHostName());
        for (int i = 0; i < hostinfo.AddressList.Length; i++)
        {
            if (hostinfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                return hostinfo.AddressList[i];
            }
        }

        //if some reason it can't be found return the local host anyways though
        return IPAddress.Parse("127.0.0.1");
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