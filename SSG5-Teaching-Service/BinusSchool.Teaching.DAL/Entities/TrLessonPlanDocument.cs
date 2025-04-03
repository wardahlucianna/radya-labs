using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class TrLessonPlanDocument : AuditEntity, ITeachingEntity
    {
        public string PathFile { get; set; }
        public string Filename { get; set; }
        public string Status { get; set; }
        public string IdLessonPlan { get; set; }
        public DateTime LessonPlanDocumentDate { get; set; }
        public bool? IsLate { get; set; }
        public virtual TrLessonPlan LessonPlan { get; set; }

    }

    internal class TrLessonPlanDocumentConfiguration : AuditEntityConfiguration<TrLessonPlanDocument>
    {
        public override void Configure(EntityTypeBuilder<TrLessonPlanDocument> builder)
        {
            builder.HasOne(x => x.LessonPlan)
               .WithMany(x => x.LessonPlanDocuments)
               .HasForeignKey(fk => fk.IdLessonPlan)
               .HasConstraintName("FK_TrLessonPlanDocument_TrLessonPlan")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();
            builder.Property(p => p.Status).HasMaxLength(36).IsRequired();
            builder.Property(p => p.Filename).HasMaxLength(100).IsRequired();
            builder.Property(p => p.IsLate).HasDefaultValue(false);
            base.Configure(builder);
        }
    }
}
