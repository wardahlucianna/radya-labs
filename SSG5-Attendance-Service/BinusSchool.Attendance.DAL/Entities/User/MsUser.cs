using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class MsUser : AuditEntity, IAttendanceEntity
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActiveDirectory { get; set; }
        public bool Status { get; set; }
        //public virtual ICollection<MsUserEvent> UserEvents { get; set; }
        //public virtual ICollection<MsEventIntendedForAtdPICStudent> EventIntendedForAtdPICStudents { get; set; }
        public virtual ICollection<TrNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsUserPlatform> UserPlatforms { get; set; }
        public virtual ICollection<TrUserEvent> TrUserEvents { get; set; }
        public virtual ICollection<TrEventIntendedForAtdPICStudent> TrEventIntendedForAtdPICStudents { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<TrEventCoordinator> EventCoordinators { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }

        internal class MsUserConfiguration : AuditEntityConfiguration<MsUser>
        {
            public override void Configure(EntityTypeBuilder<MsUser> builder)
            {
                builder.Property(x => x.DisplayName)
                  .HasMaxLength(100)
                  .IsRequired();

                builder.Property(x => x.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                // builder.HasIndex(x => x.Username)
                //     .IsUnique();

                builder.Property(x => x.Email)
                    .HasMaxLength(128);
                base.Configure(builder);
            }
        }
    }
}
