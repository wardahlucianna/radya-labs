using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageForPersonal : AuditEntity, IUserEntity
    {
        public string IdMessageFor { get; set; }
        public string IdUser { get; set; }
        public virtual TrMessageFor MessageFor { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrMessageForPersonalConfiguration : AuditEntityConfiguration<TrMessageForPersonal>
    {
        public override void Configure(EntityTypeBuilder<TrMessageForPersonal> builder)
        {
            builder.HasOne(x => x.MessageFor)
               .WithMany(x => x.MessageForPersonals)
               .HasForeignKey(fk => fk.IdMessageFor)
               .HasConstraintName("FK_TrMessageForPersonal_TrMessageFor")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.User)
               .WithMany(x => x.MessageForPersonals)
               .HasForeignKey(fk => fk.IdUser)
               .HasConstraintName("FK_TrMessageForPersonal_MsUser")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
