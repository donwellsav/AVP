using System;
using Rug.Osc;

namespace AVP.Services;

public interface IOscService
{
    event EventHandler<OscMessage>? MessageReceived;
    bool Start(int port);
    void Stop();
}
