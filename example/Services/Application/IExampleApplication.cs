using RaftCore.Adapters;
using TinyFp;

namespace RaftApplication.Services.Application
{
    public interface IExampleApplication : IApplication
    {
        Unit Initialise();
        Unit Deinitialise();
    }
}
