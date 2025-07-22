
using System.Drawing;

class RainbowPattern : ILedPattern
{

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        await LoopRainbowAsync(channel, controller, 0.1f, cts.Token);
    }

    private async Task LoopRainbowAsync(int channel, QxLedController controller, float delay, CancellationToken token)
    {
        try
        {
            int count = controller?.GetLedCount(channel) ?? 0;
            if (count <= 0) return;

            float hueStep = 1f / count;
            float shift = 0f;

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < count; i++)
                {
                    float hue = (hueStep * i + shift) % 1f;
                    Color color = HsvToRgb(hue, 1f, 1f);
                    controller?.SetLed(channel, i, color.R, color.G, color.B);
                }

                controller?.MarkDirty(channel);
                shift = (shift + hueStep) % 1f;
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public static Color HsvToRgb(float h, float s, float v)
    {
        int i = (int)(h * 6);
        float f = h * 6 - i;
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);

        float r, g, b;
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        return Color.FromArgb(
            (int)(r * 255),
            (int)(g * 255),
            (int)(b * 255)
        );
    }

}