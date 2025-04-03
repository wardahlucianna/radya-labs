using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.Teaching
{
    public class MsTeacherPosition : CodeEntity, ISchedulingEntity
    {
        public string IdPosition { get; set; }
        public string IdSchool { get; set; }
        public virtual LtPosition Position { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrEventIntendedForPosition> EventIntendedForPositions { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<HTrEventIntendedForPosition> HistoryEventIntendedForPositions { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsAbsentMappingAttendance> AbsentMappingAttendances { get; set; }
        public virtual ICollection<MsSettingEmailScheduleRealization> SettingEmailScheduleRealizations { get; set; }
        public virtual ICollection<TrInvitationBookingSettingVenueDtl> InvitationBookingSettingVenueDtl { get; set; }
        public virtual ICollection<TrInvBookingSettingRoleParticipant> InvBookingSettingRoleParticipants { get; set; }
        public virtual ICollection<MsEmailRecepient> EmailRecepients { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
    }

    internal class MsTeacherPositionConfiguration : CodeEntityConfiguration<MsTeacherPosition>
    {
        public override void Configure(EntityTypeBuilder<MsTeacherPosition> builder)
        {
            builder.HasOne(x => x.School)
                 .WithMany(x => x.TeacherPositions)
                 .HasForeignKey(fk => fk.IdSchool)
                 .HasConstraintName("FK_MsTeacherPosition_MsSchool")
                 .OnDelete(DeleteBehavior.NoAction)
                  .IsRequired();

            builder.HasOne(x => x.Position)
                .WithMany(x => x.TeacherPositions)
                .HasForeignKey(fk => fk.IdPosition)
                .HasConstraintName("FK_MsTeacherPosition_LtPosition")
                .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired();

            base.Configure(builder);
        }
    }
}
