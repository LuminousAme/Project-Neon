using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;


namespace NCRTestClient
{
    public static class TestClient
    {
        private static Socket TcpClient;
        private static Socket UdpClient;

        private static EndPoint udpRemoteEP;

        private static IPAddress serverIp;
        private static IPAddress localIp;

        private static byte[] TcpRecBuffer = new byte[1024];
        private static byte[] UdpRecBuffer = new byte[1024];
        private static string RoomCode;

        private static int counter = 0;

        private static Guid id;

        static void Main(string[] args)
        {
            RoomCode = Console.ReadLine();

            serverIp = IPAddress.Parse("10.10.138.18");

            TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpClient.SetKeepAliveValues(1500, 500);
            TcpClient.Blocking = true;

            //attemp a connection
            try
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());
                localIp = HelperFunctions.FindIP4V(false);

                TcpClient.BeginConnect(serverIp, 11111, new AsyncCallback(TcpConnectCallBack), TcpClient);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Thread sendThread = new Thread(new ThreadStart(SendLoop));
            sendThread.Name = "Send Thread";
            sendThread.Start();

            Console.ReadLine();
        }

        static void TcpConnectCallBack(IAsyncResult result)
        {
            try
            {
                TcpClient.EndConnect(result);

                string connectMessage = "0$TestClient$0$noguid$noroomcode"; //create or join $ username $ guid or not $ guid.ToString $ roomcode

                if (!string.IsNullOrEmpty(RoomCode))
                {
                    connectMessage = "1$TestClient$0$noguid$" + RoomCode;
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
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        static void TcpSendCallBack(IAsyncResult result)
        {
            try
            {
                Socket client = (Socket)result.AsyncState;
                client.EndSend(result);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void TcpRecieveCallBack(IAsyncResult result)
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

                    if(splitrecMsg[0] == "9")
                    {
                        Guid.TryParse(splitrecMsg[1], out id);
                    }
                    else
                    {
                        Console.WriteLine("Recieved on TCP: " + recMsg);
                    }
                }

                TcpClient.BeginReceive(TcpRecBuffer, 0, TcpRecBuffer.Length, 0, new AsyncCallback(TcpRecieveCallBack), TcpClient);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock) Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void UdpSendCallBack(IAsyncResult result)
        {
            try
            {
                UdpClient.EndSendTo(result);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        static void UdpRecieveCallBack(IAsyncResult result)
        {
            try
            {
                int rec = UdpClient.EndReceiveFrom(result, ref udpRemoteEP);

                if (rec > 0)
                {
                    byte[] data = new byte[rec];
                    Array.Copy(UdpRecBuffer, data, rec);

                    string recMsg = Encoding.ASCII.GetString(data);

                    Console.WriteLine("Recieved on UDP: " + recMsg);
                }

                UdpClient.BeginReceiveFrom(UdpRecBuffer, 0, UdpRecBuffer.Length, 0, ref udpRemoteEP, new AsyncCallback(UdpRecieveCallBack), UdpClient);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        static void SendLoop()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                if (udpRemoteEP != null && UdpClient != null && id != null && stopwatch.ElapsedMilliseconds >= 1000)
                {
                    stopwatch.Restart();
                    counter++;
                    byte[] toSend = Encoding.ASCII.GetBytes(id.ToString() + "$" + counter.ToString());
                    UdpClient.BeginSendTo(toSend, 0, toSend.Length, 0, udpRemoteEP, new AsyncCallback(UdpSendCallBack), UdpClient);
                }
            }
        }
    }

    public static class HelperFunctions
    {
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

        //check if a socket is connected
        public static bool IsConnected(this Socket s)
        {
            //taken from here https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c/2661876#2661876
            try
            {
                return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock) return true;
                else return false;
            }
        }

        //control how long a socket can stay alive
        public static void SetKeepAliveValues(this Socket s, int keepAliveTime, int keepAliveInterval)
        {
            //based on code from here https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c/2661876#2661876
            int size = sizeof(uint);
            byte[] values = new byte[size * 3];

            BitConverter.GetBytes((uint)(1)).CopyTo(values, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(values, size);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(values, size * 2);

            byte[] outvalues = BitConverter.GetBytes(0);

            s.IOControl(IOControlCode.KeepAliveValues, values, outvalues);
        }
    }
}
