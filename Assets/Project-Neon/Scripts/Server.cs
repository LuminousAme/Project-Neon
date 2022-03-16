using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    private GameObject myCube;
    private static byte[] buffer = new byte[512];
    private static Socket server;
    private static IPEndPoint client;
    private static EndPoint remoteClient;
    private static int rec = 0;

    public static void RunServer()
    {
        byte[] buffer = new byte[512];
        IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());
        // IPAddress ip = hostInfo.AddressList[1];//[0] ipv6
        IPAddress ip = null;

        for (int i = 0; i < hostInfo.AddressList.Length; ++i)
        {
            //check for IPv4 address adn add it to list
            if (hostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                ip = hostInfo.AddressList[i];
        }

        Debug.Log("Server name: " + hostInfo.HostName + "   IP:" + ip);
        IPEndPoint localEP = new IPEndPoint(ip, 11111);

        try
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client = new IPEndPoint(IPAddress.Any, 0);

            remoteClient = (EndPoint)client;
            server.Bind(localEP);

            Debug.Log("Waiting for data...");
        }

        catch (SocketException e)
        {
            Debug.Log("Exception: " + e.ToString());
        }



    }

    // Start is called before the first frame update
    private void Start()
    {
        RunServer();
    }

    // Update is called once per frame
    private void Update()
    {

    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }

    public ServerClient(TcpClient clientSocket, string name)
    {
        clientName = name;
        tcp = clientSocket;
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System;
//using UnityEngine;

//public class Server : MonoBehaviour
//{
//    private GameObject myCube;
//    private static byte[] buffer = new byte[512];
//    private static Socket server;
//    private static IPEndPoint client;
//    private static EndPoint remoteClient;
//    private static int rec = 0;

//    private List<ServerClient> clients;
//    private List<ServerClient> disconnectList;

//    //lecture 5
//    private float[] pos;
//    private byte[] bpos;

//    public static void RunServer()
//    {
//        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
//        IPAddress ip = IPAddress.Parse("127.0.0.1");
//        Debug.Log("Server name: " + host.HostName + "   IP:" + ip);
//        IPEndPoint localEP = new IPEndPoint(ip, 11111);

//        server = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
//        client = new IPEndPoint(IPAddress.Any, 0);

//        remoteClient = (EndPoint)client;
//        server.Bind(localEP);

//        Debug.Log("Waiting for data...");
//    }

//    // Start is called before the first frame update
//    private void Start()
//    {
//        myCube = GameObject.Find("Player");
//        RunServer();

//        //lecture 5
//        //non blocking mode

//        server.Blocking = false;
//    }

//    // Update is called once per frame
//    private void Update()
//    {
//        try
//        {
//            rec = server.ReceiveFrom(buffer, ref remoteClient);
//        }
//        catch (SocketException e)
//        {
//            Debug.Log("Exception: " + e.ToString());
//        }

//        //posx = float.Parse(Encoding.ASCII.GetString(buffer, 0, rec));
//        //Debug.Log("PosX: " + posx);

//        // This is how you update the Server side cube's position

//        //lecture 5
//        pos = new float[rec / 4];
//        Buffer.BlockCopy(buffer, 0, pos, 0, rec);

//        myCube.transform.position = new Vector3(pos[0], pos[1], pos[2]);
//    }

//}

//public class ServerClient
//{
//    public TcpClient tcp;
//    public string clientName;

//    public ServerClient(TcpClient clientSocket)
//    {
//        clientName = "Guest";
//        tcp = clientSocket;
//    }

//    public ServerClient(TcpClient clientSocket, string name)
//    {
//        clientName = name;
//        tcp = clientSocket;
//    }

//}