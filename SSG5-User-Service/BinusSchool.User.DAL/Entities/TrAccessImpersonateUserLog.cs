using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrAccessImpersonateUserLog : AuditEntity, IUserEntity
    {
        public string IdAccessImpersonateUser { get; set; }
        public string ImpersonatedIdUser { get; set; }
        public DateTime LoginTime { get; set; }
        public string  IpAddress { get; set; }
        public virtual MsAccessImpersonateUser AccessImpersonateUser { get; set; }
        public virtual MsUser User { get; set; }
    }
    internal class TrAccessImpersonateUserLogConfiguration : AuditEntityConfiguration<TrAccessImpersonateUserLog>
    {
        public override void Configure(EntityTypeBuilder<TrAccessImpersonateUserLog> builder)
        {
            builder.Property(x => x.IdAccessImpersonateUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.ImpersonatedIdUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            builder.HasOne(x => x.AccessImpersonateUser)
             .WithMany(x => x.AccessImpersonateUserLogs)
             .HasForeignKey(fk => fk.IdAccessImpersonateUser)
             .HasConstraintName("FK_TrAccessImpersonateUser_MsAccessImpersonateUser")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.AccessImpersonateUserLogs)
             .HasForeignKey(fk => fk.ImpersonatedIdUser)
             .HasConstraintName("FK_TrAccessImpersonateUser_MsUser")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            base.Configure(builder);
        }

    }
}
