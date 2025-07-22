using System.Drawing;
using Quixant.LibQLed;


public class HearthbeatPattern : ILedPattern
{
    private readonly Color _color;
    float intensity = 1f, beatDuration = 0.2f, restDuration = 0.6f;
    bool doubleBeat = true;

    public HearthbeatPattern(Color color)
    {
        _color = color;
    }

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        try
        {
            Color darkColor = MultiplyColor(_color, 0.2f);

            while (!cts.IsCancellationRequested)
            {
                await controller.FadeToColor(channel, _color.R, _color.G, _color.B, beatDuration);
                await controller.FadeToColor(channel, darkColor.R, darkColor.G, darkColor.B, beatDuration);

                if (doubleBeat)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cts.Token);
                    await controller.FadeToColor(channel, _color.R, _color.G, _color.B, beatDuration);
                    await controller.FadeToColor(channel, darkColor.R, darkColor.G, darkColor.B, beatDuration);
                }

                await Task.Delay(TimeSpan.FromSeconds(restDuration), cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static Color MultiplyColor(Color color, float factor)
    {
        return Color.FromArgb(
            (int)(color.R * factor),
            (int)(color.G * factor),
            (int)(color.B * factor)
        );
    }
}