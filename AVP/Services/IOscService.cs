using System.Threading.Tasks;

namespace AVP.Services;

public interface IOscService
{
    void Start(int port);
    void Stop();
}
