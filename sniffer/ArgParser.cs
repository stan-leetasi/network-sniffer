namespace zeta;

using System;
using System.Linq;

/// <summary>
/// Parses startup command-line arguments.
/// </summary>
public class ArgParser
{
    /// <summary>
    /// Interface to sniff from
    /// </summary>
    public string Interface { get; set; } = "";
    
    /// <summary>
    /// TCP protocol
    /// </summary>
    public bool Tcp { get; set; } = false;
    
    /// <summary>
    /// UDP protocol
    /// </summary>
    public bool Udp { get; set; } = false;
    
    /// <summary>
    /// Server port for source or destination
    /// </summary>
    public ushort Port { get; set; } = 0;
    
    /// <summary>
    /// Server port source
    /// </summary>
    public ushort PortSource { get; set; } = 0;
    
    /// <summary>
    /// Server port destination
    /// </summary>
    public ushort PortDestination { get; set; } = 0;
    
    /// <summary>
    /// Display only ICMPv4 packets
    /// </summary>
    public bool Icmp4 { get; set; } = false;
    
    /// <summary>
    /// Display only ICMPv6 echo/request response
    /// </summary>
    public bool Icmp6 { get; set; } = false;
    
    /// <summary>
    /// Display only ARP frames
    /// </summary>
    public bool Arp { get; set; } = false;
    
    /// <summary>
    /// Display only NDP packets
    /// </summary>
    public bool Ndp { get; set; } = false;
    
    /// <summary>
    /// Display only IGMP packets
    /// </summary>
    public bool Igmp { get; set; } = false;
    
    /// <summary>
    /// Display only MLD packets
    /// </summary>
    public bool Mld { get; set; } = false;
    
    /// <summary>
    /// Number of packets to display
    /// </summary>
    public int PacketCountDisplay { get; set; } = 1;
    
    /// <summary>
    /// Help message
    /// </summary>
    public bool ShowHelp = false;

    /// <summary>
    /// Indicates to print the available interfaces
    /// </summary>
    public bool ShowInterfaces = false;
    
    /// <summary>
    /// List of parameters
    /// </summary>
    private readonly string[] _parameters =
        ["-i", "--interface", "-t", "--tcp", "-u", "--udp",
         "-p", "--port-destination", "--port-source", "--icmp4",
         "--icmp6", "--arp", "--ndp", "--igmp", "--mld", "-n" ];

    /// <summary>
    /// Parse command-line arguments.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>0 on success, 1 on failure.</returns>
    public int Parse(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i" or "--interface":
                    if (i+1 == args.Length || _parameters.Contains(args[i+1]))
                    {
                        ShowInterfaces = true;
                        return 0;
                    }
                    Interface = args[++i];
                    break;
                case "-p":
                    if (i+1 == args.Length || _parameters.Contains(args[i+1]))
                    {
                        Console.Error.WriteLine($"ERR: Missing value for argument {args[i]}");
                        return 1;
                    }
                    Port = ushort.Parse(args[++i]);
                    break;
                case "--port-source":
                    if (i+1 == args.Length || _parameters.Contains(args[i+1]))
                    {
                        Console.Error.WriteLine($"ERR: Missing value for argument {args[i]}");
                        return 1;
                    }
                    PortSource = ushort.Parse(args[++i]);
                    break;
                case "--port-destination":
                    if (i+1 == args.Length || _parameters.Contains(args[i+1]))
                    {
                        Console.Error.WriteLine($"ERR: Missing value for argument {args[i]}");
                        return 1;
                    }
                    PortDestination = ushort.Parse(args[++i]);
                    break;
                case "--tcp" or "-t":
                    Tcp = true;
                    break;
                case "--udp" or "-u":
                    Udp = true;
                    break;
                case "--arp":
                    Arp = true;
                    break;
                case "--ndp":
                    Ndp = true;
                    break;
                case "--icmp4":
                    Icmp4 = true;
                    break;
                case "--icmp6":
                    Icmp6 = true;
                    break;
                case "--igmp":
                    Igmp = true;
                    break;
                case "--mld":
                    Mld = true;
                    break;
                case "-n":
                    if (i+1 == args.Length || _parameters.Contains(args[i+1]))
                    {
                        Console.Error.WriteLine($"ERR: Missing value for argument {args[i]}");
                        return 1;
                    }
                    PacketCountDisplay = int.Parse(args[++i]);
                    break;
                case "-h":
                    ShowHelp = true;
                    break;
                default:
                    Console.Error.WriteLine($"ERR: Unknown argument: {args[i]}");
                    return 1;
            }
        }

        if (args.Length == 0 || Interface == "")
            ShowInterfaces = true;

        // Print help message if requested
        if (ShowHelp)
            PrintHelp();
        
        return 0;
    }

    /// <summary>
    /// Prints help message.
    /// </summary>
    private static void PrintHelp()
    {
        Console.WriteLine("Usage: ipk-sniffer [-i interface | --interface interface] {-p|--port-source|--port-destination port [--tcp|-t] [--udp|-u]} [--arp] [--ndp] [--icmp4] [--icmp6] [--igmp] [--mld] {-n num}\n");
        Console.WriteLine("Options:");
        Console.WriteLine("  -i, --interface    Specifies the network interface to sniff packets from. If not specified, a list of active interfaces is printed.");
        Console.WriteLine("  -t, --tcp          Display TCP segments.");
        Console.WriteLine("  -u, --udp          Display UDP datagrams.");
        Console.WriteLine("  -p                 Filter TCP/UDP based on port number.");
        Console.WriteLine("                     Can occur in source OR destination part of TCP/UDP headers.");
        Console.WriteLine("  --port-destination port");
        Console.WriteLine("                     Filter TCP/UDP based on destination port number.");
        Console.WriteLine("  --port-source port");
        Console.WriteLine("                     Filter TCP/UDP based on source port number.");
        Console.WriteLine("  --icmp4            Display only ICMPv4 packets.");
        Console.WriteLine("  --icmp6            Display only ICMPv6 echo request/response.");
        Console.WriteLine("  --arp              Display only ARP frames.");
        Console.WriteLine("  --ndp              Display only NDP packets (subset of ICMPv6).");
        Console.WriteLine("  --igmp             Display only IGMP packets.");
        Console.WriteLine("  --mld              Display only MLD packets (subset of ICMPv6).");
        Console.WriteLine("  -n                 Specifies the number of packets to display.");
        Console.WriteLine("                     If not specified, displays only one packet.\n");
        Console.WriteLine("Arguments can be in any order.");
    }
}