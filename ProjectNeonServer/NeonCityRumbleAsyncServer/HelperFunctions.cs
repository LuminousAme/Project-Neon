using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NeonCityRumbleAsyncServer
{
    public static class ServerHelperFunctions
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