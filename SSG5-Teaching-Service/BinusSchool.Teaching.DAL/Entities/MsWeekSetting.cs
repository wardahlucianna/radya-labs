using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsWeekSetting : AuditEntity, ITeachingEntity
    {
        public string IdPeriod { get; set; }
        public string Method { get; set; }
        public bool Status { get; set; }
        public virtual MsPeriod Period { get; set; }
        public virtual ICollection<MsWeekSettingDetail> WeekSettingDetails { get; set; }
    }

    internal class MsWeekSettingConfiguration : AuditEntityConfiguration<MsWeekSetting>
    {
        public override void Configure(EntityTypeBuilder<MsWeekSetting> builder)
        {
            builder.HasOne(x => x.Period)
               .WithMany(x => x.WeekSettings)
               .HasForeignKey(fk => fk.IdPeriod)
               .HasConstraintName("FK_MsWeekSetting_MsPeriod")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}