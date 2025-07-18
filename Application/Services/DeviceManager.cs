using System.Diagnostics;
using Quixant.LibRAV;

public class DeviceManager : IDisposable
{

    private readonly SerialPortIndex _defaultPort = SerialPortIndex.SerialPort4;
    private IDeviceAdapter _currentDeviceAdapter;

    public DeviceManager(Func<SerialPortIndex, IDeviceAdapter> adapterFactory)
    {
        _currentDeviceAdapter = adapterFactory(_defaultPort);
    }

    public void Initalize()
    {
        _currentDeviceAdapter.Open();
        Console.WriteLine($"DeviceManager Initalize() Port opened on {_defaultPort.Name}.");
        _currentDeviceAdapter.Init();
        Console.WriteLine($"DeviceManager Initalize() Device on port {_defaultPort.Name} initialized. \n Polling Started.");

        _currentDeviceAdapter.Poll();
    }

    public void StopPolling()
    {
        _currentDeviceAdapter.IsPolling = false;
        Console.WriteLine("DeviceManager StopPolling() IsPolling false.");
    }

    public void StartPolling()
    {
        _currentDeviceAdapter.IsPolling = true;
        Console.WriteLine("DeviceManager StartPolling() IsPolling true., Starting poll.");
        _currentDeviceAdapter.Poll();
    }

    public void ReturnBill()
    {
        _currentDeviceAdapter.ReturnBill();
    }

    public void StackBill()
    {
        _currentDeviceAdapter.StackBill();
    }

    public void ClosePort()
    {
        try
        {
            Console.WriteLine($"DeviceManager ClosePort() closing port ....");
            _currentDeviceAdapter.IsPolling = false;
            _currentDeviceAdapter.Dispose();
            Console.WriteLine($"DeviceManager ClosePort() Device on port closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeviceManager ClosePort() ❌ Error closing device: {ex.Message}");
        }
    }

    public void Dispose()
    {
        ClosePort();
    }
}
