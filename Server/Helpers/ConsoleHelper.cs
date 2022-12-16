using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Helpers
{
    public enum ClientInfoType { Connected, Disconnected }

    public static class ConsoleHelper
    {
        public static void ShowClientInfo(DeviceInfo device,ClientInfoType type = ClientInfoType.Connected)
        {
            ShowProperyHorizantal("Host Name", device.Hostname);
            ShowProperyHorizantal("IpV4 Address", device.IpAddress);

            switch (type)
            {
                case ClientInfoType.Connected:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(" connected. ");
                    break;
                case ClientInfoType.Disconnected:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(" disconnected. ");
                    break;
                default:
                    break;
            }

            Console.ResetColor();
        }

        public static void ShowPropery(string name, string value)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{name}: ");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(value);

            Console.ResetColor();
        }

        public static void ShowProperyHorizantal(string name, string value)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{name}: ");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write($"{value} ");

            Console.ResetColor();
        }



    }
}
