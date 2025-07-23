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

    private int _threadSleepTime = 200;
    public JCMDeviceAdapter(SerialPortIndex port)
    {
        _device = new RAVDevice(port, ProtocolIdentifier.JCM);
        _port = port;
    }

    public void Init()
    {
        Console.WriteLine("JCMDeviceAdapter Init() starting...");
        Stopwatch sw = Stopwatch.StartNew();
        bool success = false;

        // --- 1. Define all commands ---
        JCMCommand getStatus = new JCMCommand(JCMInstruction.GetStatus, 0, 128);
        JCMCommand reset = new JCMCommand(JCMInstruction.Reset, 0, 0);

        // Configuration Commands
        JCMCommand setEnableDisable = new JCMCommand(JCMInstruction.SetEnableDisable, 2, 0);
        setEnableDisable.InputBuffer[0] = 0x00;
        setEnableDisable.InputBuffer[1] = 0x00;

        JCMCommand setSecurity = new JCMCommand(JCMInstruction.SetSecurity, 2, 0);
        setSecurity.InputBuffer[0] = 0x00;
        setSecurity.InputBuffer[1] = 0x00;

        JCMCommand setOptionalFunction = new JCMCommand(JCMInstruction.SetOptionalFunction, 2, 0);
        setOptionalFunction.InputBuffer[0] = 0x03; // Enable four-way acceptance

        JCMCommand setInhibit = new JCMCommand(JCMInstruction.SetInhibit, 1, 0);
        setInhibit.InputBuffer[0] = 0x00; // Enable ALL banknote denominations

        JCMCommand setBarcodeFunction = new JCMCommand(JCMInstruction.SetBarcodeFunction, 2, 0);
        setBarcodeFunction.InputBuffer[0] = 0x01; // Enable barcode reader
        setBarcodeFunction.InputBuffer[1] = 0x12;

        JCMCommand setBarInhibit = new JCMCommand(JCMInstruction.SetBarInhibit, 1, 0);
        setBarInhibit.InputBuffer[0] = 0xFC;

        try
        {
            // --- 2. Reset the device ---
            Console.WriteLine("JCMDeviceAdapter Init() Sending Reset command...");
            _device.Execute(reset);

            // --- 3. WAIT for the device to finish initializing ---
            Console.Write("JCMDeviceAdapter Init() Waiting for device to become ready");
            int retries = 0;
            do
            {
                Thread.Sleep(200); // Give the device time to process between polls
                _device.Get(getStatus);
                retries++;

                if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Initializing)
                {
                    Console.Write(".");
                }

            } while (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Initializing && retries < 100);

            Console.WriteLine(); // New line after the waiting dots..

            // --- 4. Check if the device is ready or timed out ---
            byte finalStatus = getStatus.OutputBuffer[0];
            if (finalStatus == (byte)JCMStatusResponse.Initializing)
            {
                throw new Exception("Device timed out and is still initializing.");
            }

            Console.WriteLine($"JCMDeviceAdapter Init() Device is ready. Status: 0x{finalStatus:X2}");

            // --- 5. NOW that it's ready, send all configuration commands ---
            Console.WriteLine("JCMDeviceAdapter Init() Sending configuration commands...");
            _device.Set(setEnableDisable);
            _device.Set(setSecurity);
            _device.Set(setOptionalFunction);
            _device.Set(setInhibit);
            _device.Set(setBarcodeFunction);
            _device.Set(setBarInhibit);

            success = true;
        }
        catch (Exception exc)
        {
            Console.WriteLine($"\nJCMDeviceAdapter Init() Test failed: {exc.Message}");
        }
        finally
        {
            sw.Stop();
        }

        if (success)
        {
            Console.WriteLine("\nJCMDeviceAdapter Init() Test succeeded.");
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
        byte statusCode;
        JCMStatusResponse status;
        while (IsPolling)
        {
            try
            {
                sw = Stopwatch.StartNew();
                Console.WriteLine("JCMDeviceAdapter Poll() Starting polling..................................");
                do
                {
                    _device.Get(getStatus);
                    retries++;
                    statusCode = getStatus.OutputBuffer[0];
                    status = (JCMStatusResponse)statusCode;
                    if (status == JCMStatusResponse.Disabled)
                    {
                        Console.WriteLine("JCMDeviceAdapter Poll() Enable Device SetInhibit command...\n");
                        _device.Set(setInihibit);
                        if (_device.LastErrorCode != RAVResult.Q_SUCCESS)
                            Console.WriteLine("JCMDeviceAdapter Poll() Error in SetInhibit command. Received error code: {0}\n",
                                _device.LastErrorCode);
                        Thread.Sleep(_threadSleepTime);
                    }
                    else if (status != JCMStatusResponse.Accepting && status != JCMStatusResponse.Escrow)
                    {
                        Thread.Sleep(_threadSleepTime);
                    }

                    Console.WriteLine($"JCMDeviceAdapter Poll() Waiting for Accept Status: {status} (0x{statusCode:X2})");

                } while (status != JCMStatusResponse.Accepting && status != JCMStatusResponse.Escrow && retries < 100);

                if (status != JCMStatusResponse.Accepting && status != JCMStatusResponse.Escrow)
                {
                    Console.WriteLine("JCMDeviceAdapter Poll() Timed out or error occurred. Exiting test.");
                    return;
                }

                Console.WriteLine("JCMDeviceAdapter Poll() Accepting... Waiting for bill to be Escrewed. _________________");

                retries = 0;

                do
                {
                    _device.Get(getStatus);
                    statusCode = getStatus.OutputBuffer[0];
                    status = (JCMStatusResponse)statusCode;
                    retries++;

                    if (status != JCMStatusResponse.Escrow)
                    {
                        Thread.Sleep(_threadSleepTime);
                    }
                    Console.Write($"JCMDeviceAdapter Poll() Waiting for Escrew Status: {status} (0x{statusCode:X2}) ");
                } while (status != JCMStatusResponse.Escrow && retries < 10);

                if (status != JCMStatusResponse.Escrow)
                {
                    Console.WriteLine("JCMDeviceAdapter Poll() Timed out or error occurred. When Waiting gor Escrew Exiting test");
                    return;
                }

                Console.WriteLine("JCMDeviceAdapter Poll() Escrowed");

                Console.WriteLine("JCMDeviceAdapter Poll() Sending Stack-1");
                _device.Execute(stack1);
                retries = 0;
                do
                {
                    _device.Get(getStatus);
                    statusCode = getStatus.OutputBuffer[0];
                    status = (JCMStatusResponse)statusCode;
                    retries++;

                    if (status == JCMStatusResponse.Stacking)
                    {
                        if (retries == 1)
                            Console.WriteLine("JCMDeviceAdapter Poll() Stacking...\nWaiting for status 0x15 (VEND VALID)\nCurrent status: ");
                        Thread.Sleep(_threadSleepTime);
                    }
                    else
                    {
                        Console.WriteLine($"JCMDeviceAdapter Poll()  Waiting for VendValid Status: {status} (0x{statusCode:X2}) ");
                        Thread.Sleep(_threadSleepTime);
                    }

                } while (status != JCMStatusResponse.VendValid && retries < 10);

                if (status != JCMStatusResponse.VendValid)
                {
                    Console.WriteLine("JCMDeviceAdapter Poll() Timed out or error occurred. Exiting test.");
                    continue;
                }

                Console.WriteLine("JCMDeviceAdapter Poll() Vend valid");

                _device.Execute(ack);
                Thread.Sleep(_threadSleepTime);

                Console.WriteLine("JCMDeviceAdapter Poll() Acknowledge sent");

                retries = 0;

                do
                {
                    _device.Get(getStatus);
                    statusCode = getStatus.OutputBuffer[0];
                    status = (JCMStatusResponse)statusCode;
                    retries++;

                    if (status == JCMStatusResponse.Stacked)
                    {
                        if (retries == 1)
                            Console.WriteLine("JCMDeviceAdapter Poll() Stacked\nWaiting for idling...\n");

                        Thread.Sleep(_threadSleepTime);
                    }
                    else if (status == JCMStatusResponse.VendValid)
                    {
                        // resend ack
                        _device.Execute(ack);
                        Console.WriteLine("JCMDeviceAdapter Poll() Acknowledge resent\n");
                    }
                    else if (status != JCMStatusResponse.Enable)
                    {

                        Console.Write($"JCMDeviceAdapter Poll() JCMStatusResponse.Enable Waiting for idling status. Current status {status} (0x{statusCode:X2})\n");
                        Thread.Sleep(_threadSleepTime);
                    }

                } while (status != JCMStatusResponse.Enable && retries < 10);

                if (status != JCMStatusResponse.Enable)
                {
                    Console.Write("JCMDeviceAdapter Poll() Enable Timed out or error occurred. Exiting test.\n");
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
        Console.WriteLine("JCMDeviceAdapter Poll() Ended");
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