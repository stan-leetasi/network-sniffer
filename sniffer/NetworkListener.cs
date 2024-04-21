using System.Collections.Generic;

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
    
        public int StartListener(CaptureDeviceList devices, ArgParser argParserInstance)
        {
            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.Error.WriteLine("ERR: No interfaces found!");
                return 1;
            }
            
            _listener = devices.FirstOrDefault(d => d.Name == argParserInstance.Interface);
            if (_listener == null)
            {
                Console.Error.WriteLine("ERR: Invalid device name");
                return 1;
            }
            
            // Register handler function to the 'packet arrival' event
            _listener.OnPacketArrival += device_OnPacketArrival;
            
            _listener.Open(DeviceModes.Promiscuous); // Open the device for capturing
            
            string filter = ConstructFilter(argParserInstance); // Apply filter
            Console.WriteLine(filter);
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

        public void StopListener()
        {
            _listener?.StopCapture();
            Running = false;
        }

        /// <summary>
        /// Prints received packets on stdout
        /// </summary>
        private static void device_OnPacketArrival(object sender, PacketCapture e)
        {
            if (!Running) return;
            
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var time = e.GetPacket().Timeval.Date;
            var len = e.GetPacket().Data.Length;
            var ethPacket = packet.Extract<EthernetPacket>();
            var ipPacket = packet.Extract<IPPacket>();

            var srcIp = ipPacket?.SourceAddress;
            var dstIp = ipPacket?.DestinationAddress;
            var srcMac = ethPacket?.SourceHardwareAddress;
            var dstMac = ethPacket?.DestinationHardwareAddress;
            
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
                Console.WriteLine($"src MAC: {srcMac}");
            if(dstMac != null)
                Console.WriteLine($"dst MAC: {dstMac}");
            Console.WriteLine($"frame length: {len} bytes");
            if(srcIp != null)
                Console.WriteLine($"src IP: {srcIp}");
            if(dstIp != null)
                Console.WriteLine($"dst IP: {dstIp}");
            if(srcPort != null)
                Console.WriteLine($"src port: {srcPort}");
            if(dstPort != null)
                Console.WriteLine($"dst port: {dstPort}");
            Console.WriteLine();
            
            // Print the packet data
            PrintData(e.GetPacket().Data, e.GetPacket().PacketLength); 
            
            if (_displayedPackets == _packetLimit-1)
            {
                Running = false;
            }

            _displayedPackets++;
        }

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
        /// Constructs a filter according to tpcdump's filter syntax
        /// </summary>
        private string ConstructFilter(ArgParser argParser)
        {
            string ports = "";
            string protocols = "";
            
            if (argParser.PortSource != 0)
            {
                if (string.IsNullOrEmpty(ports))
                    ports += $"src port {argParser.PortSource} ";
            }

            if (argParser.PortDestination != 0)
            {
                if (string.IsNullOrEmpty(ports))
                    ports += $"dst port {argParser.PortSource} ";
                else
                    ports += $"and dst port {argParser.PortSource} ";
            }
            
            if (argParser.Tcp)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "tcp ";
            }

            if (argParser.Udp)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "udp ";
                else
                    protocols += "or udp ";
            }

            if (argParser.Icmp4)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "icmp ";
                else
                    protocols += "or icmp ";
            }

            if (argParser.Icmp6)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "icmp6 ";
                else
                    protocols += "or icmp6 ";
            }

            if (argParser.Arp)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "arp ";
                else
                    protocols += "or arp ";
            }

            if (argParser.Ndp)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "ndp ";
                else
                    protocols += "or ndp ";
            }

            if (argParser.Igmp)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "igmp ";
                else
                    protocols += "or igmp ";
            }

            if (argParser.Mld)
            {
                if (string.IsNullOrEmpty(protocols))
                    protocols += "mld ";
                else
                    protocols += "or mld ";
            }

            if (string.IsNullOrEmpty(ports)) // No port was specified
                return protocols;
            
            if (!string.IsNullOrEmpty(protocols)) // Ports and protocols were specified
                ports += "and " + protocols;

            return ports;
        }
    }
