using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrLoginTransactionLog : AuditEntity, IUserEntity
    {
        public string IdUser { get; set; }
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; }
        public bool? SignInWithActiveDirectory { get; set; }
        public string Action { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrLoginTransactionLogConfiguration : AuditEntityConfiguration<TrLoginTransactionLog>
    {
        public override void Configure(EntityTypeBuilder<TrLoginTransactionLog> builder)
        {
            builder.Property(x => x.IdUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            builder.Property(x => x.Action)
                .HasMaxLength(10)
                .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.LoginTransactionLogs)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_TrLoginTransactionLog_MsUser")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            base.Configure(builder);
        }

    }
}
