using System.Drawing;
using Quixant.LibQLed;

public class LEDController : IDisposable
{
    private QxLedController _controller;
    private readonly Dictionary<int, CancellationTokenSource> _channelLoops = new();

    public QxLedController Controller => _controller;

    public void Init()
    {
        _controller = new QxLedController();
        _controller.StartLedService();
    }

    public void DisposeController()
    {
        StopAllLoops();
        _controller?.Dispose();
        _controller = null;
    }

    public void SetStaticColors()
    {
        this.StopAllLoops();
        this.SetSolidColor(0, Color.Red);
        this.SetSolidColor(1, Color.Blue);
        this.SetSolidColor(2, Color.Orange);
        this.SetSolidColor(3, Color.Green);
    }

    public void SetLoopFade()
    {
        this.StopAllLoops();
        this.LoopFade(0, Color.Red, Color.Blue, 1f);
        this.LoopFade(1, Color.Green, Color.Yellow, 1f);
        this.LoopFade(2, Color.Orange, Color.Purple, 1f);
        this.LoopFade(3, Color.Cyan, Color.Magenta, 1f);
    }

    public void SetSolidColor(int ch, Color color)
    {
        StopLoop(ch);
        _controller?.SetAllLeds(ch, ToByte(color.R), ToByte(color.G), ToByte(color.B));
    }

    public async Task FadeColor(int ch, Color targetColor, float duration)
    {
        StopLoop(ch);
        if (_controller != null)
        {
            await _controller.FadeToColor(ch, ToByte(targetColor.R), ToByte(targetColor.G), ToByte(targetColor.B),
                duration);
        }
    }

    public async Task ChaseColor(int ch, Color color, float delayPerLed)
    {
        StopLoop(ch);
        if (_controller != null)
        {
            await _controller.Chase(ch, ToByte(color.R), ToByte(color.G), ToByte(color.B), delayPerLed);
        }
    }

    public void Clear(int ch)
    {
        StopLoop(ch);
        _controller?.ClearChannel(ch);
    }

    public void ClearAll()
    {
        for (int ch = 0; ch < 4; ch++)
            Clear(ch);
    }

    public async void LoopFade(int ch, Color a, Color b, float duration)
    {
        StopLoop(ch);
        var cts = new CancellationTokenSource();
        _channelLoops[ch] = cts;
        await LoopFadeAsync(ch, a, b, duration, cts.Token);
    }

    private async Task LoopFadeAsync(int ch, Color a, Color b, float duration, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await FadeColor(ch, a, duration);
                await FadeColor(ch, b, duration);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async void LoopChase(int ch, Color color, float delayPerLed)
    {
        StopLoop(ch);
        var cts = new CancellationTokenSource();
        _channelLoops[ch] = cts;
        await LoopChaseAsync(ch, color, delayPerLed, cts.Token);
    }

    private async Task LoopChaseAsync(int ch, Color color, float delay, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await ChaseColor(ch, color, delay);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async void LoopRainbow(int ch, float delayPerLed)
    {
        StopLoop(ch);
        var cts = new CancellationTokenSource();
        _channelLoops[ch] = cts;
        await LoopRainbowAsync(ch, delayPerLed, cts.Token);
    }

    private async Task LoopRainbowAsync(int ch, float delay, CancellationToken token)
    {
        try
        {
            int count = _controller?.GetLedCount(ch) ?? 0;
            if (count <= 0) return;

            float hueStep = 1f / count;
            float shift = 0f;

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < count; i++)
                {
                    float hue = (hueStep * i + shift) % 1f;
                    Color color = HsvToRgb(hue, 1f, 1f);
                    _controller?.SetLed(ch, i, ToByte(color.R), ToByte(color.G), ToByte(color.B));
                }

                _controller?.MarkDirty(ch);
                shift = (shift + hueStep) % 1f;
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void StopLoop(int ch)
    {
        if (_channelLoops.TryGetValue(ch, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _channelLoops.Remove(ch);
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

    public void StopAllLoops()
    {
        Console.WriteLine("LEDController StopAllLoops() Stopping all loops");
        foreach (var cts in _channelLoops.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }

        _channelLoops.Clear();
    }

    private static byte ToByte(float f) => (byte)Math.Clamp(f * 255, 0, 255);

    public void Dispose()
    {
        DisposeController();
        Console.WriteLine("LEDController Dispose()");

    }
}