using System.Drawing;
using Quixant.LibQLed;

public class LEDController : IDisposable
{
    private QxLedController? _controller;
    protected readonly Dictionary<int, CancellationTokenSource> _channelLoops = new();

    public void Init()
    {
        _controller = new QxLedController();
        _controller.StartLedService();
    }

    public void ApplyPattern(int ch, ILedPattern pattern)
    {
        StopLoop(ch);

        if (_controller != null)
        {
            var cts = new CancellationTokenSource();
            _channelLoops[ch] = cts;
            _ = pattern.StartAsync(ch, _controller, cts);
        }
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

    public void Dispose()
    {
        DisposeController();
        Console.WriteLine("LEDController Dispose()");
    }
}