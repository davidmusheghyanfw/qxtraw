

using System;
using System.IO.Ports;

public class PrinterConnectorService
{
    private SerialPort _serialPort;

    protected bool IsConnecting { get; private set; }
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Connects to the JCM printer on the specified port with the given baud rate.
    /// </summary>
    /// <param name="portName">The COM port name to connect to (default: COM3)</param>
    /// <param name="baudRate">The baud rate for the connection (default: 38400)</param>
    /// <returns>A Task that completes when the connection attempt finishes</returns>
    public async Task<SerialPort> ConnectAsync(SerialPortConfig config)
    {
        if (IsConnecting)
        {
            Console.WriteLine("PrinterConnectorService ConnectAsync() Connection attempt already in progress");
            return null;
        }

        if (IsConnected)
        {
            Console.WriteLine("PrinterConnectorService ConnectAsync() Printer is already connected");
            return null;
        }

        IsConnecting = true;

        try
        {
            Disconnect(); // Ensure any existing connection is closed

            // string foundPort = await FindWorkingPrinterPortAsync(config);

            // if (string.IsNullOrEmpty(foundPort))
            // {
            //     Console.WriteLine("PrinterConnectorService ConnectAsync() No working printer port found");
            //     return null;
            // }

            await EstablishConnectionAsync("/dev/ttyS2/", config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to printer: {ex.Message}");
            Disconnect();
        }
        finally
        {
            IsConnecting = false;
        }

        if (_serialPort == null)
        {
            Console.WriteLine("Failed to connect to printer");
            return null;
        }

        return _serialPort;
    }

    /// <summary>
    /// Disconnects from the printer and cleans up resources.
    /// </summary>
    public void Disconnect()
    {
        if (_serialPort != null)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    Console.WriteLine("PrinterConnectorService Disconnect() Printer connection closed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PrinterConnectorService Disconnect() Error closing printer connection: {ex.Message}");
            }
            finally
            {
                _serialPort?.Dispose();
                _serialPort = null;
            }
        }

        IsConnected = false;
    }

    /// <summary>
    /// Finds a working printer port using the CustomExpandingMethods utility.
    /// </summary>
    private async Task<string> FindWorkingPrinterPortAsync(SerialPortConfig config)
    {
        return await FindWorkingPort(async (portName) =>
        {
            SerialPort testPort = null;
            try
            {
                testPort = CreateSerialPort(portName, config);
                testPort.Open();
                await Task.Delay(config.ConnectionDelayMs);

                if (testPort.IsOpen)
                {
                    // Test if this is actually a printer by sending a simple command
                    if (await TestPrinterConnectionAsync(testPort))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to test port {portName}: {ex.Message}");
            }
            finally
            {
                testPort?.Close();
                testPort?.Dispose();
            }

            return false;
        }, "JCM_PRINTER");
    }

    /// <summary>
    /// Establishes the final connection to the printer.
    /// </summary>
    private async Task EstablishConnectionAsync(string portName, SerialPortConfig config)
    {
        _serialPort = CreateSerialPort(portName, config);

        try
        {
            _serialPort.Open();
            await Task.Delay(config.ConnectionDelayMs);

            if (_serialPort.IsOpen)
            {
                IsConnected = true;
                Console.WriteLine($"Successfully connected to JCM printer on port {portName} at {config.BaudRate} baud");
            }
            else
            {
                throw new InvalidOperationException("Serial port failed to open");
            }
        }
        catch (Exception ex)
        {
            _serialPort?.Dispose();
            _serialPort = null;
            throw new InvalidOperationException($"Failed to establish printer connection: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a SerialPort instance with the specified configuration.
    /// </summary>
    private SerialPort CreateSerialPort(string portName, SerialPortConfig config)
    {
        return new SerialPort(portName, config.BaudRate)
        {
            ReadTimeout = config.ReadTimeout,
            WriteTimeout = config.WriteTimeout,
            DataBits = config.DataBits,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Handshake = Handshake.XOnXOff
        };
    }

    /// <summary>
    /// Tests if the connected device is actually a printer by sending a simple command.
    /// </summary>
    private async Task<bool> TestPrinterConnectionAsync(SerialPort port)
    {
        try
        {
            // Send a simple status query command (this may need to be adjusted based on the specific printer model)
            byte[] statusCommand = { 0x1B, 0x76 }; // ESC v - Status command
            port.Write(statusCommand, 0, statusCommand.Length);

            await Task.Delay(100); // Wait for response

            // Check if we received any response (indicating it's likely a printer)
            return port.BytesToRead > 0 || port.IsOpen;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Printer test failed: {ex.Message}");
            return false;
        }
    }

    public static async Task<string> FindWorkingPort(
        Func<string, Task<bool>> tryOpenCallback,
        string deviceName = "Device",
        int optionalPostOpenDelayMs = 100)
    {
        string[] availablePorts;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            availablePorts = SerialPort.GetPortNames();
        }
        else
        {
            try
            {
                availablePorts = Directory.GetFiles("/dev/", "ttyUSB*")
                    .Concat(Directory.GetFiles("/dev/", "ttyS*"))
                    .ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{deviceName}: Error listing serial ports: {e.Message}");
                return null;
            }
        }

        Console.WriteLine($"{deviceName}: Found ports: {string.Join(", ", availablePorts)}");

        foreach (string portName in availablePorts)
        {
            Console.WriteLine($"{deviceName}: Trying port {portName}");

            try
            {
                if (await tryOpenCallback(portName))
                {
                    if (optionalPostOpenDelayMs > 0)
                        await Task.Delay(optionalPostOpenDelayMs);

                    Console.WriteLine($"{deviceName}: Connected on port {portName}");
                    return portName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{deviceName}: Exception trying port {portName}: {ex.Message}");
            }
        }

        Console.WriteLine($"{deviceName}: No working port found.");
        return null;
    }
}
