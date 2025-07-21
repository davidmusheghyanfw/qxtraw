public struct PrinterDeviceData
{
    public string Type;
    public string Name;
    public string Model;
    public string SerialNumber;
    public bool IsActive;
    public string PortName;
    public string Manufacturer;

    public PrinterDeviceData(string type, string name, string model, string serialNumber, bool isActive, string portName, string manufacturer)
    {
        Type = type;
        Name = name;
        Model = model;
        SerialNumber = serialNumber;
        IsActive = isActive;
        PortName = portName;
        Manufacturer = manufacturer;
    }
}