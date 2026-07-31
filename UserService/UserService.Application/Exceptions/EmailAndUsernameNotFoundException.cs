using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class EmailAndUsernameNotFoundException : BaseException
{
    public EmailAndUsernameNotFoundException() : base($"Username and Email not found", StatusCodes.Status404NotFound)
    {
    }
}