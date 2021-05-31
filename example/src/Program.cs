using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TinyFp.Extensions;

namespace RaftApplication
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main()
            => Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureLogging(logging => logging
                                                .Tee(_ => _.ClearProviders())
                                                .Tee(_ => _.AddConsole()))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
    }
}
