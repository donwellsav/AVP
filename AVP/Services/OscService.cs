using AVP.Services;
using Rug.Osc;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AVP.Services;

public class OscService : IOscService, IDisposable
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private OscReceiver? _receiver;
    private Thread? _thread;
    private volatile bool _isRunning;

    public OscService(IMediaPlayerService mediaPlayerService)
    {
        _mediaPlayerService = mediaPlayerService;
    }

    public void Start(int port)
    {
        if (_isRunning)
        {
            Log.Warning("OSC Service is already running.");
            return;
        }

        try
        {
            _receiver = new OscReceiver(port);
            _receiver.Connect();

            _isRunning = true;
            _thread = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "OscListenerThread"
            };
            _thread.Start();

            Log.Information($"OSC Service started on port {port}");
        }
        catch (Exception ex)
        {
             Log.Error(ex, $"Failed to start OSC Service on port {port}");
             _isRunning = false;
        }
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        try
        {
            _receiver?.Close();
            // _thread?.Join(1000); // Wait for thread to exit nicely
            Log.Information("OSC Service stopped.");
        }
        catch (Exception ex)
        {
             Log.Error(ex, "Error stopping OSC Service");
        }
    }

    private void ListenLoop()
    {
        try
        {
            while (_receiver != null && _receiver.State != OscSocketState.Closed)
            {
                if (_receiver.State == OscSocketState.Connected)
                {
                    // This blocks until a packet is received or the socket is closed
                    OscPacket packet = _receiver.Receive();

                    if (packet is OscMessage message)
                    {
                        ProcessMessage(message);
                    }
                    else if (packet is OscBundle bundle)
                    {
                        foreach (var msg in bundle)
                        {
                           if (msg is OscMessage m) ProcessMessage(m);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (_isRunning)
            {
                 Log.Error(ex, "Error in OSC Listen Loop");
            }
        }
    }

    private void ProcessMessage(OscMessage message)
    {
        Log.Debug($"Received OSC: {message.Address} {string.Join(" ", message)}");

        try
        {
            switch (message.Address)
            {
                case "/play":
                    _mediaPlayerService.Play();
                    break;
                case "/pause":
                    _mediaPlayerService.Pause();
                    break;
                case "/stop":
                    _mediaPlayerService.Stop();
                    break;
                case "/load":
                    if (message.Count > 0 && message[0] is string path)
                    {
                        _mediaPlayerService.Load(path);
                    }
                    break;
                case "/position":
                     if (message.Count > 0)
                     {
                         float pos = 0f;
                         var arg = message[0];
                         if (arg is float f) pos = f;
                         else if (arg is int i) pos = (float)i;
                         else if (arg is double d) pos = (float)d;

                         _mediaPlayerService.SetPosition(pos);
                     }
                     break;
                case "/volume":
                     if (message.Count > 0 && message[0] is int vol)
                     {
                         _mediaPlayerService.Volume = vol;
                     }
                     break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error processing OSC message: {message.Address}");
        }
    }

    public void Dispose()
    {
        Stop();
        _receiver?.Dispose();
        GC.SuppressFinalize(this);
    }
}
