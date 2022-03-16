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

    // Start is called before the first frame update
    //starts client
    private void Start()
    {
        byte[] buffer = new byte[512];

        //get ip
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ip = host.AddressList[1];

        //create
        IPEndPoint localEP = new IPEndPoint(ip, 11111);
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint client = new IPEndPoint(IPAddress.Any, 0); // 0 for any available port
        EndPoint remoteClient = (EndPoint)client;

        if (master == false)
        {
        }

        try
        {
            server.Bind(localEP);
            Console.WriteLine("Waiting for data...");
            while (true)
            {
                int rec = server.ReceiveFrom(buffer, ref remoteClient);
                Console.WriteLine("Client: {0}  | Data: {1}", remoteClient.ToString(), Encoding.ASCII.GetString(buffer, 0, rec));
                Console.WriteLine("Waiting for data...");
            }
            //server.Shutdown(SocketShutdown.Both);
            //server.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}