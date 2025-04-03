using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqDefaultPICGroup : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqType { get; set; }
        public string IdRole { get; set; }
        public virtual MsDocumentReqType DocumentReqType { get; set; }
        public virtual LtRole Role { get; set; }
    }

    internal class MsDocumentReqDefaultPICGroupConfiguration : AuditEntityConfiguration<MsDocumentReqDefaultPICGroup>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqDefaultPICGroup> builder)
        {
            builder.Property(x => x.IdDocumentReqType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdRole)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqType)
                .WithMany(x => x.DocumentReqDefaultPICGroups)
                .HasForeignKey(fk => fk.IdDocumentReqType)
                .HasConstraintName("FK_MsDocumentReqDefaultPICGroup_MsDocumentReqType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany(x => x.DocumentReqDefaultPICGroups)
                .HasForeignKey(fk => fk.IdRole)
                .HasConstraintName("FK_MsDocumentReqDefaultPICGroup_LtRole")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
