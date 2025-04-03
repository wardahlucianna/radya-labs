using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentExitSetting : AuditEntity, IStudentEntity
    {
        public string IdHomeroomStudent { get; set; }
        public bool IsExit { get; set; }
        public string IdAcademicYear { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class MsStudentExitSettingConfiguration : AuditEntityConfiguration<MsStudentExitSetting>
    {
        public override void Configure(EntityTypeBuilder<MsStudentExitSetting> builder)
        {

            builder.Property(x => x.IsExit)
               .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
             .WithOne(y => y.StudentExitSetting)
             .HasForeignKey<MsStudentExitSetting>(fk => fk.IdHomeroomStudent)
             .HasConstraintName("FK_MsStudentExitSetting_MsHomeroomStudent")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.AcademicYear)
             .WithMany(y => y.StudentExitSettings)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_MsStudentExitSetting_MsAcademicYear")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            base.Configure(builder);
        }

    }
}
