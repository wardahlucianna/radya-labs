using System;
using System.Diagnostics;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Constants;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.SchedulingDb
{
    public class SchedulingDbContext : AppDbContext<SchedulingDbContext>, ISchedulingDbContext
    {
        private readonly ILogger<SchedulingDbContext> _logger;

        public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<SchedulingDbContext>>();
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
            
            modelBuilder.ApplyEntityRegistration3<ISchedulingEntity>();
            
            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

        DbSet<UEntity> IAppDbContext<ISchedulingEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}
