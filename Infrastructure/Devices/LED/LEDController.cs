using System.Drawing;
using Quixant.LibQLed;

public class LEDController : IDisposable
{
    private QxLedController _controller;
    protected readonly Dictionary<int, CancellationTokenSource> _channelLoops = new();

    public void Init()
    {
        _controller = new QxLedController();
        _controller.StartLedService();
    }

    public void ApplyPattern(int ch, ILedPattern pattern)
    {
        StopLoop(ch);
        var cts = new CancellationTokenSource();
        _channelLoops[ch] = cts;
        _ = pattern.StartAsync(ch, _controller, cts);
    }

    public void DisposeController()
    {
        StopAllLoops();
        _controller?.Dispose();
        _controller = null;
    }

    public void ApplyPatterns()
    {
        this.StopAllLoops();
        this.ApplyPattern(0, new SolidColorPattern(Color.Blue));
        this.ApplyPattern(1, new HearthbeatPattern(Color.Red));
        this.ApplyPattern(2, new LoopFadePattern());
        this.ApplyPattern(3, new RainbowPattern());
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

    public void StopLoop(int ch)
    {
        if (_channelLoops.TryGetValue(ch, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _channelLoops.Remove(ch);
        }
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