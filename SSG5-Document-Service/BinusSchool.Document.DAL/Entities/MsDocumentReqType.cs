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
    public class MsDocumentReqType : AuditEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public bool VisibleToParent { get; set; }
        public string IdAcademicYearStart { get; set; }
        public string IdAcademicYearEnd { get; set; }
        public bool IsAcademicDocument { get; set; }
        public bool DocumentHasTerm { get; set; }
        public decimal Price { get; set; }
        public int? InvoiceDueHoursPayment { get; set; }
        public int DefaultNoOfProcessDay { get; set; }
        public bool IsUsingNoOfCopy { get; set; }
        public int? MaxNoOfCopy { get; set; }
        public bool IsUsingNoOfPages { get; set; }
        public int? DefaultNoOfPages { get; set; }
        public bool ParentNeedApproval { get; set; }
        public bool HardCopyAvailable { get; set; }
        public bool SoftCopyAvailable { get; set; }
        public bool IsUsingGradeMapping { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsAcademicYear StartAcademicYear { get; set; }
        public virtual MsAcademicYear EndAcademicYear { get; set; }
        public virtual ICollection<MsDocumentReqTypeGradeMapping> DocumentReqTypeGradeMappings { get; set; }
        public virtual ICollection<MsDocumentReqDefaultPIC> DocumentReqDefaultPICs { get; set; }
        public virtual ICollection<MsDocumentReqDefaultPICGroup> DocumentReqDefaultPICGroups { get; set; }
        public virtual ICollection<MsDocumentReqFormField> DocumentReqFormFields { get; set; }
        public virtual ICollection<MsDocumentReqFormFieldAnswered> DocumentReqFormFieldAnswereds { get; set; }
        public virtual ICollection<TrDocumentReqApplicantDetail> DocumentReqApplicantDetails { get; set; }
    }

    internal class MsDocumentReqTypeConfiguration : AuditEntityConfiguration<MsDocumentReqType>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqType> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.IdAcademicYearStart)
                .HasMaxLength(36);

            builder.Property(x => x.IdAcademicYearEnd)
                .HasMaxLength(36);

            builder.Property(x => x.Price)
               .HasColumnType("money")
               .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.DocumentReqTypes)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsDocumentReqType_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.StartAcademicYear)
                .WithMany(x => x.StartAYDocumentReqTypes)
                .HasForeignKey(fk => fk.IdAcademicYearStart)
                .HasConstraintName("FK_MsDocumentReqType_MsAcademicYear_Start")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.EndAcademicYear)
                .WithMany(x => x.EndAYDocumentReqTypes)
                .HasForeignKey(fk => fk.IdAcademicYearEnd)
                .HasConstraintName("FK_MsDocumentReqType_MsAcademicYear_End")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
