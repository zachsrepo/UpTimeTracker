using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using UpTimeTracker.Models;

public class UptimeChecker
{
    private readonly AppDbContext _db;
    private readonly EmailService _emailService;
    private readonly HttpClient _http;
    private readonly ILogger<UptimeChecker> _logger;

    public UptimeChecker(AppDbContext db, EmailService emailService, ILogger<UptimeChecker> logger)
    {
        _db = db;
        _emailService = emailService;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _logger = logger;
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
                var response = await _http.GetAsync(service.Url);
                statusCode = (int)response.StatusCode;
                isUp = response.IsSuccessStatusCode;
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
                service.LastKnownStatus = isUp;
            }

            service.LastChecked = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}
