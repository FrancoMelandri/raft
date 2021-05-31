using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using TinyFp.Extensions;
using static TinyFp.Extensions.Functional;

namespace RaftApplication
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        protected IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
            => services
                .Tee(_ => _.AddControllers());

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseRouting()
                .UseEndpoints(_ => _.MapControllers());
    }
}
