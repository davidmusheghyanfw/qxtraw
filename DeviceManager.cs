using System.Diagnostics;
using Quixant.LibRAV;

public class DeviceManager : IDisposable
{
    private readonly Dictionary<SerialPortIndex, RAVDevice> _devices = new();
    private readonly ProtocolIdentifier _defaultProtocol = ProtocolIdentifier.MEI;
    private readonly SerialPortIndex _defaultPort = SerialPortIndex.SerialPort4;
    bool pollOn = true;
    RAVDevice currentDevice;

    public DeviceManager()
    {
        _devices.Add(_defaultPort,
                  new RAVDevice(_defaultPort, _defaultProtocol));
    }

    public void StartAllDevices()
    {
        foreach (var kvp in _devices)
        {
            _initDevice(kvp.Key);
        }
    }

    private void _initDevice(SerialPortIndex port)
    {
        this.currentDevice = _devices[port];
        if (this.currentDevice.IsOpen)
        {
            Console.WriteLine($"DeviceManager InitDevice() Port {port.Name} already open.");
            return;
        }

        Console.WriteLine($"DeviceManager InitDevice() Attempting to initialize {_defaultProtocol} device on port {port.Name}...");

        try
        {
            this.currentDevice.Protocol = _defaultProtocol;
            this.currentDevice.Open(port.Name);
            Console.WriteLine($"DeviceManager InitDevice() Device on port {port.Name} initialized.");

            _initMEI(this.currentDevice, MEIInstruction.InitAndPoll);
            StartPolling();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeviceManager InitDevice() ❌ Error initializing device: {ex.Message}");
        }
    }

    private void _initMEI(RAVDevice device, MEIInstruction instruction)
    {
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
            Console.WriteLine("DeviceManager initMEI() Waiting for the device to initialize...");

            while (initWait < 30)
            {
                try
                {
                    device.Get(stdHostToAcc);
                    Console.WriteLine("DeviceManager initMEI() Initialization done");
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("DeviceManager initMEI() initWait.");
                    initWait++;
                    Thread.Sleep(200);
                }
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Operation failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setDenom);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set denomination failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setInt);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set interrupt failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setSec);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set security failed: " + exc.Message);
            return;
        }

        try
        {
            device.Set(setOri);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set orientation failed: " + exc.Message);
            return;
        }

        try
        {
            setEscrow.RunOn(device); //alternate way of calling a command
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set escrow failed: " + exc.Message);
            return;
        }

        try
        {
            setPush.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set push failed: " + exc.Message);
            return;
        }

        try
        {
            setBar.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set barcode decoding failed: " + exc.Message);
            return;
        }

        try
        {
            setPup.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set powerup failed: " + exc.Message);
            return;
        }
        // try
        // {
        //     switch (instruction)
        //     {
        //         case MEIInstruction.InitAndPoll:   // Normal mode
        //             {
        //                 // Enable extended note reporting
        //                 setNote.InputBuffer[0] = 0x00;
        //                 setNote.RunOn(device);
        //                 break;
        //             }
        //         case MEIInstruction.InitExtCfscAndPoll:   // Extended Note CFSC - 8 bytes of denomination
        //             {
        //                 // Enable extended note reporting 
        //                 setNote.InputBuffer[0] = 0x01;
        //                 setNote.RunOn(device);

        //                 //Enable all the Bank Note
        //                 MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
        //                     (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);
        //                 device.Set(setExtendedNote);

        //                 break;
        //             }
        //         case MEIInstruction.InitExtScaScrAndPoll:   // Extended Note SC Adv SCR - 19 bytes of denomination
        //             {
        //                 // Enable extended note reporting
        //                 setNote.InputBuffer[0] = 0x02;
        //                 setNote.RunOn(device);

        //                 //Enable all the Bank Note
        //                 MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
        //                     (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);
        //                 device.Set(setExtendedNote);

        //                 break;
        //             }

        //         default:
        //             break;
        //     }
        // }
        // catch (Exception exc)
        // {
        //     Console.WriteLine("DeviceManager initMEI() Init and Poll failed: " + exc.Message);
        //     return;
        // }
        try
        {
            setCpn.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Disable extended coupon reporting failed: " + exc.Message);
            return;
        }

        Console.Write("DeviceManager initMEI() Test executed successfully\n");
    }



    public void StopPolling()
    {
        pollOn = false;
        Console.WriteLine("DeviceManager StopPolling() Polling stopped.");
    }

    public void StartPolling()
    {
        pollOn = true;
        Console.WriteLine("DeviceManager StartPolling() Pollinenabled.");
        MeiPoll(this.currentDevice);
    }

    private void MeiPoll(RAVDevice device)
    {
        uint outLen = 0;
        byte[] buffer = new byte[128];
        if (pollOn)
            Console.WriteLine("Devicemanager MeiPoll() Start task (E7 command) send polling to Note Acceptor \n");

        MEICommand stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);

        // Poll the device each 200 ms 
        while (pollOn)
        {
            // Standard host to acceptor poll. When using input length 0 the library fills in the
            // data with the current configuration
            outLen = stdHostToAcc.RunOn(device); //device.Get(stdHostToAcc);

            /*
             (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1)) & MeiStatus.Escrowed)
             */
            if (outLen >= 5 && (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1)) & MeiStatus.Escrowed) == MeiStatus.Escrowed)
            {
                Console.WriteLine("Devicemanager MeiPoll() Received escrowed event");
                // StackBillTEST();
            }
            else if (outLen >= 10 && (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 2)) & MeiStatus.Escrowed) == MeiStatus.Escrowed)
            {
                Console.WriteLine("Devicemanager MeiPoll() Received status Extended : 0x{0:X2} 0x{1:X2} 0x{2:X2}", buffer[0], buffer[1], buffer[2]);
                Console.WriteLine("Devicemanager MeiPoll() Received escrowed event");
                // StackBillTEST();
            }
            else if (outLen >= 5 && BitConverter.ToUInt16(stdHostToAcc.OutputBuffer, 1) != 0x1001)
            {
                Console.WriteLine("Devicemanager MeiPoll() Received status: 0x{0:X8}", stdHostToAcc.OutputBuffer[1]);
            }

            Thread.Sleep(200);
        }

        Console.WriteLine("Devicemanager MeiPoll() exited polling loop.");
    }
    public void ReturnBill()
    {
        Console.WriteLine("DeviceManager ReturnBill() Bill return.");

        this.currentDevice.ExecuteWithMenuOption(MenuOption.Return);
    }

    public void StackBill()
    {
        Console.WriteLine("DeviceManager StackBill() ");
        Thread.Sleep(1000);
        this.currentDevice.ExecuteWithMenuOption(MenuOption.MEI_Stack);
    }

    public void StackBillTEST()
    {
        Console.WriteLine("DeviceManager StackBillTEST()");
        Thread.Sleep(1000);
        MEICommand stack = new MEICommand(MEIInstruction.Stack, 0, 0);
        stack.RunOn(this.currentDevice);
    }


    public void ClosePort(SerialPortIndex port)
    {
        this.StopPolling();
        if (_devices[port] == null || !_devices[port].IsOpen)
        {
            Console.WriteLine($"DeviceManager ClosePort() Port {port.Name} is not open.");
            return;
        }

        try
        {
            var protocol = _devices[port].Protocol;
            _devices[port].Dispose();
            _devices[port] = new RAVDevice(port, protocol);
            Console.WriteLine($"DeviceManager ClosePort() Device on port {port.Name} closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeviceManager ClosePort() ❌ Error closing device: {ex.Message}");
        }
    }

    public void Dispose()
    {
        ClosePort(_defaultPort);
    }
}
