using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsSettingEmailScheduleRealization : AuditEntity, ISchedulingEntity
    {
        public bool IsSetSpecificUser { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdBinusian { get; set; }
        public string IdSchool { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsSettingEmailScheduleRealizationConfiguration : AuditEntityConfiguration<MsSettingEmailScheduleRealization>
    {
        public override void Configure(EntityTypeBuilder<MsSettingEmailScheduleRealization> builder)
        {
            builder.HasOne(x => x.Role)
             .WithMany(x => x.SettingEmailScheduleRealizations)
             .HasForeignKey(fk => fk.IdRole)
             .HasConstraintName("FK_MsSettingEmailScheduleRealization_MsRole")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
             .WithMany(x => x.SettingEmailScheduleRealizations)
             .HasForeignKey(fk => fk.IdTeacherPosition)
             .HasConstraintName("FK_MsSettingEmailScheduleRealization_MsTeacherPosition")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Staff)
             .WithMany(x => x.SettingEmailScheduleRealizations)
             .HasForeignKey(fk => fk.IdBinusian)
             .HasConstraintName("FK_MsSettingEmailScheduleRealization_MsUser")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.School)
              .WithMany(x => x.SettingEmailScheduleRealizations)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsSettingEmailScheduleRealization_MsSchool")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
