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

public record CreateShopperAssistantCommand(CreateShopperAssistantRequest Request) : IRequest<ShopperAssistantResponse>;

public class CreateShopperAssistantCommandHandler : IRequestHandler<CreateShopperAssistantCommand, ShopperAssistantResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<CreateShopperAssistantCommandHandler> _logger;

    public CreateShopperAssistantCommandHandler(IUserServiceDbContext dbContext, ILogger<CreateShopperAssistantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<ShopperAssistantResponse> Handle(CreateShopperAssistantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Shopper Assistant. Name: {Name}, Surname: {Surname}", request.Request.Name,
            request.Request.Surname);

        var email = request.Request.Email.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber.Trim().ToLower();

        var exists =
            await _dbContext.ShopperAssistants.AnyAsync(
                e => e.Email == email || e.PhoneNumber == phoneNumber, cancellationToken);
        
        if (exists)
        {
            _logger.LogWarning("Shopper Assistant already exists with email or phone number: {Email}, {PhoneNumber}", email, phoneNumber);
            throw new ShopperAssistantAlreadyExistsException();
        }
        
        var shopperAssistant = new ShopperAssistantEntity()
        {
            Name = request.Request.Name,
            Surname = request.Request.Surname,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
            Role = RoleStatus.ShopperAssistant,
            Position = request.Request.Position,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow.AddHours(5)
        };

        await _dbContext.ShopperAssistants.AddAsync(shopperAssistant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Shopper Assistant created successfully with ID: {Id}", shopperAssistant.Id);

        return new ShopperAssistantResponse()
        {
            Id = shopperAssistant.Id,
            Name = shopperAssistant.Name,
            Surname = shopperAssistant.Surname,
            Role = shopperAssistant.Role,
            Position = shopperAssistant.Position,
            Email = shopperAssistant.Email,
            PhoneNumber = shopperAssistant.PhoneNumber,
            CreatedAt = shopperAssistant.CreatedAt
        };
    }
}