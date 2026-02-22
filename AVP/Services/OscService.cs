using Rug.Osc;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AVP.Services;

public class OscService : IOscService, IDisposable
{
    private OscReceiver? _receiver;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _listenerTask;

    public event EventHandler<OscMessage>? MessageReceived;

    public bool Start(int port)
    {
        Stop(); // Ensure any existing listener is stopped

        try
        {
            _receiver = new OscReceiver(port);
            _receiver.Connect(); // Binds the socket

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _listenerTask = Task.Run(() => ListenLoop(token), token);
            Log.Information($"OSC Service started on port {port}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to start OSC Service on port {port}");
            return false;
        }
    }

    private void ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _receiver != null && _receiver.State != OscSocketState.Closed)
        {
            try
            {
                if (_receiver.State == OscSocketState.Connected)
                {
                    // Receive blocks until a packet arrives or the socket is closed.
                    // If the socket is closed during Receive(), it throws an exception or returns null/empty?
                    // Usually throws. We catch it.
                    var packet = _receiver.Receive();

                    if (packet == null) continue;

                    ProcessPacket(packet);
                }
                else
                {
                    // Avoid busy wait if not connected but loop is running (though Connect() sets it to connected)
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested && _receiver?.State != OscSocketState.Closed)
                {
                    Log.Error(ex, "Error receiving OSC message");
                }
            }
        }
    }

    private void ProcessPacket(OscPacket packet)
    {
        if (packet is OscMessage message)
        {
            MessageReceived?.Invoke(this, message);
        }
        else if (packet is OscBundle bundle)
        {
            foreach (var bundlePacket in bundle)
            {
                ProcessPacket(bundlePacket); // Recursive processing for bundles within bundles
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        try
        {
            _receiver?.Close(); // This should unblock Receive and throw exception or just return
            _receiver?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error closing OSC receiver");
        }

        _receiver = null;
        _cancellationTokenSource = null;
        Log.Information("OSC Service stopped");
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
