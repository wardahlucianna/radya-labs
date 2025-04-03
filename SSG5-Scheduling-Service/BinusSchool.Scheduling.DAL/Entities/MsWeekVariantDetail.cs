using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsWeekVariantDetail : AuditEntity, ISchedulingEntity
    {
        public string IdWeekVariant { get; set; }
        public string IdWeek { get; set; }

        public virtual MsWeek Week { get; set; }
        public virtual MsWeekVariant WeekVariant { get; set; }
        public virtual ICollection<MsSchedule> Schedules { get; set; }
    }

    internal class MsWeekVariantDetailConfiguration : AuditEntityConfiguration<MsWeekVariantDetail>
    {
        public override void Configure(EntityTypeBuilder<MsWeekVariantDetail> builder)
        {
            builder.HasOne(x => x.Week)
                .WithMany(x => x.WeekVarianDetails)
                .HasForeignKey(fk => fk.IdWeek)
                .HasConstraintName("FK_MsWeekVariantDetail_MsWeek")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            builder.HasOne(x => x.WeekVariant)
                .WithMany(x => x.WeekVarianDetails)
                .HasForeignKey(fk => fk.IdWeekVariant)
                .HasConstraintName("FK_MsWeekVariantDetail_MsWeekVariant")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
