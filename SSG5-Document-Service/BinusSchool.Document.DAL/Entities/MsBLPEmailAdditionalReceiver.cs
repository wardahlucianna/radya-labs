using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsBLPEmailAdditionalReceiver : AuditEntity, IDocumentEntity
    {
        public string IdBLPEmail { get; set; }
        public string IdUser { get; set; }
        public string AddressType { get; set; }
        public virtual MsBLPEmail BLPEmail { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class MsBLPEmailAdditionalReceiverConfiguration : AuditEntityConfiguration<MsBLPEmailAdditionalReceiver>
    {
        public override void Configure(EntityTypeBuilder<MsBLPEmailAdditionalReceiver> builder)
        {
            builder.Property(x => x.IdBLPEmail)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.IdUser)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.AddressType)
              .HasMaxLength(10)
              .IsRequired();

            builder.HasOne(x => x.BLPEmail)
                .WithMany(x => x.BLPEmailAdditionalReceivers)
                .HasForeignKey(fk => fk.IdBLPEmail)
                .HasConstraintName("FK_MsBLPEmailAdditionalReceiver_MsBLPEmail")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.BLPEmailAdditionalReceivers)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsBLPEmailAdditionalReceiver_MsUser")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
