using MediatR;

namespace UserService.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(long UserId) : IRequest, IRequest<bool>;