
using System.Drawing;
using Quixant.LibQLed;

public class SolidColorPattern : ILedPattern
{
    private readonly Color _color;

    public SolidColorPattern(Color color)
    {
        _color = color;
    }

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        controller.SetAllLeds(channel, _color.R, _color.G, _color.B);
    }
}