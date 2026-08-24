using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Admins.Commands;
public record DeleteAdminCommand(long AdminId) : IRequest<bool>;

public class DeleteAdminCommandHandler : IRequestHandler<DeleteAdminCommand, bool>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<DeleteAdminCommandHandler> _logger;

    public DeleteAdminCommandHandler(IUserServiceDbContext dbContext, ILogger<DeleteAdminCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<bool> Handle(DeleteAdminCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Admin with ID: {Id}", request.AdminId);

        var admin = await _dbContext.Admins.FirstOrDefaultAsync(e => e.Id == request.AdminId,
                cancellationToken);
        
        if (admin == null)
        {
            _logger.LogWarning("Admin with ID {Id} not found", request.AdminId);
            throw new AdminNotFoundException(request.AdminId);
        }

        _dbContext.Admins.Remove(admin);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Admin deleted successfully with ID: {AdminId}", request.AdminId);

        return true;
    }
}