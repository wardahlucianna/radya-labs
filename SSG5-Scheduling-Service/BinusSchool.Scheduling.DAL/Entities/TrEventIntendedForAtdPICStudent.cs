using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventIntendedForAtdPICStudent : AuditEntity, ISchedulingEntity
    {
        public string IdEventIntendedForAttendanceStudent { get; set; }
        public EventIntendedForAttendancePICStudent Type { get; set; }
        public string IdUser { get; set; }
        public MsUser User { get; set; }

        public virtual TrEventIntendedForAttendanceStudent EventIntendedForAttendanceStudent { get; set; }
    }

    internal class TrEventIntendedForAttendancePICStudentConfiguration : AuditEntityConfiguration<TrEventIntendedForAtdPICStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEventIntendedForAtdPICStudent> builder)
        {
            builder.Property(x => x.Type)
               .HasConversion<string>()
               .HasMaxLength(16)
               .IsRequired();

            builder.HasOne(x => x.EventIntendedForAttendanceStudent)
                .WithMany(x => x.EventIntendedForAtdPICStudents)
                .HasForeignKey(fk => fk.IdEventIntendedForAttendanceStudent)
                .HasConstraintName("FK_TrEventIntendedForAtdPICStudent_TrEventIntendedForAttendanceStudent")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.TrEventIntendedForAtdPICStudents)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_TrEventIntendedForAtdPICStudent_MsUser")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
