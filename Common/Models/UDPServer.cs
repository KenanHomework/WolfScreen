using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common.Models
{
    public class UDPServer
    {

        public EndPoint ListenEP = null;

        public EndPoint EndPoint = null;

        public Socket Listener = null;

        public byte[] Buffer = new byte[ushort.MaxValue];


        public UDPServer()
        {
            Listener = new Socket(
                                     AddressFamily.InterNetwork,
                                     SocketType.Dgram,
                                     ProtocolType.Udp
                                     );
            IPAddress ip = IPAddress.Loopback;
            ListenEP = new IPEndPoint(ip, 27001);
            Listener.Bind(ListenEP);
            EndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public void AssignEndpoint(IPEndPoint endPoint)
        {
            //EndPoint = endPoint;
        }

        public string Recieve()
        {
            int len = Listener.ReceiveFrom(Buffer, ref EndPoint);
            return Encoding.Default.GetString(Buffer, 0, len);
        }

        public int Send(string data) => Listener.SendTo(Encoding.Default.GetBytes(data), SocketFlags.None, ListenEP);

        public int Send(byte[] data) => Listener.SendTo(data, SocketFlags.None, EndPoint);

        public int SendResponse(string data)
        {
            int len = Listener.SendTo(Encoding.Default.GetBytes(data), SocketFlags.None, EndPoint);
            Listener.ReceiveFrom(Buffer,ref ListenEP);
            return len;
        }

        public int SendResponse(byte[] data)
        {
            int len = Listener.SendTo(data, SocketFlags.None, EndPoint);
            Listener.ReceiveFrom(Buffer, ref ListenEP);
            return len;
        }

    }
}
