using System.Net;
using System.Net.Sockets;
using System.Text;

public class TcpServer
{
    private readonly TcpListener _listener;
    private StreamWriter? _writer;

    public TcpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
    }

    public async Task WaitForClientAsync()
    {
        var client = await _listener.AcceptTcpClientAsync();
        var stream = client.GetStream();
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
    }

    public async Task SendMessageAsync(string message)
    {
        if (_writer == null)
            throw new InvalidOperationException("No client connected.");

        await _writer.WriteLineAsync(message);
        Console.WriteLine($"✅ Sent: {message}");
    }
}
