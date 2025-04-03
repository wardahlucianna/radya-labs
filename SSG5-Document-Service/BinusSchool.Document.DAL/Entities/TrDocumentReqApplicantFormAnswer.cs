using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrDocumentReqApplicantFormAnswer : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public string IdDocumentReqFormFieldAnswered { get; set; }
        public string IdDocumentReqOptionCategory { get; set; }
        public string IdDocumentReqOption { get; set; }
        public string TextValue { get; set; }
        public virtual TrDocumentReqApplicantDetail DocumentReqApplicantDetail { get; set; }
        public virtual MsDocumentReqFormFieldAnswered DocumentReqFormFieldAnswered { get; set; }
        public virtual MsDocumentReqOptionCategory DocumentReqOptionCategory { get; set; }
        public virtual MsDocumentReqOption DocumentReqOption { get; set; }
    }

    internal class TrDocumentReqApplicantFormAnswerConfiguration : AuditEntityConfiguration<TrDocumentReqApplicantFormAnswer>
    {
        public override void Configure(EntityTypeBuilder<TrDocumentReqApplicantFormAnswer> builder)
        {
            builder.Property(x => x.IdDocumentReqApplicantDetail)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdDocumentReqFormFieldAnswered)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdDocumentReqOptionCategory)
               .HasMaxLength(36);

            builder.Property(x => x.IdDocumentReqOption)
               .HasMaxLength(36);

            builder.Property(x => x.TextValue)
               .HasMaxLength(300);

            builder.HasOne(x => x.DocumentReqApplicantDetail)
                .WithMany(x => x.DocumentReqApplicantFormAnswers)
                .HasForeignKey(fk => fk.IdDocumentReqApplicantDetail)
                .HasConstraintName("FK_TrDocumentReqApplicantFormAnswer_TrDocumentReqApplicantDetail")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqFormFieldAnswered)
                .WithMany(x => x.DocumentReqApplicantFormAnswers)
                .HasForeignKey(fk => fk.IdDocumentReqFormFieldAnswered)
                .HasConstraintName("FK_TrDocumentReqApplicantFormAnswer_MsDocumentReqFormFieldAnswered")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqOptionCategory)
                .WithMany(x => x.DocumentReqApplicantFormAnswers)
                .HasForeignKey(fk => fk.IdDocumentReqOptionCategory)
                .HasConstraintName("FK_TrDocumentReqApplicantFormAnswer_MsDocumentReqOptionCategory")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentReqOption)
                .WithMany(x => x.DocumentReqApplicantFormAnswers)
                .HasForeignKey(fk => fk.IdDocumentReqOption)
                .HasConstraintName("FK_TrDocumentReqApplicantFormAnswer_MsDocumentReqOption")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
