using System;
using System.Net;
using System.Net.NetworkInformation;
using PacketDotNet;
using SharpPcap;

public class PacketSender
{
    private static string srcIpv6 = "::1";
    private static string dstIpv6 = "::1";

    public static void Main(string[] args)
    {
        SendTcpPacket();
        SendUdpPacket();
        SendNdpNs();
        SendRouterSolicitation();
        SendArpRequest();
    }

    public static byte[] createMac(string address)
    {
        string[] octets = address.Split(':');
        
        byte[] macBytes = new byte[6];
        
        for (int i = 0; i < 6; i++)
        {
            macBytes[i] = Convert.ToByte(octets[i], 16);
        }

        return macBytes;
    }

    // Function to send TCP packet
    public static void SendTcpPacket()
    {
        var packet = new EthernetPacket(
            new PhysicalAddress(createMac("10:20:30:40:50:60")),  // Source MAC address
            new PhysicalAddress(createMac("00:00:00:00:00:00")),  // Destination MAC address
            EthernetType.IPv4);

        var ipPacket = new IPv4Packet(
                IPAddress.Parse("127.0.0.1"),  // Source IP address
                IPAddress.Parse("127.0.0.1"))   // Destination IP address
            {
                Protocol = ProtocolType.Tcp
            };
        
        var sourcePort = (ushort)100;
        var destinationPort = (ushort)4567;

        var tcpPacket = new TcpPacket(sourcePort, destinationPort);

        ipPacket.PayloadPacket = tcpPacket;
        packet.PayloadPacket = ipPacket;

        SendPacket(packet);
    }
    
    // Function to send UDP packet
public static void SendUdpPacket()
{
    var ethernetPacket = new EthernetPacket(
        new PhysicalAddress(createMac("60:50:40:30:20:10")),  // Source MAC address
        new PhysicalAddress(createMac("00:00:00:00:00:00")),  // Destination MAC address
        EthernetType.IPv4);

    var ipPacket = new IPv4Packet(
        IPAddress.Parse("127.0.0.1"),  // Source IP address
        IPAddress.Parse("127.0.0.1"))   // Destination IP address
    {
        Protocol = ProtocolType.Udp
    };
    
    var sourcePort = (ushort)100;
    var destinationPort = (ushort)4567;

    var udpPacket = new UdpPacket(sourcePort, destinationPort);

    ipPacket.PayloadPacket = udpPacket;
    ethernetPacket.PayloadPacket = ipPacket;

    SendPacket(ethernetPacket);
}

// Function to send NDP packet
public static void SendNdpNs()
{
    var ethernetPacket = new EthernetPacket(
        new PhysicalAddress(createMac("60:50:40:30:20:10")),  // Source MAC address
        new PhysicalAddress(createMac("00:00:00:00:00:00")),  // Destination MAC address
        EthernetType.IPv6);

    var ipPacket = new IPv6Packet(
        IPAddress.Parse("1.45.1.0"),  // Source IP address
        IPAddress.Parse("1.45.1.0"))   // Destination IP address
    {
        NextHeader = ProtocolType.IcmpV6
    };

    var icmpv6Packet = new IcmpV6Packet(
        new PacketDotNet.Utils.ByteArraySegment(new byte[] { 
            (byte)IcmpV6Type.NeighborSolicitation, 
            0,  // Code
            0, 0,  // Checksum (will be calculated automatically)
            // Additional data or options if needed
        }));


    ipPacket.PayloadPacket = icmpv6Packet;
    ethernetPacket.PayloadPacket = ipPacket;

    SendPacket(ethernetPacket);
}


// Function to send Router Solicitation
public static void SendRouterSolicitation()
{
    var ethernetPacket = new EthernetPacket(
        new PhysicalAddress(createMac("60:50:40:30:20:10")),  // Source MAC address
        new PhysicalAddress(createMac("00:00:00:00:00:00")),  // Destination MAC address
        EthernetType.IPv6);

    var ipPacket = new IPv6Packet(
        IPAddress.Parse(srcIpv6),  // Source IP address
        IPAddress.Parse("ff02::2"))   // Destination IP address
    {
        NextHeader = ProtocolType.IcmpV6
    };

    var macAddressBytes = createMac("00:00:00:00:00:00");

    var icmpv6Packet = new IcmpV6Packet(
        new PacketDotNet.Utils.ByteArraySegment(new byte[] { 
            (byte)IcmpV6Type.RouterSolicitation, // Type
            0,  // Code
            0, 0,  // Checksum (will be calculated automatically)
            1,  // Option type: Source Link-Layer Address
            1,  // Option length: 1 address (6 bytes)
            macAddressBytes[0], macAddressBytes[1], macAddressBytes[2], macAddressBytes[3], macAddressBytes[4], macAddressBytes[5] // MAC address
        }));

    ipPacket.PayloadPacket = icmpv6Packet;
    ethernetPacket.PayloadPacket = ipPacket;

    SendPacket(ethernetPacket);
}

// Function to send ARP request
public static void SendArpRequest()
{
    var arpPacket = new PacketDotNet.ArpPacket(
        ArpOperation.MarsJoin,
        PhysicalAddress.Parse("00:00:00:00:00:00"), // Sender hardware address
        IPAddress.Parse("127.0.0.1"),  // Sender protocol address
        PhysicalAddress.Parse("00:00:00:00:00:00"), // Target hardware address
        IPAddress.Parse("127.0.0.1"));  // Target protocol address

    SendPacket(arpPacket);
}


    // Function to send a packet
    private static void SendPacket(Packet packet)
    {
        // Get the default network device
        var devices = CaptureDeviceList.Instance;
        if (devices.Count < 1)
        {
            Console.WriteLine("No device found on this machine.");
            return;
        }

        var device = devices[0];

        // Open the device for sending
        device.Open();

        // Send the packet
        device.SendPacket(packet);

        // Close the device
        device.Close();
    }
    
}
