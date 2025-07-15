using System.Diagnostics;
using Quixant.LibRAV;

public class DeviceManager : IDisposable
{
    private readonly Dictionary<SerialPortIndex, RAVDevice> _devices = new();
    private readonly ProtocolIdentifier _defaultProtocol = ProtocolIdentifier.MEI;
    private readonly SerialPortIndex _defaultPort = SerialPortIndex.SerialPort4;
    static bool pollOn = false;
    static bool loopExtended = false;
    static bool measureTime = false;

    public DeviceManager()
    {
        _devices.Add(_defaultPort,
                  new RAVDevice(_defaultPort, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort1,
        //     new RAVDevice(SerialPortIndex.SerialPort1, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort2,
        //     new RAVDevice(SerialPortIndex.SerialPort2, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort3,
        //     new RAVDevice(SerialPortIndex.SerialPort3, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort4,
        //     new RAVDevice(SerialPortIndex.SerialPort4, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort5,
        //     new RAVDevice(SerialPortIndex.SerialPort5, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort6,
        //     new RAVDevice(SerialPortIndex.SerialPort6, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort7,
        //     new RAVDevice(SerialPortIndex.SerialPort7, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort8,
        //     new RAVDevice(SerialPortIndex.SerialPort8, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort9,
        //     new RAVDevice(SerialPortIndex.SerialPort9, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort10,
        //     new RAVDevice(SerialPortIndex.SerialPort10, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort11,
        //     new RAVDevice(SerialPortIndex.SerialPort11, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort12,
        //     new RAVDevice(SerialPortIndex.SerialPort12, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort13,
        //     new RAVDevice(SerialPortIndex.SerialPort13, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort14,
        //     new RAVDevice(SerialPortIndex.SerialPort14, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort15,
        //     new RAVDevice(SerialPortIndex.SerialPort15, _defaultProtocol));
        // _devices.Add(SerialPortIndex.SerialPort16,
        //     new RAVDevice(SerialPortIndex.SerialPort16, _defaultProtocol));

    }

    public void StartAllDevices()
    {
        foreach (var kvp in _devices)
        {
            InitDevice(kvp.Key);
        }
    }

    public void InitDevice(SerialPortIndex port)
    {
        if (_devices[port].IsOpen)
        {
            Console.WriteLine($"Port {port.Name} already open.");
            return;
        }

        Console.WriteLine($"Attempting to initialize {_defaultProtocol} device on port {port.Name}...");

        try
        {
            _devices[port].Protocol = _defaultProtocol;
            _devices[port].Open(port.Name);
            Console.WriteLine($"✅ Device on port {port.Name} initialized.");

            testMeiInitialize(_devices[port], MEIInstruction.InitAndPoll);
            MeiPoll(_devices[port]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing device: {ex.Message}");
        }
    }

    public void ClosePort(SerialPortIndex port)
    {
        StopPolling();
        if (_devices[port] == null || !_devices[port].IsOpen)
        {
            Console.WriteLine($"Port {port.Name} is not open.");
            return;
        }

        try
        {
            var protocol = _devices[port].Protocol;
            _devices[port].Dispose();
            _devices[port] = new RAVDevice(port, protocol);
            Console.WriteLine($"✅ Device on port {port.Name} closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error closing device: {ex.Message}");
        }
    }

    private static void testMeiInitialize(RAVDevice device, MEIInstruction instruction)
    {
        uint outLen;
        MEICommand reset = new MEICommand(MEIInstruction.SoftReset, 0, 0);
        MEICommand stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);
        MEICommand setDenom = new MEICommand(MEIInstruction.SetDenomination, 1, 0);
        MEICommand setInt = new MEICommand(MEIInstruction.SetSpecialInterruptMode, 1, 0);
        MEICommand setSec = new MEICommand(MEIInstruction.SetSecurity, 1, 0);
        MEICommand setOri = new MEICommand(MEIInstruction.SetOrientation, 2, 0);
        MEICommand setEscrow = new MEICommand(MEIInstruction.SetEscrowMode, 1, 0);
        MEICommand setPush = new MEICommand(MEIInstruction.SetPushMode, 1, 0);
        MEICommand setBar = new MEICommand(MEIInstruction.SetBarcodeDecoding, 1, 0);
        MEICommand setPup = new MEICommand(MEIInstruction.SetPowerup, 2, 0);
        MEICommand setNote = new MEICommand(MEIInstruction.SetExtendedNoteReporting, 1, 0);
        MEICommand setCpn = new MEICommand(MEIInstruction.SetExtendedCouponReporting, 1, 0);
        MEICommand stack = new MEICommand(MEIInstruction.Stack, 0, 0);
        setDenom.InputBuffer[0] = 0x7f;
        setInt.InputBuffer[0] = 0x00;
        setSec.InputBuffer[0] = 0x00;
        setOri.InputBuffer[0] = 0x03;
        setOri.InputBuffer[1] = 0x00;
        setEscrow.InputBuffer[0] = 0x01;
        setPush.InputBuffer[0] = 0x00;
        setBar.InputBuffer[0] = 0x01;
        setPup.InputBuffer[0] = 0x00;
        setPup.InputBuffer[1] = 0x00;
        setCpn.InputBuffer[0] = 0x00;

        try
        {
            int initWait = 0;

            device.Execute(reset);
            Console.Write("Waiting for the device to initialize...");

            while (initWait < 30)
            {
                try
                {
                    device.Get(stdHostToAcc);
                    Console.WriteLine("Initialization done");
                    break;
                }
                catch (Exception)
                {
                    Console.Write(".");
                    initWait++;
                    Thread.Sleep(200);
                }
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine("Operation failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setDenom);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set denomination failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setInt);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set interrupt failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setSec);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set security failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setOri);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set orientation failed: " + exc.Message);
            return;
        }

        try
        {
            setEscrow.RunOn(device); //alternate way of calling a command
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set escrow failed: " + exc.Message);
            return;
        }

        try
        {
            setPush.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set push failed: " + exc.Message);
            return;
        }

        try
        {
            setBar.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set barcode decoding failed: " + exc.Message);
            return;
        }

        try
        {
            setPup.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Set powerup failed: " + exc.Message);
            return;
        }

        try
        {
            switch (instruction)
            {
                case MEIInstruction.InitAndPoll:   // Normal mode
                    {
                        // Enable extended note reporting
                        setNote.InputBuffer[0] = 0x00;
                        setNote.RunOn(device);
                        break;
                    }
                case MEIInstruction.InitExtCfscAndPoll:   // Extended Note CFSC - 8 bytes of denomination
                    {
                        // Enable extended note reporting 
                        setNote.InputBuffer[0] = 0x01;
                        setNote.RunOn(device);

                        //Enable all the Bank Note
                        MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
                            (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);
                        device.Set(setExtendedNote);

                        break;
                    }
                case MEIInstruction.InitExtScaScrAndPoll:   // Extended Note SC Adv SCR - 19 bytes of denomination
                    {
                        // Enable extended note reporting
                        setNote.InputBuffer[0] = 0x02;
                        setNote.RunOn(device);

                        //Enable all the Bank Note
                        MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
                            (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);
                        device.Set(setExtendedNote);

                        break;
                    }

                default:
                    break;
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine("Init and Poll failed: " + exc.Message);
            return;
        }
        try
        {
            setCpn.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("Disable extended coupon reporting failed: " + exc.Message);
            return;
        }

        Console.Write("Test executed successfully\n");
    }

    public void StopPolling()
    {
        pollOn = false;
        Console.WriteLine("Polling stopped.");
    }
    public void EnablePolling()
    {
        pollOn = true;
        Console.WriteLine("Polling enabled.");
    }


    private static void MeiPoll(RAVDevice device)
    {
        uint outLen = 0;
        byte[] buffer = new byte[128];
        if (pollOn)
            Console.WriteLine("Start task (E7 command) send polling to Note Acceptor \n");

        MEICommand stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);
        MEICommand stack = new MEICommand(MEIInstruction.Stack, 0, 0);

        // Poll the device each 200 ms 
        while (pollOn)
        {
            // Standard host to acceptor poll. When using input length 0 the library fills in the
            // data with the current configuration
            outLen = device.Get(stdHostToAcc);

            /*
             (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1)) & MeiStatus.Escrowed)
             */
            if (outLen >= 5 && (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1)) & MeiStatus.Escrowed) == MeiStatus.Escrowed)
            {
                Console.WriteLine("Received escrowed event");
                Console.WriteLine("Sending stack command");
                Thread.Sleep(1000);
                stack.RunOn(device);
            }
            else if (outLen >= 10 && (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 2)) & MeiStatus.Escrowed) == MeiStatus.Escrowed)
            {
                Console.WriteLine("Received status Extended : 0x{0:X2} 0x{1:X2} 0x{2:X2}", buffer[0], buffer[1], buffer[2]);
                Console.WriteLine("Received escrowed event");
                Console.WriteLine("Sending stack command...");
                Thread.Sleep(1000);
                stack.RunOn(device);
            }
            else if (outLen >= 5 && BitConverter.ToUInt16(stdHostToAcc.OutputBuffer, 1) != 0x1001)
            {
                Console.WriteLine("Received status: 0x{0:X8}", stdHostToAcc.OutputBuffer[1]);
            }

            Thread.Sleep(200);
        }

        Console.WriteLine("Stop send polling to Note Acceptor");
    }

    public void Dispose()
    {
        ClosePort(_defaultPort);
    }
}
