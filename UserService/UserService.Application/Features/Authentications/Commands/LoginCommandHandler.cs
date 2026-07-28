using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Application.Services;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Authentications.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<TokenResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponse>
{
    private readonly IUserServiceDbContext _context;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly JwtService _jwtService;

    public LoginCommandHandler(IUserServiceDbContext context, ILogger<LoginCommandHandler> logger, JwtService jwtService)
    {
        _context = context;
        _logger = logger;
        _jwtService = jwtService;
    }
    
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logging to the system");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Request.Email,
            cancellationToken: cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("Email not found");
            throw new EmailNotFoundException();
        }

        var isValid = BCrypt.Net.BCrypt.Verify(request.Request.Password, user.PasswordHash);
        if (!isValid)
        {
            _logger.LogInformation("Invalid Password");
            throw new InvalidPasswordException();
        }

        var accessToken = _jwtService.GenerateToken(user);

        var refreshToken = new RefreshToken()
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        return new TokenResponse()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }
}