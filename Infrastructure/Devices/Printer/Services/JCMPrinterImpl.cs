class JCMPrinterImpl : PrinterServiceAdapter
{

    protected override int DEFAULT_BAUD_RATE => 38400;
    protected override int DEFAULT_READ_TIMEOUT => 1000;
    protected override int DEFAULT_WRITE_TIMEOUT => 1000;
    protected override int DEFAULT_DATA_BITS => 8;
    protected override int CONNECTION_DELAY_MS => 200;

}