using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Helpers
{
    public static class ConsoleHelper
    {
        public static void ShowDevice(DeviceInfo device)
        {
            ShowProperyHorizantal("Host Name", device.Hostname);
            ShowProperyHorizantal("IpV4 Address", device.IpAddress);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(" connected. ");
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
