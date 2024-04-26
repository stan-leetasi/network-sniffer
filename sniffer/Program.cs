using System.Threading;
using PacketDotNet;

namespace zeta;

using System;
using System.Linq;

using SharpPcap;
using SharpPcap.LibPcap;

public static class Sniffer {
    
    /// <summary>
    /// Argument parser instance
    /// </summary>
    private static readonly ArgParser ArgParserInstance = new();

    /// <summary>
    /// Network listener instance
    /// </summary>
    private static NetworkListener _listener = new();
    
    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="cliArgs">Command-line arguments passed to the application.</param>
    /// <returns>Integer representing the program's exit code.</returns>
    public static int Main(string[] cliArgs)
    {
        // End the application when Ctrl+C is pressed
        var exitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true; // Prevent default termination
            _listener.StopListener();
            exitEvent.Set(); // Set the event to signal exit
            
            Environment.Exit(0);
        };
        
        if (ArgParserInstance.Parse(cliArgs) != 0) // Parse CLI arguments, if unsuccessful, exit
            return 1;

        // Exit after printing help message
        if (ArgParserInstance.ShowHelp) 
            return 0;
        
        // Retrieve the interfaces list
        var devices = CaptureDeviceList.Instance;
        
        if (ArgParserInstance.ShowInterfaces) // Print available interfaces
        {
            if (devices.Count < 1)
            {
                Console.Error.WriteLine("ERR: No interfaces found!");
                return 1;
            }

            Console.WriteLine("Available Interfaces:");
            Console.WriteLine("----------------------------------------");

            // Print out the available interfaces
            for (int i = 0; i < devices.Count; i++)
            {
                ICaptureDevice dev = devices[i];
                Console.WriteLine($"{dev.Name} - {dev.Description}");
            }

            return 0;
        }

        if (_listener.StartListener(devices, ArgParserInstance) != 0) // Start the listener
            return 1;
        
        // Run application until the packet display limit is reached or Ctrl+C is pressed
        while (NetworkListener.Running && !exitEvent.WaitOne(0))
        {
            Thread.Sleep(1000);
        }
        
        return 0;
    }

    
}