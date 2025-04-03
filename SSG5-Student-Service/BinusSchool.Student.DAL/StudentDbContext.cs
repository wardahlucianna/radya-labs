using System;
using System.Diagnostics;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Constants;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.StudentDb
{
    public class StudentDbContext : AppDbContext<StudentDbContext>, IStudentDbContext
    {
        private readonly ILogger<StudentDbContext> _logger;

        public StudentDbContext(DbContextOptions<StudentDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<StudentDbContext>>();
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
            
            modelBuilder.ApplyEntityRegistration3<IStudentEntity>();
            
            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

        DbSet<UEntity> IAppDbContext<IStudentEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}
