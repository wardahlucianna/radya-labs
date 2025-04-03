using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsCertificateTemplate : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear  { get; set; }
        public string Name { get; set; }
        public bool IsUseBinusLogo { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string Background { get; set; }
        public string Signature1 { get; set; }
        public string Signature1As { get; set; }
        public string Signature2 { get; set; }
        public string Signature2As { get; set; }
        /// <summary>
        /// Value For Approval Status <br/>
        /// 1. On Review ({State}) <br/>
        /// 2. Approved <br/>
        /// 3. Declined <br/>
        /// </summary>
        public string ApprovalStatus { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsUser User1 { get; set; }
        public virtual MsUser User2 { get; set; }
        public virtual ICollection<TrEvent> Events { get; set; }
        public virtual ICollection<HTrEvent> HistoryEvents { get; set; }
        public virtual ICollection<MsCertificateTemplateApprover> CertificateTemplateApprovers { get; set; }
        public virtual ICollection<HMsCertificateTemplateApprover> HistoryCertificateTemplateApprovers { get; set; }
    }

    internal class MsCertificateTemplateConfiguration : AuditEntityConfiguration<MsCertificateTemplate>
    {
        public override void Configure(EntityTypeBuilder<MsCertificateTemplate> builder)
        {
            builder.HasOne(x => x.AcademicYear)
            .WithMany(x => x.CertificateTemplates)
            .HasForeignKey(fk => fk.IdAcademicYear )
            .HasConstraintName("FK_MsCertificateTemplate_MsAcademicYear")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.User1)
            .WithMany(x => x.Signature1)
            .HasForeignKey(fk => fk.Signature1)
            .HasConstraintName("FK_MsCertificateTemplate_MsUser_Signature1")
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User2)
            .WithMany(x => x.Signature2)
            .HasForeignKey(fk => fk.Signature2)
            .HasConstraintName("FK_MsCertificateTemplate_MsUser_Signature2")
            .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(50).IsRequired();
            builder.Property(x => x.SubTitle).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(450).IsRequired();
            builder.Property(x => x.Background).HasMaxLength(450).IsRequired();
            
            base.Configure(builder);
        }
    }
}
