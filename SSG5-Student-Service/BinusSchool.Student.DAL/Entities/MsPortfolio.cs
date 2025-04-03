using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsPortfolio : AuditEntity, IStudentEntity
    {
        public string Name { get; set; }
        public int Type { get; set; }
    }

    internal class MsPortfolioConfiguration : AuditEntityConfiguration<MsPortfolio>
    {
        public override void Configure(EntityTypeBuilder<MsPortfolio> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100);

            base.Configure(builder);

        }
    }
}
