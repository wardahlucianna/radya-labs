using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HMsCertificateTemplateApprover : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHMsCertificateTemplateApprover { get; set; }
        public string IdCertificateTemplate { get; set; }
        public int State { get; set; }
        public bool IsApproved { get; set; }
        public string Reason { get; set; }
        public string IdUser { get; set; }
        public virtual MsCertificateTemplate CertificateTemplate { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class HTrCertificateTemplateApproverConfiguration : AuditNoUniqueEntityConfiguration<HMsCertificateTemplateApprover>
    {
        public override void Configure(EntityTypeBuilder<HMsCertificateTemplateApprover> builder)
        {
            builder.HasKey(x => x.IdHMsCertificateTemplateApprover);

            builder.Property(x => x.IdHMsCertificateTemplateApprover)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.CertificateTemplate)
           .WithMany(x => x.HistoryCertificateTemplateApprovers)
           .HasForeignKey(fk => fk.IdCertificateTemplate)
           .HasConstraintName("FK_HMsCertificateTemplateApprover_MsCertificateTemplate")
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired();

            builder.HasOne(x => x.User)
            .WithMany(x => x.HistoryCertificateTemplateApprovers)
            .HasForeignKey(fk => fk.IdUser)
            .HasConstraintName("FK_HMsCertificateTemplateApprover_MsUser")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
