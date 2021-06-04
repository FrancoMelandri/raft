using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raft.Cluster;
using Raft.Node;
using RaftApplication.Services;
using RaftApplication.Services.Application;
using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                .Tee(InitializeRaftCore)
                .Tee(InitializeApplication)
                .Tee(InitializeNodeConfiguration)
                .Tee(InitializeClosterConfiguration);

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseRouting()
                .UseEndpoints(_ => _.MapControllers());

        private void InitializeControllers(IServiceCollection services)
            => services
                .Tee(_ => _.AddControllers());

        private void InitializeHostedService(IServiceCollection services)
            => services
                .Tee(_ => _.AddSingleton<RaftService>())
                .Tee(_ => _.AddHostedService<RaftService>());

        private void InitializeRaftCore(IServiceCollection services)
            => services
                .Tee(_ => _.AddSingleton<ICluster>(svc => 
                                new Cluster(svc.GetService<ClusterConfiguration>()
                                                .Nodes
                                                .Map(__ => new ClusterNode(__,
                                                                           svc.GetService<IMessageSender>()))
                                                .ToArray())))
                .Tee(_ => _.AddSingleton<IStatusRepository, FileStatusRepository>())
                .Tee(_ => _.AddSingleton<IElection, Election>())
                .Tee(_ => _.AddSingleton<ILeader, Leader>())
                .Tee(_ => _.AddSingleton<IAgent, Agent>())
                .Tee(_ => _.AddSingleton<IMessageSerializer, ApplicationMessageSerializer>())
                .Tee(_ => _.AddSingleton<IMessageListener, TcpMessageListener>())
                .Tee(_ => _.AddSingleton<IMessageSender, TcpMessageSender>())
                .Tee(_ => _.AddSingleton<ILeaderFailureDetector, LeaderFailureDetector>())
                .Tee(_ => _.AddSingleton<ILocalNode, LocalNode>());

        private void InitializeApplication(IServiceCollection services)
            => services
                .Tee(_ => _.AddSingleton<IApplication, ExampleApplication>());

        private void InitializeNodeConfiguration(IServiceCollection services)
            => Configuration
                .GetSection(typeof(LocalNodeConfiguration).Name)
                .Get<LocalNodeConfiguration>()
                .Map(services.AddSingleton);

        private void InitializeClosterConfiguration(IServiceCollection services)
            => Configuration
                .GetSection(typeof(ClusterConfiguration).Name)
                .Get<ClusterConfiguration>()
                .Map(services.AddSingleton);
    }
}
