using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsWeekSettingDetail : AuditEntity, ITeachingEntity
    {
        public string IdWeekSetting { get; set; }
        public int WeekNumber { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public bool Status { get; set; }
        public virtual MsWeekSetting WeekSetting { get; set; }
        public virtual ICollection<TrLessonPlan> LessonPlans { get; set; }
    }

    internal class MsWeekSettingDetailConfiguration : AuditEntityConfiguration<MsWeekSettingDetail>
    {
        public override void Configure(EntityTypeBuilder<MsWeekSettingDetail> builder)
        {
            builder.HasOne(x => x.WeekSetting)
               .WithMany(x => x.WeekSettingDetails)
               .HasForeignKey(fk => fk.IdWeekSetting)
               .HasConstraintName("FK_MsWeekSettingDetail_MsWeekSetting")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}