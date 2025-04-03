using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.User.DAL;

public interface IAppDbContext
{
    DatabaseFacade Database { get; }

    ChangeTracker ChangeTracker { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    DbSet<TEntity> Entity<TEntity>() where TEntity : class;
}