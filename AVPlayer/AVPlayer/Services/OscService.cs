using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AVPlayer.Services
{
    public interface IOscService
    {
        void Start(int port);
        void Stop();
        event EventHandler<string> CommandReceived;
    }

    public class OscService : IOscService
    {
        private readonly ILogger<OscService> _logger;
        private UdpClient? _udpClient;
        private bool _isRunning;

        public event EventHandler<string>? CommandReceived;

        public OscService(ILogger<OscService> logger)
        {
            _logger = logger;
        }

        public void Start(int port)
        {
            try
            {
                _udpClient = new UdpClient(port);
                _isRunning = true;
                _logger.LogInformation("OSC Server started on port {Port}", port);

                Task.Run(ListenLoop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start OSC server.");
            }
        }

        private async Task ListenLoop()
        {
            while (_isRunning && _udpClient != null)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();

                    // Manual OSC Address Parsing
                    // OSC Address is a string starting with '/' and null-terminated.
                    // Following that are type tags starting with ',' also null-terminated.
                    // We only care about the address for simple commands like /take.

                    if (result.Buffer.Length > 0 && result.Buffer[0] == '/')
                    {
                        // Find the first null byte which terminates the address string
                        int nullIndex = Array.IndexOf(result.Buffer, (byte)0);
                        if (nullIndex > 0)
                        {
                            string address = Encoding.ASCII.GetString(result.Buffer, 0, nullIndex);
                            _logger.LogInformation("OSC Received: {Address}", address);

                            // Invoke on UI thread via event? No, event is generic. subscriber handles dispatch.
                            CommandReceived?.Invoke(this, address);
                        }
                    }
                }
                catch (ObjectDisposedException) { /* Closing */ }
                catch (Exception ex)
                {
                    if (_isRunning) _logger.LogError(ex, "OSC Receive Error");
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}
