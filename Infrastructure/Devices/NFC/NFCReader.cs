using PCSC;
using PCSC.Exceptions;
using PCSC.Iso7816;
using PCSC.Monitoring;
using PCSC.Utils;

class NFCReader
{
    private static bool NoReaderFound(ICollection<string> readerNames) =>
                readerNames == null || readerNames.Count < 1;


    public event CardInsertedEvent OnCardInserted;
    public event CardRemovedEvent OnCardRemoved;
    public event CardInitializedEvent OnCardInitialized;
    public event StatusChangeEvent OnStatusChanged;
    public event MonitorExceptionEvent OnException;

    private bool IsEmpty(ICollection<string> readerNames) => readerNames == null || readerNames.Count < 1;

    private bool _pooling = true;

    private TcpServer _server;
    public void Init(TcpServer server)
    {
        _server = server;

        Console.WriteLine("NFCReader Init()");

        var readerNames = GetReaderNames();

        if (IsEmpty(readerNames))
        {
            Console.WriteLine("NFCReader Init() There are currently no readers installed.");
            return;
        }

        OnCardInserted += (sender, args) => DisplayEvent("CardInserted", args);
        OnCardRemoved += (sender, args) => DisplayEvent("CardRemoved", args);
        OnCardInitialized += (sender, args) => DisplayEvent("Initialized", args);
        // OnStatusChanged += StatusChanged;
        OnException += MonitorException;

        OnCardInserted += async (sender, args) =>
        {
            Console.WriteLine($"[NFC] Card Inserted: {args.ReaderName}");
            string atrString = BitConverter.ToString(args.Atr ?? new byte[0]);
            Console.WriteLine($"[NFC]: {atrString}");
            await _server.SendMessageAsync($"NFC:{atrString}"); // Send the ATR
            Thread.Sleep(1000); // Allow time for processing if needed
        };
        using (var monitor = MonitorFactory.Instance.Create(SCardScope.System))
        {
            AttachToAllEvents(monitor);

            ShowUserInfo(readerNames);

            monitor.Start(readerNames);

            // Let the program run until the user presses CTRL-Q
            while (_pooling)
            {
                if (monitor.Monitoring)
                {
                    monitor.Cancel();
                }
                else
                {
                    monitor.Start(readerNames);
                    Thread.Sleep(500);
                }
            }
            Console.WriteLine("NFCReader StopPolling() Stopping polling...");

            DetachFromEvents(monitor);
        }
    }

    private void ShowUserInfo(IEnumerable<string> readerNames)
    {
        foreach (var reader in readerNames)
        {
            Console.WriteLine($"NFCReader ShowUserInfo() Start monitoring for reader {reader}.");
        }

        Console.WriteLine("NFCReader ShowUserInfo() Press Ctrl-Q to exit or any key to toggle monitor.");
    }

    private void AttachToAllEvents(ISCardMonitor monitor)
    {
        monitor.CardInserted += OnCardInserted;
        monitor.CardRemoved += OnCardRemoved;
        monitor.Initialized += OnCardInitialized;
        monitor.StatusChanged += OnStatusChanged;
        monitor.MonitorException += OnException;
    }

    private void DetachFromEvents(ISCardMonitor monitor)
    {
        Console.WriteLine("NFCReader DetachFromEvents() Detaching events...");
        monitor.CardInserted -= OnCardInserted;
        monitor.CardRemoved -= OnCardRemoved;
        monitor.Initialized -= OnCardInitialized;
        monitor.StatusChanged -= OnStatusChanged;
        monitor.MonitorException -= OnException;
    }

    private void DisplayEvent(string eventName, CardStatusEventArgs unknown)
    {
        Console.WriteLine("NFCReader DisplayEvent()>> {0} Event for reader: {1}", eventName, unknown.ReaderName);
        Console.WriteLine("NFCReader DisplayEvent() ATR: {0}", BitConverter.ToString(unknown.Atr ?? new byte[0]));
        Console.WriteLine("NFCReader DisplayEvent() State: {0}\n", unknown.State);
    }

    private void StatusChanged(object sender, StatusChangeEventArgs args)
    {
        Console.WriteLine("NFCReader StatusChanged()>> StatusChanged Event for reader: {0}", args.ReaderName);
        Console.WriteLine("NFCReader StatusChanged() ATR: {0}", BitConverter.ToString(args.Atr ?? new byte[0]));
        Console.WriteLine("NFCReader StatusChanged() Last state: {0}\nNew state: {1}\n", args.LastState, args.NewState);
    }

    private void MonitorException(object sender, PCSCException ex)
    {
        Console.WriteLine("NFCReader MonitorException() Monitor exited due an error:");
        Console.WriteLine(SCardHelper.StringifyError(ex.SCardError));
    }

    private string[] GetReaderNames()
    {
        using (var context = ContextFactory.Instance.Establish(SCardScope.System))
        {
            return context.GetReaders();
        }
    }

    public void StopPolling()
    {
        _pooling = false;
    }

}