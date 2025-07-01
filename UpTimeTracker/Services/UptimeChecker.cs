using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using UpTimeTracker.Models;
using Microsoft.Data.SqlClient;

public class UptimeChecker
{
    private readonly AppDbContext _db;
    private readonly EmailService _emailService;
    private readonly HttpClient _http;
    private readonly ILogger<UptimeChecker> _logger;
    private readonly IConfiguration _config;
    private DateTime? _lastCleanup;

    public UptimeChecker(AppDbContext db, EmailService emailService, ILogger<UptimeChecker> logger, IConfiguration config)
    {
        _db = db;
        _emailService = emailService;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _logger = logger;
        _config = config;
    }

    public async Task RunAsync()
    {
        var services = await _db.MonitoredServices.ToListAsync();

        foreach (var service in services)
        {
            var start = DateTime.UtcNow;
            bool isUp = false;
            int statusCode = 0;
            int responseTime = 0;

            try
            {
                if (service.Type == "http")
                {
                    var response = await _http.GetAsync(service.Url);
                    statusCode = (int)response.StatusCode;
                    isUp = response.IsSuccessStatusCode;
                }
                else if (service.Type == "sql")
                {
                    using var conn = new SqlConnection(service.Url); // Use full connection string in Url field
                    await conn.OpenAsync();
                    isUp = true;
                    statusCode = 200;
                }
            }
            catch
            {
                statusCode = 0;
            }

            responseTime = (int)(DateTime.UtcNow - start).TotalMilliseconds;

            _db.UptimeLogs.Add(new UptimeLog
            {
                Url = service.Url,
                IsUp = isUp,
                StatusCode = statusCode,
                ResponseTimeMs = responseTime,
                Timestamp = DateTime.UtcNow
            });

            if (service.LastKnownStatus != isUp)
            {
                _logger.LogInformation($"Status changed for {service.Url}: {(isUp ? "UP" : "DOWN")}");
                await _emailService.SendAsync(service.Name!, service.Url, isUp);
                if (isUp)
                {
                    service.UpSince = DateTime.UtcNow;
                }
                else
                {
                    service.UpSince = null;
                }
                service.LastKnownStatus = isUp;
            }

            service.LastChecked = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        if (_lastCleanup == null || _lastCleanup.Value.Date < DateTime.UtcNow.Date)
        {
            await CleanupOldLogsAsync();
            _lastCleanup = DateTime.UtcNow;
        }
    }
    private async Task CleanupOldLogsAsync()
    {
        try
        {
            int retentionDays = int.TryParse(_config["RetentionDays"], out var days) ? days : 90;
            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

            var oldLogs = _db.UptimeLogs.Where(l => l.Timestamp < cutoff);
            int count = await oldLogs.CountAsync();

            if (count > 0)
            {
                _db.UptimeLogs.RemoveRange(oldLogs);
                await _db.SaveChangesAsync();
                _logger.LogInformation($"Deleted {count} old uptime logs (older than {retentionDays} days).");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up old uptime logs.");
        }
    }
}
