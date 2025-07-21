using PCSC;
using PCSC.Exceptions;
using PCSC.Iso7816;
using PCSC.Monitoring;
using PCSC.Utils;

class NFCReader
{
    private static bool NoReaderFound(ICollection<string> readerNames) =>
                readerNames == null || readerNames.Count < 1;


    public void Init()
    {
        Console.WriteLine("NFCReader Init()");

        var readerNames = GetReaderNames();

        if (IsEmpty(readerNames))
        {
            Console.WriteLine("NFCReader Init() There are currently no readers installed.");
            return;
        }

        // Create smart-card monitor using a context factory. 
        // The context will be automatically released after monitor.Dispose()
        using (var monitor = MonitorFactory.Instance.Create(SCardScope.System))
        {
            AttachToAllEvents(monitor); // Remember to detach, if you use this in production!

            ShowUserInfo(readerNames);

            monitor.Start(readerNames);

            // Let the program run until the user presses CTRL-Q
            while (true)
            {
                var key = Console.ReadKey();
                if (ExitRequested(key))
                {
                    break;
                }

                if (monitor.Monitoring)
                {
                    monitor.Cancel();
                    Console.WriteLine("NFCReader Init() Monitoring paused. (Press CTRL-Q to quit)");
                }
                else
                {
                    monitor.Start(readerNames);
                    Console.WriteLine("NFCReader Init() Monitoring started. (Press CTRL-Q to quit)");
                }
            }
        }
    }

    private static void ShowUserInfo(IEnumerable<string> readerNames)
    {
        foreach (var reader in readerNames)
        {
            Console.WriteLine($"NFCReader ShowUserInfo() Start monitoring for reader {reader}.");
        }

        Console.WriteLine("NFCReader ShowUserInfo() Press Ctrl-Q to exit or any key to toggle monitor.");
    }

    private static void AttachToAllEvents(ISCardMonitor monitor)
    {
        // Point the callback function(s) to the anonymous & static defined methods below.
        monitor.CardInserted += (sender, args) => DisplayEvent("CardInserted", args);
        monitor.CardRemoved += (sender, args) => DisplayEvent("CardRemoved", args);
        monitor.Initialized += (sender, args) => DisplayEvent("Initialized", args);
        monitor.StatusChanged += StatusChanged;
        monitor.MonitorException += MonitorException;
    }

    private static void DisplayEvent(string eventName, CardStatusEventArgs unknown)
    {
        Console.WriteLine("NFCReader DisplayEvent()>> {0} Event for reader: {1}", eventName, unknown.ReaderName);
        Console.WriteLine("NFCReader DisplayEvent() ATR: {0}", BitConverter.ToString(unknown.Atr ?? new byte[0]));
        Console.WriteLine("NFCReader DisplayEvent() State: {0}\n", unknown.State);
    }

    private static void StatusChanged(object sender, StatusChangeEventArgs args)
    {
        Console.WriteLine("NFCReader StatusChanged()>> StatusChanged Event for reader: {0}", args.ReaderName);
        Console.WriteLine("NFCReader StatusChanged() ATR: {0}", BitConverter.ToString(args.Atr ?? new byte[0]));
        Console.WriteLine("NFCReader StatusChanged() Last state: {0}\nNew state: {1}\n", args.LastState, args.NewState);
    }

    private static void MonitorException(object sender, PCSCException ex)
    {
        Console.WriteLine("NFCReader MonitorException() Monitor exited due an error:");
        Console.WriteLine(SCardHelper.StringifyError(ex.SCardError));
    }

    private static string[] GetReaderNames()
    {
        using (var context = ContextFactory.Instance.Establish(SCardScope.System))
        {
            return context.GetReaders();
        }
    }

    private static bool ExitRequested(ConsoleKeyInfo key) =>
        key.Modifiers == ConsoleModifiers.Control &&
        key.Key == ConsoleKey.Q;

    private static bool IsEmpty(ICollection<string> readerNames) => readerNames == null || readerNames.Count < 1;
    private void StartMonitoring()
    {
        using (var context = ContextFactory.Instance.Establish(SCardScope.System))
        {
            var readerNames = context.GetReaders();
            if (NoReaderFound(readerNames))
            {
                Console.WriteLine("NFCReader You need at least one reader in order to run this example.");
                return;
            }

            var readerName = readerNames[0];
            if (readerName == null)
            {
                return;
            }

            using (var rfidReader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
            {
                var apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.Protocol)
                {
                    CLA = 0xFF,
                    Instruction = InstructionCode.GetData,
                    P1 = 0x00,
                    P2 = 0x00,
                    Le = 0 // We don't know the ID tag size
                };

                using (rfidReader.Transaction(SCardReaderDisposition.Leave))
                {
                    Console.WriteLine("Retrieving the UID .... ");

                    var sendPci = SCardPCI.GetPci(rfidReader.Protocol);
                    var receivePci = new SCardPCI(); // IO returned protocol control information.

                    var receiveBuffer = new byte[256];
                    var command = apdu.ToArray();

                    var bytesReceived = rfidReader.Transmit(
                        sendPci, // Protocol Control Information (T0, T1 or Raw)
                        command, // command APDU
                        command.Length,
                        receivePci, // returning Protocol Control Information
                        receiveBuffer,
                        receiveBuffer.Length); // data buffer

                    var responseApdu =
                        new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, rfidReader.Protocol);

                    Console.WriteLine("SW1: {0:X2}, SW2: {1:X2}\nUid: {2}",
                        responseApdu.SW1,
                        responseApdu.SW2,
                        responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received");
                }
            }
        }
    }

}