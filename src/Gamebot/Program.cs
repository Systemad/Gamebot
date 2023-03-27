using Coravel;
using Gamebot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("log/log_.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
        (services) =>
        {
            services.AddFusionCache();
            services.AddHostedService<TwitchClientWorkerService>().AddSingleton<API>();
            //services.AddScheduler();
            //services.AddTransient<RehydrateMatches>();
        }
    )
    .UseSerilog()
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
