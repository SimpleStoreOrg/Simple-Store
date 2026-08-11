namespace UserService.Domain.Interfaces;

public interface IHasDeleted
{
    DateTime? DeletedAt { get; set; }
}