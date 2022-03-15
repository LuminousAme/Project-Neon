using System.Net;
using UnityEngine;

namespace PeertoPeer    
{
    public class Peers : MonoBehaviour
    {
        public IPAddress ip;

        public string username;

        public Peers(IPAddress newIP, string newName)
        {
            ip = newIP;
            username = newName;
        }
    }
}