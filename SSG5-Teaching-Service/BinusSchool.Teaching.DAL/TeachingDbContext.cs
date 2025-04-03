using System;
using System.Diagnostics;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Constants;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.TeachingDb
{
    public class TeachingDbContext : AppDbContext<TeachingDbContext>, ITeachingDbContext
    {
        private readonly ILogger<TeachingDbContext> _logger;

        public TeachingDbContext(DbContextOptions<TeachingDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<TeachingDbContext>>();
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
            
            modelBuilder.ApplyEntityRegistration3<ITeachingEntity>();
            
            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

        DbSet<UEntity> IAppDbContext<ITeachingEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}
