using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace zeta;

using System;
using System.Linq;

using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;

public class NetworkListener {
    
    /// <summary>
    /// Number of displayed packets in the current run
    /// </summary>
    private static int _displayedPackets = 0;
    
    /// <summary>
    /// Number of displayed packets in the current run
    /// </summary>
    private static int _packetLimit = 0;

    /// <summary>
    /// Indicates if the listener is running at the moment
    /// </summary>
    public static bool Running = false;

    /// <summary>
    /// Device which captures received packets
    /// </summary>
    private ICaptureDevice? _listener;
    
        /// <summary>
        /// Starts the network listener with options parsed from CLI arguments.
        /// </summary>
        /// <param name="devices">List of available interfaces.</param>
        /// <param name="argParserInstance">Contains CLI argument values.</param>
        /// <returns>Integer representing the sniffer startup process success/failure.</returns>
        public int StartListener(CaptureDeviceList devices, ArgParser argParserInstance)
        {
            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.Error.WriteLine("ERR: No interfaces found!");
                return 1;
            }

            if (argParserInstance.Interface == null)
            {
                _listener = devices.FirstOrDefault(d => d.Name == argParserInstance.InterfaceName);
                if (_listener == null)
                {
                    Console.Error.WriteLine("ERR: Invalid device name");
                    return 1;
                }
            }
            else
                _listener = argParserInstance.Interface;
            
            // Register handler function to the 'packet arrival' event
            _listener.OnPacketArrival += OnPacketArrival;
            
            _listener.Open(DeviceModes.Promiscuous); // Open the device for capturing
            
            string filter = ConstructFilter(argParserInstance); // Apply filter if not empty
            if (!string.IsNullOrEmpty(filter))
                _listener.Filter = filter; 
   
            Running = true;
            _packetLimit = argParserInstance.PacketCountDisplay;
            
            Console.WriteLine();
            Console.WriteLine($"Listening on {_listener.Name}, press Ctrl+C to stop");

            // Start the capturing process
            _listener.StartCapture();

            return 0;
        }

        /// <summary>
        /// Stops the listener
        /// </summary>
        public void StopListener()
        {
            _listener?.StopCapture();
            Running = false;
        }
        
        /// <summary>
        /// Prints received packets on stdout.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">The captured packet.</param>
        private static void OnPacketArrival(object sender, PacketCapture e)
        {
            if (!Running) return;

            var rawCapture = e.GetPacket();
            var time = rawCapture.Timeval.Date;
            var len = rawCapture.Data.Length;

            var ethPacket = Packet.ParsePacket(LinkLayers.Ethernet, rawCapture.Data).Extract<EthernetPacket>();
            var ipPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data).Extract<IPPacket>();
            
            System.Net.IPAddress? srcIp = null;
            System.Net.IPAddress? dstIp = null;
            PhysicalAddress? srcMac = null;
            PhysicalAddress? dstMac = null;
            
            ArpPacket? arpPacket = null;
            ArpOperation? arpOperation = null;

            if (ethPacket.PayloadPacket is ArpPacket) // Extract arp packet data
            {
                arpPacket = (ArpPacket?)ethPacket.PayloadPacket; 
                srcIp = arpPacket?.SenderProtocolAddress;
                dstIp = arpPacket?.TargetProtocolAddress;
                srcMac = arpPacket?.SenderHardwareAddress;
                dstMac = arpPacket?.TargetHardwareAddress;
                arpOperation = arpPacket?.Operation;
            }
            else
            {
                srcIp = ipPacket?.SourceAddress;
                dstIp = ipPacket?.DestinationAddress;
                srcMac = ethPacket?.SourceHardwareAddress;
                dstMac = ethPacket?.DestinationHardwareAddress;
            }
            var srcMacStr = FormatMac(srcMac);
            var dstMacStr = FormatMac(dstMac);

            // Extract ports if the packet is udp or tcp
            var srcPort = ipPacket?.PayloadPacket?.GetType().Name switch
            {
                "UdpPacket" => (ipPacket.PayloadPacket as UdpPacket)?.SourcePort,
                "TcpPacket" => (ipPacket.PayloadPacket as TcpPacket)?.SourcePort,
                _ => null
            };
            var dstPort = ipPacket?.PayloadPacket?.GetType().Name switch
            {
                "UdpPacket" => (ipPacket.PayloadPacket as UdpPacket)?.DestinationPort,
                "TcpPacket" => (ipPacket.PayloadPacket as TcpPacket)?.DestinationPort,
                _ => null
            };

            // Print the packet information
            Console.WriteLine($"timestamp: {time}");
            if (srcMac != null)
                Console.WriteLine($"src MAC: {srcMacStr}");
            if(dstMac != null)
                Console.WriteLine($"dst MAC: {dstMacStr}");

            Console.WriteLine($"frame length: {len} bytes");

            if(srcIp != null)
                Console.WriteLine($"src IP: {srcIp}");
            if(dstIp != null)
                Console.WriteLine($"dst IP: {dstIp}");
            if(srcPort != null)
                Console.WriteLine($"src port: {srcPort}");
            if(dstPort != null)
                Console.WriteLine($"dst port: {dstPort}");
            
            if (arpPacket != null)
                Console.WriteLine("type: ARP");
            if(arpOperation != null)
                Console.WriteLine($"arp operation: {arpOperation}");
            
            if (ethPacket?.PayloadPacket is IPv4Packet ipv4Packet)
            {
                Console.WriteLine("Network Protocol: IPv4");
                Console.WriteLine($"type: {ipv4Packet.Protocol}");
            }

            if (ethPacket?.PayloadPacket is IPv6Packet ipv6Packet)
            {
                Console.WriteLine("Network Protocol: IPv6");
                Console.WriteLine($"type: {ipv6Packet.Protocol}");
                if (ipv6Packet.PayloadPacket is IcmpV6Packet icmpV6Packet)
                {
                    switch (icmpV6Packet.Type)
                    {
                        case IcmpV6Type.RouterAdvertisement:
                        case IcmpV6Type.RouterSolicitation:
                        case IcmpV6Type.NeighborAdvertisement:
                        case IcmpV6Type.NeighborSolicitation:
                            Console.WriteLine("subtype: NDP");
                            break;
                    
                        case IcmpV6Type.MulticastListenerQuery:
                        case IcmpV6Type.MulticastListenerReport:
                        case IcmpV6Type.MulticastListenerDone:
                            Console.WriteLine("subtype: MLD");
                            break;
                    }
                }
            }
            
            Console.WriteLine(); // Empty line for output formatting
            
            PrintData(rawCapture.Data, rawCapture.PacketLength); 
            
            if (_displayedPackets == _packetLimit-1)
                Running = false;
            
            _displayedPackets++;
        }
        
        /// <summary>
        /// Formats the mac address, if not correctly formatted.
        /// </summary>
        /// <param name="macAddress">Mac address object.</param>
        /// <returns>Formatted mac address string.</returns>
        private static string FormatMac(PhysicalAddress? macAddress)
        {
            if (macAddress == null)
                return "";

            string macStr = macAddress.ToString();

            if (macStr.Contains(":"))
                return macStr;
            
            macStr = string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                macStr.Substring(0, 2),
                macStr.Substring(2, 2),
                macStr.Substring(4, 2),
                macStr.Substring(6, 2),
                macStr.Substring(8, 2),
                macStr.Substring(10, 2));

            return macStr;
        }

        /// <summary>
        /// Formats and prints the data from a packet to stdout.
        /// </summary>
        /// <param name="packetData">Data from the packet in a byte[] format.</param>
        /// <param name="packetLength">Length of the packet.</param>
        private static void PrintData(byte[] packetData, int packetLength)
        {
            for (int i = 0; i < packetLength; i += 16)
            {
                Console.Write($"0x{i:X4}: "); // Line number

                // Print 16 bytes of data in hexadecimal form
                int lineCount = 0;
                for (int j = i; j < Math.Min(i + 16, packetLength); j++)
                {
                    Console.Write($"{packetData[j]:X2} ");
                    lineCount++;
                }

                // Add padding
                int padding = 48 - lineCount*3;
                if (padding > 0)
                    Console.Write(new string(' ', padding));

                // Print 16 bytes of data in ASCII form
                for (int j = i; j < Math.Min(i + 16, packetLength); j++)
                {
                    char c = (char)packetData[j];
                    Console.Write(char.IsControl(c) ? '.' : c);
                }

                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Constructs a filter according to tpcdump's filter syntax.
        /// <returns>String representing the constructed filter.</returns>
        /// </summary>
        private string ConstructFilter(ArgParser argParser)
        {
            string ports = "";
            string protocols = "";

            if (argParser.Port != 0)
            {
                // If only -p is specified, packets from port source OR destination will be captured
                if (argParser.PortSource == 0 && argParser.PortDestination == 0)
                {
                    ports += $"src port {argParser.Port} or dst port {argParser.Port} ";
                }
                else // -p will overwrite empty port source or port destination values
                {
                    if (argParser.PortSource == 0)
                        argParser.PortSource = argParser.Port;

                    if (argParser.PortDestination == 0)
                        argParser.PortDestination = argParser.Port;
                }
            }
            
            if (argParser.PortSource != 0)
            {
                ports += $"src port {argParser.PortSource} ";
            }

            if (argParser.PortDestination != 0)
            {
                if (! string.IsNullOrEmpty(ports))
                    ports += "and ";
                
                ports += $"dst port {argParser.PortDestination} ";
            }
            
            if (argParser.Tcp)
            {
                protocols += "tcp ";
            }

            if (argParser.Udp)
            {
                if (! string.IsNullOrEmpty(protocols))
                    protocols += "or ";
                
                protocols += "udp ";
            }

            if (argParser.Icmp4)
            {
                if (! string.IsNullOrEmpty(protocols))
                    protocols += "or ";
                
                protocols += "icmp ";
            }

            if (argParser.Icmp6)
            {
                if (!string.IsNullOrEmpty(protocols))
                    protocols += "or ";
                
                protocols += "icmp6 ";
            }

            if (argParser.Arp)
            {
                if (!string.IsNullOrEmpty(protocols))
                    protocols += "or ";
                
                protocols += "arp ";
            }

            if (argParser.Ndp)
            {
                if (!string.IsNullOrEmpty(protocols))
                    protocols += "or ";

                protocols += "(icmp6 and (icmp6[0] == 133 or icmp6[0] == 134 or icmp6[0] == 135 or icmp6[0] == 136 or icmp6[0] == 137)) ";
            }

            if (argParser.Igmp)
            {
                if (!string.IsNullOrEmpty(protocols))
                    protocols += "or ";
                
                protocols += "igmp ";
            }

            if (argParser.Mld)
            {
                if (!string.IsNullOrEmpty(protocols))
                    protocols += "or ";
                
                protocols += "(icmp6 and (icmp6[0] == 130 or icmp6[0] == 131 or icmp6[0] == 132 or icmp6[0] == 143)) ";
            }

            if (string.IsNullOrEmpty(ports))// No port was specified
            {
                protocols = protocols.Trim();
                return protocols;
            }

            if (!string.IsNullOrEmpty(protocols)) // Ports and protocols were specified
                ports += "and " + protocols;

            ports = ports.Trim();
            return ports;
        }
    }
