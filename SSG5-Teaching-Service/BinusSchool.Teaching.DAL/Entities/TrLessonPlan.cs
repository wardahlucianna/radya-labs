using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class TrLessonPlan : AuditEntity, ITeachingEntity
    {
        public string IdWeekSettingDetail { get; set; }
        public string IdLessonTeacher { get; set; }
        public string IdSubjectMappingSubjectLevel { get; set; }
        public string Status { get; set; }
        public virtual MsWeekSettingDetail WeekSettingDetail { get; set; }
        public virtual MsLessonTeacher LessonTeacher { get; set; }
        public virtual MsSubjectMappingSubjectLevel SubjectMappingSubjectLevel { get; set; }
        public virtual ICollection<TrLessonPlanApproval> LessonPlanApprovals { get; set; }
        public virtual ICollection<TrLessonPlanDocument> LessonPlanDocuments { get; set; }

    }

    internal class TrLessonPlanConfiguration : AuditEntityConfiguration<TrLessonPlan>
    {
        public override void Configure(EntityTypeBuilder<TrLessonPlan> builder)
        {
            builder.HasOne(x => x.WeekSettingDetail)
               .WithMany(x => x.LessonPlans)
               .HasForeignKey(fk => fk.IdWeekSettingDetail)
               .HasConstraintName("FK_TrLessonPlan_MsWeekSettingDetail")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.LessonTeacher)
               .WithMany(x => x.LessonPlans)
               .HasForeignKey(fk => fk.IdLessonTeacher)
               .HasConstraintName("FK_TrLessonPlan_MsLessonTearcher")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.SubjectMappingSubjectLevel)
               .WithMany(x => x.LessonPlans)
               .HasForeignKey(fk => fk.IdSubjectMappingSubjectLevel)
               .HasConstraintName("FK_TrLessonPlan_MsSubjectMappingSubjectLevel")
               .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
