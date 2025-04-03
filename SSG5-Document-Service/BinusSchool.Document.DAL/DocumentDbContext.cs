using System;
using System.Diagnostics;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Constants;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Persistence.DocumentDb
{
    public class DocumentDbContext : AppDbContext<DocumentDbContext>, IDocumentDbContext
    {
        private readonly ILogger<DocumentDbContext> _logger;

        public DocumentDbContext(DbContextOptions<DocumentDbContext> options)
            : base(options)
        {
            try
            {
                _logger = this.GetService<ILogger<DocumentDbContext>>();
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
            
            modelBuilder.ApplyEntityRegistration3<IDocumentEntity>();
            
            sw.Stop();
            if (_logger is null)
                Console.WriteLine(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(MessageTemplate.CreatingEntityModel, sw.ElapsedMilliseconds);
        }

        DbSet<UEntity> IAppDbContext<IDocumentEntity>.Entity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}
