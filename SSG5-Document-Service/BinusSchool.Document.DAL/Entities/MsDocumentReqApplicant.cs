using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqApplicant : AuditEntity, IDocumentEntity
    {
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public int EstimationFinishDays { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public string IdParentApplicant { get; set; }
        public string IdStudent { get; set; }
        public string IdSchool { get; set; }
        public int IdStudentStatus { get; set; }
        public string IdHomeroom { get; set; }
        public bool DocumentNeedApproval { get; set; }
        public bool CanProcessBeforePaid { get; set; }
        public string CreatedBy { get; set; }
        public string IdBinusianApprover { get; set; }
        public DocumentRequestApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalRemarks { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public virtual MsParent Parent { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual LtStudentStatus LtStudentStatus { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual LtDocumentReqStatusWorkflow DocumentRequestStatusWorkflow { get; set; }
        public virtual ICollection<TrDocumentReqApplicantDetail> DocumentReqApplicantDetails { get; set; }
        public virtual ICollection<TrDocumentReqStatusTrackingHistory> DocumentReqStatusTrackingHistories { get; set; }
        public virtual ICollection<MsDocumentReqApplicantCollection> DocumentReqApplicantCollections { get; set; }
        public virtual ICollection<TrDocumentReqPaymentMapping> DocumentReqPaymentMappings { get; set; }
    }

    internal class MsDocumentReqApplicantConfiguration : AuditEntityConfiguration<MsDocumentReqApplicant>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqApplicant> builder)
        {
            builder.Property(x => x.RequestNumber)
                .HasMaxLength(13)
                .IsRequired();

            builder.Property(x => x.IdParentApplicant)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdHomeroom)
                .HasMaxLength(36);

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.IdBinusianApprover)
                .HasMaxLength(36);
            
            builder.Property(x => x.ApprovalRemarks)
                .HasMaxLength(500);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.DocumentReqApplicants)
                .HasForeignKey(fk => fk.IdParentApplicant)
                .HasConstraintName("FK_MsDocumentReqApplicant_MsParent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.DocumentReqApplicants)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsDocumentReqApplicant_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.LtStudentStatus)
                .WithMany(x => x.DocumentReqApplicants)
                .HasForeignKey(fk => fk.IdStudentStatus)
                .HasConstraintName("FK_MsDocumentReqApplicant_LtStudentStatus")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.DocumentReqApplicants)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsDocumentReqApplicant_MsHomeroom")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.DocumentReqApplicants)
                .HasForeignKey(fk => fk.IdBinusianApprover)
                .HasConstraintName("FK_MsDocumentReqApplicant_MsStaff")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.School)
                .WithMany(x => x.DocumentReqApplicants)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsDocumentReqApplicant_MsSchool")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.DocumentRequestStatusWorkflow)
                .WithMany(x => x.DocumentReqApplicants)
                .HasForeignKey(fk => fk.IdDocumentReqStatusWorkflow)
                .HasConstraintName("FK_MsDocumentReqApplicant_LtDocumentReqStatusWorkflow")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
