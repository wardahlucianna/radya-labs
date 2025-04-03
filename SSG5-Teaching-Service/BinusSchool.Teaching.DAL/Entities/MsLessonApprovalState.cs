using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsLessonApprovalState : AuditEntity, ITeachingEntity
    {
        public string IdLessonApproval { get; set; }
        public string IdUser { get; set; }
        public int StateNumber { get; set; }
        public virtual MsLessonApproval LessonApproval {get;set;}
        public virtual MsUser User { get; set; }
    }

    internal class MsLessonApprovalStateConfiguration : AuditEntityConfiguration<MsLessonApprovalState>
    {
        public override void Configure(EntityTypeBuilder<MsLessonApprovalState> builder)
        {
            builder.HasOne(x => x.LessonApproval)
               .WithMany(x => x.LessonApprovalStates)
               .HasForeignKey(fk => fk.IdLessonApproval)
               .HasConstraintName("FK_MsLessonApprovalState_MsLessonApproval")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.User)
              .WithMany(x => x.LessonApprovalStates)
              .HasForeignKey(fk => fk.IdUser)
              .HasConstraintName("FK_MsLessonApprovalState_MsUser")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
