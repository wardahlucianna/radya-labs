using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.User
{
    public class MsUser : AuditEntity, ITeachingEntity
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActiveDirectory { get; set; }
        public bool Status { get; set; }
        public virtual ICollection<TrNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<TrTeachingLoad> TeachingLoads { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        public virtual ICollection<TrLessonPlanApproval> LessonPlanApprovals { get; set; }
        public virtual ICollection<MsLessonApprovalState> LessonApprovalStates { get; set; }
        public virtual ICollection<MsUserPlatform> UserPlatforms { get; set; }

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
