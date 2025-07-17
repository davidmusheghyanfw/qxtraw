using Quixant.LibRAV;

public interface IDeviceAdapter : IDisposable
{
    bool IsOpen { get; }

    bool IsPolling { get; set; }


    SerialPortIndex port { get; set; }
    void Open();
    void Init();
    void Poll();
    void ReturnBill();
    void StackBill();
    // void Run(IDeviceCommand command);
    // void Execute(IDeviceCommand command);
    // void ExecuteWithMenuOption(MenuOption option);
    // void Set(IDeviceCommand command);
    // void Set(IDeviceExtendedCommand command);
    // uint Get(IDeviceCommand command);
}
