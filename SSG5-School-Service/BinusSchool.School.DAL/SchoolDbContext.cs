using System;
using System.Diagnostics;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Constants;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.SchoolDb
{
    public class SchoolDbContext : AppDbContext<SchoolDbContext>, ISchoolDbContext
    {
        private readonly ILogger<SchoolDbContext> _logger;

        public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<SchoolDbContext>>();
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
            
            modelBuilder.ApplyEntityRegistration3<ISchoolEntity>();
            
            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

       
        DbSet<UEntity> IAppDbContext<ISchoolEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}
