using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendAsync(string name, string url, bool isUp)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_config["Email:From"]));
        message.To.Add(MailboxAddress.Parse(_config["Email:To"]));
        message.Subject = $"{name ?? url} is {(isUp ? "UP" : "DOWN")}";
        message.Body = new TextPart("plain") { Text = $"The service at {url} is now {(isUp ? "UP" : "DOWN")}." };

        using var client = new SmtpClient();
        await client.ConnectAsync(_config["Email:Smtp"], 2525, MailKit.Security.SecureSocketOptions.None);
        await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
