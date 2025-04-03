using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsLessonApproval : CodeEntity, ITeachingEntity
    {
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsLevelApproval> LevelApprovals { get; set; }
        public virtual ICollection<MsLessonApprovalState> LessonApprovalStates { get; set; }
    }

    internal class MsLessonApprovalConfiguration : CodeEntityConfiguration<MsLessonApproval>
    {
        public override void Configure(EntityTypeBuilder<MsLessonApproval> builder)
        {
            builder.HasOne(x => x.School)
               .WithMany(x => x.LessonApprovals)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsLessonApproval_MsSchool")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
