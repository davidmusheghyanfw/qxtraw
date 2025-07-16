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

    static void Main(string[] args)
    {
        // Configure your port here
        string portName = "/dev/ttyS3"; // e.g., COM3 on Windows, /dev/ttyS3 on Linux

        // Create MEI device
        var device = new RAVDevice(SerialPortIndex.FromString(portName), ProtocolIdentifier.MEI);

        Console.WriteLine($"Opening port {portName}...");
        device.Open(portName);

        Console.WriteLine("Sending Soft Reset...");
        device.Execute(new MEICommand(MEIInstruction.SoftReset, 0, 0));

        // Wait for device initialization
        var stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);
        int tries = 0;
        while (tries < 30)
        {
            try
            {
                device.Get(stdHostToAcc);
                Console.WriteLine("Initialization complete.");
                break;
            }
            catch
            {
                Console.Write(".");
                Thread.Sleep(200);
                tries++;
            }
        }

        if (tries == 30)
        {
            Console.WriteLine("\nTimeout waiting for initialization.");
            return;
        }

        // Enable Extended Note Reporting (0x01 = Extended CFSC)
        var setNoteReporting = new MEICommand(MEIInstruction.SetExtendedNoteReporting, 1, 0);
        setNoteReporting.InputBuffer[0] = 0x01;
        device.Set(setNoteReporting);

        // Enable all banknotes
        var setExtendedNote = new MEIExtendedCommand(MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);
        for (int i = 0; i < 8; i++)
            setExtendedNote.InputBuffer[i] = 0xFF;
        device.Set(setExtendedNote);

        // Enable Barcode Decoding
        var setBarcode = new MEICommand(MEIInstruction.SetBarcodeDecoding, 1, 0);
        setBarcode.InputBuffer[0] = 0x01;
        device.Set(setBarcode);

        // Enable Escrow
        var setEscrow = new MEICommand(MEIInstruction.SetEscrowMode, 1, 0);
        setEscrow.InputBuffer[0] = 0x01;
        device.Set(setEscrow);

        Console.WriteLine("Initialization complete. Starting polling...");

        // Start polling
        while (true)
        {
            try
            {
                uint outLen = device.Get(stdHostToAcc);

                byte header = stdHostToAcc.OutputBuffer[0];

                if ((header & 0xF0) == (int)MEIInstruction.StdAccToHost)
                {
                    // Standard message (banknote)
                    uint status = BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1);
                    Console.WriteLine($"[STD] Status: 0x{status:X8}");

                    if (((MeiStatus)status & MeiStatus.Escrowed) == MeiStatus.Escrowed)
                    {
                        Console.WriteLine("Escrowed banknote detected.");
                        int denomination = (stdHostToAcc.OutputBuffer[3] & 0x38) >> 3;
                        Console.WriteLine($"Denomination: {denomination}");

                        Console.WriteLine("Sending Stack command...");
                        Thread.Sleep(500);
                        device.Execute(new MEICommand(MEIInstruction.Stack, 0, 0));
                    }
                }
                else if ((header & 0xF0) == (int)MEIInstruction.ExtendedMsgSet)
                {
                    byte subtype = stdHostToAcc.OutputBuffer[1];
                    if (subtype == (byte)MEIMessageExtendedSubtype.ExtendedBarcodeReply)
                    {
                        Console.Write("[EXT] Barcode detected: ");
                        for (int i = 8; i < 28 && stdHostToAcc.OutputBuffer[i] != 0x28; i++)
                            Console.Write((char)stdHostToAcc.OutputBuffer[i]);
                        Console.WriteLine();

                        Console.WriteLine("Sending Stack command...");
                        Thread.Sleep(500);
                        device.Execute(new MEICommand(MEIInstruction.Stack, 0, 0));
                    }
                    else
                    {
                        Console.WriteLine($"[EXT] Received unhandled subtype: 0x{subtype:X2}");
                    }
                }
                else
                {
                    Console.WriteLine($"[???] Unknown message header: 0x{header:X2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Polling error: {ex.Message}");
            }

            Thread.Sleep(200);
        }
    }
}
}
