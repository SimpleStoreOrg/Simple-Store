using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IUserServiceDbContext dbContext, ILogger<CreateUserCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("New User creation");
        var email = request.Request.Email.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber.Trim().ToLower();

        var exists =
            await _dbContext.Users.AnyAsync(u => u.Email.Trim().ToLower() == email || u.PhoneNumber.Trim() == phoneNumber, cancellationToken);
        
        if (exists)
        {
            _logger.LogWarning("User with Email {UserEmail} or Phone number {UserPhoneNumber} already exists", email,
                phoneNumber);
            throw new UserAlreadyExistsException();
        }

        var user = new UserEntity()
        {
            Name = request.Request.Name,
            Surname = request.Request.Surname,
            Role = request.Request.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User {UserId}: {UserName} created successfully", user.Id, user.Name);

        return new UserResponse()
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Role = user.Role,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt
        };
    }
}