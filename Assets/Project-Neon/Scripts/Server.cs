using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine;

public class Server : MonoBehaviour
{
    private GameObject myCube;
    private static byte[] buffer = new byte[512];
    private static Socket server;
    private static IPEndPoint client;
    private static EndPoint remoteClient;
    private static int rec = 0;
    private float posx;

    //lecture 5
    private float[] pos;
    private byte[] bpos;

    public static void RunServer()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        Debug.Log("Server name: " + host.HostName + "   IP:" + ip);
        IPEndPoint localEP = new IPEndPoint(ip, 11111);

        server = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        client = new IPEndPoint(IPAddress.Any, 0);

        remoteClient = (EndPoint)client;
        server.Bind(localEP);

        Debug.Log("Waiting for data...");
    }

    // Start is called before the first frame update
    private void Start()
    {
        myCube = GameObject.Find("Player");
        RunServer();

        //lecture 5
        //non blocking mode

        server.Blocking = false;
    }

    // Update is called once per frame
    private void Update()
    {
        try
        {
            rec = server.ReceiveFrom(buffer, ref remoteClient);
        }
        catch (SocketException e)
        {
            Debug.Log("Exception: " + e.ToString());
        }

        //posx = float.Parse(Encoding.ASCII.GetString(buffer, 0, rec));
        //Debug.Log("PosX: " + posx);

        // This is how you update the Server side cube's position


        //lecture 5
        pos = new float[rec / 4];
        Buffer.BlockCopy(buffer, 0, pos, 0, rec);


        myCube.transform.position = new Vector3(pos[0], pos[1], pos[2]);
    }
}
