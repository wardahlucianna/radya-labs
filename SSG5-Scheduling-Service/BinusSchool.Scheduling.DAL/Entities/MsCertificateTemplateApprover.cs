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
    public class MsCertificateTemplateApprover : AuditEntity, ISchedulingEntity
    {
        public int State { get; set; }
        public string IdCertificateTemplate { get; set; }
        public string IdUser { get; set; }
        public virtual MsCertificateTemplate CertificateTemplate { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class MsCertificateTemplateApproverConfiguration : AuditEntityConfiguration<MsCertificateTemplateApprover>
    {
        public override void Configure(EntityTypeBuilder<MsCertificateTemplateApprover> builder)
        {
            builder.Property(x => x.State).IsRequired();

            builder.HasOne(x => x.CertificateTemplate)
              .WithMany(x => x.CertificateTemplateApprovers)
              .HasForeignKey(fk => fk.IdCertificateTemplate)
              .HasConstraintName("FK_MsCertificateTemplateApprover_MsCertificateTemplate")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.User)
             .WithMany(x => x.CertificateTemplateApprovers)
             .HasForeignKey(fk => fk.IdUser)
             .HasConstraintName("FK_MsCertificateTemplateApprover_MsUser")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
