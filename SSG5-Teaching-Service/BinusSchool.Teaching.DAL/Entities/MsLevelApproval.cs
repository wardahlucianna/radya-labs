using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsLevelApproval : AuditEntity, ITeachingEntity
    {
        public string IdLevel { get; set; }
        public string IdLessonApproval { get; set; }
        public bool IsApproval { get; set; }
        public virtual MsLessonApproval LessonApproval { get; set; }
        public virtual MsLevel Level { get; set; }
    }

    internal class MsLevelApprovalConfiguration : AuditEntityConfiguration<MsLevelApproval>
    {
        public override void Configure(EntityTypeBuilder<MsLevelApproval> builder)
        {
            builder.HasOne(x => x.Level)
               .WithMany(x => x.LevelApprovals)
               .HasForeignKey(fk => fk.IdLevel)
               .HasConstraintName("FK_MsLevelApproval_MsLevel")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.LessonApproval)
             .WithMany(x => x.LevelApprovals)
             .HasForeignKey(fk => fk.IdLessonApproval)
             .HasConstraintName("FK_MsLevelApproval_MsLessonApproval")
             .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
