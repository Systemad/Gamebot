using Gamebot;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("log/log_.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFusionCache();
builder.Services.AddHostedService<TwitchClientWorkerService>().AddSingleton<API>();

// TODO: Wait for this https://github.com/serilog/serilog-extensions-hosting/pull/70
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/redirect/", (HttpRequest request) =>
{
    var queryString = request.QueryString;
    var queryDictionary = QueryHelpers.ParseQuery(queryString.Value);
    var context = request.HttpContext;
    var code=  queryDictionary.TryGetValue("code", out var myQueryParam) ? Results.Text(myQueryParam) : Results.Text("No code was found");
    context.Response.Redirect("/");
});
await app.RunAsync();
