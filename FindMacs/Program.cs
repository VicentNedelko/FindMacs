using DreamNucleus.Heos;
using DreamNucleus.Heos.Commands.Player;
using DreamNucleus.Heos.Infrastructure.Heos;
using DreamNucleus.Heos.Infrastructure.Telnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FindMacs
{
    class Program
    {
        static async Task Main(string[] args)
        {

            List<MacIpPair> mip = new();
            List<MacIpPair> denons = new();

            System.Diagnostics.Process pProcess = new();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a ";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string cmdOutput = pProcess.StandardOutput.ReadToEnd();
            string pattern = @"(?<ip>([0-9]{1,3}\.?){4})\s*(?<mac>([a-f0-9]{2}-?){6})";

            foreach (Match m in Regex.Matches(cmdOutput, pattern, RegexOptions.IgnoreCase))
            {
                mip.Add(new MacIpPair()
                {
                    MacAddress = m.Groups["mac"].Value,
                    IpAddress = m.Groups["ip"].Value
                });
            }

            foreach(var machine in mip)
            {
                Console.WriteLine($"IP - {machine.IpAddress} --> MAC - {machine.MacAddress}");
                var mac = machine.MacAddress.Split(new char[] { '-' });
                if(mac[0] == "00" & mac[1] == "05" & mac[2] == "cd")
                {
                    denons.Add(machine);
                }
            }
            Console.WriteLine("-----------------------");
            foreach(var denon in denons)
            {
                Console.WriteLine($"Denon - {denon.IpAddress} --> {denon.MacAddress}");
                Ping ping = new();
                IPAddress ip = IPAddress.Parse(denon.IpAddress);
                PingReply reply = ping.Send(ip);
                if(reply.Status == IPStatus.Success)
                {
                    try
                    {
                        var telnetClient = new SimpleTelnetClient(denon.IpAddress);
                        var heosClient = new HeosClient(telnetClient, CancellationToken.None);
                        var commandProcessor = new CommandProcessor(heosClient);
                        var plyrs = await commandProcessor.Execute(new GetPlayersCommand());
                        foreach (var p in plyrs.Payload)
                        {
                            Console.WriteLine($"Player ID - {p.Pid}");
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Unable to establish TelNet connection to {ip}");
                    }
                    
                }
                else
                {
                    Console.WriteLine($"{denon.IpAddress} --> Status : {reply.Status}");
                }
            }
            Console.ReadKey();
        }
    }


    public struct MacIpPair
    {
        public string MacAddress;
        public string IpAddress;
    }
}
