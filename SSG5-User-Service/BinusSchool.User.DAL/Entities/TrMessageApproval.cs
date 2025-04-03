using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageApproval : AuditEntity, IUserEntity
    {
        public string IdMessage { get; set; }
        public int StateNumber { get; set; }
        public bool IsApproved { get; set; }
        public string IdRole { get; set; }
        public string IdUser { get; set; }
        public string Reason { get; set; }
        public bool IsUnsendApproved { get; set; }
        public string UnsendReason { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsUser User { get; set; }
        public virtual TrMessage Message { get; set; }
    }

    internal class TrMessageApprovalConfiguration : AuditEntityConfiguration<TrMessageApproval>
    {
        public override void Configure(EntityTypeBuilder<TrMessageApproval> builder)
        {
            builder.HasOne(x => x.Role)
                 .WithMany(x => x.MessageApprovals)
                 .HasForeignKey(fk => fk.IdRole)
                 .HasConstraintName("FK_TrMessageApproval_LtRole")
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.MessageApprovals)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_TrMessageApproval_MsUser")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Message)
               .WithMany(x => x.MessageApprovals)
               .HasForeignKey(fk => fk.IdMessage)
               .HasConstraintName("FK_TrMessageApproval_TrMessage")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.Property(x => x.Reason).HasMaxLength(450);

            builder.Property(x => x.UnsendReason).HasMaxLength(450);

            base.Configure(builder);
        }
    }
}
