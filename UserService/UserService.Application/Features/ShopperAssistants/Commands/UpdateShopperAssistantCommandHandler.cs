using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.ShopperAssistants.Commands;

public record UpdateShopperAssistantCommand(long ShopperAssistantId, UpdateShopperAssistantRequest Request)
    : IRequest<ShopperAssistantResponse>;

public class UpdateShopperAssistantCommandHandler : IRequestHandler<UpdateShopperAssistantCommand, ShopperAssistantResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateShopperAssistantCommandHandler> _logger;

    public UpdateShopperAssistantCommandHandler(IUserServiceDbContext dbContext,
        ILogger<UpdateShopperAssistantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ShopperAssistantResponse> Handle(UpdateShopperAssistantCommand request,
        CancellationToken cancellationToken)

    {
        _logger.LogInformation("Updating Shopper Assistant with ID: {Id}", request.ShopperAssistantId);

        var shopperAssistant = await _dbContext.Users.OfType<ShopperAssistantEntity>()
            .FirstOrDefaultAsync(x => x.Id == request.ShopperAssistantId, cancellationToken: cancellationToken);
        
        if (shopperAssistant == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {Id} not found", request.ShopperAssistantId);
            throw new ShopperAssistantNotFoundException(request.ShopperAssistantId);
        }
        
        var username = request.Request.Username?.Trim().ToLower();
        var email = request.Request.Email?.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber?.Trim().ToLower();

        var exists = await _dbContext.Users
            .AnyAsync(e =>
                    e.Id != request.ShopperAssistantId && e.Role == RoleStatus.ShopperAssistant &&
                    (e.UserName!.Trim().ToLower() == username || e.Email!.Trim().ToLower() == email ||
                     e.PhoneNumber!.Trim().ToLower() == phoneNumber),
                cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Shopper Assistant already exists with username or email or phone number: {Email}, {PhoneNumber}", email,
                phoneNumber);
            throw new ShopperAssistantAlreadyExistsException();
        }
        
        shopperAssistant.Name = request.Request.Name;
        shopperAssistant.Surname = request.Request.Surname;
        shopperAssistant.Email = email;
        shopperAssistant.Position = request.Request.Position;
        shopperAssistant.UserName = request.Request.Username;
        shopperAssistant.PhoneNumber = phoneNumber;
        shopperAssistant.UpdatedAt = DateTime.UtcNow.AddHours(5);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Shopper Assistant {Id} updated successfully. Name: {Name}, Surname: {Surname}", request.ShopperAssistantId,
            shopperAssistant.Name, shopperAssistant.Surname);

        return new ShopperAssistantResponse
        {
            Id = request.ShopperAssistantId,
            Name = shopperAssistant.Name,
            Surname = shopperAssistant.Surname,
            Email = shopperAssistant.Email,
            Role = shopperAssistant.Role,
            Position = shopperAssistant.Position,
            Username = shopperAssistant.UserName,
            PhoneNumber = shopperAssistant.PhoneNumber,
            CreatedAt = shopperAssistant.CreatedAt,
            UpdatedAt = shopperAssistant.UpdatedAt
        };
    }
}