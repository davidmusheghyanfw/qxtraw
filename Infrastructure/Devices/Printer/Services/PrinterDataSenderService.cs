using System;
using System.Collections.Generic;
using System.IO.Ports;

public class PrinterDataSenderService
{
    private SerialPort _serialPort;

    public PrinterDataSenderService(SerialPort serialPort)
    {
        _serialPort = serialPort;
    }

    public async void SendData(List<byte> printBuffer)
    {
        if (_serialPort.IsOpen)
        {
            try
            {
                List<byte> buffer = printBuffer;

                if (buffer.Count > 0)
                {
                    Console.WriteLine("PrinterDataSenderService SendData() --------------------------");
                    Console.WriteLine($"PrinterDataSenderService SendData() {buffer.ToArray()}");
                    _serialPort.Write(buffer.ToArray(), 0, buffer.Count);

                    Console.WriteLine("PrinterDataSenderService SendData() Sent the complete command buffer to the printer");
                }
                else
                {
                    Console.WriteLine("PrinterDataSenderService SendData() Command buffer is empty. Nothing to send.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PrinterDataSenderService SendData() Error sending data to printer: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("PrinterDataSenderService SendData() Serial port is not open.");
        }
    }
}
