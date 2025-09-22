using System;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace StarOS.Network
{
    public static class Dhcp
    {
        public static void Release()
        {
            var client = new DHCPClient();
            client.SendReleasePacket();
            client.Close();

            NetworkConfiguration.ClearConfigs();

            Console.WriteLine("DHCP configuration released.");
        }

        public static bool Ask()
        {
            var client = new DHCPClient();
            if (client.SendDiscoverPacket() != -1)
            {
                client.Close();
                Console.WriteLine("DHCP configuration applied! Your local IPv4 Address is " + NetworkConfiguration.CurrentAddress + ".");
                return true;
            }
            else
            {
                NetworkConfiguration.ClearConfigs();
                client.Close();
                Console.WriteLine("DHCP request failed.");
                return false;
            }
        }
    }
}
