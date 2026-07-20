using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Entities;

namespace UserService.Application.Features.ShopperAssistant.Commands.CreateShopperAssistant;

public class CreateShopperAssistantCommandHandler : IRequestHandler<CreateShopperAssistantCommand, UserResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<CreateShopperAssistantCommandHandler> _logger;

    public CreateShopperAssistantCommandHandler(IUserServiceDbContext dbContext, ILogger<CreateShopperAssistantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<UserResponse> Handle(CreateShopperAssistantCommand request, CancellationToken cancellationToken)
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
            Role = request.Request.Role,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.ShopperAssistants.AddAsync(shopperAssistant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Shopper Assistant created successfully with ID: {Id}", shopperAssistant.Id);

        return new UserResponse()
        {
            Id = shopperAssistant.Id,
            Name = shopperAssistant.Name,
            Surname = shopperAssistant.Surname,
            Role = shopperAssistant.Role,
            Email = shopperAssistant.Email,
            PhoneNumber = shopperAssistant.PhoneNumber,
            CreatedAt = shopperAssistant.CreatedAt
        };
    }
}