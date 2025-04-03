using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrLoginAs : AuditEntity, IUserEntity
    {
        public string IdCurrentUser { get; set; }
        public string IdImpresonatingUser { get; set; }
        public string IpAddress { get; set; }
        public DateTime LoginTime { get; set; }

        public virtual MsUser CurrentUser { get; set; }
        public virtual MsUser ImpresonatingUser { get; set; }
    }

    internal class TrLoginAsConfiguration : AuditEntityConfiguration<TrLoginAs>
    {
        public override void Configure(EntityTypeBuilder<TrLoginAs> builder)
        {
            builder.Property(x => x.IdCurrentUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdImpresonatingUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IpAddress)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.LoginTime)
                .IsRequired();

            builder.HasOne(x => x.CurrentUser)
                .WithMany(x => x.LoginAsCurrentUser)
                .HasForeignKey(fk => fk.IdCurrentUser)
                .HasConstraintName("FK_TrLoginAs_MsUser_CurrentUser")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.ImpresonatingUser)
                .WithMany(x => x.LoginAsImpresonatingUser)
                .HasForeignKey(fk => fk.IdImpresonatingUser)
                .HasConstraintName("FK_TrLoginAs_MsUser_ImpresonatingUser")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
