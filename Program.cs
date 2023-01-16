using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Overtime.Model;
using Overtime.TrapHandler;
using Overtime.Verb;
using Serilog;

namespace Overtime
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Async(s => s.Console())
                .CreateLogger();

            Log.Information("Building host.");

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder.Sources.Clear();
                    configurationBuilder
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json", optional: true);
                })
                .ConfigureServices((hostBuilderContext, serviceCollection) =>
                    serviceCollection
                        .Configure<Configuration>(hostBuilderContext.Configuration.GetSection(nameof(Configuration)))
                        .AddDbContextFactory<Context>()
                        .AddSingleton<IVerb, SetTimeZone>()
                        .AddSingleton<IVerb, RemoveData>()
                        .AddSingleton<ITrapHandler, ConvertibleTime>()
                        .AddSingleton<IButtonHandler, ConvertibleTime>()
                        .AddHostedService<Service.Client>()
                )
                .UseSerilog()
                .Build();

            Log.Information("Starting host.");
            await host.RunAsync();
            Log.Information("Shutting down.");
            Environment.Exit(0);
        }
    }
}