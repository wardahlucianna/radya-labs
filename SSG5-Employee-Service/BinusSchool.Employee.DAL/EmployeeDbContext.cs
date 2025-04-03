using System;
using System.Diagnostics;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Constants;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.EmployeeDb
{
    public class EmployeeDbContext : AppDbContext<EmployeeDbContext>, IEmployeeDbContext
    {
        private readonly ILogger<EmployeeDbContext> _logger;

        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<EmployeeDbContext>>();
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
            
            modelBuilder.ApplyEntityRegistration3<IEmployeeEntity>();
            
            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

        DbSet<UEntity> IAppDbContext<IEmployeeEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}
