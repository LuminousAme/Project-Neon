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
    bool recievedUdp = false;

    private Socket TcpClient;
    private Socket UdpClient;

    private static EndPoint udpRemoteEP;
    private static IPAddress serverIp;

    private static byte[] TcpRecBuffer = new byte[1024];
    private static byte[] UdpRecBuffer = new byte[1024];

    private List<Player> players = new List<Player>();
    public List<Player> GetPlayers() => players;
    private Guid thisClientId;
    public Guid GetThisClientID() => thisClientId;
    private float timeBetweenConnectionChecks = 1f, elapsedTime = 0f;

    private void Start()
    {
        if (instance != null) Destroy(this.gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void EnterGame(int type)
    {
        StartClient(type);
    }

    public void SendTcpPacket(Packet packet)
    {
        try
        {
            byte[] data = packet.Pack();
            TcpClient.BeginSend(data, 0, data.Length, 0, new AsyncCallback(TcpSendCallBack), TcpClient);
        }
        catch
        {
            //ignore any excpetions
        }
    }

    public void SendUdpPacket(Packet packet)
    {
        try
        {
            byte[] data = packet.Pack();
            UdpClient.BeginSendTo(data, 0, data.Length, 0, udpRemoteEP, new AsyncCallback(UdpSendCallBack), UdpClient);
        }
        catch
        {
            //ignore any excpetions
        }
    }

    private void StartClient(int type)
    {
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
            if(type == 0)
            {
                //need to edit this for various guids later
                connectMessage = "0$" + PlayerPrefs.GetString("DisplayName") + "$0$noguid$noroomcode"; 
            }
            else if (type == 1)
            {
                connectMessage = "1$" + PlayerPrefs.GetString("DisplayName") + "$0$noguid$" + roomCode;
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

                string recMsg = Encoding.ASCII.GetString(data);
                string[] splitrecMsg = recMsg.Split('$');

                if (splitrecMsg[0] == "9")
                {
                    Guid.TryParse(splitrecMsg[1], out thisClientId);
                }
                else
                {
                    Packet recievedPacket = new Packet(data);
                    ProcessTcpPackets(recievedPacket.UnPack());
                }
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
                recievedUdp = true;
                byte[] data = new byte[rec];
                Array.Copy(UdpRecBuffer, data, rec);

                Packet recievedPacket = new Packet(data);

                ProcessUdpPackets(recievedPacket.UnPack());
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

    }

    void ProcessUdpPackets(List<Packet> packets)
    {

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
