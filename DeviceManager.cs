using Quixant.LibRAV;

public class DeviceManager
{
    private readonly Dictionary<SerialPortIndex, RAVDevice> _devices = new();
    private readonly ProtocolIdentifier _defaultProtocol = ProtocolIdentifier.MEI;
    private readonly SerialPortIndex _defaultPort = SerialPortIndex.SerialPort4;

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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing device: {ex.Message}");
        }
    }

    public void ClosePort(SerialPortIndex port)
    {
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
}
