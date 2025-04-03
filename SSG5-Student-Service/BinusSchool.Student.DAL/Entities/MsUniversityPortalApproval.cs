using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsUniversityPortalApproval : AuditEntity, IStudentEntity
    {
        public string IdUniversityPortal { get; set; }
        public string IdSchool { get; set; }
        public string StatusApproval { get; set; }
        public string ApprovalIdUser { get; set; }
        public DateTime ApprovalDate { get; set; }
        public virtual MsUniversityPortal UniversityPortal { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsUniversityPortalApprovalConfiguration : AuditEntityConfiguration<MsUniversityPortalApproval>
    {
        public override void Configure(EntityTypeBuilder<MsUniversityPortalApproval> builder)
        {
            builder.Property(x => x.StatusApproval).HasMaxLength(30);
            builder.Property(x => x.ApprovalIdUser).HasMaxLength(36);
            builder.Property(x => x.ApprovalDate);

            builder.HasOne(x => x.School)
             .WithMany(x => x.UniversityPortalApproval)
             .HasForeignKey(fk => fk.IdSchool)
             .HasConstraintName("FK_MsUniversityPortalApproval_MsSchool")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.UniversityPortal)
            .WithMany(x => x.UniversityPortalApproval)
            .HasForeignKey(fk => fk.IdUniversityPortal)
            .HasConstraintName("FK_MsUniversityPortalApproval_MsUniversityPortal")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }

}
