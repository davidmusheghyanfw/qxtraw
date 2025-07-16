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

            _initMEI(this.currentDevice, MEIInstruction.InitExtCfscAndPoll);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeviceManager InitDevice() ❌ Error initializing device: {ex.Message}");
        }
    }

    private void _initMEI(RAVDevice device, MEIInstruction instruction)
    {
        uint outLen = 0;
        MEICommand reset = new MEICommand(MEIInstruction.SoftReset, 0, 0);
        MEICommand stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);
        MEICommand setDenom = new MEICommand(MEIInstruction.SetDenomination, 1, 0);
        MEICommand setInt = new MEICommand(MEIInstruction.SetSpecialInterruptMode, 1, 0);
        // MEICommand setSec = new MEICommand(MEIInstruction.SetSecurity, 1, 0);
        MEICommand setOri = new MEICommand(MEIInstruction.SetOrientation, 2, 0);
        MEICommand setEscrow = new MEICommand(MEIInstruction.SetEscrowMode, 1, 0);
        MEICommand setPush = new MEICommand(MEIInstruction.SetPushMode, 1, 0);
        MEICommand setBar = new MEICommand(MEIInstruction.SetBarcodeDecoding, 1, 0);
        MEICommand setPup = new MEICommand(MEIInstruction.SetPowerup, 2, 0);
        MEICommand setNote = new MEICommand(MEIInstruction.SetExtendedNoteReporting, 1, 0);
        MEICommand setCpn = new MEICommand(MEIInstruction.SetExtendedCouponReporting, 1, 0);
        setDenom.InputBuffer[0] = 0x7f;
        setInt.InputBuffer[0] = 0x00;
        // setSec.InputBuffer[0] = 0x00;
        setOri.InputBuffer[0] = 0x03;
        setOri.InputBuffer[1] = 0x00;
        setEscrow.InputBuffer[0] = 0x01;
        setPush.InputBuffer[0] = 0x00;
        setBar.InputBuffer[0] = 0x01;
        setPup.InputBuffer[0] = 0x00;
        setPup.InputBuffer[1] = 0x00;
        setCpn.InputBuffer[0] = 0x01;

        try
        {
            int initWait = 0;

            device.Execute(reset);
            Console.WriteLine("DeviceManager initMEI() Waiting for the device to initialize...");

            while (initWait < 30)
            {
                try
                {
                    outLen = device.Get(stdHostToAcc);
                    Console.WriteLine("DeviceManager initMEI() Initialization done");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DeviceManager initMEI() initWait. {ex.Message}");
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
        Stopwatch sw = null;
        sw = Stopwatch.StartNew();

        try
        {
            Console.WriteLine("DeviceManager initMEI()  device.Set(setDenom);");
            device.Set(setDenom);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set denomination failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("DeviceManager initMEI()  device.Set(setInt);");
            device.Set(setInt);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set interrupt failed: " + exc.Message);
            return;
        }

        // try
        // {
        //     device.Set(setSec);
        // }
        // catch (Exception exc)
        // {
        //     Console.WriteLine("DeviceManager initMEI() Set security failed: " + exc.Message);
        //     return;
        // }

        try
        {
            Console.WriteLine("DeviceManager initMEI()   device.Set(setOri);");

            device.Set(setOri);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set orientation failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("DeviceManager initMEI()  setEscrow.RunOn(device);");
            setEscrow.RunOn(device); //alternate way of calling a command
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set escrow failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("DeviceManager initMEI()   setPush.RunOn(device);");
            setPush.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set push failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("DeviceManager initMEI()   setBar.RunOn(device);");
            setBar.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set barcode decoding failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("DeviceManager initMEI()    setPup.RunOn(device);");
            setPup.RunOn(device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("DeviceManager initMEI() Set powerup failed: " + exc.Message);
            return;
        }
        sw.Stop();
        printTime(sw.ElapsedTicks, 1);
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

                        for (int i = 0; i < 8; i++)
                            setExtendedNote.InputBuffer[i] = 0xFF;
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
            Console.WriteLine("DeviceManager initMEI() Init and Poll failed: " + exc.Message);
            return;
        }
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
        MeiPoll(this.currentDevice, stdHostToAcc, outLen);
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
        // MeiPoll(this.currentDevice);
    }

    private void MeiPoll(RAVDevice device, MEICommand stdHostToAcc, uint outLen)
    {
        byte[] buffer = new byte[128];
        if (pollOn)
            Console.WriteLine("Devicemanager MeiPoll() Start task (E7 command) send polling to Note Acceptor \n");

        while (pollOn)
        {
            try
            {
                outLen = stdHostToAcc.RunOn(device);

                // Check whether the received response is a standard Acceptor to Host message
                if ((stdHostToAcc.OutputBuffer[0] & 0xF0) == (int)MEIInstruction.StdAccToHost)
                {
                    // In this case, the status data bytes are retrieved from the second index in the array
                    if (outLen >= 5 && (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1)) & MeiStatus.Escrowed) == Quixant.LibRAV.MeiStatus.Escrowed)
                    {
                        Console.WriteLine("Received escrowed event of a bank note");
                        int denomination = (stdHostToAcc.OutputBuffer[3] & 0x38) >> 3; // Bits 3-5 represent the denomination
                        Console.WriteLine("Denomination: " + denomination);
                        Console.WriteLine("Sending stack command...");
                        Thread.Sleep(1000);
                    }
                    else if (outLen >= 5 && BitConverter.ToUInt16(stdHostToAcc.OutputBuffer, 1) != 0x1001)
                    {
                        uint statusValue = BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1);
                        Console.WriteLine("Received status: " + statusValue.ToString("X8"));
                    }
                }
                else if ((stdHostToAcc.OutputBuffer[0] & 0xF0) == (int)MEIInstruction.ExtendedMsgSet)
                {
                    // Switch the extended command subtype
                    switch (stdHostToAcc.OutputBuffer[1])
                    {
                        case (byte)MEIMessageExtendedSubtype.ExtendedBarcodeReply:
                            {
                                Console.WriteLine("Received escrowed event of a ticket");
                                // The extended data field for Barcodes is 28 bytes long and represented in ASCII.
                                // The Barcode data is left justified LSC (Least Significant Character)
                                // and all unused bytes are filled with 0x28.
                                // First 8 bytes are:
                                // Message type + Sybtype + Status data (4 bytes) + Model# + Revision#
                                Console.Write("Barcode value: ");
                                for (int i = 8; i < 28 && stdHostToAcc.OutputBuffer[i] != 0x28; i++)
                                {
                                    Console.Write((char)stdHostToAcc.OutputBuffer[i]);
                                }
                                Console.WriteLine();
                                Console.WriteLine("Sending stack command...");
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("\nGet error: " + exc.Message);
            }
            Thread.Sleep(200);
        }
        return;
        // Poll the device each 200 ms 
        while (pollOn)
        {
            // Standard host to acceptor poll. When using input length 0 the library fills in the
            // data with the current configuration
            // outLen = device.Get(stdHostToAcc);
            uint status = BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1);
            Console.WriteLine($"Polling status: 0x{status:X8} ({(MeiStatus)status})");

            Console.Write("Status bytes: ");
            for (int i = 1; i <= 4; i++)
            {
                Console.Write($"0x{stdHostToAcc.OutputBuffer[i]:X2} ");
            }
            Console.WriteLine();

            if (stdHostToAcc.OutputBuffer[0] == (byte)MEIInstruction.ExtendedMsgSet)
            {
                byte subtype = stdHostToAcc.OutputBuffer[1];
                if (subtype == (byte)MEIMessageExtendedSubtype.ExtendedBarcodeReply)
                {
                    byte denomId = stdHostToAcc.OutputBuffer[8];
                    Console.WriteLine($"Detected Denomination ID: {denomId}");
                }
            }

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

            Thread.Sleep(50);
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
    private static void printTime(long ticks, int repetitions)
    {
        double seconds = ((double)ticks) / ((double)Stopwatch.Frequency);
        double avg = seconds / repetitions;

        Console.Write("\n----------------  Results  ----------------\n\n");
        Console.Write("Total execution time: ");

        if (seconds > 1)
            Console.WriteLine("{0:f4}s", seconds);
        else if (seconds * 1000 > 1)
            Console.WriteLine("{0:f4}ms", seconds * 1000);
        else
            Console.WriteLine("{0:f4}us", seconds * 1000 * 1000);

        Console.Write("Average cycle execution time: ");

        if (avg > 1)
            Console.WriteLine("{0:f4}s", avg);
        else if (avg * 1000 > 1)
            Console.WriteLine("{0:f4}ms", avg * 1000);
        else
            Console.WriteLine("{0:f4}us", avg * 1000 * 1000);

        Console.Write("\n----------------  End of results  ----------------\n");
    }
    public void Dispose()
    {
        ClosePort(_defaultPort);
    }
}
