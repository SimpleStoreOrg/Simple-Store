using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.ShopperAssistant.Commands.UpdateShopperAssistant;

public class UpdateShopperAssistantCommandHandler : IRequestHandler<UpdateShopperAssistantCommand, UserResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateShopperAssistantCommandHandler> _logger;

    public UpdateShopperAssistantCommandHandler(IUserServiceDbContext dbContext, ILogger<UpdateShopperAssistantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<UserResponse> Handle(UpdateShopperAssistantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Shopper Assistant with ID: {Id}", request.ShopperAssistantId);

        var email = request.Request.Email.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber.Trim().ToLower();

        var exists = await _dbContext.ShopperAssistants
            .AnyAsync(e =>
                    e.Id != request.ShopperAssistantId &&
                    (e.Email == email || e.PhoneNumber == phoneNumber), cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Shopper Assistant already exists with email or phone number: {Email}, {PhoneNumber}", email, phoneNumber);
            throw new ShopperAssistantAlreadyExistsException();
        }
        
        var shopperAssistant =
            await _dbContext.ShopperAssistants.FirstOrDefaultAsync(e => e.Id == request.ShopperAssistantId, cancellationToken);

        if (shopperAssistant == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {Id} not found", request.ShopperAssistantId);
            throw new ShopperAssistantNotFoundException(request.ShopperAssistantId);
        }

        shopperAssistant.Name = request.Request.Name;
        shopperAssistant.Surname = request.Request.Surname;
        shopperAssistant.Email = email;
        shopperAssistant.Role = request.Request.Role;
        shopperAssistant.PhoneNumber = phoneNumber;
        shopperAssistant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Shopper Assistant {Id} updated successfully. Name: {Name}, Surname: {Surname}", request.ShopperAssistantId,
            shopperAssistant.Name, shopperAssistant.Surname);

        return new UserResponse()
        {
            Id = request.ShopperAssistantId,
            Name = shopperAssistant.Name,
            Surname = shopperAssistant.Surname,
            Email = shopperAssistant.Email,
            PhoneNumber = shopperAssistant.PhoneNumber,
            CreatedAt = shopperAssistant.CreatedAt,
            UpdatedAt = shopperAssistant.UpdatedAt
        };
    }
}