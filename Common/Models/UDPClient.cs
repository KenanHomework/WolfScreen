using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common.Models
{
    public class UDPClient
    {

        public Socket Client { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public byte[] Buffer { get; set; } = new byte[ushort.MaxValue];

        public EndPoint EndPoint = null;

        public UDPClient(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }


        public int Send(string data) => Client.SendTo(Encoding.Default.GetBytes(data), SocketFlags.None, EndPoint);

        public int ReciveLen() => Client.ReceiveFrom(Buffer, SocketFlags.None, ref EndPoint);

        public string Recive()
        {
            int len = Client.ReceiveFrom(Buffer, SocketFlags.None, ref EndPoint);
            return Encoding.Default.GetString(Buffer, 0, len);
        }

        public string SendRequest(string request)
        {
            Client.SendTo(Encoding.Default.GetBytes(request), SocketFlags.None, EndPoint);
            int len = Client.ReceiveFrom(Buffer, SocketFlags.None, ref EndPoint);
            return Encoding.Default.GetString(Buffer, 0, len);
        }

    }
}
