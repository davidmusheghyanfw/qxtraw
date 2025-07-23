using System.Text;
using Microsoft.VisualBasic;

abstract class PrinterServiceAdapter : IPrinter
{
    #region Serial Port Configuration
    protected abstract int DEFAULT_BAUD_RATE { get; }
    protected abstract int DEFAULT_READ_TIMEOUT { get; }
    protected abstract int DEFAULT_WRITE_TIMEOUT { get; }
    protected abstract int DEFAULT_DATA_BITS { get; }
    protected abstract int CONNECTION_DELAY_MS { get; }
    #endregion

    private PrinterConnectorService _printerConnector;
    private PrinterDataSenderService _printerDataSender;
    public bool IsConnected => _printerConnector?.IsConnected ?? false;

    public async void Init()
    {
        try
        {
            _printerConnector = new PrinterConnectorService();

            var config = new SerialPortConfig(
                DEFAULT_BAUD_RATE,
                DEFAULT_READ_TIMEOUT,
                DEFAULT_WRITE_TIMEOUT,
                DEFAULT_DATA_BITS,
                CONNECTION_DELAY_MS);

            _printerDataSender = new PrinterDataSenderService(
                await _printerConnector.ConnectAsync(config));

        }
        catch (Exception ex)
        {
            Console.WriteLine($"PrinterService Init() Failed to open serial port: {ex.Message}");
        }

    }

    public void Print(String data)
    {
        List<byte> dataBytes = Encoding.ASCII.GetBytes(data).ToList();
        Print(dataBytes);
    }


    public void PrintDemoTicket()
    {
        Print(CreateDemoPrintBuffer());
    }

    private void Print(List<byte> printBuffer)
    {
        if (IsConnected)
        {
            try
            {
                _printerDataSender.SendData(printBuffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("PrinterService Print() Printer error: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("PrinterService Print() Printer is not connected.");
        }
    }

    List<byte> CreateDemoPrintBuffer()
    {
        return Encoding.ASCII.GetBytes(MOCK_DATA).ToList();
    }


    private const string MOCK_DATA = $"^P|0|1|000000000000099899|Your Establishment|Your Location|CITY / STATE / ZIP|||000000000000099899|02/10/2012|00:02:21|Ticket # 12346|EIGHTY FIVE DOLLARS AND TWO CENTS|PR G|$99.899||30 days|MACHINE#1234-678|000000000000099899|^";
}
