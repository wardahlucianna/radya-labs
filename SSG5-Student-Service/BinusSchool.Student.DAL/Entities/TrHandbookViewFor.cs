using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrHandbookViewFor : AuditEntity, IStudentEntity
    {
        public string IdTrHandbook { get; set; }
        public virtual TrHandbook Handbook { get; set; }
        public HandbookFor For { get; set; }
    }

    internal class TrHandbookViewForConfiguration : AuditEntityConfiguration<TrHandbookViewFor>
    {
        public override void Configure(EntityTypeBuilder<TrHandbookViewFor> builder)
        {
            builder.HasOne(x => x.Handbook)
             .WithMany(x => x.HandbookViewFors)
             .HasForeignKey(fk => fk.IdTrHandbook)
             .HasConstraintName("FK_TrHandbookViewFor_TrHandbook")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
