using CCnetWPF.Adapter;
using CCnetWPF.Connections.CCtalk;
using CCnetWPF.Events;

public class Mpost
{
    private IBillValidator _billValidator;

    public Mpost()
    {
        InitializeBillValidator("/dev/ttyS3"); // Example port, adjust as needed
    }

    private void InitializeBillValidator(string portName)
    {
        _billValidator = new CcTalkBillValidator();
        _billValidator.OnConnectedEvent += OnBillValidatorConnected;
        _billValidator.Init(portName);
    }

    private void OnBillValidatorConnected(object sender, BillConnectedEventArgs e)
    {
        EnableBillAcceptance();
        SubscribeToBillValidatorEvents();
        ToggleBarcode(true);
    }
    private void SubscribeToBillValidatorEvents()
    {
        _billValidator.BillReceived += OnBillReceived;
        _billValidator.BillStacked += OnBillStacked;
    }

    private async void OnBillReceived(object sender, BillReceivedEventArgs e)
    {
        Console.WriteLine($"Bill received: Status - {e.Status}");
        Console.WriteLine(
           $"Bill received: Status - {e.Status}");

        switch (e.Status)
        {
            case BillReceivedEventArgs.BillRecievedStatus.Rejected:
                Console.WriteLine($"Bill rejected: {e.RejectedReason}");
                Console.WriteLine(
                   $"Bill rejected: {e.RejectedReason}");
                break;

            case BillReceivedEventArgs.BillRecievedStatus.Accepted:
                EscrowStack();
                Console.WriteLine($"Bill accepted: Value - {e.Value} EUR");
                Console.WriteLine(
                   $"Bill accepted: Value - {e.Value} EUR");
                break;

            default:
                Console.WriteLine("Unknown bill received status.");
                Console.WriteLine(
                   $"Unknown bill received status.");
                break;
        }
    }

    private void OnBillStacked(object sender, BillStackedEventArgs e)
    {
        Console.WriteLine($"Bill stacked with cash code: {e.Cashcode}");
        Console.WriteLine(
           $"Bill stacked with cash code: {e.Cashcode}");

        // Example: Cancel stacking under specific conditions
        e.Cancel = ShouldCancelStacking(e.Cashcode);
    }

    private bool ShouldCancelStacking(int cashCode)
    {
        // Example condition: Cancel stacking for specific denominations
        return cashCode == 1000 || cashCode == 5000 || cashCode == 10000;
    }

    public void EnableBillAcceptance()
    {
        _billValidator?.EnableBill();
        Console.WriteLine("Bill acceptance enabled.");
        Console.WriteLine(
            $"Bill acceptance enabled.");
    }

    public void DisableBillAcceptance()
    {
        _billValidator?.DisableBill();
        Console.WriteLine("Bill acceptance disabled.");
        Console.WriteLine(
            $"Bill acceptance disabled.");
    }

    public void ReturnBill()
    {
        _billValidator?.ReturnBill();
        Console.WriteLine("Bill returned.");
        Console.WriteLine(
            $"Bill returned.");
    }

    public void EscrowStack()
    {
        _billValidator?.EscrowStack();
        Console.WriteLine("On Escrow stacked.");
        Console.WriteLine(
            $"On Escrow stacked.");
    }

    public void ToggleBarcode(bool state)
    {
        _billValidator?.ToggleBarcode(state);
        Console.WriteLine($"Toggle Barcode: {state}");
        Console.WriteLine(
            $"Toggle Barcode: {state}");
    }

    public void CheckBillValidatorStatus()
    {
        if (_billValidator == null)
        {
            Console.WriteLine("Bill validator is not initialized.");
            Console.WriteLine(
                $"Bill validator is not initialized.");
            return;
        }

        // Check if the bill validator is connected
        bool isConnected = _billValidator.IsConnected; // Assuming IsConnected is a property in IBillValidator

        Console.WriteLine($"Bill validator is connected: {isConnected}");
        Console.WriteLine(
            $"Bill validator is connected: {isConnected}");
    }
}