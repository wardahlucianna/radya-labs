using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class LtBLPStatus : AuditEntity, IDocumentEntity
    {
        public string BLPStatusName { get; set; }
        public string ShortName { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrBLPGroupStudent> BLPGroupStudents { get; set; }
        public virtual ICollection<TrBLPUpdatedConsentStatus> BeforeBLPUpdatedConsentStatuses { get; set; }
        public virtual ICollection<TrBLPUpdatedConsentStatus> AfterBLPUpdatedConsentStatuses { get; set; }
    }
    internal class LtBLPStatusConfiguration : AuditEntityConfiguration<LtBLPStatus>
    {
        public override void Configure(EntityTypeBuilder<LtBLPStatus> builder)
        {
            builder.Property(x => x.BLPStatusName)
              .HasMaxLength(100)
              .IsRequired();

            builder.Property(x => x.IdSchool)
              .HasMaxLength(36)
              .HasDefaultValue("1")
              .IsRequired();

            builder.Property(x => x.ShortName)
              .HasMaxLength(36)
              .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.BLPStatuses)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_LtBLPStatus_MsSchool")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
