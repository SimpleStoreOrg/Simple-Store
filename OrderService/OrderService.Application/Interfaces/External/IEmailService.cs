namespace OrderService.Application.Interfaces.External;

public interface IEmailService
{
    Task SendStatusEmailAsync(
        string toEmail,
        long orderId,
        string newStatus,
        CancellationToken cancellationToken = default);
}