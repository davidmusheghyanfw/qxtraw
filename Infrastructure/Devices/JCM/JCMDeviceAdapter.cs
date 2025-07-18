using System.Diagnostics;
using Quixant.LibRAV;

class JCMDeviceAdapter : IDeviceAdapter
{
    private readonly RAVDevice _device;

    public bool IsOpen => _device.IsOpen;

    public bool _isPolling = true;
    public bool IsPolling { get => _isPolling; set => _isPolling = value; }
    private SerialPortIndex _port;
    public SerialPortIndex port { get => _port; set => _port = value; }

    public JCMDeviceAdapter(SerialPortIndex port)
    {
        _device = new RAVDevice(port, ProtocolIdentifier.JCM);
        _port = port;
    }

    public void Init()
    {
        bool success = false;
        int retries = 0;
        JCMCommand getStatus = new JCMCommand(JCMInstruction.GetStatus, 0, 128);
        JCMCommand reset = new JCMCommand(JCMInstruction.Reset, 0, 0);
        JCMCommand setEnableDisable = new JCMCommand(JCMInstruction.SetEnableDisable, 2, 0);
        setEnableDisable.InputBuffer[0] = 0x00;
        setEnableDisable.InputBuffer[1] = 0x00;
        JCMCommand setSecurity = new JCMCommand(JCMInstruction.SetSecurity, 2, 0);
        setSecurity.InputBuffer[0] = 0x00;
        setSecurity.InputBuffer[1] = 0x00;
        JCMCommand setOptionalFunction = new JCMCommand(JCMInstruction.SetOptionalFunction, 2, 0);
        setOptionalFunction.InputBuffer[0] = 0x03;
        setOptionalFunction.InputBuffer[1] = 0x00;
        JCMCommand setInhibit = new JCMCommand(JCMInstruction.SetInhibit, 1, 0);
        setInhibit.InputBuffer[0] = 0x00;
        JCMCommand setBarcodeFunction = new JCMCommand(JCMInstruction.SetBarcodeFunction, 2, 0);
        setBarcodeFunction.InputBuffer[0] = 0x01;
        setBarcodeFunction.InputBuffer[1] = 0x12;
        JCMCommand setBarInhibit = new JCMCommand(JCMInstruction.SetBarInhibit, 1, 0);
        setBarInhibit.InputBuffer[0] = 0xFC;
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            _device.Get(getStatus);
            _device.Execute(reset);
            _device.Get(getStatus);
            _device.Set(setEnableDisable);
            _device.Set(setSecurity);
            _device.Set(setOptionalFunction);
            _device.Set(setInhibit);
            _device.Set(setBarcodeFunction);
            _device.Set(setBarInhibit);

            do
            {
                _device.Get(getStatus);
                retries++;

                if (getStatus.OutputBuffer[0] != (byte)JCMInstruction.GetStatus)
                    Thread.Sleep(1000);

                if (retries == 1 && getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Initializing)
                    Console.Write("JCMDeviceAdapter Init() Initializing");
                else if (retries > 1 && getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Initializing)
                    Console.Write(".");
                else
                    Console.WriteLine("\nJCMDeviceAdapter Init() Status: 0x{0:X2}", getStatus.OutputBuffer[0]);

            } while (getStatus.OutputBuffer[0] != (byte)JCMInstruction.GetStatus && retries < 100);

            success = true;
        }
        catch (Exception exc)
        {
            Console.WriteLine("JCMDeviceAdapter Init() Test failed: " + exc.Message);
        }
        finally
        {
            sw.Stop();
        }

        if (success)
        {
            Console.WriteLine("JCMDeviceAdapter Init() Test succeeded.");
            printTime(sw.ElapsedTicks, 1);
        }
    }


    public void Open()
    {
        if (_device.IsOpen)
        {
            Console.WriteLine($"JCMDeviceAdapter Open() Device on port {port.Name} already open.");
            return;
        }
        Console.WriteLine($"JCMDeviceAdapter Open() Initializing {_device.Protocol} _device on port {port.Name}...");

        try
        {
            _device.Open(port.Name);

        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"JCMDeviceAdapter Open() Error initializing _device: {ex.Message}");
        }
    }

    public void Poll()
    {
        int retries = 0;
        Stopwatch sw = null;
        JCMCommand getStatus = new JCMCommand(JCMInstruction.GetStatus, 0, 128);
        JCMCommand stack1 = new JCMCommand(JCMInstruction.Stack1, 0, 0);
        JCMCommand ack = new JCMCommand(JCMInstruction.Ack, 0, 0);
        JCMCommand setInihibit = new JCMCommand(JCMInstruction.SetInhibit, 1, 0);
        Console.Write("JCMDeviceAdapter Poll() Please insert bill...\n");

        try
        {
            sw = Stopwatch.StartNew();

            do
            {
                _device.Get(getStatus);
                retries++;

                if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Disabled)
                {
                    Console.Write("JCMDeviceAdapter Poll() Device is currently disabled. Now enabling it sending a SetInhibit command...\n");
                    _device.Set(setInihibit);
                    if (_device.LastErrorCode != RAVResult.Q_SUCCESS)
                        Console.Write("JCMDeviceAdapter Poll() Error in SetInhibit command. Received error code: {0}\n",
                            _device.LastErrorCode);
                    Thread.Sleep(500);
                }
                else if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Accepting)
                {
                    Thread.Sleep(500);
                }

                if (retries == 1)
                    Console.Write("JCMDeviceAdapter Poll() Waiting for status 0x12 (ACCEPTING)\nCurrent status: ");
                else
                    Console.Write("JCMDeviceAdapter Poll() 0x{0:X2}  ", getStatus.OutputBuffer[0]);

            } while (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Accepting && retries < 100);

            if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Accepting)
            {
                Console.Write("\nJCMDeviceAdapter Poll() Timed out or error occurred. Exiting test.\n");
                return;
            }

            Console.Write("\nJCMDeviceAdapter Poll() Accepting...\n");

            retries = 0;

            do
            {
                _device.Get(getStatus);
                retries++;

                if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Escrow)
                {
                    Thread.Sleep(500);
                }

                if (retries == 1)
                    Console.Write("JCMDeviceAdapter Poll() Waiting for status 0x13 (ESCROW)\nCurrent status: ");
                else
                    Console.Write("JCMDeviceAdapter Poll() 0x{0:X2}  ", getStatus.OutputBuffer[0]);

            } while (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Escrow && retries < 10);

            if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Escrow)
            {
                Console.Write("\nJCMDeviceAdapter Poll() Timed out or error occurred. Exiting test.\n");
                return;
            }

            Console.Write("\nJCMDeviceAdapter Poll() Escrow\n");

            Console.Write("JCMDeviceAdapter Poll() Sending Stack-1\n");
            _device.Execute(stack1);

            retries = 0;

            do
            {
                _device.Get(getStatus);
                retries++;

                if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Stacking)
                {
                    if (retries == 1)
                        Console.Write("JCMDeviceAdapter Poll() Stacking...\nWaiting for status 0x15 (VEND VALID)\nCurrent status: ");
                    Thread.Sleep(500);
                }
                else
                {
                    Console.Write("JCMDeviceAdapter Poll() 0x{0:X2}  ", getStatus.OutputBuffer[0]);
                    Thread.Sleep(500);
                }

            } while (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.VendValid && retries < 10);

            if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.VendValid)
            {
                Console.Write("\nJCMDeviceAdapter Poll() Timed out or error occurred. Exiting test.\n");
                return;
            }

            Console.Write("\nJCMDeviceAdapter Poll() Vend valid\n");

            // Send acknowledge
            _device.Execute(ack);
            Thread.Sleep(500);

            Console.Write("JCMDeviceAdapter Poll() Acknowledge sent\n");

            retries = 0;

            do
            {
                _device.Get(getStatus);
                retries++;

                if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Stacked)
                {
                    if (retries == 1)
                        Console.Write("JCMDeviceAdapter Poll() Stacked\nWaiting for idling...\n");

                    Thread.Sleep(500);
                }
                else if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.VendValid)
                {
                    // resend ack
                    _device.Execute(ack);
                    Console.Write("JCMDeviceAdapter Poll() Acknowledge resent\n");
                }
                else if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Enable)
                {
                    Console.Write("JCMDeviceAdapter Poll() Waiting for idling status. Current status 0x{0:X2}\n", getStatus.OutputBuffer[0]);
                    Thread.Sleep(500);
                }

            } while (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Enable && retries < 10);

            if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Enable)
            {
                Console.Write("JCMDeviceAdapter Poll() Timed out or error occurred. Exiting test.\n");
                return;
            }

            Console.Write("JCMDeviceAdapter Poll() Idling\n");
            sw.Stop();

            Console.Write("JCMDeviceAdapter Poll() Test executed successfully\n");
            printTime(sw.ElapsedTicks, 1);
        }
        catch (Exception exc)
        {
            Console.WriteLine("JCMDeviceAdapter Poll() Test failed somewhere: " + exc.Message);
        }
    }

    public void ReturnBill()
    {
        this._device.ExecuteWithMenuOption(MenuOption.Return);
    }

    public void StackBill()
    {
        this._device.ExecuteWithMenuOption(MenuOption.Stack1);
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