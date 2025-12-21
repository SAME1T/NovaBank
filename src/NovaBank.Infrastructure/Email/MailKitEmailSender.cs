using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NovaBank.Application.Common.Email;

namespace NovaBank.Infrastructure.Email;

public sealed class MailKitEmailSender : IEmailSender
{
    private readonly SmtpOptions _opt;

    public MailKitEmailSender(IOptions<SmtpOptions> opt)
    {
        _opt = opt.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = subject;

        var body = new BodyBuilder { HtmlBody = htmlBody };
        msg.Body = body.ToMessageBody();

        using var client = new SmtpClient();

        var secure = _opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        await client.ConnectAsync(_opt.Host, _opt.Port, secure, ct);

        if (!string.IsNullOrWhiteSpace(_opt.Username))
            await client.AuthenticateAsync(_opt.Username, _opt.Password, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}

