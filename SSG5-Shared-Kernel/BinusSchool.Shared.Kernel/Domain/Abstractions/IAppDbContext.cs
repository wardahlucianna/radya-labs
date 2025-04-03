using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Domain.Abstractions
{
    public interface IAppDbContext
    {
        /// <summary>
        /// EF Core DatabaseFacade.
        /// </summary>
        DatabaseFacade DbFacade { get; }

        /// <summary>
        /// EF Core ChangeTracker.
        /// </summary>
        ChangeTracker ChgTracker { get; }

        /// <summary>
        /// EF Core BeginTransactionAsync.
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default, IsolationLevel isolationLevel = IsolationLevel.Snapshot);

        /// <summary>
        /// Custom EF Core SaveChangesAsync with additional auditable, auditrail and sync ref table feature.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Custom EF Core SaveChangesAsync with additional auditable, auditrail and sync ref table feature.
        /// With ignoreable tables to sync.
        /// </summary>
        Task<int> SaveChangesAsync(IEnumerable<string> ignoreTablesToSync, CancellationToken cancellationToken = default);

        /// <summary>
        /// EF Core SaveChangesAsync. Used by sync ref table handler without additional feature.
        /// </summary>
        Task<int> SaveRefChangesAsync(CancellationToken cancellationToken = default);
    }
    
    public interface IAppDbContext<TEntity> : IAppDbContext 
        where TEntity : class, IEntity
    {
        DbSet<UEntity> Entity<UEntity>() where UEntity : class, TEntity;
    }
}
