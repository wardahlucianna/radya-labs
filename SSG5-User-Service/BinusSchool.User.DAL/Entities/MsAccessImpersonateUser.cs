using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsAccessImpersonateUser : AuditEntity, IUserEntity
    {
        public string IdUser { get; set; }
        public bool CanImpersonateLogin { get; set; }
        public virtual MsUser User { get; set; }
        public virtual IEnumerable<TrAccessImpersonateUserLog> AccessImpersonateUserLogs { get; set; }
    }

    internal class MsAccessImpersonateUserConfiguration : AuditEntityConfiguration<MsAccessImpersonateUser>
    {
        public override void Configure(EntityTypeBuilder<MsAccessImpersonateUser> builder)
        {
            builder.Property(x => x.IdUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasIndex(x => x.IdUser)
                .IsUnique();

            builder.HasOne(x => x.User)
                .WithMany(x => x.AccessImpersonateUsers)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsAccessImpersonateUser_MsUser")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
