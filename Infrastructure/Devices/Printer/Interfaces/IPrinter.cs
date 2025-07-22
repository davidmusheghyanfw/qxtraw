public interface IPrinter
{
    bool IsConnected { get; }
    void Init();
    void Print(String data);
    void PrintDemoTicket();
}
