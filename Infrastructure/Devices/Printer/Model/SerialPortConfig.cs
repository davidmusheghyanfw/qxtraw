
public struct SerialPortConfig
{
    public int BaudRate;
    public int ReadTimeout;
    public int WriteTimeout;
    public int DataBits;
    public int ConnectionDelayMs;

    public SerialPortConfig(int baudRate, int readTimeout, int writeTimeout, int dataBits, int connectionDelayMs)
    {
        BaudRate = baudRate;
        ReadTimeout = readTimeout;
        WriteTimeout = writeTimeout;
        DataBits = dataBits;
        ConnectionDelayMs = connectionDelayMs;
    }
}