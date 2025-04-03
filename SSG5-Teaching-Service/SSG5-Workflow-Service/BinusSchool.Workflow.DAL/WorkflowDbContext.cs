using System;
using System.Diagnostics;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Constants;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.WorkflowDb
{
    public class WorkflowDbContext : AppDbContext<WorkflowDbContext>, IWorkflowDbContext
    {
        private readonly ILogger<WorkflowDbContext> _logger;

        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<WorkflowDbContext>>();
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
            
            modelBuilder.ApplyEntityRegistration3<IWorkflowEntity>();
            
            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

        DbSet<UEntity> IAppDbContext<IWorkflowEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}
