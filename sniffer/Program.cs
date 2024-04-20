using System;
using System.Linq;

namespace zeta;

using SharpPcap;
using SharpPcap.LibPcap;

public static class Sniffer {
    
    /// <summary>
    /// Argument parser
    /// </summary>
    public static ArgParser ArgParserInstance = new();


    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="cliArgs">Command-line arguments passed to the application.</param>
    /// <returns>An asynchronous task representing the program's exit code.</returns>
    public static int Main(string[] cliArgs)
    {
        // End the application when Ctrl+C is pressed
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true; // Prevent default termination
            
            Environment.Exit(0);
        };
        
        if (ArgParserInstance.Parse(cliArgs) != 0) // Parse CLI arguments, if unsuccessful, exit
            return 1;

        // Exit after printing help message or available interfaces
        if (ArgParserInstance.ShowHelp) 
            return 0;
        
        // Retrieve the device list
        var devices = LibPcapLiveDeviceList.Instance;
        
        if (ArgParserInstance.ShowInterfaces)
        {
            var nonEmptyDevices = devices.Where(dev => !string.IsNullOrWhiteSpace(dev.Description)).ToList();
            if (devices.Count < 1)
            {
                Console.WriteLine("No interfaces found!");
                return 1;
            }

            Console.WriteLine("Available Interfaces:");
            Console.WriteLine("---------------------");

            // Print out the available interfaces
            for (int i = 0; i < nonEmptyDevices.Count; i++)
            {
                ICaptureDevice dev = nonEmptyDevices[i];
                Console.WriteLine($"{dev.Name} - {dev.Description}");
            }
        }
        
        
        
    
        
        return 0;
    }
}