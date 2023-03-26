using Coravel;
using Gamebot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}";

ILogger logger = new LoggerConfiguration().Enrich
    .FromLogContext()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.File(
        "log/log_.txt",
        outputTemplate: outputTemplate,
        rollingInterval: RollingInterval.Day
    )
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
        (services) =>
        {
            services.AddHostedService<TwitchClientWorkerService>();
            //services.AddScheduler();
            //services.AddTransient<RehydrateMatches>();
            services.AddFusionCache();
            services.AddHttpClient<API>(
                client =>
                {
                    client.BaseAddress = new Uri("https://www.hltv.org/");
                }
            );
        }
    )
    .Build();

/*
host.Services.UseScheduler(
    scheduler =>
    {
        scheduler.Schedule<RehydrateMatches>().EveryFifteenMinutes().RunOnceAtStart();
    }
);
*/
await host.RunAsync();
