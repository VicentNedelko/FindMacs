using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace FindMacs
{
    class Program
    {
        static void Main(string[] args)
        {
            var machines = NetworkInterface.GetAllNetworkInterfaces()
                .Where(m => m.OperationalStatus == OperationalStatus.Up);
            foreach(var machine in machines)
            {
                var IP = machine.GetIPProperties().UnicastAddresses.Last().Address;
                if(IP.GetAddressBytes().First() == 192)
                {
                    Console.WriteLine($"IP - {IP}");
                    Console.WriteLine($"Family - {IP.AddressFamily}");
                    Console.WriteLine($"Byte1 = {IP.GetAddressBytes().First()}");
                    Console.WriteLine($"MAC - {machine.GetPhysicalAddress().GetAddressBytes().First()}");
                    Console.WriteLine($"MAC - {machine.GetPhysicalAddress()}");
                    Console.WriteLine("--------------------");
                }
            }
            Console.ReadKey();
        }
    }
}
