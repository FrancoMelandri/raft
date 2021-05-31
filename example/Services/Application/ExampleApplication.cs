using RaftCore.Models;
using TinyFp;

namespace RaftApplication.Services.Application
{
    public class ExampleApplication : IExampleApplication
    {
        public Unit Deinitialise()
            => Unit.Default;

        public Unit Initialise()
            => Unit.Default;

        public Unit NotifyMessage(Message message)
            => Unit.Default;
    }
}
