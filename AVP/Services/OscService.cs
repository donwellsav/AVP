using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreOSC;
using CoreOSC.IO;
using Serilog;

namespace AVP.Services;

public class OscService : IOscService, IDisposable
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private CancellationTokenSource? _cts;
    private readonly int _port;
    private bool _disposed;

    public OscService(IMediaPlayerService mediaPlayerService, int port = 8000)
    {
        _mediaPlayerService = mediaPlayerService;
        _port = port;
    }

    public void Start()
    {
        if (_cts != null) return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Task.Run(async () => await ListenLoop(token), token);
        Log.Information("OSC Service starting on port {Port}", _port);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        Log.Information("OSC Service stopped");
    }

    private async Task ListenLoop(CancellationToken token)
    {
        try
        {
            using var udpClient = new UdpClient(_port);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // CoreOSC provides ReceiveMessageAsync extension for UdpClient
                    // OscMessage appears to be a struct in this version of CoreOSC
                    var message = await udpClient.ReceiveMessageAsync();
                    HandleMessage(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Log.Error(ex, "Error receiving OSC message");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "OSC Listener failed to start on port {Port}", _port);
        }
    }

    private void HandleMessage(OscMessage message)
    {
        // Address and OscMessage appear to be structs in this version of CoreOSC
        string addressStr = message.Address.ToString();
        var args = message.Arguments?.ToArray() ?? Array.Empty<object>();

        Log.Debug("Received OSC: {Address} with {Count} arguments", addressStr, args.Length);

        try
        {
            switch (addressStr.ToLowerInvariant())
            {
                case "/avp/play":
                    _mediaPlayerService.Play();
                    break;
                case "/avp/pause":
                    _mediaPlayerService.Pause();
                    break;
                case "/avp/stop":
                    _mediaPlayerService.Stop();
                    break;
                case "/avp/volume":
                    if (args.Length > 0)
                    {
                        var arg = args[0];
                        if (arg is int iVol)
                        {
                            _mediaPlayerService.Volume = iVol;
                        }
                        else if (arg is float fVol)
                        {
                            _mediaPlayerService.Volume = (int)(fVol * 100);
                        }
                    }
                    break;
                case "/avp/seek":
                    if (args.Length > 0 && args[0] is float position)
                    {
                        _mediaPlayerService.SetPosition(position);
                    }
                    break;
                default:
                    Log.Warning("Unknown OSC address: {Address}", addressStr);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error handling OSC message: {Address}", addressStr);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _disposed = true;
    }
}
