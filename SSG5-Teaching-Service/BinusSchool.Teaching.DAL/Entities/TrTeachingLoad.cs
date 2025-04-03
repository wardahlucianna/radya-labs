using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class TrTeachingLoad : AuditEntity, ITeachingEntity
    {
        public string IdTimetablePrefDetail { get; set; }
        public string IdUser { get; set; }
        public string IdSubjectCombination { get; set; }
        public int Load { get; set; }

        public virtual TrTimetablePrefDetail TimetablePrefDetail { get; set; }
        public virtual MsSubjectCombination SubjectCombination { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrTeachingLoadConfiguration : AuditEntityConfiguration<TrTeachingLoad>
    {
        public override void Configure(EntityTypeBuilder<TrTeachingLoad> builder)
        {
            builder.HasOne(x => x.User)
               .WithMany(x => x.TeachingLoads)
               .HasForeignKey(fk => fk.IdUser)
               .HasConstraintName("FK_TrTeachingLoad_MsUser")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.SubjectCombination)
               .WithMany(x => x.TeachingLoads)
               .HasForeignKey(fk => fk.IdSubjectCombination)
               .HasConstraintName("FK_TrTeachingLoad_MsSubjectCombination")
               .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.IdSubjectCombination)
                .HasMaxLength(36);
                
            builder.Property(x => x.Load)
                .IsRequired();

            builder.HasOne(x => x.TimetablePrefDetail)
                .WithMany(x => x.TeachingLoads)
                .HasForeignKey(fk => fk.IdTimetablePrefDetail)
                .HasConstraintName("FK_TrTeachingLoad_TrTimetablePrefDetail")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
