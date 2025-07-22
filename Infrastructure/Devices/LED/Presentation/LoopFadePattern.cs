using System.Drawing;

class LoopFadePattern : ILedPattern
{
    private readonly Random _random = new();

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        await LoopFadeRandomAsync(channel, controller, 1f, cts.Token);
    }

    private async Task LoopFadeRandomAsync(int channel, QxLedController controller, float duration, CancellationToken token)
    {
        try
        {
            Color currentColor = GetRandomColor();

            while (!token.IsCancellationRequested)
            {
                Color nextColor = GetRandomColor();
                await controller.FadeToColor(channel, nextColor.R, nextColor.G, nextColor.B, duration);
                currentColor = nextColor;
            }
        }
        catch (OperationCanceledException)
        {
            // canceled
        }
    }

    private Color GetRandomColor()
    {
        return Color.FromArgb(
            _random.Next(256),
            _random.Next(256),
            _random.Next(256)
        );
    }

}
