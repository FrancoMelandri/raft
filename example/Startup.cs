using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaftApplication.Services;
using RaftApplication.Services.Application;
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
                .Tee(InitializeControllers)
                .Tee(InitializeHostedService)
                .Tee(InitializeApplication);

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseRouting()
                .UseEndpoints(_ => _.MapControllers());

        private static void InitializeControllers(IServiceCollection services)
            => services
                .Tee(_ => _.AddControllers());

        private static void InitializeHostedService(IServiceCollection services)
            => services
                .Tee(_ => _.AddSingleton<RaftService>())
                .Tee(_ => _.AddHostedService<RaftService>());

        private static void InitializeApplication(IServiceCollection services)
            => services
                .Tee(_ => _.AddSingleton<ExampleApplication>())
                .Tee(_ => _.AddSingleton<IExampleApplication>(_ =>
                                new LoggedApplication(_.GetRequiredService<ExampleApplication>(),
                                                      _.GetRequiredService<ILogger<LoggedApplication>>())));
    }
}
