using System.Data;
using BinusSchool.Document.Kernel.Abstractions;
using BinusSchool.Document.Kernel.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.DAL;

public class ApplicationDbContext : DbContext, IAppDbContext
{
    private readonly ICurrentUser _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        _currentUser = this.GetService<ICurrentUser>();
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot)
    {
        throw new NotImplementedException();
    }

    public DbSet<TEntity> Entity<TEntity>() where TEntity : class
        => Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeUtil.ServerTime;
        var executor = Guid.Empty.ToString(); // use empty Guid to indicate insert/update by System
        if (_currentUser != null && _currentUser.TryGetUser(out var user))
            executor = user.Id;

        var modifiedEntities = ChangeTracker
            .Entries()
            .Where(x
                => x.Entity is AuditEntity
                   && (x.State == EntityState.Added || x.State == EntityState.Modified ||
                       x.State == EntityState.Deleted));

        foreach (var entry in modifiedEntities)
        {
            if (entry.Entity is AuditEntity auditable2)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable2.UserIn ??= executor;
                        auditable2.DateIn ??= now;
                        auditable2.IsActive = true;
                        executor = auditable2.UserIn; // update executor based on inserted UserIn
                        break;

                    case EntityState.Modified:
                        if (auditable2.UserUp is null || auditable2.UserUp != executor)
                            auditable2.UserUp = executor;
                        auditable2.DateUp = now;
                        executor = auditable2.UserUp; // update executor based on inserted UserUp
                        break;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}