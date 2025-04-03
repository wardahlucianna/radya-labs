using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqOptionCategory : AuditEntity, IDocumentEntity
    {
        public string CategoryDescription { get; set; }
        public string IdDocumentReqFieldType { get; set; }
        public bool IsDefaultImportData { get; set; }
        public string IdSchool { get; set; }
        public string Code { get; set; }
        public virtual LtDocumentReqFieldType DocumentReqFieldType { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsDocumentReqOption> DocumentReqOptions { get; set; }
        public virtual ICollection<TrDocumentReqApplicantFormAnswer> DocumentReqApplicantFormAnswers { get; set; }
        public virtual ICollection<MsDocumentReqFormField> DocumentReqFormFields { get; set; }
        public virtual ICollection<MsDocumentReqFormFieldAnswered> DocumentReqFormFieldAnswereds { get; set; }
    }

    internal class MsDocumentReqOptionCategoryConfiguration : AuditEntityConfiguration<MsDocumentReqOptionCategory>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqOptionCategory> builder)
        {
            builder.Property(x => x.CategoryDescription)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.IdDocumentReqFieldType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Code)
                .HasMaxLength(50);

            builder.HasOne(x => x.DocumentReqFieldType)
                .WithMany(x => x.DocumentReqOptionCategories)
                .HasForeignKey(fk => fk.IdDocumentReqFieldType)
                .HasConstraintName("FK_MsDocumentReqOptionCategory_LtDocumentReqFieldType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.DocumentReqOptionCategories)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsDocumentReqOptionCategory_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
