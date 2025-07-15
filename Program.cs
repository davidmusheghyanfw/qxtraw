using Quixant.LibRAV;

var server = new TcpServer(5000);
var deviceManager = new DeviceManager();

// Start devices
deviceManager.StartAllDevices();

Console.WriteLine("✅ Server started. Waiting for Unity client...");

// Accept Unity connection
await server.WaitForClientAsync();

Console.WriteLine("🎮 Unity client connected!");

while (true)
{
    Console.Write("Enter message: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    await server.SendMessageAsync(input);
}
