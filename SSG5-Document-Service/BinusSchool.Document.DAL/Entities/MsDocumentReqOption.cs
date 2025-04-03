using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqOption : AuditEntity, IDocumentEntity
    {
        public string OptionDescription { get; set; }
        public string IdDocumentReqOptionCategory { get; set; }
        public bool IsImportOption { get; set; }
        public bool Status { get; set; }
        public virtual MsDocumentReqOptionCategory DocumentReqOptionCategory { get; set; }
        public virtual ICollection<TrDocumentReqApplicantFormAnswer> DocumentReqApplicantFormAnswers { get; set; }
    }

    internal class MsDocumentReqOptionConfiguration : AuditEntityConfiguration<MsDocumentReqOption>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqOption> builder)
        {
            builder.Property(x => x.OptionDescription)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.IdDocumentReqOptionCategory)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqOptionCategory)
                .WithMany(x => x.DocumentReqOptions)
                .HasForeignKey(fk => fk.IdDocumentReqOptionCategory)
                .HasConstraintName("FK_MsDocumentReqOption_MsDocumentReqOptionCategory")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
