using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

//using PeertoPeer;
public class Client : MonoBehaviour
{
    //[SerializeField] private GameObject debugg;

    //ip of the lobby
    [SerializeField] private TMP_InputField lobbyCode;

    private Socket client;

    private IPEndPoint clientEP;

    private IPEndPoint localEP;

    private EndPoint remoteClient;
    private byte[] buffer = new byte[1024];

    private bool connection;
    //public playerIps

    private List<IPAddress> addresses = new List<IPAddress>();

    //starts client

    //get list of ips from server
    public void GetIPs()
    {
        //need function to get ip of server
        // IPAddress serverIP = host.AddressList[1];
    }

    private void StartClient()
    {
        //get ip
        //IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        //IPAddress ip = host.AddressList[1];// user's ipv4
        IPAddress ip = IPAddress.Parse(lobbyCode.text);// server's ip

        localEP = new IPEndPoint(ip, 11111);

        //create
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // clientEP = new IPEndPoint(IPAddress.Any, 0); // 0 for any available port
        //remoteClient = (EndPoint)clientEP;

        // Attempt a connection
        try
        {
            Debug.Log("Connecting to server...");
            client.Connect(localEP);
            Debug.Log("Client Connected to IP: " + client.RemoteEndPoint.ToString());
        }
        catch (ArgumentNullException anexc)
        {
            Console.WriteLine("ArgumentNullException: {0}", anexc.ToString());
        }
        catch (SocketException se)
        {
            Console.WriteLine("SocketException: {0}", se.ToString());
        }
    }

    public bool IsConnected(Socket socket)
    {
        return socket.Connected;

        //try
        //{
        //    return !(socket.Poll(100, SelectMode.SelectRead) && socket.Available == 0);
        //}
        //catch (SocketException) { return false; }
    }

    private void Start()
    {
        StartClient();
        connection = IsConnected(client);
    }

    // Update is called once per frame
    private void Update()
    {
        //check for connection
        connection = IsConnected(client);
        if (!connection)
        {
            //if not connected release the socket
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        //try
        //{
        //    byte[] msg = Encoding.ASCII.GetBytes("Testinggggggg!!!!");

        //    client.Send(msg);
        //    //int recv = client.Receive(buffer);
        //    //Debug.Log("Received: {0} " + Encoding.ASCII.GetString(buffer, 0, recv));
        //    // Release the resource
        //    client.Shutdown(SocketShutdown.Both);
        //    client.Close();
        //}
        //catch (SocketException se)
        //{
        //    Console.WriteLine("SocketException: {0}", se.ToString());
        //}
    }
}