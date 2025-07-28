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

    static void Main(string[] args)
    {
         var server = new TcpServer(5000);

        Console.WriteLine("Starting device...");
        Console.WriteLine("\nDeviceManager Interactive Console");
        Console.WriteLine("==================================");
        Console.WriteLine("Commands:");
        Console.WriteLine("  [1] Start poll");
        Console.WriteLine("  [2] Stop poll");
        Console.WriteLine("  [3] Return bill");
        Console.WriteLine("  [4] Stack bill extension");
        Console.WriteLine("  [5] Exit");
        Console.WriteLine("  [7] Animate Colors");
        Console.WriteLine("  [8] Dispose Led controller");
        Console.WriteLine("  [9] Print Demo Ticket");

        Console.WriteLine();
        // var deviceManager = new DeviceManager((port) => new MEIDeviceAdapter(port));
        var deviceManager = new DeviceManager((port) => new JCMDeviceAdapter(port));
        var deviceThread = new Thread(() =>
               {
                   deviceManager.Initalize();

               });

        deviceThread.Start();



        var nfcReader = new NFCReader();
        var nfcThread = new Thread(() =>
        {
            nfcReader.Init();
        });

        nfcThread.Start();

        nfcReader.OnCardInserted += async (sender, args) =>
        {
            Console.WriteLine($"[NFC] Tag Detected: {args}");
            await server.SendMessageAsync($"NFC:{args}");
            Thread.Sleep(1000);
        };

        server.OnMessageReceived += HandleUnityCommand;


        var ledController = new LEDController();

        var ledThread = new Thread(() =>
        {
            ledController.Init();
            ledController.ApplyPattern(0, new SolidColorPattern(Color.Blue));
            ledController.ApplyPattern(1, new HearthbeatPattern(Color.Red));
            ledController.ApplyPattern(2, new LoopFadePattern());
            ledController.ApplyPattern(3, new RainbowPattern());
        });

        ledThread.Start();
        IPrinter printerService = new JCMPrinterImpl();

        var printerThread = new Thread(() =>
              {
                  printerService.Init();
                  printerService.PrintDemoTicket();
              });

        printerThread.Start();

        var inputThread = new Thread(() => InputLoop(deviceManager, ledController, printerService, nfcReader));
        inputThread.Start();
        Console.WriteLine("✅ Server started. Waiting for Unity client...");

        // Wait for Unity connection
        // await server.WaitForClientAsync();

        Console.WriteLine("🎮 Unity client connected!");

    }

       private static void HandleUnityCommand(object? sender, string command)
    {
        Console.WriteLine($"[COMMAND FROM Client] Processing: {command}");
        string[] parts = command.Split(':');
        string commandType = parts[0].ToUpper();

        switch (commandType)
        {
            // ... (existing LED_ON, LED_OFF, LED_PATTERN, BILL_RETURN commands) ...

    
        }
    }

    private static void InputLoop(DeviceManager manager, LEDController ledController, IPrinter printerService, NFCReader nFCReader)
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
                    nFCReader.StopPolling();
                    return;
                case "7":
                    ledController.ApplyPatterns();
                    break;
                case "8":
                    ledController.DisposeController();
                    break;
                case "9":
                    printerService.PrintDemoTicket();
                    break;
                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }
    }
}
