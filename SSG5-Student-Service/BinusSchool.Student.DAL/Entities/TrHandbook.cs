using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrHandbook : AuditEntity, IStudentEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string IdAcademicYear { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrHandbookViewFor> HandbookViewFors { get; set; }
        public virtual ICollection<TrHandbookLevel> HandbookViewLevel { get; set; }
        public virtual ICollection<TrHandbookAttachment> HandbookAttachment { get; set; }
    }

    internal class TrHandbookConfiguration : AuditEntityConfiguration<TrHandbook>
    {
        public override void Configure(EntityTypeBuilder<TrHandbook> builder)
        {
            builder.Property(x => x.Title).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(450);
            builder.Property(x => x.Url).HasMaxLength(450);

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.Handbook)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_TrHandbook_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
