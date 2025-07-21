using System.Diagnostics;
using Quixant.LibRAV;

public class MEIDeviceAdapter : IDeviceAdapter
{
    private readonly RAVDevice _device;

    public MEIDeviceAdapter(SerialPortIndex port)
    {
        _device = new RAVDevice(port, ProtocolIdentifier.MEI);
        _port = port;
    }


    public bool IsOpen => _device.IsOpen;

    public bool _isPolling = true;


    private SerialPortIndex _port;
    public SerialPortIndex port { get => _port; set => _port = value; }
    public bool IsPolling { get => _isPolling; set => _isPolling = value; }

    public void Open()
    {
        if (_device.IsOpen)
        {
            Console.WriteLine($"Device on port {port.Name} already open.");
            return;
        }
        Console.WriteLine($"Initializing {_device.Protocol} _device on port {port.Name}...");

        try
        {
            _device.Open(port.Name);

        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error initializing _device: {ex.Message}");
        }
    }

    public void Init()
    {
        MEIInstruction instruction = MEIInstruction.InitExtCfscAndPoll;//TODO change to Icommand adapter
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

            _device.Execute(reset);
            Console.WriteLine("MEIDeviceAdapter initMEI() Waiting for the _device to initialize...");

            while (initWait < 30)
            {
                try
                {
                    outLen = _device.Get(stdHostToAcc);
                    Console.WriteLine("MEIDeviceAdapter initMEI() Initialization done");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MEIDeviceAdapter initMEI() initWait. {ex.Message}");
                    initWait++;
                    Thread.Sleep(50);
                }
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Operation failed: " + exc.Message);
            return;
        }
        Stopwatch sw = null;
        sw = Stopwatch.StartNew();

        try
        {
            Console.WriteLine("MEIDeviceAdapter initMEI()  _device.Set(setDenom);");
            _device.Set(setDenom);
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Set denomination failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("MEIDeviceAdapter initMEI()  _device.Set(setInt);");
            _device.Set(setInt);
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Set interrupt failed: " + exc.Message);
            return;
        }

        // try
        // {
        //     _device.Set(setSec);
        // }
        // catch (Exception exc)
        // {
        //     Console.WriteLine("MEIDeviceAdapter initMEI() Set security failed: " + exc.Message);
        //     return;
        // }

        try
        {
            Console.WriteLine("MEIDeviceAdapter initMEI()   _device.Set(setOri);");

            _device.Set(setOri);
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Set orientation failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("MEIDeviceAdapter initMEI()  setEscrow.RunOn(_device);");
            setEscrow.RunOn(_device); //alternate way of calling a command
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Set escrow failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("MEIDeviceAdapter initMEI()   setPush.RunOn(_device);");
            setPush.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Set push failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("MEIDeviceAdapter initMEI()   setBar.RunOn(_device);");
            setBar.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Set barcode decoding failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("MEIDeviceAdapter initMEI()    setPup.RunOn(_device);");
            setPup.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Set powerup failed: " + exc.Message);
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
                        setNote.RunOn(_device);

                        break;
                    }
                case MEIInstruction.InitExtCfscAndPoll:   // Extended Note CFSC - 8 bytes of denomination
                    {
                        // Enable extended note reporting 
                        setNote.InputBuffer[0] = 0x01;
                        setNote.RunOn(_device);

                        //Enable all the Bank Note
                        MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
                            (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);

                        for (int i = 0; i < 8; i++)
                            setExtendedNote.InputBuffer[i] = 0xFF;
                        _device.Set(setExtendedNote);
                        break;
                    }
                case MEIInstruction.InitExtScaScrAndPoll:   // Extended Note SC Adv SCR - 19 bytes of denomination
                    {
                        // Enable extended note reporting
                        setNote.InputBuffer[0] = 0x02;
                        setNote.RunOn(_device);

                        //Enable all the Bank Note
                        MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
                            (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);
                        _device.Set(setExtendedNote);

                        break;
                    }

                default:
                    break;
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Init and Poll failed: " + exc.Message);
            return;
        }
        try
        {
            setCpn.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("MEIDeviceAdapter initMEI() Disable extended coupon reporting failed: " + exc.Message);
            return;
        }

        Console.Write("MEIDeviceAdapter initMEI() Test executed successfully\n");
    }

    public void Poll()
    {
        byte[] buffer = new byte[128];
        if (IsPolling)
            Console.WriteLine("MEIDeviceAdapter MeiPoll() Start task (E7 command) send polling to Note Acceptor \n");

        uint outLen = 0;
        while (IsPolling)
        {
            // Standard host to acceptor poll. When using input length 0 the library fills in the
            // data with the current configuration
            MEICommand stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);
            try
            {
                outLen = _device.Get(stdHostToAcc);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"MEIDeviceAdapter Poll() ERROR {ex.Message}");
                continue;
            }
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
                int denominationIndex = (stdHostToAcc.OutputBuffer[3] & 0x38) >> 3;
                Console.WriteLine($"Denomination index: {denominationIndex}");
            }
            else if (outLen >= 10 && (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 2)) & MeiStatus.Escrowed) == MeiStatus.Escrowed)
            {
                Console.WriteLine("Devicemanager MeiPoll() Received status Extended : 0x{0:X2} 0x{1:X2} 0x{2:X2}", buffer[0], buffer[1], buffer[2]);
                Console.WriteLine("Devicemanager MeiPoll() Received escrowed event");
                int denominationIndex = (stdHostToAcc.OutputBuffer[3] & 0x38) >> 3;
                Console.WriteLine($"Denomination index: {denominationIndex}");
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
        Console.WriteLine("MEIDeviceAdapter ReturnBill() Bill return.");
        this._device.ExecuteWithMenuOption(MenuOption.MEI_Return);
    }

    public void StackBill()
    {
        Console.WriteLine("MEIDeviceAdapter StackBill() ");
        this._device.ExecuteWithMenuOption(MenuOption.MEI_Stack);
    }

    public void Dispose() => _device.Dispose();

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


}
