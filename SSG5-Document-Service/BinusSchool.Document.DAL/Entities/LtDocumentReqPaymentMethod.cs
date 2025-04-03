using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class LtDocumentReqPaymentMethod : AuditEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public bool UsingManualVerification { get; set; }
        public bool IsVirtualAccount { get; set; }
        public string AccountNumber { get; set; }
        public string DescriptionHTML { get; set; }
        public string ImageUrl { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrDocumentReqPaymentMapping> DocumentReqPaymentMappings { get; set; }
    }

    internal class LtDocumentReqPaymentMethodConfiguration : AuditEntityConfiguration<LtDocumentReqPaymentMethod>
    {
        public override void Configure(EntityTypeBuilder<LtDocumentReqPaymentMethod> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.AccountNumber)
                .HasMaxLength(50);

            builder.Property(x => x.DescriptionHTML)
               .HasColumnType("text")
               .IsRequired();

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            builder.HasOne(x => x.School)
                .WithMany(x => x.LtDocumentReqPaymentMethods)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_LtDocumentReqPaymentMethod_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            
            base.Configure(builder);
        }
    }
}
