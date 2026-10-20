using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using OrderService.Application.Interfaces.External;

namespace OrderService.Infrastructure.Services;
public class EmailService : IEmailService
{
    private readonly GmailService _gmailService;
    private readonly string _senderAddress;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _logger = logger;
        _senderAddress = config["Email:SenderAddress"] ??
                         throw new InvalidOperationException("Email:SenderAddress is missing from config");

        var credentialsPath = config["Email:CredentialsPath"] ?? "credentials.json";
        var tokenDirectory = config["Email:TokenDirectory"] ?? ".tokens";

        var baseDir = AppContext.BaseDirectory;
        var credentialsFile = Path.Combine(baseDir, credentialsPath);
        var tokensFolder = Path.Combine(baseDir, tokenDirectory);

        if (!File.Exists(credentialsFile))
        {
            throw new FileNotFoundException($"Could not find credentials file at {credentialsFile}");
        }

        var credential = GoogleWebAuthorizationBroker
            .AuthorizeAsync(
                GoogleClientSecrets.FromFile(credentialsFile).Secrets,
                new[] { GmailService.Scope.GmailSend },
                "user",
                CancellationToken.None,
                new FileDataStore(tokensFolder, true))
            .GetAwaiter()
            .GetResult();

        _gmailService = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "SimpleStore-OrderService"
        });

        _logger.LogInformation("Email service ready sending from {Sender}", _senderAddress);
    }

    public async Task SendStatusEmailAsync(
        string toEmail,
        long orderId,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        var subject = GetSubject(orderId, newStatus);
        var body = GetBody(orderId, newStatus);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SimpleStore", _senderAddress));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var stream = new MemoryStream();
        message.WriteTo(stream);

        var encoded = Convert.ToBase64String(stream.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        try
        {
            var result = await _gmailService.Users.Messages
                .Send(new Message { Raw = encoded }, "me")
                .ExecuteAsync(cancellationToken);

            _logger.LogInformation(
                "Sent email for order {OrderId} to {Email} (gmail id {GmailId})",
                orderId, toEmail, result.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for order {OrderId}", orderId);
            throw;
        }
    }

    private static string GetSubject(long orderId, string status)
    {
        if (status == "Collecting")
        {
            return $"Order #{orderId}: we're collecting your items";
        }

        if (status == "ReadyToGo")
        {
            return $"Order #{orderId} is ready for pickup";
        }

        if (status == "Completed")
        {
            return $"Order #{orderId} completed - thank you!";
        }

        if (status == "CancelledByShop")
        {
            return $"Order #{orderId} has been cancelled";
        }

        if (status == "CancelledByCustomer")
        {
            return $"Order #{orderId} cancelled";
        }

        return $"Order #{orderId} status update";
    }

    private static string GetBody(long orderId, string status)
    {
        if (status == "Collecting")
        {
            return $"Hi,\n\n" +
                   $"We've started collecting the items for your order #{orderId}. " +
                   $"We'll let you know when it's ready for pickup.\n\n" +
                   $"Thanks,\nSimpleStore";
        }

        if (status == "ReadyToGo")
        {
            return $"Hi,\n\n" +
                   $"Your order #{orderId} is ready for pickup. " +
                   $"Please come by whenever you can.\n\n" +
                   $"Thanks,\nSimpleStore";
        }

        if (status == "Completed")
        {
            return $"Hi,\n\n" +
                   $"Your order #{orderId} is complete. Thanks for shopping with us!\n\n" +
                   $"SimpleStore";
        }

        if (status == "CancelledByShop")
        {
            return $"Hi,\n\n" +
                   $"Sorry, order #{orderId} was cancelled by the shop. " +
                   $"If you paid, a refund will be processed soon.\n\n" +
                   $"SimpleStore";
        }

        if (status == "CancelledByCustomer")
        {
            return $"Hi,\n\n" +
                   $"Your order #{orderId} has been cancelled.\n\n" +
                   $"SimpleStore";
        }

        return $"Hi,\n\n" +
               $"Your order #{orderId} status changed to: {status}.\n\n" +
               $"SimpleStore";
    }
}