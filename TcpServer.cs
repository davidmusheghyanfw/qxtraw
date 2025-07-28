using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System; // For EventHandler

public class TcpServer : IDisposable // Implement IDisposable for proper cleanup
{
    private readonly TcpListener _listener;
    private StreamWriter? _writer;
    private StreamReader? _reader; // New: For reading incoming messages
    private TcpClient? _client; // New: Store the client reference

    // Event to notify when a message is received
    public event EventHandler<string>? OnMessageReceived;

    public TcpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        Console.WriteLine($"Server listening on port {port}...");
    }

    public async Task WaitForClientAsync()
    {
        Console.WriteLine("Waiting for Unity client...");
        _client = await _listener.AcceptTcpClientAsync();
        Console.WriteLine("Unity client connected!");

        var stream = _client.GetStream();
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        _reader = new StreamReader(stream, Encoding.UTF8); // Initialize reader

        // Start listening for incoming messages in the background
        _ = Task.Run(ListenForIncomingMessages);
    }

    public async Task SendMessageAsync(string message)
    {
        if (_writer == null || _client == null || !_client.Connected)
        {
            Console.WriteLine("Warning: No client connected to send message.");
            return; // Or throw an exception
        }

        try
        {
            await _writer.WriteLineAsync(message);
            Console.WriteLine($"✅ Sent to Unity: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to Unity: {ex.Message}");
            // Handle disconnection here if necessary
        }
    }

    private async Task ListenForIncomingMessages()
    {
        if (_reader == null || _client == null) return;

        try
        {
            while (_client.Connected)
            {
                string? message = await _reader.ReadLineAsync(); // Reads until newline
                if (message == null) // Client disconnected
                {
                    Console.WriteLine("Client disconnected.");
                    // You might want to raise a disconnection event here
                    break;
                }
                Console.WriteLine($"📧 Received from Client: {message}");
                OnMessageReceived?.Invoke(this, message); // Raise the event
            }
        }
        catch (IOException ioEx) when (ioEx.InnerException is SocketException sex && sex.SocketErrorCode == SocketError.ConnectionReset)
        {
            Console.WriteLine("Client forcibly disconnected.");
        }
        catch (ObjectDisposedException)
        {
            // Stream or client was disposed, graceful shutdown
            Console.WriteLine("TCP server listener or stream disposed during read.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listening for client messages: {ex.Message}");
        }
    }

    // Implement IDisposable for proper resource cleanup
    public void Dispose()
    {
        Console.WriteLine("Disposing TcpServer...");
        _client?.Close(); // Close the client connection
        _listener?.Stop(); // Stop listening for new connections
        _writer?.Dispose();
        _reader?.Dispose();
        Console.WriteLine("TcpServer disposed.");}
}