using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Auth.Abstractions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Persistence
{
    public abstract class AppDbContext<T> : DbContext, IAppDbContext
        where T : DbContext
    {
        private readonly ICurrentUser _currentUser;
        private readonly IAuditTrail _auditTrail;
        private readonly ISyncReferenceTable _syncReferenceTable;

        public AppDbContext(DbContextOptions<T> options) 
            : base(options)
        {
            try
            {
                _currentUser = this.GetService<ICurrentUser>();
                _auditTrail = this.GetService<IAuditTrail>();
                _syncReferenceTable = this.GetService<ISyncReferenceTable>();
            }
            catch
            {
                // ignored
            }
        }

        public DatabaseFacade DbFacade => Database;
        public ChangeTracker ChgTracker => ChangeTracker;

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default, IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(null, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(IEnumerable<string> ignoreTablesToSync, CancellationToken cancellationToken = default)
        {
            var now = DateTimeUtil.ServerTime;
            var executor = Guid.Empty.ToString(); // use empty Guid to indicate insert/update by System
            if (_currentUser != null && _currentUser.TryGetUser(out var user))
                executor = user.Id;

            var modifiedEntities = ChangeTracker
                .Entries()
                .Where(x 
                    => x.Entity is IEntity 
                    && (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted));
            var dbName = default(string);
            var changeLogs = new List<AuditChangeLog>();

            foreach (var entry in modifiedEntities)
            {
                if (entry.Entity is IAuditable auditable2)
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

                var entityFullName = entry.Entity.GetType().FullName!.Split('.');
                dbName ??= entityFullName[^3].Remove(entityFullName[^3].Length - 2, 2);
                var entityName = entityFullName[^1];
                var primaryKey = GetPrimaryKey(entry);
                var action = entry.State switch
                {
                    EntityState.Added => AuditAction.Insert,
                    EntityState.Modified => AuditAction.Update,
                    EntityState.Deleted => AuditAction.Remove,
                    _ => AuditAction.None
                };
                var value = entry.OriginalValues.Properties
                    .Where(x => !primaryKey.Select(y => y.Key).Contains(x.Name))
                    .ToDictionary(x => x.Name, prop =>
                    {
                        var strOgEntryValue = entry.OriginalValues[prop] is DateTime ogDt
                            ? ogDt.ToString("s", DateTimeFormatInfo.InvariantInfo) 
                            : (entry.OriginalValues[prop]?.ToString());

                        var strCurrEntryValue = entry.CurrentValues[prop] is DateTime currDt
                            ? currDt.ToString("s", DateTimeFormatInfo.InvariantInfo) 
                            : (entry.CurrentValues[prop]?.ToString());
                        
                        var entryValue = entry.State switch
                        {
                            EntityState.Added    => (null, strCurrEntryValue),
                            EntityState.Modified => (strOgEntryValue, strCurrEntryValue),
                            EntityState.Deleted  => (strOgEntryValue, null),
                            _                    => (null, null)
                        };

                        return entryValue;
                    });

                changeLogs.Add(new AuditChangeLog(entityName, primaryKey, action, value));
            }
            
            var result = await base.SaveChangesAsync(cancellationToken);
            if (result >= 1 && changeLogs.Count != 0)
            {
                //remove audit trail first
                // _ = Task.WhenAll(
                //     _auditTrail.SaveChangeLog(dbName, executor, now, changeLogs), 
                //     _syncReferenceTable.SendChanges(dbName, changeLogs, ignoreTablesToSync));

                _ = _syncReferenceTable.SendChanges(dbName, changeLogs, ignoreTablesToSync);
            }

            return result;
        }

        public Task<int> SaveRefChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        protected abstract override void OnModelCreating(ModelBuilder modelBuilder);

        private IDictionary<string, string> GetPrimaryKey(EntityEntry entityEntry)
        {
            var keyNames = entityEntry.OriginalValues.EntityType
                .FindPrimaryKey().Properties
                .Select(x => x.Name);
                
            var keyValues = entityEntry.Entity
                .GetType()
                .GetProperties()
                .Where(x => keyNames.Contains(x.Name))
                .Select(x => KeyValuePair.Create(x.Name, x.GetValue(entityEntry.Entity, null)?.ToString()));
            
            var compositePrimaryKeys = new Dictionary<string, string>(keyValues);
            
            return compositePrimaryKeys;
        }
    }
}
