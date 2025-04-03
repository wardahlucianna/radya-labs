using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class TrLessonPlanApproval : AuditEntity, ITeachingEntity
    {
        public string IdLessonPlan { get; set; }
        public string IdUser { get; set; }
        public int StateNumber { get; set; }
        public bool IsApproved { get; set; }
        public string Reason { get; set; }
        public DateTime LessonPlanApprovalDate { get; set; }
        public virtual TrLessonPlan LessonPlan { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrLessonPlanApprovalConfiguration : AuditEntityConfiguration<TrLessonPlanApproval>
    {
        public override void Configure(EntityTypeBuilder<TrLessonPlanApproval> builder)
        {

            builder.Property(x => x.Reason)
              .HasMaxLength(450);

            builder.HasOne(x => x.LessonPlan)
               .WithMany(x => x.LessonPlanApprovals)
               .HasForeignKey(fk => fk.IdLessonPlan)
               .HasConstraintName("FK_TrLessonPlanApproval_TrLessonPlan")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.User)
               .WithMany(x => x.LessonPlanApprovals)
               .HasForeignKey(fk => fk.IdUser)
               .HasConstraintName("FK_TrLessonPlanApproval_MsUser")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
