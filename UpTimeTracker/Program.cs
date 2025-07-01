using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(config["LogFile"]!)
    .CreateLogger();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("Default")));

        services.AddSingleton(config);
        services.AddSingleton<EmailService>();
        services.AddTransient<UptimeChecker>();
        services.AddLogging(builder => builder.AddSerilog());
    })
    .Build();

var interval = int.Parse(config["CheckIntervalSeconds"] ?? "60");
var checker = host.Services.GetRequiredService<UptimeChecker>();

while (true)
{
    await checker.RunAsync();
    await Task.Delay(TimeSpan.FromSeconds(interval));
}

