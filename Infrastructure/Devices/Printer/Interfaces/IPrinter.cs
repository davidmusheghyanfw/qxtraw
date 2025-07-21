public interface IPrinter
{
    bool IsConnected { get; }
    void Print();
    void PrintDemoTicket();
}
