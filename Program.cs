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
        Console.WriteLine("  [4] Stack bill (TEST)");
        Console.WriteLine("  [5] Stack bill extension");
        Console.WriteLine("  [6] Exit");
        Console.WriteLine();
        var deviceManager = new DeviceManager();

        var deviceThread = new Thread(() =>
               {
                   deviceManager.StartAllDevices();

               });

        deviceThread.Start();

        var inputThread = new Thread(() => InputLoop(deviceManager));

        inputThread.Start();

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
                    manager.StackBillTEST();
                    break;

                case "5":
                    manager.StackBill();
                    break;

                case "6":
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
