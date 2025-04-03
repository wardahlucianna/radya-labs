using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace BinusSchool.Persistence.DocumentDb.Entities.User
{
    public class MsUser : AuditEntity, IDocumentEntity
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActiveDirectory { get; set; }
        public bool Status { get; set; }
        public virtual ICollection<MsBLPEmailAdditionalReceiver> BLPEmailAdditionalReceivers { get; set; }
        public virtual ICollection<MsFormAssignmentUser> FormAssignmentUsers { get; set; }
        public virtual ICollection<HMsFormDocApproval> FormDocApprovals { get; set; }
        public virtual ICollection<HMsFormDocChange> FormDocChanges { get; set; }
        public virtual ICollection<HMsFormDocChangeApproval> DocChangeApprovals { get; set; }
        public virtual ICollection<TrDocumentReqAttachment> DocumentReqAttachments { get; set; }


        //public virtual MsUserPassword UserPassword { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        //public virtual ICollection<MsUserSchool> UserSchools { get; set; }
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

            // builder.HasIndex(x => x.Username)
            //     .IsUnique();

            builder.Property(x => x.Email)
                .HasMaxLength(128);

            //builder.HasIndex(x=>x.Email)
            //    .IsUnique();

            //builder.HasOne(x => x.UserPassword)
            //    .WithOne(x => x.User)
            //    .HasForeignKey<MsUserPassword>(x => x.Id)
            //    .HasConstraintName("FK_MsUserPassword_MsUser")
            //    .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
