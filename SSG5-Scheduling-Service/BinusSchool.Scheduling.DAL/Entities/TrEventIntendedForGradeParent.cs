using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventIntendedForGradeParent : AuditEntity, ISchedulingEntity
    {
        public string IdEventIntendedFor { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLevel { get; set; }

        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual TrEventIntendedFor EventIntendedFor { get; set; }
    }

    internal class TrEventIntendedForGradeParentConfiguration : AuditEntityConfiguration<TrEventIntendedForGradeParent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForGradeParent> builder)
        {
            builder.HasOne(x => x.EventIntendedFor)
              .WithMany(x => x.EventIntendedForGradeParents)
              .HasForeignKey(fk => fk.IdEventIntendedFor)
              .HasConstraintName("FK_TrEventIntendedForGradeParent_TrEventIntendedFor")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Homeroom)
             .WithMany(x => x.TrEventIntendedForGradeParents)
             .HasForeignKey(fk => fk.IdHomeroom)
             .HasConstraintName("FK_TrEventIntendedForGradeParent_MsHomeroom")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);

            builder.HasOne(x => x.Level)
             .WithMany(x => x.TrEventIntendedForGradeParents)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_TrEventIntendedForGradeParent_MsLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
