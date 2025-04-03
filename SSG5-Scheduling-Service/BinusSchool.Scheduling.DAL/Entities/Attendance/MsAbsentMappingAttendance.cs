using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsAbsentMappingAttendance : AuditEntity, ISchedulingEntity
    {
        public string IdMappingAttendance {get;set;}
        public string IdTeacherPosition {get;set;}

        public virtual MsMappingAttendance MappingAttendance {get;set;}
        public virtual MsTeacherPosition TeacherPosition {get;set;}
        public virtual ICollection<MsListMappingAttendanceAbsent> ListMappingAttendanceAbsents {get;set;}
    }
    
    internal class MsAbsentMappingAttendanceConfiguration : AuditEntityConfiguration<MsAbsentMappingAttendance>
    {
        public override void Configure(EntityTypeBuilder<MsAbsentMappingAttendance> builder)
        {
            builder.HasOne(x => x.MappingAttendance)
                .WithMany(x => x.AbsentMappingAttendances)
                .HasForeignKey(fk => fk.IdMappingAttendance)
                .HasConstraintName("FK_MsAbsentMappingAttendance_MsMappingAttendance")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
                .WithMany(x => x.AbsentMappingAttendances)
                .HasForeignKey(fk => fk.IdTeacherPosition)
                .HasConstraintName("FK_MsAbsentMappingAttendance_MsTeacherPosition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
