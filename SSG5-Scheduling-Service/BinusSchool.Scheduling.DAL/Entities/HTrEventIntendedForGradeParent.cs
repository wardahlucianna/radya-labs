using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForGradeParent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLevel { get; set; }
        public string IdHTrEventIntendedFor { get; set; }

        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsLevel Level { get; set; }
    }

    internal class HTrEventIntendedForGradeParentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForGradeParent>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForGradeParent> builder)
        {
            builder.ToTable(nameof(HTrEventIntendedForGradeParent));

            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForGradeParent).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventIntendedFor)
              .WithMany(x => x.EventIntendedForGradeParents)
              .HasForeignKey(fk => fk.IdHTrEventIntendedFor)
              .HasConstraintName("FK_HTrEventIntendedForGradeParent_HTrEventIntendedFor")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Homeroom)
             .WithMany(x => x.HistoryEventIntendedForGradeParents)
             .HasForeignKey(fk => fk.IdHomeroom)
             .HasConstraintName("FK_HTrEventIntendedForGradeParent_MsHomeroom")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);

            builder.HasOne(x => x.Level)
             .WithMany(x => x.HistoryEventIntendedForGradeParents)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_HTrEventIntendedForGradeParent_MsLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
