using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

//using PeertoPeer;
public class Client : MonoBehaviour
{
    // player who joins first, creates lobby, is master
    private bool master = false;

    // List<PeertoPeer.Peers> peers = new List<PeertoPeer.Peers>();

    private Socket client;

    private IPEndPoint clientEP;

    private IPEndPoint localEP;

    private EndPoint remoteClient;
    private byte[] buffer = new byte[512];

    // Start is called before the first frame update
    //starts client
    private void Start()
    {
        //get ip
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ip = host.AddressList[1];

        localEP = new IPEndPoint(ip, 11111);

        //create
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        clientEP = new IPEndPoint(IPAddress.Any, 0); // 0 for any available port
        remoteClient = (EndPoint)clientEP;

        //  client.Bind(localEP);

        //if (master == false)
        //{
        //}

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

    // Update is called once per frame
    private void Update()
    {
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