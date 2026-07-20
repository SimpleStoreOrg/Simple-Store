using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(IUserServiceDbContext dbContext, ILogger<UpdateUserCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating User with ID: {UserId}", request.UserId);

        var name = request.Request.Name.Trim().ToLower();
        var surname = request.Request.Surname.Trim().ToLower();
        
        var exists =
            await _dbContext.Users.AnyAsync(u => u.Name.Trim().ToLower() == name || u.Surname.Trim().ToLower() == surname,
                cancellationToken);

        if (exists)
        {
            _logger.LogWarning("User already exists with name: {Name}", name);
            throw new UserAlreadyExistsException();
        }
        
        var user = await _dbContext.Users.FirstAsync(u => u.Id == request.UserId, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", request.UserId);
            throw new UserNotFoundException(user.Id);
        }

        user.Name = request.Request.Name;
        user.Surname = request.Request.Surname;
        user.Email = request.Request.Email;
        user.Role = request.Request.Role;
        user.PhoneNumber = request.Request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} updated successfully. Name: {UserName} {UserSurname}", request.UserId,
            user.Name, user.Surname);
        
        return new UserResponse()
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Role = user.Role,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}