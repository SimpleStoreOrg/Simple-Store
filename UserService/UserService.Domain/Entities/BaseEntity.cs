using UserService.Domain.Interfaces;

namespace UserService.Domain.Entities;

public class BaseEntity<T> : IHasCreated, IHasUpdated, IHasDeleted
{
    public T Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}