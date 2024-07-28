# IPK Project #2 - Network sniffer

A cross-platform command line network sniffer with functionality modeled after Wireshark.

## Program structure

##### sniffer
- `Program.cs` - Sniffer class - starting point of the client
- `ArgParser.cs` - ArgParser class - parser for CLI arguments
- `NetworkListener.cs` - NetworkListener class - processes and prints received packets

##### PacketSender
- `Program.cs` - PacketSender class - sends example packets

## Implementation details

### Sniffer
The sniffer component is implemented to capture and display network packets. It's main functionality is handled by the `NetworkListener` class, which sets up the capture device, configures filters, and handles the packet arrival event to print the captured packets. The `ArgParser` class is responsible for parsing command-line arguments, such as the network interface to listen on and any filters to apply.

### PacketSender
The PacketSender component is a simple utility whose purpose is to test the sniffer's functionality by sending various types of packets. The type of packet to send is selected through a menu interface, making it easy to test different scenarios.

**Both components utilize the following libraries**
- **SharpPcap**: A packet capture framework for .NET, providing an API for capturing packets.
- **PacketDotNet**: A .NET library that works with SharpPcap to decode packets and present them in an easy-to-use format.

## Installation and usage
The installation process is the same for Linux and Windows, .NET 8 SDK or later is required. Must be built with elevated privileges.
  
1. Clone the repository: `git clone https://github.com/stan-leetasi/network-sniffer.git`
2. Build the projects: `dotnet build`
3. Executables will be located in: `sniffer/bin/Debug/net8.0` and `PacketSender/bin/Debug/net8.0`

### Sniffer usage
Must be executed with elevated privileges to assure proper capture device access. The program can be terminated at any given moment with the Ctrl + C sequence.

```
./sniffer [-i interface] {-p|--port-source|--port-destination port [--tcp|-t] [--udp|-u]} [--arp] [--ndp] [--icmp4] [--icmp6] [--igmp] [--mld] {-n num}
```
All arguments can be in any order. If no protocol is specified, all traffic will be printed.

* `-i eth0` capture interface. If this parameter is not used, an interface can also be selected at program startup by inputting the interface's number.
* `-n 10` number of packets to display. If this parameter is not used, the sniffer will run indefinitely.
* `-t` or `--tcp` TCP segments
* `-u` or `--udp` UDP datagrams
* `-p` filters TCP/UDP based on port number. The given port can occur in source OR destination part of TCP/UDP headers.
* `--port-destination 23` only destination port (higher priority than `-p`)
* `--port-source 23` only source port (higher priority than `-p`)
* `--icmp4`
* `--icmp6`
* `--arp`
* `--ndp`
* `--igmp`
* `--mld`

### PacketSender usage
Must be executed with elevated privileges to assure proper capture device access.

`./PacketSender`

The interface can be selected at startup from the numbered active interface list. Then by pressing the keys `0-8`, different types of packets will be sent using the selected interface.