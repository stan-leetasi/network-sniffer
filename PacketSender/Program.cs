using System.Net;
using System.Net.NetworkInformation;
using PacketDotNet;
using SharpPcap;

namespace PacketSender;

public abstract class PacketSender
{
    private static ILiveDevice? captureDevice;
    public static void Main(string[] args)
    {
        SelectDevice();
        PrintInfo();
        
        while (true) {
            var inputKey = Console.ReadKey(true);

            switch (inputKey.KeyChar)
            {
                case '0':
                    SendTcpPacket(); // Transport layer
                    break;
                case '1':
                    SendUdpPacket();
                    break;
                case '2':
                    SendIcmpv4Packet(); // Network layer
                    break;
                case '3':
                    SendIcmpv6Packet(); 
                    break;
                case '4':
                    SendNdpNs();
                    break;
                case '5':
                    SendNdpRS();
                    break;
                case '6':
                    SendMld();
                    break;
                case '7':
                    SendIgmpPacket();
                    break;
                case '8':
                    SendArpRequest(); // Link layer
                    break;
                default:
                    return;
            }
        }
    }

    public static byte[] CreateMac(string address)
    {
        return address.Split(':').Select(hex => Convert.ToByte(hex, 16)).ToArray();
    }
    
    public static void SendTcpPacket()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("10:20:30:40:50:60")),  // Source
            new PhysicalAddress(CreateMac("00:00:00:00:00:00")),  // Destination
            EthernetType.IPv4);

        var ipPacket = new IPv4Packet(
                IPAddress.Parse("127.0.0.1"),  // Source
                IPAddress.Parse("127.0.0.1"))   // Destination
            {
                Protocol = ProtocolType.Tcp
            };
        


        var tcpPacket = new TcpPacket(100, 4567);

        ConstructPacket(tcpPacket, ipPacket, ethernetPacket);
    }
    
    public static void SendUdpPacket()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("60:50:40:30:20:10")),  // Source
            new PhysicalAddress(CreateMac("00:00:00:00:00:00")),  // Destination
            EthernetType.IPv4);

        var ipPacket = new IPv4Packet(
                IPAddress.Parse("127.0.0.1"),  // Source
                IPAddress.Parse("127.0.0.1"))   // Destination
            {
                Protocol = ProtocolType.Udp
            };
        

        var udpPacket = new UdpPacket(100, 4567);

        ConstructPacket(udpPacket, ipPacket, ethernetPacket);
    }
    
    public static void SendIcmpv4Packet()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("60:50:40:30:20:10")),  // Source
            new PhysicalAddress(CreateMac("00:00:00:00:00:00")),  // Destination
            EthernetType.IPv4);

        var ipPacket = new IPv4Packet(
                IPAddress.Parse("127.0.0.1"),  // Source
                IPAddress.Parse("127.0.0.1"))   // Destination
            {
                Protocol = ProtocolType.Icmp
            };

        var icmpPacket = new IcmpV4Packet(
            new PacketDotNet.Utils.ByteArraySegment(new byte[] {
                8,  // Type for ICMP echo request
                0,  // Code 
                0, 0,  // Checksum
                0, 0x30, 0x39, 0, 0,   // Identifier 
                0, 1,  // Sequence number
            }));

        ConstructPacket(icmpPacket, ipPacket, ethernetPacket);
    }
    
    public static void SendIcmpv6Packet()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("60:50:40:30:20:10")),  // Source
            new PhysicalAddress(CreateMac("33:33:00:00:00:01")),  // Multicast address for all-nodes
            EthernetType.IPv6);

        var ipPacket = new IPv6Packet(
                IPAddress.Parse("fe80::1"),  // Source
                IPAddress.Parse("ff02::1"))   // Destination
            {
                Protocol = ProtocolType.IcmpV6
            };

        var icmpPacket = new IcmpV6Packet(
            new PacketDotNet.Utils.ByteArraySegment(new byte[] {
                128,  // Type for ICMP echo request
                0,  // Code 
                0, 0,  // Checksum
                0, 0x30, 0x39, 0, 0,   // Identifier 
                0, 1,  // Sequence number
            }));

        ConstructPacket(icmpPacket, ipPacket, ethernetPacket);
    }
    
    public static void SendNdpNs()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("60:50:40:30:20:10")),  // Source
            new PhysicalAddress(CreateMac("00:00:00:00:00:00")),  // Destination
            EthernetType.IPv6);

        var ipPacket = new IPv6Packet(
            IPAddress.Parse("fe80::1"),
            IPAddress.Parse("ff02::1"))
            {
                NextHeader = ProtocolType.IcmpV6
            };

        var icmpv6Packet = new IcmpV6Packet(
            new PacketDotNet.Utils.ByteArraySegment(new byte[] { 
                (byte)IcmpV6Type.NeighborSolicitation, 
                0,  // Code
                0, 0,  // Checksum 
            }));

        ConstructPacket(icmpv6Packet, ipPacket, ethernetPacket);
    }
    
    public static void SendNdpRS()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("60:50:40:30:20:10")),  // Source
            new PhysicalAddress(CreateMac("00:00:00:00:00:00")),  // Destination
            EthernetType.IPv6);

        var ipPacket = new IPv6Packet(
                IPAddress.Parse("fe80::1"),  // Source
                IPAddress.Parse("ff02::2"))   // Destination
            {
                NextHeader = ProtocolType.IcmpV6
            };

        var macAddressBytes = CreateMac("00:00:00:00:00:00");

        var icmpv6Packet = new IcmpV6Packet(
            new PacketDotNet.Utils.ByteArraySegment(new byte[] { 
                (byte)IcmpV6Type.RouterSolicitation, // Type
                0,  // Code
                0, 0,  // Checksum 
                1,  // Option type: Source Link-Layer Address
                1,  // Option length: 1 address (6 bytes)
                macAddressBytes[0], macAddressBytes[1], macAddressBytes[2], macAddressBytes[3], macAddressBytes[4], macAddressBytes[5]
            }));

        ConstructPacket(icmpv6Packet, ipPacket, ethernetPacket);
    }

    public static void SendMld()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("60:50:40:30:20:10")),  // Source
            new PhysicalAddress(CreateMac("00:00:00:00:00:00")),  // Destination
            EthernetType.IPv6);

        var ipPacket = new IPv6Packet(
                IPAddress.Parse("1.45.1.0"),  // Source
                IPAddress.Parse("1.45.1.0"))   // Destination
            {
                NextHeader = ProtocolType.IcmpV6
            };

        var icmpv6Packet = new IcmpV6Packet(
            new PacketDotNet.Utils.ByteArraySegment(new byte[] { 
                (byte)IcmpV6Type.MulticastListenerQuery, 
                0,  // Code
                0, 0,  // Checksum 
            }));

        ConstructPacket(icmpv6Packet, ipPacket, ethernetPacket);
    }

    public static void SendIgmpPacket()
    {
        var ethernetPacket = new EthernetPacket(
            new PhysicalAddress(CreateMac("60:50:40:30:20:10")),  // Source
            new PhysicalAddress(CreateMac("00:00:00:00:00:00")),  // Destination
            EthernetType.IPv4);

        var ipPacket = new IPv4Packet(
                IPAddress.Parse("127.0.0.1"),  // Source
                IPAddress.Parse("224.0.0.1"))   // Destination
        {
            Protocol = ProtocolType.Igmp
        };

        var igmpPacket = new IgmpV2Packet(
            new PacketDotNet.Utils.ByteArraySegment(new byte[]
            {
                (byte)IgmpMessageType.MembershipReportIGMPv2,
                0,
                0, 0,
                0, 0, 0, 0
            }));

        ConstructPacket(igmpPacket, ipPacket, ethernetPacket);
    }

    public static void SendArpRequest()
    {
        var arpPacket = new PacketDotNet.ArpPacket(
            ArpOperation.Request,
            PhysicalAddress.Parse("ff:ff:ff:ff:ff:ff"), // Target hardware address
            IPAddress.Parse("127.0.0.1"),  // Sender protocol address
            PhysicalAddress.Parse("00:00:00:00:00:00"), // Sender hardware address
            IPAddress.Parse("127.0.0.1"));  // Target protocol address
        
        var ethernetPacket = new PacketDotNet.EthernetPacket(
            PhysicalAddress.Parse("00:00:00:00:00:00"), // Sender hardware address
            PhysicalAddress.Parse("ff:ff:ff:ff:ff:ff"), // Destination MAC address (broadcast for ARP request)
            EthernetType.Arp)
        {
            PayloadPacket = arpPacket
        };
        
        ethernetPacket.UpdateCalculatedValues();
        SendPacket(ethernetPacket);
    }
    
    

    private static void ConstructPacket(Packet packet, Packet ipPacket, Packet ethernetPacket)
    {
        packet.UpdateCalculatedValues();
        ipPacket.PayloadPacket = packet;
        ipPacket.UpdateCalculatedValues();
        ethernetPacket.PayloadPacket = ipPacket;
        
        SendPacket(ethernetPacket);
    }

    private static void SelectDevice()
    {
        var devices = CaptureDeviceList.Instance;
        if (devices.Count < 1)
        {
            Console.WriteLine("No device found on this machine.");
            Environment.Exit(0);
        }
        
        Console.WriteLine("Select a device:");
        for (int i = 0; i < devices.Count; i++)
        {
            Console.WriteLine($"{i}: {devices[i].Name}: {devices[i].Description}");
        }

        string? input = Console.ReadLine();
        if (int.TryParse(input, out int deviceNum) && deviceNum >= 0 && deviceNum < devices.Count)
            captureDevice = devices[deviceNum];
        else
        {
            Console.Error.WriteLine("ERR: Invalid value");
            Environment.Exit(1);
        }
    }
    
    private static void SendPacket(Packet packet)
    {
        try
        {
            captureDevice.Open();
            captureDevice.SendPacket(packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERR: Failed to send packet: {ex.Message}");
        }
        finally
        {
            captureDevice?.Close();
        }
    }
    
    private static void PrintInfo(){
        Console.WriteLine("Press the corresponding key to send a packet of type:");
        Console.WriteLine("0: TCP");
        Console.WriteLine("1: UDP");
        Console.WriteLine("IcmpV4");
        Console.WriteLine("IcmpV6");
        Console.WriteLine("NDP neighbour solicitation");
        Console.WriteLine("NDP router solicitation");
        Console.WriteLine("MLD");
        Console.WriteLine("Igmp");
        Console.WriteLine("Arp");
    }
}