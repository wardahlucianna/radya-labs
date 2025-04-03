using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsHomeroomStudent : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
        public int Semester { get; set; }
        public Gender Gender { get; set; }
        public string Religion { get; set; }

        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual ICollection<TrEventActivityAward> EventActivityAwards { get; set; }
        public virtual ICollection<HTrEventActivityAward> HistoryEventActivityAwards { get; set; }
        public virtual ICollection<TrInvitationBookingDetail> InvitationBookingDetails { get; set; }
        public virtual ICollection<TrInvitationBookingSettingUser> InvitationBookingSettingUsers { get; set; }
        public virtual ICollection<TrInvitationEmail> InvitationEmails { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollments { get; set; }
        //public virtual ICollection<HMsHomeroomStudentEnrollment> HMsHomeroomStudentEnrollments { get; set; }
        public virtual ICollection<HTrMoveStudentHomeroom> HTrMoveStudentHomerooms { get; set; }
    }

    internal class MsHomeroomStudentConfiguration : AuditEntityConfiguration<MsHomeroomStudent>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomStudent> builder)
        {
            builder.HasOne(x => x.Student)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsHomeroomStudent_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.Semester)
                .IsRequired();

            builder.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(6)
                .IsRequired();

            builder.Property(x => x.Religion)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomStudent_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
