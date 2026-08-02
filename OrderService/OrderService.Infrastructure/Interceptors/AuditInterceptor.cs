using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrderService.Domain.Interfaces;

namespace OrderService.Infrastructure.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var context = eventData.Context;
        if (context == null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IHasCreated created && entry.State == EntityState.Added)
            {
                created.CreatedAt = DateTime.UtcNow.AddHours(5);
            }

            if (entry.Entity is IHasUpdated updated && entry.State == EntityState.Modified)
            {
                updated.UpdatedAt = DateTime.UtcNow.AddHours(5);
            }

            if (entry.Entity is IHasDeleted deleted && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                deleted.DeletedAt = DateTime.UtcNow.AddHours(5);
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}