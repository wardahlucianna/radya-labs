using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsBLPSetting : AuditEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public bool NeedBarcode { get; set; }
        public bool NeedHardCopy { get; set; }
        public string FooterEmail { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsBLPEmail> BLPEmails { get; set; }
    }

    internal class MsBLPEmailSettingConfiguration : AuditEntityConfiguration<MsBLPSetting>
    {
        public override void Configure(EntityTypeBuilder<MsBLPSetting> builder)
        {
            builder.Property(x => x.IdSchool)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.NeedBarcode)
              .IsRequired();

            builder.Property(x => x.FooterEmail)
              .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.BLPEmails)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsBLPSetting_MsSchool")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
