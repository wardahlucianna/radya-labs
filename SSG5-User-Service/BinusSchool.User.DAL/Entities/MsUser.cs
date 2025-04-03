using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsUser : AuditEntity, IUserEntity
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActiveDirectory { get; set; }
        public bool Status { get; set; }
        public string RequestChangePasswordCode { get; set; }
        public DateTime? UsedDate { get; set; }
        public int CountResetRequest { get; set; }


        public virtual MsUserPassword UserPassword { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        public virtual ICollection<TrMessage> Messages { get; set; }
        public virtual ICollection<MsFeedbackType> FeedbackTypeTo { get; set; }
        public virtual ICollection<MsFeedbackType> FeedbackTypeCc { get; set; }
        public virtual ICollection<TrMessageRecepient> MessageRecepients { get; set; }
        public virtual ICollection<TrMessageApproval> MessageApprovals { get; set; }
        public virtual ICollection<TrNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsUserPlatform> UserPlatforms { get; set; }
        public virtual ICollection<TrNotificationUser> NotificationUsers { get; set; }
        public virtual ICollection<MsMessageApprovalState> MessageApprovalStates { get; set; }
        public virtual ICollection<MsAccessImpersonateUser> AccessImpersonateUsers { get; set; }
        public virtual ICollection<TrAccessImpersonateUserLog> AccessImpersonateUserLogs { get; set; }
        public virtual ICollection<MsUserBlocking> UserBlockings { get; set; }
        public virtual ICollection<TrLoginTransactionLog> LoginTransactionLogs { get; set; }
        public virtual ICollection<TrLoginAs> LoginAsCurrentUser { get; set; }
        public virtual ICollection<TrLoginAs> LoginAsImpresonatingUser { get; set; }
        public virtual ICollection<MsGroupMailingList> GroupMailingLists { get; set; }
        public virtual ICollection<MsGroupMailingListMember> GroupMailingListMembers { get; set; }
        public virtual ICollection<TrMessageForPersonal> MessageForPersonals { get; set; }
    }

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

            //builder.HasIndex(x => x.Username)
            //    .IsUnique();

            builder.Property(x => x.Email)
                .HasMaxLength(128);

            //builder.HasIndex(x => x.Email)
            //    .IsUnique();

            builder.Property(x => x.CountResetRequest)
                .HasDefaultValue(0);

            builder.Property(x => x.RequestChangePasswordCode)
                .HasMaxLength(36);

            builder.HasOne(x => x.UserPassword)
                .WithOne(x => x.User)
                .HasForeignKey<MsUserPassword>(x => x.Id)
                .HasConstraintName("FK_MsUserPassword_MsUser")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
