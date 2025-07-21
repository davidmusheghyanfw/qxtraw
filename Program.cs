// using Quixant.LibRAV;

// var server = new TcpServer(5000);
// var deviceManager = new DeviceManager();

// // Start devices
// deviceManager.StartAllDevices();

// Console.WriteLine("✅ Server started. Waiting for Unity client...");

// // Accept Unity connection
// await server.WaitForClientAsync();

// Console.WriteLine("🎮 Unity client connected!");

// while (true)
// {
//     Console.Write("Enter message: ");
//     var input = Console.ReadLine();

//     if (string.IsNullOrWhiteSpace(input))
//         continue;

//     await server.SendMessageAsync(input);
// }
using System.Drawing;
using Quixant.LibRAV;

class Program
{
    private static bool exitRequested = false;

    static async Task Main(string[] args)
    {
        // var server = new TcpServer(5000);

        Console.WriteLine("Starting device...");
        Console.WriteLine("\nDeviceManager Interactive Console");
        Console.WriteLine("==================================");
        Console.WriteLine("Commands:");
        Console.WriteLine("  [1] Start poll");
        Console.WriteLine("  [2] Stop poll");
        Console.WriteLine("  [3] Return bill");
        Console.WriteLine("  [4] Stack bill extension");
        Console.WriteLine("  [5] Exit");
        Console.WriteLine();
        // var deviceManager = new DeviceManager((port) => new MEIDeviceAdapter(port));
        var deviceManager = new DeviceManager((port) => new JCMDeviceAdapter(port));
        var deviceThread = new Thread(() =>
               {
                   deviceManager.Initalize();

               });

        deviceThread.Start();

        var inputThread = new Thread(() => InputLoop(deviceManager));

        inputThread.Start();

        var nfcReader = new NFCReader();
        var nfcThread = new Thread(() =>
        {
            nfcReader.Init();
        });

        nfcThread.Start();


        var ledController = new LEDController();
        var ledThread = new Thread(() =>
        {
            ledController.Init();
            ledController.StopAllLoops();
            ledController.SetSolidColor(0, Color.Red);
            ledController.SetSolidColor(1, Color.Blue);
            ledController.SetSolidColor(2, Color.Orange);
            ledController.SetSolidColor(3, Color.Green);
        });

        nfcThread.Start();

        Console.WriteLine("✅ Server started. Waiting for Unity client...");

        // Wait for Unity connection
        // await server.WaitForClientAsync();

        Console.WriteLine("🎮 Unity client connected!");

    }

    private static void InputLoop(DeviceManager manager)
    {
        while (!exitRequested)
        {
            Console.Write("Main InputLoop() Enter command number: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            switch (input)
            {
                case "1":
                    manager.StartPolling();
                    break;

                case "2":
                    manager.StopPolling();
                    break;

                case "3":
                    manager.ReturnBill();
                    break;

                case "4":
                    manager.StackBill();
                    break;

                case "5":
                    Console.WriteLine("Exiting...");
                    exitRequested = true;
                    manager.StopPolling();
                    return;

                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }
    }
}
