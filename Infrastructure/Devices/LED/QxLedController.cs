using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class QxLedController : IDisposable
{

    public float frameRate = 20f;

    public bool overrideLedCount = false;
    public int manualLedCount = 30;

    private const string LIB = "qled";
    private const int CHANNEL_COUNT = 4;

    private IntPtr[] _handles = new IntPtr[CHANNEL_COUNT];
    private byte[][] _frames = new byte[CHANNEL_COUNT][];
    private int[] _ledCounts = new int[CHANNEL_COUNT];
    private bool[] _dirty = new bool[CHANNEL_COUNT];
    private readonly object[] _locks = new object[CHANNEL_COUNT];
    private CancellationTokenSource _cts;

    private bool _running;

    #region Native
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct QLedAdapterInventory
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string board_name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string micro_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string micro_vendor;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] micro_sign;
        public byte micro_bootable;
        public byte channels;
        public ushort max_pxls;
        public byte auto_num;
        public ushort auto_pxls;
        public byte supp_playlist;
        public ushort flash_size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] supp_devices;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] fw_rev;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] protocol_rev;
        public ulong lib_version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lib_name;
    }

    [DllImport(LIB)] private static extern IntPtr qlcOpen(string device);
    [DllImport(LIB)] private static extern int qlcWriteFrame(IntPtr h, byte[] leds, int size);
    [DllImport(LIB)] private static extern void qlcClose(IntPtr h);
    [DllImport(LIB)] private static extern int qlcGetAdapterInventory(IntPtr h, out QLedAdapterInventory inv);
    [DllImport(LIB)] private static extern int qlcGetLastError(IntPtr h);
    #endregion


    public void StartLedService()
    {
        if (_running) return;

        _running = true;
        _cts = new CancellationTokenSource();

        for (int ch = 0; ch < CHANNEL_COUNT; ch++)
        {
            _locks[ch] ??= new object();

            try
            {
                string deviceId = $"qb029,{ch}";
                _handles[ch] = qlcOpen(deviceId);

                if (_handles[ch] == IntPtr.Zero)
                {
                    Console.WriteLine($"[QxLED] Failed to open {deviceId}, error {qlcGetLastError(IntPtr.Zero)}");
                    continue;
                }

                if (qlcGetAdapterInventory(_handles[ch], out var info) != 0)
                {
                    Console.WriteLine($"[QxLED] Could not get adapter info for {deviceId}");
                    _ledCounts[ch] = overrideLedCount ? manualLedCount : 30;
                }
                else
                {
                    _ledCounts[ch] = overrideLedCount ? manualLedCount : info.auto_pxls;
                    Console.WriteLine($"[QxLED] {deviceId} detected {_ledCounts[ch]} LEDs");
                }

                _frames[ch] = new byte[_ledCounts[ch] * 3];
                _dirty[ch] = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QxLED] Exception during init of channel {ch}: {ex}");
            }
        }

        Task.Run(() => FrameUpdateLoop(_cts.Token), _cts.Token);
    }

    private async Task FrameUpdateLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                for (int ch = 0; ch < CHANNEL_COUNT; ch++)
                {
                    if (_handles[ch] != IntPtr.Zero && _dirty[ch])
                    {
                        lock (_locks[ch])
                        {
                            qlcWriteFrame(_handles[ch], _frames[ch], _ledCounts[ch]);
                            _dirty[ch] = false;
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1f / frameRate), token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"[QxLED] Frame loop crashed: {ex}");
        }
    }


    public void StopLedService()
    {
        if (!_running) return;
        _running = false;

        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[QxLED] Exception on cancellation: {ex}");
        }

        for (int ch = 0; ch < CHANNEL_COUNT; ch++)
        {
            try
            {
                if (_handles[ch] != IntPtr.Zero)
                {
                    qlcClose(_handles[ch]);
                    _handles[ch] = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QxLED] Error closing channel {ch}: {ex}");
            }
        }

        Console.WriteLine("[QxLED] All channels closed");
    }

    public void SetAllLeds(int ch, byte r, byte g, byte b)
    {
        if (!Validate(ch)) return;

        lock (_locks[ch])
        {
            for (int i = 0; i < _ledCounts[ch]; i++)
            {
                _frames[ch][i * 3 + 0] = r;
                _frames[ch][i * 3 + 1] = g;
                _frames[ch][i * 3 + 2] = b;
            }

            _dirty[ch] = true;
        }
    }

    public void SetLed(int ch, int index, byte r, byte g, byte b)
    {
        if (!Validate(ch) || index < 0 || index >= _ledCounts[ch]) return;

        lock (_locks[ch])
        {
            int i = index * 3;
            _frames[ch][i + 0] = r;
            _frames[ch][i + 1] = g;
            _frames[ch][i + 2] = b;
            _dirty[ch] = true;
        }
    }

    private const float FrameRate = 60.0f;

    public async Task FadeToColor(int ch, byte targetR, byte targetG, byte targetB, float duration)
    {
        if (!Validate(ch)) return;

        // Calculate the number of steps for the animation based on duration and frame rate.
        int steps = (int)Math.Ceiling(duration * FrameRate);
        if (steps <= 0) steps = 1;

        // Create a copy of the starting frame to interpolate from.
        byte[] startFrame = new byte[_ledCounts[ch] * 3];
        lock (_locks[ch])
        {
            Array.Copy(_frames[ch], startFrame, startFrame.Length);
        }

        for (int step = 0; step <= steps; step++)
        {
            // 't' is the interpolation factor, from 0.0 to 1.0.
            float t = step / (float)steps;

            lock (_locks[ch])
            {
                for (int i = 0; i < _ledCounts[ch]; i++)
                {
                    int idx = i * 3;
                    _frames[ch][idx + 0] = (byte)Lerp(startFrame[idx + 0], targetR, t);
                    _frames[ch][idx + 1] = (byte)Lerp(startFrame[idx + 1], targetG, t);
                    _frames[ch][idx + 2] = (byte)Lerp(startFrame[idx + 2], targetB, t);
                }
                _dirty[ch] = true;
            }

            // Wait for the next frame.
            await Task.Delay(TimeSpan.FromSeconds(1f / FrameRate));
        }
    }

    public async Task Chase(int ch, byte r, byte g, byte b, float delayPerLed)
    {
        if (!Validate(ch)) return;

        int count = _ledCounts[ch];
        for (int i = 0; i < count; i++)
        {
            lock (_locks[ch])
            {
                // Turn on the current LED 'i' and turn all others off.
                for (int j = 0; j < count; j++)
                {
                    int idx = j * 3;
                    _frames[ch][idx + 0] = (j == i) ? r : (byte)0;
                    _frames[ch][idx + 1] = (j == i) ? g : (byte)0;
                    _frames[ch][idx + 2] = (j == i) ? b : (byte)0;
                }
                _dirty[ch] = true;
            }

            await Task.Delay(TimeSpan.FromSeconds(delayPerLed));
        }
    }

    public void ClearChannel(int ch)
    {
        SetAllLeds(ch, 0, 0, 0);
    }

    private bool Validate(int ch)
    {
        if (ch < 0 || ch >= CHANNEL_COUNT || _handles[ch] == IntPtr.Zero)
        {
            Console.WriteLine($"[QxLED] Invalid channel {ch}");
            return false;
        }

        return true;
    }

    public int GetLedCount(int ch)
    {
        return Validate(ch) ? _ledCounts[ch] : 0;
    }

    public void MarkDirty(int ch)
    {
        if (!Validate(ch)) return;

        lock (_locks[ch])
        {
            _dirty[ch] = true;
        }
    }

    private static float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    public void Dispose()
    {
        StopLedService();
    }
}
