using System;
using System.Diagnostics;
using System.Linq;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.AttendanceDb
{
    public class AttendanceDbContext : AppDbContext<AttendanceDbContext>, IAttendanceDbContext
    {
        private readonly ILogger<AttendanceDbContext> _logger;

        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<AttendanceDbContext>>();
            }
            catch
            {
                // ignored
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var sw = new Stopwatch();
            sw.Start();

            modelBuilder.ApplyEntityRegistration3<IAttendanceEntity>();

            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

        DbSet<UEntity> IAppDbContext<IAttendanceEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }

        public void AttachEntity(object entity)
        {
            Attach(entity);
        }

        public void DetachChanges()
        {
            var changedEntriesCopy = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
    }
}
