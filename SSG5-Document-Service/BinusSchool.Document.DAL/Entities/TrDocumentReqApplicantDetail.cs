using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrDocumentReqApplicantDetail : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqApplicant { get; set; }
        public string IdDocumentReqType { get; set; }
        public int? NoOfPages { get; set; }
        public int NoOfCopy { get; set; }
        public decimal PriceReal { get; set; }
        public decimal PriceInvoice { get; set; }
        public string IdAcademicYearDocument { get; set; }
        public string IdPeriodDocument { get; set; }
        public bool NeedHardCopy { get; set; }
        public bool NeedSoftCopy { get; set; }
        public DateTime? ReceivedDateByStaff { get; set; }
        public string IdBinusianReceiver { get; set; }

        public virtual MsDocumentReqApplicant DocumentReqApplicant { get; set; }
        public virtual MsDocumentReqType DocumentReqType { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsPeriod Period { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual ICollection<TrDocumentReqAttachment> DocumentReqAttachments { get; set; }
        public virtual ICollection<TrDocumentReqPIC> DocumentReqPICs { get; set; }
        public virtual ICollection<TrDocumentReqApplicantFormAnswer> DocumentReqApplicantFormAnswers { get; set; }
    }

    internal class TrDocumentReqApplicantDetailConfiguration : AuditEntityConfiguration<TrDocumentReqApplicantDetail>
    {
        public override void Configure(EntityTypeBuilder<TrDocumentReqApplicantDetail> builder)
        {
            builder.Property(x => x.IdDocumentReqApplicant)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdDocumentReqType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.PriceReal)
               .HasColumnType("money")
               .IsRequired();

            builder.Property(x => x.PriceInvoice)
               .HasColumnType("money")
               .IsRequired();

            builder.Property(x => x.IdAcademicYearDocument)
                .HasMaxLength(36);

            builder.Property(x => x.IdPeriodDocument)
                .HasMaxLength(36);

            builder.Property(x => x.IdBinusianReceiver)
                .HasMaxLength(36);

            builder.HasOne(x => x.DocumentReqApplicant)
                .WithMany(x => x.DocumentReqApplicantDetails)
                .HasForeignKey(fk => fk.IdDocumentReqApplicant)
                .HasConstraintName("FK_TrDocumentReqApplicantDetail_MsDocumentReqApplicant")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqType)
                .WithMany(x => x.DocumentReqApplicantDetails)
                .HasForeignKey(fk => fk.IdDocumentReqType)
                .HasConstraintName("FK_TrDocumentReqApplicantDetail_MsDocumentReqType")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.DocumentReqApplicantDetails)
                .HasForeignKey(fk => fk.IdAcademicYearDocument)
                .HasConstraintName("FK_TrDocumentReqApplicantDetail_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Period)
                .WithMany(x => x.DocumentReqApplicantDetails)
                .HasForeignKey(fk => fk.IdPeriodDocument)
                .HasConstraintName("FK_TrDocumentReqApplicantDetail_MsPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.DocumentReqApplicantDetails)
                .HasForeignKey(fk => fk.IdBinusianReceiver)
                .HasConstraintName("FK_TrDocumentReqApplicantDetail_MsStaff")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
