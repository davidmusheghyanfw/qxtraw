using System.Drawing;

public interface ILedPattern
{
    public Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts);
}