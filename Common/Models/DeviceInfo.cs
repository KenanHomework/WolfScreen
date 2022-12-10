using Common.Enums;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class DeviceInfo
    {
        public string Hostname { get; set; } = string.Empty;

        public string IpAddress { get; set; } = IPAddress.Loopback.ToString();

        public string ServerIpAddress { get; set; } = string.Empty;

        public ClientConnectionStatus Status { get; set; } = ClientConnectionStatus.Free;

        public DeviceInfo()
        {
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        // Get Ipv4 Address
            //        IpAddress = ip.ToString();

            //        break;
            //    }
            //}

            IpAddress = IPAddress.Loopback.ToString();

            // Get the Name of HOST  
            Hostname = Dns.GetHostName();

        }

    }
}
