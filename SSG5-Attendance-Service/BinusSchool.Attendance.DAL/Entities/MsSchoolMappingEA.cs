using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsSchoolMappingEA : AuditEntity, IAttendanceEntity
    {
        public string IdSchool { get; set; }
        public bool IsGrouped { get; set; }

        public virtual MsSchool School { get; set; }
    }

    internal class MsSchoolMappingEAConfiguration : AuditEntityConfiguration<MsSchoolMappingEA>
    {
        public override void Configure(EntityTypeBuilder<MsSchoolMappingEA> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SchoolMappingEAs)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchoolMappingEA_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.IsGrouped)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
