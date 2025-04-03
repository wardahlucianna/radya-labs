using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsMessageApprovalState : AuditEntity, IUserEntity
    {
        public string IdMessageApproval { get; set; }
        public string IdRole { get; set; }
        public int Number { get; set; }
        public string IdUser { get; set; }
        public virtual MsMessageApproval MessageApproval { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class MsMessageApprovalStateConfiguration : AuditEntityConfiguration<MsMessageApprovalState>
    {
        public override void Configure(EntityTypeBuilder<MsMessageApprovalState> builder)
        {
            builder.HasOne(x => x.MessageApproval)
               .WithMany(x => x.ApprovalStates)
               .HasForeignKey(fk => fk.IdMessageApproval)
               .HasConstraintName("FK_MsMessageApprovalState_MsMessageApproval")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Role)
             .WithMany(x => x.ApprovalStates)
             .HasForeignKey(fk => fk.IdRole)
             .HasConstraintName("FK_MsMessageApprovalState_LtRole")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.User)
               .WithMany(x => x.MessageApprovalStates)
               .HasForeignKey(fk => fk.IdUser)
               .HasConstraintName("FK_MsMessageApprovalState_MsUser")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
